// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine 
{
    // NOTE - In my opinion, don't shorten current
    public EnemyState currEnemyState { get; set; }

    public void Init(EnemyState startingState)
    {
        currEnemyState = startingState;
        currEnemyState.EnterState();
    }

    public void ChangeState(EnemyState nextState)
    {
        currEnemyState.ExitState();
        currEnemyState = nextState;
        currEnemyState.EnterState();

        // NOTE - Remove unused code 
        //Debug.Log(currEnemyState);
    }
}
