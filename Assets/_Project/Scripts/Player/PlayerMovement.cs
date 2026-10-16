using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PsHome.Player
{
    /// <summary>
    /// 自分のアバターだけを動かす。スマホでは On-Screen Stick（Control Path: &lt;Gamepad&gt;/leftStick）、
    /// エディタでは WASD / 矢印キーで操作できる。移動方向はカメラ基準。
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(NetworkPlayer))]
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float walkSpeed = 2.0f;
        [SerializeField] private float runSpeed = 4.5f;
        [SerializeField] private float runThreshold = 0.7f;
        [SerializeField] private float turnSpeed = 720f;
        [SerializeField] private float gravity = -15f;

        private CharacterController _controller;
        private NetworkPlayer _player;
        private InputAction _move;
        private float _verticalVelocity;

        /// <summary>チャット入力中などに移動を止めたいときに false にする。</summary>
        public static bool InputEnabled { get; set; } = true;

        public override void Spawned()
        {
            _controller = GetComponent<CharacterController>();
            _player = GetComponent<NetworkPlayer>();

            if (!HasStateAuthority)
                return;

            _move = new InputAction("Move", InputActionType.Value);
            _move.AddBinding("<Gamepad>/leftStick");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            _move.Enable();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _move?.Dispose();
            _move = null;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || _move == null)
                return;

            Vector2 input = InputEnabled ? _move.ReadValue<Vector2>() : Vector2.zero;
            float magnitude = Mathf.Clamp01(input.magnitude);

            Vector3 direction = CameraRelative(input);
            float speed = magnitude >= runThreshold ? runSpeed : walkSpeed * (magnitude / runThreshold);
            if (magnitude < 0.05f)
                speed = 0f;

            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Runner.DeltaTime);
            }

            _verticalVelocity = _controller.isGrounded ? -1f : _verticalVelocity + gravity * Runner.DeltaTime;

            Vector3 velocity = direction * speed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Runner.DeltaTime);

            _player.Speed = speed;
        }

        private static Vector3 CameraRelative(Vector2 input)
        {
            var cam = Camera.main;
            if (cam == null || input.sqrMagnitude < 0.0001f)
                return new Vector3(input.x, 0f, input.y).normalized;

            Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
            return (forward * input.y + right * input.x).normalized;
        }
    }
}
