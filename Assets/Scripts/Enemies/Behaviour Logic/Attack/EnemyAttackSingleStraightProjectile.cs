// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack- Single Straight Shot", menuName = "Enemy Logic/Attack State/Single Straight Shot")]
// NOTE - Remove this line break
public class EnemyAttackSingleStraightProjectile : EnemyAttackSOBase
{
    // NOTE - In my opinion, this should be below Serialized fields, also, make
    // this SerializeField private
    public GameObject BulletPrefab;
   
    [SerializeField] private float _shotCooldown = 3f;
    [SerializeField] private float _bulletSpeed = 1f;

    [SerializeField] private float _timeTillExit = 2f;
    [SerializeField] private float _distanceToCountExit = 4.5f;

    private RangedEnemy rangedEnemy;


    private float _exitTimer;
    private float _timer;
   
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        // NOTE - You can cache this
        rangedEnemy = enemy.gameObject.GetComponent<RangedEnemy>();
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        // NOTE - You can cache this
        transform.GetComponent<RangedEnemy>().HideIndicator();
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

        // NOTE - Check note in Enemy.cs
        enemy.MoveEnemy(Vector3.zero);

        Vector3 lookPos = (playerTransform.transform.position - transform.position).normalized;
        lookPos.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(lookPos);
        // NOTE - Hardcoded speed
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3);

        _timer += Time.deltaTime;

        if (_timer > _shotCooldown - 0.5f)
        {
            // NOTE - You can cache this, this will call multiple times a frame.
            // Make sure you only do this once.
            transform.GetComponent<RangedEnemy>().ShowIndicator();
        }
        if (_timer > _shotCooldown)
        {
            _timer = 0f;

            Vector3 dir = (playerTransform.position - rangedEnemy.firePoint.position).normalized;

            GameObject bullet = ObjectPoolManager.Instance.SpawnObject(BulletPrefab, rangedEnemy.firePoint.position, enemy.transform.localRotation);

            // Cache the GetComponent after you run it once, so you don't do it
            // again which costs a bit
            bullet.GetComponent<EnemyProjectileBasic>().ScaleProjectile(playerTransform.localScale);
            bullet.GetComponent<EnemyProjectileBasic>().MoveProjectile(dir * _bulletSpeed);

            // NOTE - You can also cache this.
            transform.GetComponent<RangedEnemy>().HideIndicator();
        }

        if (Vector3.Distance(playerTransform.position, enemy.transform.position) > (_distanceToCountExit * playerTransform.localScale.x))
        {
            _exitTimer += Time.deltaTime;

            if (_exitTimer >= _timeTillExit)
            {
                // NOTE - Check note in Enemy.cs
                enemy.stateMachine.ChangeState(enemy.chaseState);
            }
        }
        // NOTE - Remove this line break below

        else
        {
            _exitTimer = 0;
        }
        // NOTE - Remove this line break below

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
}
