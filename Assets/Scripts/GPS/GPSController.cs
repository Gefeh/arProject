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
    QuestComplete
}

[System.Serializable]
public class Landmark
{
    public string name;
    public double latitude;
    public double longitude;
    public bool isFound = false;
    public int id;
}

public class GPSController : MonoBehaviour
{
    [Header("Landmark Settings")]
    public Landmark startZone;
    public Landmark[] landmarks;
    public double triggerDistance = 15.0;

    [Header("Challenge Order")]
    public bool useRandomOrder = false;
    private List<Landmark> landmarkChallengeOrder;
    private int currentChallengeIndex = 0;

    [Header("Gamestate")]
    private GameState currentGameState = GameState.BeforeStart;

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
            GameManager.Instance.GameUI.UpdateText("Please enable Location Services in your device settings.");
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
            GameManager.Instance.GameUI.UpdateText("Unable to determine device location.");
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
        double currentLatitude = Input.location.lastData.latitude;
        double currentLongitude = Input.location.lastData.longitude;
        double distanceToStart = CalculateDistance(currentLatitude, currentLongitude, startZone.latitude, startZone.longitude);

        if (distanceToStart <= triggerDistance)
        {
            GameManager.Instance.MMUI.SetStartButtonEnabled(true);
            GameManager.Instance.MMUI.UpdateText("You are in the starting area. Press Start to begin!");
        }
        else
        {
            GameManager.Instance.MMUI.SetStartButtonEnabled(false);
            GameManager.Instance.MMUI.UpdateText($"Please go to the {startZone.name} to begin your quest. ({distanceToStart:F0}m away)");
        }
    }

    /// <summary>
    /// Checsk the player's position compared to the currently assigned landmark
    /// </summary>
    void CheckLandmarkProgress()
    {
        if (currentChallengeIndex >= landmarkChallengeOrder.Count)
        {
            GameManager.Instance.GameUI.UpdateText("Congratulations!\nYou've found all landmarks!");
            CancelInvoke(nameof(UpdateGpsData));
            return;
        }

        Landmark currentTarget = landmarkChallengeOrder[currentChallengeIndex];

        double currentLatitude = Input.location.lastData.latitude;
        double currentLongitude = Input.location.lastData.longitude;

        distanceToTarget = CalculateDistance(currentLatitude, currentLongitude, currentTarget.latitude, currentTarget.longitude);

        if (GameManager.Instance.GameUI.BottomText != null)
        {
            GameManager.Instance.GameUI.UpdateText($"Go find: {currentTarget.name}\n" +
                             $"Lat: {currentLatitude:F6}\n" +
                             $"Lon: {currentLongitude:F6}\n" +
                             $"Dist to Target: {distanceToTarget:F1} meters");
        }

        if (distanceToTarget <= triggerDistance && !currentTarget.isFound)
        {
            Debug.Log($"Found the landmark: {currentTarget.name}!");
            currentTarget.isFound = true;
            TriggerLandmarkEvent(currentTarget);
            currentChallengeIndex++;
        }
    }

    /// <summary>
    /// Triggers code in accordance with when the appropriate landmark is reached.
    /// </summary>
    /// <param name="foundLandmark">The Landmark object that was just found.</param>
    void TriggerLandmarkEvent(Landmark foundLandmark) // MODIFIED: The parameter is now a Landmark
    {
        GameManager.Instance.GameUI.AppendText($"\n--- FOUND {foundLandmark.name}! ---");

        // Now, we use the landmark's ID for the switch statement.
        // This is more robust than relying on the array index.
        switch (foundLandmark.id)
        {
            case 0:
                Debug.Log("Starting challenge for landmark with ID 0!");
                break;
            case 1:
                Debug.Log("Starting challenge for landmark with ID 1!");
                break;
            case 2:
                Debug.Log("Starting challenge for landmark with ID 2!");
                break;
            default:
                Debug.Log($"An unknown landmark with ID {foundLandmark.id} was found.");
                break;
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
}