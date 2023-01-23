using UnityEngine;

namespace _Scripts.Player
{
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private Transform pivot;
        [SerializeField] private float sensX;
        [SerializeField] private float sensY;
        [SerializeField] private float gamepadSensX;
        [SerializeField] private float gamepadSensY;

        private float _xRotation;
        private float _yRotation;
        private float _mouseX;
        private float _mouseY;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Start()
        {
            cameraHolder.parent = pivot;
            cameraHolder.localPosition = Vector3.zero;
            cameraHolder.localRotation = Quaternion.identity;
        }

        private void Update()
        {
            if (InputHandler.instance.CurrentControlScheme == InputHandler.PCScheme)
            {
                _mouseX = InputHandler.instance.LookVector.x * Time.deltaTime * sensX;
                _mouseY = InputHandler.instance.LookVector.y * Time.deltaTime * sensY;
            }
            else if (InputHandler.instance.CurrentControlScheme == InputHandler.GamepadScheme)
            {
                _mouseX = InputHandler.instance.LookVector.x * Time.deltaTime * gamepadSensX;
                _mouseY = InputHandler.instance.LookVector.y * Time.deltaTime * gamepadSensY;
            }

            _yRotation += _mouseX;
            _xRotation -= _mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        }

        private void FixedUpdate()
        {
            // cam.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
            // transform.rotation = Quaternion.Euler(0, _yRotation, 0);
            
            pivot.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * _mouseX);
            // Quaternion target = Quaternion.Euler(0, _mouseX, 0);
            // transform.rotation = Quaternion.RotateTowards(transform.rotation, target, 1f);
        }
    }
}