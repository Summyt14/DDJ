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
            float mouseX = 0, mouseY = 0;
            if (InputHandler.instance.CurrentControlScheme == InputHandler.PCScheme)
            {
                mouseX = InputHandler.instance.LookVector.x * Time.deltaTime * sensX;
                mouseY = InputHandler.instance.LookVector.y * Time.deltaTime * sensY;
            }
            else if (InputHandler.instance.CurrentControlScheme == InputHandler.GamepadScheme)
            {
                mouseX = InputHandler.instance.LookVector.x * Time.deltaTime * gamepadSensX;
                mouseY = InputHandler.instance.LookVector.y * Time.deltaTime * gamepadSensY;
            }

            _yRotation += mouseX;
            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
            
            pivot.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        private void FixedUpdate()
        {
            // cam.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
            // transform.rotation = Quaternion.Euler(0, _yRotation, 0);
        }
    }
}