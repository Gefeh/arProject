using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using TMPro;
using System;

[System.Serializable]
public class Landmark
{
    public string name;
    public double latitude;
    public double longitude;
    public bool isFound = false;
}

public class GPSController : MonoBehaviour
{
    [Header("Landmark Settings")]
    public Landmark[] landmarks;
    public double triggerDistance = 15.0;

    [Header("UI for Debugging")]
    public TextMeshProUGUI debugText;

    private int currentLandmarkIndex = 0;
    private double distanceToTarget;

    public static GPSController instance;

    private void Awake()
    {
        Debug.Log("GPSController --- Step 1: Awake() method called.");

        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        if (debugText != null) debugText.text = "GPS Controller starting...";
    }

    IEnumerator Start()
    {
        Debug.Log("GPSController --- Step 2: Start() coroutine initiated.");

        yield return new WaitForSeconds(1);

        Debug.Log("GPSController --- Step 3: Checking for location permission.");
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Debug.Log("GPSController --- Step 4: Permission not found. Requesting it now.");
            debugText.text = "Requesting location permission...";
            Permission.RequestUserPermission(Permission.FineLocation);

            float timer = 0;
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) && timer < 20f)
            {
                yield return new WaitForSeconds(1);
                timer++;
            }
        }

        Debug.Log("GPSController --- Step 5: Finished permission check. Now checking if service is enabled.");

        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("GPSController --- FAILED: User has not enabled GPS in phone settings.");
            debugText.text = "Please enable Location Services in your device settings.";
            yield break;
        }

        // --- DEBUG CHECKPOINT 6 ---
        Debug.Log("GPSController --- Step 6: Location service is enabled by user. Starting service.");
        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0 || Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("GPSController --- FAILED: Unable to determine device location or timed out.");
            debugText.text = "Unable to determine device location.";
            yield break;
        }

        Debug.Log("GPSController --- Step 7: GPS Initialized successfully! Starting updates.");
        InvokeRepeating("UpdateGpsData", 0.5f, 1f);
    }

    void UpdateGpsData()
    {
        if (Input.location.status != LocationServiceStatus.Running) return;

        if (currentLandmarkIndex >= landmarks.Length)
        {
            debugText.text = "Congratulations!\nYou've found all landmarks!";
            CancelInvoke(nameof(UpdateGpsData));
            return;
        }

        Landmark currentTarget = landmarks[currentLandmarkIndex];

        double currentLatitude = Input.location.lastData.latitude;
        double currentLongitude = Input.location.lastData.longitude;

        distanceToTarget = CalculateDistance(currentLatitude, currentLongitude, currentTarget.latitude, currentTarget.longitude);

        if (debugText != null)
        {
            debugText.text = $"Go find: {currentTarget.name}\n" +
                             $"Lat: {currentLatitude:F6}\n" +
                             $"Lon: {currentLongitude:F6}\n" +
                             $"Dist to Target: {distanceToTarget:F1} meters";
        }

        if (distanceToTarget <= triggerDistance && !currentTarget.isFound)
        {
            Debug.Log($"Found the landmark: {currentTarget.name}!");
            currentTarget.isFound = true;
            TriggerLandmarkEvent(currentLandmarkIndex);
            currentLandmarkIndex++;
        }
    }

    void TriggerLandmarkEvent(int landmarkIndex)
    {
        debugText.text += $"\n--- FOUND {landmarks[landmarkIndex].name}! ---";

        switch (landmarkIndex)
        {
            case 0:
                Debug.Log("Starting challenge for the first landmark!");
                // Example: ARChallengeManager.instance.StartChallengeOne();
                break;
            case 1:
                Debug.Log("Starting challenge for the second landmark!");
                // Example: ARChallengeManager.instance.StartChallengeTwo();
                break;
            case 2:
                Debug.Log("Starting challenge for the third landmark!");
                // Example: ARChallengeManager.instance.StartChallengeThree();
                break;
            default:
                Debug.Log("An unknown landmark was found.");
                break;
        }
    }

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