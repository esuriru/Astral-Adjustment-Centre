using PlayFab.MultiplayerModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack- Boss Attack", menuName = "Enemy Logic/Attack State/Boss Attack")]

public class BossEnemyAttack : EnemyAttackSOBase
{
    private enum AttackType
    {
        NULL,
        QUAD,
        CIRCLE
    }

    [SerializeField] private AttackType type;
    [SerializeField] private string animName;
    [SerializeField] private GameObject visualEffect;

    private Animator animator;
    private BossEnemy self;

    public Vector3 _attackRange;
    public float _posOffset;
    public float _damage;
    public float _chargeUpTimer;
    private float _timer;

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();

        //any get components, other entry logic do here
        animator = enemy.gameObject.GetComponent<Animator>();
        self = transform.GetComponent<BossEnemy>();
        // play back swing anim
        animator.SetBool(animName, true);
        _timer = _chargeUpTimer;
        // set attack indicator
        self.indicator.gameObject.SetActive(true);
        switch (type)
        {
            case AttackType.QUAD:
                self.indicator.SetChargeParameters(
                    transform.position,
                    transform.forward * _posOffset,
                    transform.localEulerAngles.y,
                    _attackRange
                    );

                break;

            case AttackType.CIRCLE:
                self.indicator.SetChargeParameters(
                    transform.position,
                    _attackRange
                    );

                break;
        }
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        
        //exit logic here
        //if you need to reset any values, return anything to pool at end of state
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

        //do state logic here
        enemy.MoveEnemy(Vector3.zero);
        // charge timer
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        else if (_timer <= 0)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                // play follow through anim
                animator.SetBool(animName, false);
            }
            // wait for follow through anim to finish
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                // swap back to chase
                if (visualEffect != null)
                {
                    GameObject obj = Instantiate(visualEffect, transform.position, transform.rotation);
                    Destroy(obj, 1);
                }
                self.indicator.ActivateHit(_damage);
                enemy.stateMachine.ChangeState(enemy.chaseState);
                self.indicator.gameObject.SetActive(false);
            }
        }
        // update attack indicator
        self.indicator.DoCharge(_chargeUpTimer, _timer);
    }

    public override void DoPhysicsLogic()
    {
        base.DoPhysicsLogic();
    }

    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void Init(GameObject gameObject, Enemy enemy)
    {
        base.Init(gameObject, enemy);
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }
}

