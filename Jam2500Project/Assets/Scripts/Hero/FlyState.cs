using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyState : HeroState
{
    ///<summary>
    /// 2 phase:
    /// a) Smoth fleght WASD => up, down , right , left
    /// b) Direct flight WASD => forward, right, left
    /// </summary>
    /// 
    [SerializeField]
    internal QuietFlightProperties _quietMode;

    [SerializeField]
    internal DirectedProperties _directedMode;

    [SerializeField]
    internal float _start_height = 7.5f;

    protected float _currentAngle = 0f;
    protected RootMotion.CameraController _cameraController;


    [System.Serializable]
    public struct QuietFlightProperties
    {
        public float _horizontalSpeed;
        public float _verticalSpeed;
    }

    [System.Serializable]
    public struct DirectedProperties
    {
        public float _speed;
        public float _slopeAngle;

        [Min(1f)]
        public float _transitionRatio;
    }
  


    protected FlyMode _mode;

    public enum FlyMode : int
    {
        Quiet = 0,
        Directed = 1
    }


    public override void Enter()
        => StartCoroutine(EnterRoutine());

    private IEnumerator EnterRoutine()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        var target = transform.position + Vector3.up * _start_height;
        _mode = FlyMode.Quiet;

        _cameraController = Camera.main.GetComponent<RootMotion.CameraController>();
        _cameraController.InfluenceOnCharacterRotation = true;

        float y = transform.eulerAngles.y;
        var startPos = transform.position;

        var expiredSeconds = 0f;
        var progress = 0f;

        while (progress < 1f)
        {
            expiredSeconds += Time.fixedDeltaTime;
            progress = (float)(expiredSeconds / 2f);

            var newY = Mathf.Lerp(y, Camera.main.transform.eulerAngles.y, progress);
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, newY, transform.eulerAngles.z);
            transform.position = Vector3.Lerp(startPos, target, progress);
            yield return null;
        }

        Ready = true;
    }

     

    public override void Exit() 
    {
        Ready = false;
        _cameraController.InfluenceOnCharacterRotation = false;
    }

    public override void UpdateState()
    {
        if (Ready == false)
            return;

        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _mode = (FlyMode)(Input.GetKey(KeyCode.LeftShift) && input.y > 0f ? 1 : 0);
        var angle = _mode == FlyMode.Directed ? _directedMode._slopeAngle : 0f; 
        switch (_mode)
        {
            case FlyMode.Quiet:
                DoQuietFlight(input);
                break;
            case FlyMode.Directed:
                DoDirectedFlight(input);
                break;
            default:
                throw new System.InvalidOperationException();
        }

        _currentAngle = Mathf.Lerp(_currentAngle, angle, Time.fixedDeltaTime * _directedMode._transitionRatio);
        transform.rotation = Quaternion
            .Euler(_currentAngle, transform.eulerAngles.y, transform.eulerAngles.z);
    }


    protected void DoQuietFlight(Vector2 input)
    {
        var vertical = input.y * Vector3.up * _quietMode._verticalSpeed;
        var horizontal = input.x * transform.right * _quietMode._horizontalSpeed;

        transform.position += (vertical + horizontal) * Time.fixedDeltaTime;
    }

    protected void DoDirectedFlight(Vector2 input)
    {
        input.y = input.y < 0f ? 0f : input.y;
        var vertical = input.y * Camera.main.transform.forward * _directedMode._speed;
        //var horizontal = input.x * transform.right * _quietMode._horizontalSpeed;

        transform.position += vertical * Time.fixedDeltaTime;
    }

}
