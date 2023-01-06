using System;
using UnityEngine;

namespace _Scripts.Player
{
    public class Swinging : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private LineRenderer lr;

        [SerializeField] private Transform gunTip;
        [SerializeField] private Transform cam;
        [SerializeField] private Transform player;
        [SerializeField] private LayerMask grappleMask;

        [Header("Swinging")] [SerializeField] private float maxSwingDistance = 25f;
        [SerializeField] private float jointSpring = 4.5f;
        [SerializeField] private float jointDamper = 7f;
        [SerializeField] private float jointMassScale = 4.5f;
        [SerializeField] private float jointMinDistance = 0.25f;
        [SerializeField] private float jointMaxDistance = 0.8f;
        [SerializeField] private float grappleTravelSpeed = 10f;
        [SerializeField] private float thresholdAngle;
        [SerializeField] private float swingAcceleration;

        private Vector3 _swingPoint, _currentGrapplePosition, _velocity;
        private SpringJoint _joint;
        private PlayerMovement _pm;
        private Rigidbody _rb;

        private void Awake()
        {
            _pm = player.gameObject.GetComponent<PlayerMovement>();
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (InputHandler.instance.LeftClickBtn.WasPressedThisFrame()) StartSwing();
            if (InputHandler.instance.LeftClickBtn.WasReleasedThisFrame()) StopSwing();

            if (_pm.IsSwinging)
            {
                float ropeAngle = Vector3.Angle(_joint.connectedAnchor - transform.position, Vector3.up);

                if (Mathf.Abs(ropeAngle) > thresholdAngle)
                {
                    _rb.AddForce(_velocity * (swingAcceleration * Mathf.Sign(ropeAngle)), ForceMode.Acceleration);
                }

                _velocity = _rb.velocity;
            }
        }

        private void StartSwing()
        {
            if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hitInfo, maxSwingDistance, grappleMask))
            {
                _pm.IsSwinging = true;
                _swingPoint = hitInfo.point;
                _joint = player.gameObject.AddComponent<SpringJoint>();
                _joint.autoConfigureConnectedAnchor = false;
                _joint.connectedAnchor = _swingPoint;

                float distanceFromPoint = Vector3.Distance(player.position, _swingPoint);

                // the distance grapple will try to keep from grapple point
                _joint.maxDistance = distanceFromPoint * jointMaxDistance;
                _joint.minDistance = distanceFromPoint * jointMinDistance;
                _joint.spring = jointSpring;
                _joint.damper = jointDamper;
                _joint.massScale = jointMassScale;

                lr.positionCount = 2;
                _currentGrapplePosition = gunTip.position;
            }
        }

        private void StopSwing()
        {
            lr.positionCount = 0;
            Destroy(_joint);
            _pm.IsSwinging = false;
        }

        private void LateUpdate()
        {
            if (!_joint) return;

            _currentGrapplePosition =
                Vector3.Lerp(_currentGrapplePosition, _swingPoint, Time.deltaTime * grappleTravelSpeed);

            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, _currentGrapplePosition);
        }
    }
}