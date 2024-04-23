// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public void Damage(float damage);

    public void Despawn();

    // NOTE - Follow naming convention
    public GameObject gameObject
    {
        get;
    }
}
