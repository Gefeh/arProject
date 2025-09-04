using System.Collections.Generic;
using UnityEngine;

public class ChainArea : MonoBehaviour
{
    public GameObject chainPrefab;
    private readonly List<GameObject> _chains = new();
    private int chainsBroken;
    private Bounds area;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var boxCollider = GetComponent<BoxCollider>();
        area = new Bounds(boxCollider.center, boxCollider.size);
        CreateChains();
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnChainBroken()
    {
        chainsBroken++;
        if (chainsBroken == _chains.Count)
        {
            Debug.Log("Broke everything!!!");
        }
    }
    
    public void CreateChains()
    {
        for (int i = 0; i < 6; i++)
        {
            float f = (area.size[i / 2] + 0.05f) * (i % 2 == 0 ? 1f : -1f);
            Vector3 pos = Vector3.zero;
            pos[i / 2] += f;
            var parameters = new InstantiateParameters();
            parameters.parent = transform;
            parameters.worldSpace = false;
            GameObject instantiate = Instantiate(chainPrefab, pos, Quaternion.identity, parameters);
            instantiate.transform.LookAt(area.ClosestPoint(instantiate.transform.localPosition) + transform.position /*Hastily go back to world position*/); // Look at the center to have a cool rotation
            _chains.Add(instantiate);
            instantiate.GetComponent<ChainController>().OnBroken = OnChainBroken;
        }
    }
}
