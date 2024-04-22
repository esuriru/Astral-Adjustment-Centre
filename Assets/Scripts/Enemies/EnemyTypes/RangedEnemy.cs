// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    // NOTE - Remove these SerializeField attributes
    [SerializeField] public Transform firePoint;

    [SerializeField]
    public GameObject indicator;

    // NOTE - If this is used in a UnityEvent, just do SetIndicatorActive(bool
    // active) instead of two functions
    public void ShowIndicator()
    {
        indicator.SetActive(true);
    }

    public void HideIndicator()
    {
        indicator.SetActive(false);
    }
}
