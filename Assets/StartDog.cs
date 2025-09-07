using UnityEngine;

public class StartDog : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.Instance.DogGameManager.SpawnBone();
        GameManager.Instance.GameUI.UpdateText(GameManager.Instance.GameUI.BottomText, "Find the bone and give it to the dog by tapping the bone, and doubble tapping the dog.");
        GameManager.Instance.GameUI.UpdateText(GameManager.Instance.GameUI.BigNumber, $"Bone Found 0/1\n" + $"Bone Given 0/1");
    }
}
