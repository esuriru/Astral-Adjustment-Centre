// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gas : MonoBehaviour
{
    // NOTE - Useless SerializeFields, just make them private
    [SerializeField] public int damage;
    [SerializeField] public float damageFrequency = 1.5f;
    private PlayerCombat player;
    private float damageTimer = 0;

    // NOTE - Missing access specifier
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCombat>();
    }

    // NOTE - Missing access specifier
    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("PlayerCollider"))
        {
            damageTimer = 0;
        }
    }

    // NOTE - Missing access specifier
    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("PlayerCollider"))
        {
            damageTimer = 0;
        }
    }
    
    // NOTE - Missing access specifier
    void OnTriggerStay(Collider collider)
    {
        if (collider.CompareTag("PlayerCollider"))
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= damageFrequency)
            {
                player.Damage(damage);
                damageTimer = 0;
            }
        }
    }

    public void DeactivateGas()
    {
        gameObject.SetActive(false);
    }

    public void PlayGasSound()
    {
        AudioManager.Instance.PlaySFX("SFXGasRelease");
    }
}
