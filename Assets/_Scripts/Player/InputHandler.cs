using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputHandler : MonoBehaviour
    {
        public const string GamepadScheme = "Gamepad";
        public const string PCScheme = "KeyboardMouse";

        public Vector2 MoveVector { get; private set; }
        public Vector2 LookVector { get; private set; }
        public InputAction LeftClickBtn { get; private set; } = new();
        public InputAction RightClickBtn { get; private set; } = new();
        public InputAction JumpBtn { get; private set; } = new();
        public InputAction SprintBtn { get; private set; } = new();
        public InputAction CrouchBtn { get; private set; } = new();
        public string CurrentControlScheme { get; private set; }

        public static InputHandler instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        private void OnEnable()
        {
            CurrentControlScheme = PCScheme;
        }

        public void OnMove(InputAction.CallbackContext context) => MoveVector = context.ReadValue<Vector2>();

        public void OnLook(InputAction.CallbackContext context) => LookVector = context.ReadValue<Vector2>();
        
        public void OnLeftClick(InputAction.CallbackContext context) => LeftClickBtn = context.action;
        
        public void OnRightClick(InputAction.CallbackContext context) => RightClickBtn = context.action;

        public void OnJump(InputAction.CallbackContext context) => JumpBtn = context.action;

        public void OnSprint(InputAction.CallbackContext context) => SprintBtn = context.action;

        public void OnCrouch(InputAction.CallbackContext context) => CrouchBtn = context.action;

        public void OnControlsChanged(PlayerInput input)
        {
            switch (input.currentControlScheme)
            {
                case PCScheme when CurrentControlScheme != PCScheme:
                    CurrentControlScheme = PCScheme;
                    break;
                case GamepadScheme when CurrentControlScheme != GamepadScheme:
                    CurrentControlScheme = GamepadScheme;
                    break;
            }
        }
    }
}