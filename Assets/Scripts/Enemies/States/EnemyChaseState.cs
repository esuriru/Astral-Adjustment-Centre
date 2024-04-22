// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyState
{
    // NOTE - Remove this line break below

    public EnemyChaseState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        // NOTE - Remove unused code
        //_playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }
    // NOTE - Remove this line break below

    // NOTE - Fix the inconsistent spacing    
    public override void AnimationTriggerEvent(Enemy.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);

        enemy.enemyChaseBaseInstance.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void EnterState()
    {
        base.EnterState();

        enemy.enemyChaseBaseInstance.DoEnterLogic();
    }

    public override void ExitState()
    {
        base.ExitState();
        enemy.enemyChaseBaseInstance.DoExitLogic();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        enemy.enemyChaseBaseInstance.DoFrameUpdateLogic();

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        enemy.enemyChaseBaseInstance.DoPhysicsLogic();
    }

}
