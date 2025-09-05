using Unity.VisualScripting;
using UnityEngine;

public class ChallengeManager : MonoBehaviour
{
    public void StartChallenge(GameObject challengeOrigin)
    {
        challengeOrigin.SetActive(true);
    }

    public void WinChallenge(GameObject challengeOrigin)
    {
        challengeOrigin.SetActive(false);
        GameManager.Instance.CurrentChallengeCount++;
    }
}
