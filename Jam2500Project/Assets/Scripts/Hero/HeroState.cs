using UnityEngine;
using System.Collections;

public abstract class HeroState : MonoBehaviour
{
    public abstract void Enter();
    public abstract void UpdateState();
    public abstract void Exit();
}

public class StateMachine
{
    HeroState currentState;

    public void ChangeState(HeroState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    public void Update()
    {
        if (currentState != null) currentState.UpdateState();
    }
}
