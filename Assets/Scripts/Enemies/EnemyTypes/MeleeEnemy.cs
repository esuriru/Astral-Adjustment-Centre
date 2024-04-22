// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
{
    // NOTE - Remove this SerializeField attributes
    [SerializeField]
    public GameObject indicator;

    // NOTE - MeleeEnemy and RangedEnemy can derived off of an 'IndicatedEnemy'
    // that has these functions and the indicator field
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
