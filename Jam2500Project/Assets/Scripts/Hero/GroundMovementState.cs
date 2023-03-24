using UnityEngine;
using System.Collections;

public class GroundMovementState : HeroState
{
    private Rigidbody _rigidbody;
    private Animator _animator;

    [Header("Properties")]
    [Tooltip("Ground movement")]
    [SerializeField]
    private MovementProperties _movementProperties;

    [Tooltip("Jump animation FX")]
    [SerializeField]
    private JumpProperties _jumpProperties;


    [System.Serializable]
    public struct MovementProperties
    {
        public float _walkSpeed;
        public float _runSpeed;
        public float _angleVelocity;
        public float _smoothTime;
    }

    [System.Serializable]
    public struct JumpProperties
    {
        public float _height;
        public float _duration;
        public AnimationCurve _verticalAnimation;
    }

    [Header("Diagnostics: Do not modify")]
    [SerializeField]
    internal bool _grounded;
    public bool Freeze { get; set; }
    private Coroutine _jumpAnimation;
    
    public enum MovementState
    {
        Idle,
        Walk,
        Run
    }

    public override void Enter()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;

        _animator = GetComponent<Animator>();
        _animator.SetBool("onGround", true);

        var _cameraController = Camera.main.GetComponent<RootMotion.CameraController>();
        _cameraController.InfluenceOnCharacterRotation = false;
       // _cameraController.smoothFollow = false;

        Ready = true;
    }

    public override void Exit()
    {
        StopAllCoroutines();
        Ready = false;
        _animator.SetBool("onGround", false);
    }


    public override void UpdateState()
    {
        var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        MovementState state = MovementState.Idle;

        if (input.magnitude != 0)
            state = Input.GetKey(KeyCode.LeftShift) ? MovementState.Run : MovementState.Walk;

        Move(input, state);

        if (Input.GetKeyDown(KeyCode.Space))
            TryPlayJumpAnimation(_jumpProperties);
    }

    protected void Move(Vector2 input, MovementState state)
    {
        if (Freeze)
        {
            //SoundAudioManager.Instance
            // .StopSound(SoundAudioManager.AudioData.Kind.Movement);

            return;
        }

        

        var camera = Camera.main.transform;
        var _direction = camera.right * input.x +
            camera.forward * input.y;

        var plane = new Vector3(_direction.x, 0f, _direction.z).normalized;

        float speed = 0f;

        switch (state)
        {
            case MovementState.Idle:
                speed = 0;
                break;
            case MovementState.Walk:
                speed = _movementProperties._walkSpeed;
                break;
            case MovementState.Run:
                speed = _movementProperties._runSpeed;
                break;
        }

        _animator.SetInteger("speedStep", (int)state);
        _animator.SetBool("isMoving", ((int)state) != 0);


        var targetAngle = Mathf.Atan2(plane.x, plane.z) * Mathf.Rad2Deg;
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y,
            targetAngle, ref _movementProperties._angleVelocity, _movementProperties._smoothTime);

        var offset = plane * speed * Time.fixedDeltaTime;

        transform.position += offset;
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        //SoundAudioManager.Instance
        //    .PlaySound(SoundAudioManager.AudioData.Kind.Movement);
    }

    protected void TryPlayJumpAnimation(JumpProperties data)
    {
        if (_grounded == false)
            return;

        Interrupt();
        _grounded = false;
        _jumpAnimation = StartCoroutine(AnimateJump(data));
    }

    public void Interrupt()
    {
        if (_jumpAnimation != null)
            StopCoroutine(_jumpAnimation);
        //Landed?.Invoke();
    }

    private IEnumerator AnimateJump(JumpProperties data)
    {
        var expiredSeconds = 0f;
        var progress = 0f;
        var vertical = transform.position.y;

        while (progress < 1f)
        {
            expiredSeconds += Time.fixedDeltaTime;
            progress = (float)(expiredSeconds / data._duration);

            var posY = vertical + data._verticalAnimation.Evaluate(progress)
              * data._height;

            var position = new Vector3(transform.position.x,
                posY , transform.position.z);

            transform.position = (position);

            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
       // if (collision.gameObject.CompareTag("Ground") == false)
        //    return;

        Interrupt();
        _grounded = true;
       // Landed?.Invoke();
    }
}
