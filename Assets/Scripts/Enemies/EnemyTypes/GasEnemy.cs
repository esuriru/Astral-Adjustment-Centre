// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasEnemy : Enemy
{
    // NOTE - Remove these SerializeField attributes
    [SerializeField] public Animator gasAnimator;
    [SerializeField] public GameObject gasObject;
}
