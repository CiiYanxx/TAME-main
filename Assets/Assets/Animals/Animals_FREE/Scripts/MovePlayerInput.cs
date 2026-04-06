using UnityEngine;
using UnityEngine.InputSystem;

namespace ithappy.Animals_FREE
{
    [RequireComponent(typeof(CreatureMover))]
    public class MovePlayerInput : MonoBehaviour
    {
        [Header("Character")]
        public InputActionReference MoveAction;
        public InputActionReference JumpAction;
        public InputActionReference RunAction;

        [Header("Camera")]
        public PlayerCamera m_Camera;
        public InputActionReference LookAction;
        public InputActionReference ScrollAction;

        private CreatureMover m_Mover;

        private Vector2 m_Axis;
        private bool m_IsRun;
        private bool m_IsJump;
        private Vector3 m_Target;
        private Vector2 m_MouseDelta;
        private float m_Scroll;

        private void Awake()
        {
            m_Mover = GetComponent<CreatureMover>();
        }

        private void OnEnable()
        {
            MoveAction.action.Enable();
            JumpAction.action.Enable();
            RunAction.action.Enable();
            LookAction.action.Enable();
            ScrollAction.action.Enable();
        }

        private void OnDisable()
        {
            MoveAction.action.Disable();
            JumpAction.action.Disable();
            RunAction.action.Disable();
            LookAction.action.Disable();
            ScrollAction.action.Disable();
        }

        private void Update()
        {
            GatherInput();
            SetInput();
        }

        private void GatherInput()
        {
            m_Axis = MoveAction.action.ReadValue<Vector2>();
            m_IsJump = JumpAction.action.IsPressed();
            m_IsRun = RunAction.action.IsPressed();

            m_Target = (m_Camera == null) ? Vector3.zero : m_Camera.Target;

            m_MouseDelta = LookAction.action.ReadValue<Vector2>();
            m_Scroll = ScrollAction.action.ReadValue<float>();
        }

        private void SetInput()
        {
            if (m_Mover != null)
                m_Mover.SetInput(in m_Axis, in m_Target, in m_IsRun, m_IsJump);

            if (m_Camera != null)
                m_Camera.SetInput(in m_MouseDelta, m_Scroll);
        }
    }
}
