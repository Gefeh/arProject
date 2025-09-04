using System;
using System.Collections.Generic;
using UnityEngine;

public class ChainController : MonoBehaviour
{
    public Material transparentMaterial;
    private bool _broken;

    private List<MeshRenderer> _childRenderers;

    public Action OnBroken = () => { };
    private float _opacity = 0.75f;
    private ParticleSystem _particles;
    private Material _transparentMaterialCopy;
    
    private readonly int _smoothnessID = Shader.PropertyToID("_Smoothness");
    private readonly int _metallicID = Shader.PropertyToID("_Metallic");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _childRenderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
        _transparentMaterialCopy = new Material(transparentMaterial);
        _particles = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (_broken)
        {
            _opacity -= Time.deltaTime * 0.5f;
            var color = _transparentMaterialCopy.color;
            color.a = _opacity;
            _transparentMaterialCopy.color = color;
            
            // get rid of the weird reflections
            //transparentMaterial.SetFloat(_metallicID, _opacity / 2); 
            //transparentMaterial.SetFloat(_smoothnessID, _opacity / 2);

            if (_opacity <= 0) Destroy(gameObject);
        }
    }

    public void Break()
    {
        _broken = true;
        _childRenderers.ForEach(x =>
        {
            if (x.gameObject.activeInHierarchy)
            {
                x.material = _transparentMaterialCopy;
            }
            else
            {
                _particles.transform.position = x.gameObject.transform.position;
                _particles.Play();
            }
        });
        OnBroken();
    }

    public bool IsBroken()
    {
        return _broken;
    }
}