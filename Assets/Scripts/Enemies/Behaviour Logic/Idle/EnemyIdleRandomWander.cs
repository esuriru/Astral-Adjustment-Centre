// NOTE - Remove superfluous usings
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle-Random Wander", menuName = "Enemy Logic/Idle Logic/Random Wander")]
public class EnemyIdleRandomWander : EnemyIdleSOBase
{
    // NOTE - Useless SerializeField, also, just make these private.
    [SerializeField] public float randomMovementRange = 5f;
    [SerializeField] public float randomMovementSpeed = 1f;

    private Vector3 _targetPos;
    private Vector3 _direction;
    
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();

        //_targetPos = playerTransform.position;
        _targetPos = GetRandomPointInCircle();
    }

    // NOTE - Useless override
    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

        _direction = (_targetPos - enemy.transform.position).normalized;
        enemy.MoveEnemy(_direction * randomMovementSpeed);

        if ((enemy.transform.position - _targetPos).sqrMagnitude < 0.01f)
        {
            _targetPos = GetRandomPointInCircle();
            //enemy.MoveEnemy(Vector2.zero);
        }
    }

    // NOTE - Useless override
    public override void DoPhysicsLogic()
    {
        base.DoPhysicsLogic();
    }

    // NOTE - Useless override
    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    // NOTE - Useless override
    public override void Init(GameObject gameObject, Enemy enemy)
    {
        base.Init(gameObject, enemy);
    }

    // NOTE - Useless override
    public override void ResetValues()
    {
        base.ResetValues();
    }

    private Vector3 GetRandomPointInCircle()
    {
        // NOTE - Remove line break below
        
        // NOTE - Remove debug log 
        Debug.Log(enemy == null);
        return enemy.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * randomMovementRange;
    }
}
