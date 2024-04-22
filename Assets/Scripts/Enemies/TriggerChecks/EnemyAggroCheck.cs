// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE - I get the point of this, I just feel like the enemy should internally
// store this and set the aggro status when this sends an event to it. Observer
// pattern, basically.
public class EnemyAggroCheck : MonoBehaviour
{
    private Enemy _enemy;

    private void Awake()
    {
        _enemy = transform.parent.gameObject.GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("PlayerCollider"))
        {
            _enemy.SetAggroStatus(true);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
         if (collision.CompareTag("PlayerCollider"))
        {
            _enemy.SetAggroStatus(false);
        }
    }
}
