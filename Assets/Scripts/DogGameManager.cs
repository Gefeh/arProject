using System.Collections;
using UnityEngine;

public class DogGameManager : MonoBehaviour
{
    public static DogGameManager Instance;
    public bool hasBone;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject bone;
    [SerializeField] private Transform dog;
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private float spawnRadius = 150f;
    private GameObject currentBone;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnBone()
    {
        if (currentBone != null) return;

        Vector3 randomPosition = dog.position + (Random.insideUnitSphere * spawnRadius);
        randomPosition.y = dog.position.y;
        currentBone = Instantiate(bone, randomPosition, Quaternion.identity, xrOrigin);

        Debug.Log("Bone spawned");
    }

    public void DespawnBone()
    {
        if (currentBone != null)
        {
            Destroy(currentBone);
            currentBone = null;
        }
    }

    public void PickUpBone()
    {
        hasBone = true;
        Debug.Log("Bone picked up");
        GameManager.Instance.GameUI.UpdateText(GameManager.Instance.GameUI.BigNumber, $"Bone Found 1/1\n" + $"Bone Given 0/1");
    }

    public void GiveBone()
    {
        hasBone = false;
        animator.SetInteger("AnimationID", 5);
        Debug.Log("Bone Given");
        GameManager.Instance.GameUI.UpdateText(GameManager.Instance.GameUI.BigNumber, $"Bone Found 1/1\n" + $"Bone Given 1/1");
        StartCoroutine(Win(2));
    }

    IEnumerator Win(int time)
    {
        yield return new WaitForSeconds(time);
        GameManager.Instance.ChallengeManager.WinChallenge(this.gameObject);
    }

}
