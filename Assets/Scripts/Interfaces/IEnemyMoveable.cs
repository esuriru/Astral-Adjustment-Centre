// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyMoveable
{
    // NOTE - Follow naming convention, prefer: Rigidbody
    Rigidbody rb { get; set; }
    
    // NOTE - Check note in Enemy.cs
    void MoveEnemy(Vector3 velocity);

}
