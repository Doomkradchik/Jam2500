using UnityEngine;
using System.Collections;

public abstract class HeroState : MonoBehaviour
{
    public bool Ready { get; protected set; } = false;
    public abstract void Enter();
    public abstract void UpdateState();
    public abstract void Exit();
}

public class StateMachine
{
    internal HeroState currentState { get; private set; }

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
