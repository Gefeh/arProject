using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using TMPro;

public enum GameState
{
    BeforeStart,
    HuntingLandmarks,
    InChallenge,
    QuestComplete
}

[System.Serializable]
public class Landmark
{
    public string name;
    public double latitude;
    public double longitude;
    public bool isFound = false;
    public GameObject challenge;
    public int id;
}

[System.Serializable]
public struct GPSCoordinate
{
    public double latitude;
    public double longitude;
}

[System.Serializable]
public class RectZone
{
    public string name;
    [Tooltip("The 4 corner points of the zone, entered in a clockwise or counter-clockwise order (e.g., SW, SE, NE, NW).")]
    public GPSCoordinate[] corners = new GPSCoordinate[4];
}

public class GPSController : MonoBehaviour
{
    [Header("Landmark Settings")]
    public RectZone startZone;
    public Landmark[] landmarks;
    public double triggerDistance = 15.0;

    [Header("Challenge Order")]
    public bool useRandomOrder = false;
    private List<Landmark> landmarkChallengeOrder;
    public int currentChallengeIndex = 0;

    [Header("Gamestate")]
    public GameState currentGameState = GameState.BeforeStart;

    private int currentLandmarkIndex = 0;
    private double distanceToTarget;

    public static GPSController instance;

    private void Awake()
    {
        #region debug
        Debug.Log("GPSController --- Step 1: Awake() method called.");
        #endregion

        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        if (GameManager.Instance.MMUI.MarkText != null) GameManager.Instance.MMUI.UpdateText("GPS Controller starting...");
    }

    IEnumerator Start()
    {
        SetupChallengeOrder();
        #region debug
        Debug.Log("GPSController --- Step 2: Start() coroutine initiated.");
        #endregion

        yield return new WaitForSeconds(1);

        #region debug
        Debug.Log("GPSController --- Step 3: Checking for location permission.");
        #endregion

        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {

            #region debug
            Debug.Log("GPSController --- Step 4: Permission not found. Requesting it now.");
            #endregion

            GameManager.Instance.MMUI.UpdateText("Requesting location permission...");
            Permission.RequestUserPermission(Permission.FineLocation);

            float timer = 0;
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) && timer < 20f)
            {
                yield return new WaitForSeconds(1);
                timer++;
            }
        }

        #region debug
        Debug.Log("GPSController --- Step 5: Finished permission check. Now checking if service is enabled.");

        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("GPSController --- FAILED: User has not enabled GPS in phone settings.");
            GameManager.Instance.GameUI.UpdateText(GameManager.Instance.GameUI.BottomText, "Please enable Location Services in your device settings.");
            yield break;
        }

        Debug.Log("GPSController --- Step 6: Location service is enabled by user. Starting service.");
        #endregion

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        #region debug
        if (maxWait <= 0 || Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("GPSController --- FAILED: Unable to determine device location or timed out.");
            GameManager.Instance.GameUI.UpdateText(GameManager.Instance.GameUI.BottomText, "Unable to determine device location.");
            yield break;
        }

        Debug.Log("GPSController --- Step 7: GPS Initialized successfully! Starting updates.");
        #endregion

        InvokeRepeating(nameof(UpdateGpsData), 0.5f, 1f);
    }

    public void StartLandmarkHunt()
    {
        if (currentGameState == GameState.BeforeStart)
        {
            Debug.Log("Landmark hunt has started!");
            SetupChallengeOrder();
            GameManager.Instance.GameUI.EnableUI();
            currentGameState = GameState.HuntingLandmarks;
        }
    }

    /// <summary>
    /// Updates the player's location according to the app.
    /// </summary>
    void UpdateGpsData()
    {
        if (Input.location.status != LocationServiceStatus.Running) return;

        switch (currentGameState)
        {
            case GameState.BeforeStart:
                CheckStartZone();
                break;
            case GameState.HuntingLandmarks:
                CheckLandmarkProgress();
                break;
            case GameState.QuestComplete:
                
                break;
        }
    }

    /// <summary>
    /// Checks if the player is in the startzone
    /// </summary>
    private void CheckStartZone()
    {
        GPSCoordinate playerPosition = new GPSCoordinate
        {
            latitude = Input.location.lastData.latitude,
            longitude = Input.location.lastData.longitude
        };

        // Call our new helper function to do the complex check
        if (IsInsideAngledRect(playerPosition, startZone))
        {
            GameManager.Instance.MMUI.SetStartButtonEnabled(true);
            GameManager.Instance.MMUI.UpdateText("You are in the starting area. Press Start to begin!");
        }
        else
        {
            GameManager.Instance.MMUI.SetStartButtonEnabled(false);
            GameManager.Instance.MMUI.UpdateText($"Please go to the {startZone.name} to begin.");
        }
    }

    /// <summary>
    /// Checsk the player's position compared to the currently assigned landmark
    /// </summary>
    void CheckLandmarkProgress()
    {
        if (currentChallengeIndex >= landmarkChallengeOrder.Count)
        {
            GameManager.Instance.GameUI.UpdateText(GameManager.Instance.GameUI.BottomText, "Congratulations!\nYou've found all landmarks!");
            CancelInvoke(nameof(UpdateGpsData));
            return;
        }

        Landmark currentTarget = landmarkChallengeOrder[currentChallengeIndex];

        double currentLatitude = Input.location.lastData.latitude;
        double currentLongitude = Input.location.lastData.longitude;

        distanceToTarget = CalculateDistance(currentLatitude, currentLongitude, currentTarget.latitude, currentTarget.longitude);

        if (GameManager.Instance.GameUI.BottomText != null)
        {
            GameManager.Instance.GameUI.UpdateText(GameManager.Instance.GameUI.BottomText, $"Go find: {currentTarget.name}\n" +
                             $"Lat: {currentLatitude:F6}\n" +
                             $"Lon: {currentLongitude:F6}\n" +
                             $"Dist to Target: {distanceToTarget:F1} meters");
        }

        if (distanceToTarget <= triggerDistance && !currentTarget.isFound)
        {
            Debug.Log($"Found the landmark: {currentTarget.name}!");
            currentTarget.isFound = true;
            currentGameState = GameState.InChallenge;
            TriggerLandmarkEvent(currentTarget);
        }
    }

    public void ResumeLandmarkHunt()
    {
        GameManager.Instance.GameUI.UpdateText(GameManager.Instance.GameUI.BigNumber, "");
        currentGameState = GameState.HuntingLandmarks;
    }

    /// <summary>
    /// Triggers code in accordance with when the appropriate landmark is reached.
    /// </summary>
    /// <param name="foundLandmark">The Landmark object that was just found.</param>
    void TriggerLandmarkEvent(Landmark foundLandmark)
    {
        GameManager.Instance.GameUI.AppendText(GameManager.Instance.GameUI.BottomText, $"\n--- FOUND {foundLandmark.name}! ---");
        if (foundLandmark.challenge != null)
        {
            Debug.Log("kill yourself");
            GameManager.Instance.ChallengeManager.StartChallenge(foundLandmark.challenge);
        }
        else
        {
            Debug.Log("kill yourself frfr");
            GameManager.Instance.ChallengeManager.WinChallenge(foundLandmark.challenge);
        }
    }

    /// <summary>
    /// Sets up quest order, if randomized.
    /// </summary>
    void SetupChallengeOrder()
    {
        landmarkChallengeOrder = new List<Landmark>(landmarks);

        if (useRandomOrder)
        {
            Debug.Log("Setting up quest in RANDOM order.");
            System.Random rng = new System.Random();
            int n = landmarkChallengeOrder.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Landmark value = landmarkChallengeOrder[k];
                landmarkChallengeOrder[k] = landmarkChallengeOrder[n];
                landmarkChallengeOrder[n] = value;
            }
        }
        else
        {
            Debug.Log("Setting up quest in Inspector array order.");
        }

        foreach (var landmark in landmarkChallengeOrder)
        {
            landmark.isFound = false;
        }
    }

    /// <summary>
    /// Calculates distance between coordinates.
    /// </summary>
    /// <param name="lat1"></param>
    /// <param name="lon1"></param>
    /// <param name="lat2"></param>
    /// <param name="lon2"></param>
    /// <returns></returns>
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371e3;
        var φ1 = lat1 * Math.PI / 180.0;
        var φ2 = lat2 * Math.PI / 180.0;
        var Δφ = (lat2 - lat1) * Math.PI / 180.0;
        var Δλ = (lon2 - lon1) * Math.PI / 180.0;

        var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                Math.Cos(φ1) * Math.Cos(φ2) *
                Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        var d = R * c;
        return d;
    }

    private bool IsInsideAngledRect(GPSCoordinate point, RectZone zone)
    {
        if (zone.corners.Length < 4) return false;

        double px = point.longitude;
        double py = point.latitude;

        double firstSign = GetSide(px, py, zone.corners[0], zone.corners[1]);

        for (int i = 1; i < 4; i++)
        {
            GPSCoordinate corner1 = zone.corners[i];
            GPSCoordinate corner2 = zone.corners[(i + 1) % 4];

            double currentSign = GetSide(px, py, corner1, corner2);

            if (firstSign != 0 && currentSign != 0 && firstSign != currentSign)
            {
                return false;
            }
        }

        return true;
    }

    private double GetSide(double px, double py, GPSCoordinate c1, GPSCoordinate c2)
    {
        double val = (c2.longitude - c1.longitude) * (py - c1.latitude) - (c2.latitude - c1.latitude) * (px - c1.longitude);
        return Math.Sign(val);
    }
}