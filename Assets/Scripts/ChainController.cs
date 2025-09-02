using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChainController : MonoBehaviour
{
    private bool broken;
    private float opacity = 0.75f;
    
    private List<MeshRenderer> childRenderers;
    public Material transparentMaterial;
    private Material transparentMaterialCopy;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        childRenderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
        transparentMaterialCopy = new Material(transparentMaterial);
    }

    // Update is called once per frame
    void Update()
    {
        if (broken)
        {
            opacity -= Time.deltaTime * 0.6f;
                Color color = transparentMaterialCopy.color;
                color.a = opacity;
                transparentMaterialCopy.color = color;

            if (opacity <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Break()
    {
        broken = true;
        childRenderers.RemoveAll(x => !x.enabled);
        childRenderers.ForEach(x => x.material = transparentMaterialCopy);
    }

    public bool IsBroken()
    {
        return broken;
    }
}
