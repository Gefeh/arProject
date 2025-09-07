using Unity.VisualScripting;
using UnityEngine;

public class ChallengeManager : MonoBehaviour
{
    public void StartChallenge(GameObject challengeOrigin)
    {
        Debug.Log("Start");
        challengeOrigin.transform.position = Camera.main.transform.position;
        challengeOrigin.SetActive(true);
    }

    public void WinChallenge(GameObject challengeOrigin)
    {
        if (challengeOrigin != null)
        {
            challengeOrigin.SetActive(false);
        }
        GameManager.Instance.IncreaseChallengeCount();
        GameManager.Instance.GPSController.currentChallengeIndex++;
        GameManager.Instance.GPSController.ResumeLandmarkHunt();
    }
}
