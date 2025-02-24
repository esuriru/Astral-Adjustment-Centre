using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelTrap : Trap
{
    [SerializeField] private GameObject effectObject;

    protected override void OnTriggerEnter(Collider collider)
    {
        // Do nothing for the barrel as it does not care about being hit by other colliders
    }

    public override IEnumerator TriggerTrap()
    {
        Debug.Log("Triggered Trap");
        yield return new WaitForSeconds(trapData.triggerDelay);

        foreach (var collider in Physics.OverlapSphere(transform.position, trapData.damageRange))
        {
            if (collider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.Damage(trapData.damage * GetAtkMultiplier());
            }
            else if (collider.TryGetComponent<BarrelTrap>(out var barrelTrap))
            {
                // If the barrel already exploded, return null
                if (!collider.gameObject.GetComponent<BarrelTrap>().isTriggered)
                {
                    collider.gameObject.GetComponent<BarrelTrap>().StartCoroutine(barrelTrap.TriggerTrap());
                }
            }
        }

        isTriggered = true;
        OnTrapTriggered();
        StartCoroutine(ObjectPoolManager.Instance.ReturnObjectToPool(this.gameObject));
    }

    protected override void OnTrapTriggered()
    {
        GameObject explosion = ObjectPoolManager.Instance.SpawnObject(effectObject, transform.position, Quaternion.identity);
        explosion.GetComponent<Transform>().localScale = new Vector3(0.3f, 0.3f, 0.3f);
        AudioManager.Instance.PlaySFX("SFXExplosion");
        ObjectPoolManager.Instance.StartCoroutine(ObjectPoolManager.Instance.ReturnObjectToPool(explosion, 1.9f));
    }
}
