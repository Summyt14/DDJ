using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace _Scripts.Player
{
    [RequireComponent(typeof(InputHandler))]
    public class PlayerMovement : MonoBehaviour
    {
        public enum MovementState
        {
            Air,
            Crouching,
            Sliding,
            Sprinting,
            Swinging,
            Walking
        }

        public MovementState State { get; private set; }
        public bool IsSwinging { get; set; }

        [Header("Movement")] [SerializeField] private float walkSpeed = 7f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float crouchSpeed = 15f;
        [SerializeField] private float swingSpeed = 4f;
        [SerializeField] private float jumpForce = 900f;
        [SerializeField] private float jumpCooldown = 0.25f;
        [SerializeField] private float gravityModifier = 4f;
        [SerializeField] private float groundMultiplier = 10f;
        [SerializeField] private float airMultiplier = 0.5f;
        [SerializeField] private float slopeMultiplier = 400f;
        [SerializeField] private float speedIncreaseMultiplier = 1.5f;
        [SerializeField] private float slopeIncreaseMultiplier = 2.5f;

        [Header("Sliding")] [SerializeField] private float maxSlideTime = 0.75f;
        [SerializeField] private float slideSpeed = 30f;
        [SerializeField] private float slideForce = 300f;

        [Header("Drag")] [SerializeField] private float groundDrag = 6f;
        [SerializeField] private float airDrag = 2f;

        [Header("Slope")] [SerializeField] private float maxSlopeAngle = 50f;

        [Header("Other stuff")] [SerializeField]
        private float playerHeight = 2f;

        [SerializeField] private float crouchYScale = 0.5f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private TextMeshProUGUI speedText;

        private Rigidbody _rb;
        private Vector3 _moveDir, _initialLocalScale, _gravityForce;
        private Transform _transform;
        private float _moveSpeed, _desiredMoveSpeed, _lastDesiredMoveSpeed, _slideTimer;
        private bool _isGrounded, _canJump = true, _exitingSlope, _isSliding;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;
            _transform = transform;
            _initialLocalScale = _transform.localScale;
        }

        private void Update()
        {
            _isGrounded = Physics.Raycast(_transform.position, Vector3.down,
                playerHeight * 0.5f + 0.2f, groundLayer);

            HandleInput();
            StateMachine();
            //SpeedControl();
            DragControl();
        }

        private void FixedUpdate()
        {
            MovePlayer();
            if (_isSliding) SlidingMovement();
        }

        private void LateUpdate()
        {
            if (speedText)
                speedText.text = "Speed: " + Math.Round(new Vector2(_rb.velocity.x, _rb.velocity.z).magnitude, 2);
        }

        private void HandleInput()
        {
            // handle jump
            if (InputHandler.instance.JumpBtn.WasPressedThisFrame() && _canJump && _isGrounded)
            {
                _canJump = false;
                _exitingSlope = true;
                _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
                _rb.AddForce(_transform.up * jumpForce, ForceMode.Impulse);
                Invoke(nameof(ResetJump), jumpCooldown);
            }

            // handle crouching
            if (InputHandler.instance.CrouchBtn.WasPressedThisFrame())
            {
                Vector3 localScale = _transform.localScale;
                localScale = new Vector3(localScale.x, crouchYScale, localScale.z);
                _transform.localScale = localScale;
                if (_isGrounded) _rb.AddForce(Vector3.down * 300f, ForceMode.Impulse);
                if (InputHandler.instance.MoveVector.magnitude != 0)
                {
                    _isSliding = true;
                    _slideTimer = maxSlideTime;
                }
            }

            if (InputHandler.instance.CrouchBtn.WasReleasedThisFrame())
            {
                _transform.localScale = _initialLocalScale;
                if (_isSliding) _isSliding = false;
            }
        }

        private void StateMachine()
        {
            switch (_isGrounded)
            {
                // Sprinting
                case true when InputHandler.instance.SprintBtn.IsPressed() &&
                               !InputHandler.instance.CrouchBtn.IsPressed():
                    State = MovementState.Sprinting;
                    _desiredMoveSpeed = sprintSpeed;
                    break;
                // Crouching
                case true when InputHandler.instance.CrouchBtn.IsPressed():
                    if (InputHandler.instance.MoveVector.magnitude != 0 && _isSliding)
                    {
                        State = MovementState.Sliding;
                        if (IsOnSlope(_moveDir, out _, out _) && _rb.velocity.y < 0.1f) _desiredMoveSpeed = slideSpeed;
                        else _desiredMoveSpeed = sprintSpeed;
                    }
                    else
                    {
                        State = MovementState.Crouching;
                        _desiredMoveSpeed = crouchSpeed;
                    }

                    break;
                // Walking
                case true:
                    State = MovementState.Walking;
                    _desiredMoveSpeed = walkSpeed;
                    break;
                case false when IsSwinging:
                    State = MovementState.Swinging;
                    _desiredMoveSpeed = swingSpeed;
                    break;
                // Air
                default:
                    State = MovementState.Air;
                    break;
            }

            // Check if desiredMoveSpeed has changed drastically
            if (Mathf.Abs(_desiredMoveSpeed - _lastDesiredMoveSpeed) > 8f && _moveSpeed != 0)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else _moveSpeed = _desiredMoveSpeed;

            _lastDesiredMoveSpeed = _desiredMoveSpeed;
        }

        private void SpeedControl()
        {
            // limit speed on slope
            if (IsOnSlope(_moveDir, out _, out _) && !_exitingSlope)
            {
                if (_rb.velocity.magnitude > _moveSpeed)
                    _rb.velocity = _rb.velocity.normalized * _moveSpeed;
            }
            // limit speed on ground or air
            else
            {
                Vector3 flatVel = new(_rb.velocity.x, 0f, _rb.velocity.z);
                if (flatVel.magnitude > _moveSpeed && !_exitingSlope)
                {
                    Vector3 limitedVel = flatVel.normalized * _moveSpeed;
                    _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
                }
            }
        }

        private void DragControl()
        {
            _rb.drag = _isGrounded ? groundDrag : airDrag;
        }

        private void ResetJump()
        {
            _canJump = true;
            _exitingSlope = false;
        }

        private IEnumerator SmoothlyLerpMoveSpeed()
        {
            float time = 0;
            float difference = Mathf.Abs(_desiredMoveSpeed - _moveSpeed);
            float startValue = _moveSpeed;

            while (time < difference)
            {
                _moveSpeed = Mathf.Lerp(startValue, _desiredMoveSpeed, time / difference);
                if (IsOnSlope(_moveDir, out _, out float slopeAngle))
                {
                    float slopeAngleIncrease = 1 + slopeAngle / 90f;
                    time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
                }
                else
                    time += Time.deltaTime * speedIncreaseMultiplier;

                yield return null;
            }

            _moveSpeed = _desiredMoveSpeed;
        }

        private void MovePlayer()
        {
            if (IsSwinging && !_isGrounded) return;

            // calculate move direction
            _moveDir = _transform.forward * InputHandler.instance.MoveVector.y +
                       _transform.right * InputHandler.instance.MoveVector.x;
            // on slope
            if (IsOnSlope(_moveDir, out Vector3 slopeMoveDir, out _) && !_exitingSlope)
            {
                _rb.AddForce(slopeMoveDir * (_moveSpeed * slopeMultiplier), ForceMode.Force);
                _rb.useGravity = false;
                if (_rb.velocity.y > 0)
                    _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
            // on ground
            else if (_isGrounded)
            {
                _rb.AddForce(_moveDir.normalized * (_moveSpeed * groundMultiplier), ForceMode.Acceleration);
                _rb.useGravity = true;
                _gravityForce = Vector3.zero;
            }
            // in air
            else if (!_isGrounded)
            {
                _rb.AddForce(_moveDir.normalized * (_moveSpeed * groundMultiplier * airMultiplier),
                    ForceMode.Acceleration);
                _rb.useGravity = true;
                _gravityForce += Physics.gravity * (gravityModifier * Time.deltaTime);
            }
            else if (IsSwinging) _gravityForce = Vector3.zero;

            if (!IsSwinging) _rb.AddForce(_gravityForce, ForceMode.Acceleration);
        }

        private void SlidingMovement()
        {
            Vector3 moveDir = _transform.forward * InputHandler.instance.MoveVector.y +
                              _transform.right * InputHandler.instance.MoveVector.x;

            // sliding normal
            if (!IsOnSlope(moveDir, out Vector3 slopeMoveDir, out _) || _rb.velocity.y > -0.1f)
            {
                _rb.AddForce(moveDir.normalized * slideForce, ForceMode.Force);
                _slideTimer -= Time.deltaTime;
            }
            // sliding down a slope
            else
                _rb.AddForce(slopeMoveDir * slideForce, ForceMode.Force);

            if (_slideTimer <= 0) _isSliding = false;
        }

        private bool IsOnSlope(Vector3 direction, out Vector3 slopeMoveDir, out float slopeAngle)
        {
            if (Physics.Raycast(_transform.position, Vector3.down, out RaycastHit slopeHit, playerHeight * 0.5f + 0.3f))
            {
                slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                slopeMoveDir = Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
                return slopeAngle != 0 && slopeAngle < maxSlopeAngle;
            }

            slopeMoveDir = Vector3.zero;
            slopeAngle = 0f;
            return false;
        }
    }
}