using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEngine;

public class ChainArea : MonoBehaviour
{
    public GameObject chainPrefab;
    private readonly List<GameObject> _chains = new();
    private int chainsBroken;
    private Bounds area;
    

    private void OnEnable()
    {
        Debug.Log("Enable chains");
        var boxCollider = GetComponent<BoxCollider>();
        area = new Bounds(boxCollider.center, boxCollider.size);
        GameManager.Instance.GameUI.UpdateText(GameManager.Instance.GameUI.BottomText, "Swipe the screen to break the chains.");
        CreateChains();
        GameManager.Instance.GameUI.UpdateText(GameManager.Instance.GameUI.BigNumber, $"Chains Broken {chainsBroken}/{_chains.Count}");
    }

    private void OnChainBroken()
    {
        chainsBroken++;
        GameManager.Instance.GameUI.UpdateText(GameManager.Instance.GameUI.BigNumber, $"Chains Broken {chainsBroken}/{_chains.Count}");
        if (chainsBroken == _chains.Count)
        {
            Debug.Log("Broke everything!!!");
            GameManager.Instance.ChallengeManager.WinChallenge(this.gameObject);
        }
        GameManager.Instance.GameUI.UpdateText(GameManager.Instance.GameUI.BigNumber, "Swipe the screen to break the chains.");
    }

    private static List<Vector2> CalculateChainPositions(float availableWidth, float availableHeight)
    {
        List<Vector2> positions = new();
        int maxAttempts = 20;
        float minDistance = 1.5f;
        while (true)
        {
            int attempts;
            for (attempts = 0; attempts < maxAttempts; attempts++)
            {
                Vector2 position = new Vector2(
                    Random.Range(-availableWidth/2f, availableWidth/2f),
                    Random.Range(-availableHeight/2f, availableHeight/2f)
                    );
                bool valid = true;
                foreach (Vector2 vector2 in positions)
                {
                    if (Vector2.Distance(vector2, position) > minDistance) continue;
                    valid = false;
                }

                if (valid)
                {
                    positions.Add(position);
                    break;
                }
            }
            if (attempts >= maxAttempts) break;
        }
        return positions;
    }
    
    public void CreateChains()
    {
        //transform.GetChild(0).localScale = new Vector3(area.size.x, area.size.y, area.size.z);
        for (int i = 0; i < 6; i++)
        {
            if (i / 2 == 1) continue; // skip up and down
            float f = (area.extents[i / 2] + 0.1f) * (i % 2 == 0 ? 1f : -1f);
            Vector3 pos = Vector3.zero;
            pos[i / 2] += f;
            foreach (Vector2 position in CalculateChainPositions((i / 2 == 0 ? area.size.z : area.size.x) - 0.5f, area.size.y - 0.5f))
            {
                var parameters = new InstantiateParameters
                {
                    parent = transform,
                    worldSpace = false
                };
                GameObject instantiate = Instantiate(chainPrefab, pos, Quaternion.identity, parameters);
                instantiate.transform.LookAt(area.ClosestPoint(instantiate.transform.localPosition) + transform.position /*Hastily go back to world position*/); // Look at the center to have a cool rotation
                instantiate.transform.localPosition += (i / 2 == 0 ? Vector3.forward : Vector3.right) * position.x + Vector3.up * position.y;
                _chains.Add(instantiate);
                instantiate.GetComponent<ChainController>().OnBroken = OnChainBroken;
            }
        }
    }

    public Bounds GetArea()
    {
        return area;
    }
}
