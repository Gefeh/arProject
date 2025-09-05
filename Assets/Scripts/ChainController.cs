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
    private AudioSource _audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _childRenderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
        _transparentMaterialCopy = new Material(transparentMaterial);
        _particles = GetComponent<ParticleSystem>();
        _audioSource = GetComponent<AudioSource>();
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
        _audioSource.Play();
        OnBroken();
    }

    public bool IsBroken()
    {
        return _broken;
    }
}