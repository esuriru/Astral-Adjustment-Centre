// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle-Waypoint Patrol", menuName = "Enemy Logic/Idle Logic/Waypoint Patrol")]
// NOTE - Remove line break here
public class EnemyIdleWaypointPatrol : EnemyIdleSOBase
{   
    // NOTE - Redundant initialization
    [SerializeField]
    private List<GameObject> waypoints = new List<GameObject>();

    // NOTE - Useless SerializeField, just make private, only used in this script
    [SerializeField] public float movementSpeed = 2.0f;
    private int _targetIndex = 0;
    private Vector3 _targetPos;
    private Vector3 _direction;
    private float _pauseTimer = 2f;
    private float _timer = 0f;


    // NOTE - Wrong indentation
     public override void DoEnterLogic()
    {
        base.DoEnterLogic();

        waypoints = enemy.FindChildObjectsWithTag(enemy.gameObject.transform.parent.gameObject, "Waypoint");

        // NOTE - Remove this comment below
        //Debug.Log(enemy.gameObject.transform.parent.gameObject.name);
        _targetPos = waypoints[_targetIndex].transform.position;
        _direction = ( _targetPos - transform.position).normalized;
    }

    // NOTE - Useless override
    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    // NOTE - Useless override
    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

        // NOTE - Check note in Enemy.cs
        enemy.MoveEnemy(_direction * movementSpeed * playerTransform.localScale.x);

        Vector3 lookPos = (waypoints[_targetIndex].transform.position - transform.position).normalized;
        lookPos.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(lookPos);
        if (transform.rotation != lookRotation)
        {
            // NOTE - Hardcoded speed
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3);
        }

        // NOTE - Remove comment below, and avoid this hardcoded value
        //Debug.Log(Vector3.Distance(transform.position, _targetPos));
        if (Vector3.Distance(transform.position, _targetPos) <= 0.31f)
        {
            //add timer to pause at waypoint
            _timer += Time.deltaTime;

            if (_timer <= _pauseTimer)
            {
                enemy.MoveEnemy(Vector3.zero);

                int targetIndex = _targetIndex + 1;

                // NOTE - Useless assignment of targetIndex, you are not even
                // setting _targetIndex
                if (targetIndex >= waypoints.Count)
                {
                    targetIndex = 0;
                }
                // NOTE - Remove line break below
                
            }
            // NOTE - Remove line break below

            else
            {
                // NOTE - Ternary operator maybe?
                if (_targetIndex == waypoints.Count - 1)
                {
                    _targetIndex = 0;
                }
                else
                {
                    _targetIndex++;
                }

                _targetPos = waypoints[_targetIndex].transform.position;
                // NOTE - Remove space
                _direction = ( _targetPos - transform.position).normalized;

                _timer = 0f;
            }
        }
        // NOTE - Remove two line breaks below


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
