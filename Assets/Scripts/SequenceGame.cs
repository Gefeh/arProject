using UnityEngine;
using TMPro;

public class SequenceGame : MonoBehaviour
{
    [Header("Assign your 4 objects in order")]
    public GameObject[] objects;

    private int currentIndex = 0;

    private void OnEnable()
    {
        Debug.Log("Enable seqeunce");
    }

    public void ObjectClicked(GameObject clickedObject)
    {
        // Always play the object's sound first
        AudioSource audio = clickedObject.GetComponent<AudioSource>();
        if (audio != null) audio.Play();

        // Now check correctness
        if (clickedObject == objects[currentIndex])
        {
            currentIndex++;
            Debug.Log("correct");

            // Check for win
            if (currentIndex >= objects.Length)
            {
                Debug.Log("🎉 You Win!");

                // If there’s audio, wait until it finishes
                if (audio != null)
                    StartCoroutine(ShowWinAfterDelay(audio.clip.length));
                else
                    ShowWinScreen();

                currentIndex = 0; 
            }
        }
        else
        {
            Debug.Log("❌ Wrong choice! Start over.");
            currentIndex = 0; 
        }
    }

    System.Collections.IEnumerator ShowWinAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowWinScreen();
    }

    void ShowWinScreen()
    {
        foreach (GameObject obj in objects)
            obj.SetActive(false);

        GameManager.Instance.ChallengeManager.WinChallenge(this.gameObject);
    }
}
