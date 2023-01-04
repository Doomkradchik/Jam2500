using UnityEngine;

[RequireComponent(typeof(GroundMovementState))]
public class HeroController : MonoBehaviour
{
    private StateMachine _heroStateMachine;

    private GroundMovementState _groundState;

    private void Awake()
    {
        _groundState = GetComponent<GroundMovementState>();
        _heroStateMachine = new StateMachine();
        _heroStateMachine.ChangeState(_groundState);
    }
    private void Update()
    {
        _heroStateMachine.Update();
    }
}
