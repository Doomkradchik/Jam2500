using UnityEngine;
using System.Collections;

public class GroundMovementState : HeroState
{
    private Rigidbody _rigidbody;

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
        public float _unitsPerSecond;
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
    
    private const float RAY_MAGNITUDE = 1.4f;

    public override void Enter()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void Exit()
    {
    }

    public override void UpdateState()
    {
        Move(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));

        if (Input.GetKeyDown(KeyCode.Space))
            TryPlayJumpAnimation(_jumpProperties);
    }

    protected void Move(Vector2 input)
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

        if (plane == Vector3.zero)
        {
            //animator.SetBool(_runMotionKey, false);

            //SoundAudioManager.Instance
            //  .StopSound(SoundAudioManager.AudioData.Kind.Movement);
            return;
        }

        //animator.SetBool(_runMotionKey, true);

        var targetAngle = Mathf.Atan2(plane.x, plane.z) * Mathf.Rad2Deg;
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y,
            targetAngle, ref _movementProperties._angleVelocity, _movementProperties._smoothTime);

        var offset = plane * _movementProperties._unitsPerSecond * Time.fixedDeltaTime;

        // _rigidbody.MovePosition(_rigidbody.position + offset);
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
