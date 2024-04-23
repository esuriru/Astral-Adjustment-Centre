// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FlickeringLight : MonoBehaviour
{
    [SerializeField] private VisualEffect _visualEffect;
    [SerializeField] private Light _light;

    // NOTE - These public variables can be made private
    public float multiplier;
    public float fadeSpeed;

    private bool started = false;

    // NOTE - Missing access specifier 
    void Awake()
    {
        _visualEffect = GetComponentInParent<VisualEffect>();
        _light = GetComponent<Light>();
    }

    // NOTE - Missing access specifier 
    void OnEnable()
    {
        started = false;
    }

    // NOTE - Missing access specifier 
    void Update()
    {
        _light.intensity = Random.Range(1, 3) * multiplier;
        multiplier -= fadeSpeed * Time.deltaTime;

        if (!started)
        {
            if (_visualEffect.aliveParticleCount > 0)
            {
                started = true;
            }
        }
        else if (_visualEffect.aliveParticleCount <= 0)
        {
            Destroy(gameObject.transform.parent.gameObject);
        }
    }
}
