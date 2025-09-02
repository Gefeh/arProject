using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // <-- ADDED: We need this for the System.Math library

[System.Serializable]
public class Landmark
{
    public string name;
    public double latitude; // Changed to double
    public double longitude; // Changed to double
    public bool isFound = false;
}

public class GPSController : MonoBehaviour
{
    [Header("Landmark Settings")]
    public Landmark[] landmarks;
    public double triggerDistance = 15.0; // <-- CHANGED to double

    [Header("UI for Debugging")]
    public TextMeshProUGUI debugText;

    // --- Private variables ---
    private int currentLandmarkIndex = 0;
    private double distanceToTarget; // <-- CHANGED to double

    // Singleton instance
    public static GPSController instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
    }

    IEnumerator Start()
    {
        // GPS Initialization (this part remains the same)
        if (!Input.location.isEnabledByUser)
        {
            debugText.text = "Location services are not enabled.";
            yield break;
        }
        Input.location.Start();
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        if (maxWait <= 0 || Input.location.status == LocationServiceStatus.Failed)
        {
            debugText.text = "Unable to determine device location.";
            yield break;
        }

        // If GPS is ready, start updating
        InvokeRepeating(nameof(UpdateGpsData), 0.5f, 1f);
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

        // Unity's API gives us floats, but they are implicitly and safely converted to doubles here.
        double currentLatitude = Input.location.lastData.latitude; // <-- CHANGED to double
        double currentLongitude = Input.location.lastData.longitude; // <-- CHANGED to double

        distanceToTarget = CalculateDistance(currentLatitude, currentLongitude, currentTarget.latitude, currentTarget.longitude);

        if (debugText != null)
        {
            // Use F6 for more precision in the debug output if you want
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

        // Use a switch statement to handle different challenges
        // This is where an enum can make the code even cleaner!
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

    // --- Haversine formula using System.Math for double precision ---
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2) // <-- CHANGED
    {
        var R = 6371e3; // metres
        var φ1 = lat1 * Math.PI / 180.0; // <-- CHANGED
        var φ2 = lat2 * Math.PI / 180.0; // <-- CHANGED
        var Δφ = (lat2 - lat1) * Math.PI / 180.0; // <-- CHANGED
        var Δλ = (lon2 - lon1) * Math.PI / 180.0; // <-- CHANGED

        var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                Math.Cos(φ1) * Math.Cos(φ2) *
                Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2); // <-- CHANGED (Math instead of Mathf)
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)); // <-- CHANGED (Math instead of Mathf)

        var d = R * c;
        return d; // <-- CHANGED (returns double)
    }
}