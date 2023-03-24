using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GroundMovementState), typeof(FlyState))]
public class HeroController : MonoBehaviour
{
    private StateMachine _heroStateMachine;

    private GroundMovementState _groundState;
    private FlyState _flyState;

    private const float CRITICAL_GROUND_DISTANCE = 1.9f;

    [Header("Diagnostics")]
    [Tooltip("Only in fly mode")]
    [SerializeField]
    internal float _toGroundDistance;

    private void Awake()
    {
        _groundState = GetComponent<GroundMovementState>();
        _flyState = GetComponent<FlyState>();
        _heroStateMachine = new StateMachine();
        _heroStateMachine.ChangeState(_groundState);
    }
    private void Update()
    {
        _heroStateMachine.Update();


        if(_heroStateMachine.currentState is FlyState fs)
            TryMakeTransitionToGround(fs);
        else 
            if(_heroStateMachine.currentState is GroundMovementState gs)
                if (Input.GetKeyDown(KeyCode.F))
                     _heroStateMachine.ChangeState(_flyState);
    }

    private void TryMakeTransitionToGround(FlyState state)
    {
        var ray = new Ray(transform.position, -Vector3.up);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f) == false)
            return;

        if (state.Ready == false)
            return;

        _toGroundDistance = (hit.point - ray.origin).magnitude;
        if (_toGroundDistance < CRITICAL_GROUND_DISTANCE)
            _heroStateMachine.ChangeState(_groundState);
    }
}
