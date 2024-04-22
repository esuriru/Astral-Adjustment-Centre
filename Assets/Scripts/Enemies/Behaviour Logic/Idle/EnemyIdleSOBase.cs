// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE - Refer to the note in Enemy.cs: To reiterate, this should be named
// EnemyIdleBehaviour... 
public class EnemyIdleSOBase : ScriptableObject
{
    protected Enemy enemy;
    protected Transform transform;
    protected GameObject gameObject;
    protected Transform playerTransform;

    public virtual void Init(GameObject gameObject, Enemy enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

    }

    public virtual void DoEnterLogic(){}
    public virtual void DoExitLogic()
    {
        ResetValues();
    }
    public virtual void DoFrameUpdateLogic()
    {
        // NOTE - Wrong indentation
         if (enemy.isAggroed)
        {
            enemy.stateMachine.ChangeState(enemy.chaseState);
        }
        // NOTE - Remove line break below 

    }
    public virtual void DoPhysicsLogic(){}
    public virtual void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType){}
    public virtual void ResetValues(){}
    
}
