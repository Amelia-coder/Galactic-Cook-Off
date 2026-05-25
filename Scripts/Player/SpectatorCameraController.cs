using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Player.Components
{
    using Godot;
    using System.Collections.Generic;

    /// <summary>
    /// Smooth follow-cam that cycles through alive players.
    /// Created as a child of Player in code — no scene node required.
    /// Activated only on permanent death, only on the local client.
    /// 
    /// Required Input Map actions:
    ///   spectate_next  (e.g. E or Right Arrow)
    ///   spectate_prev  (e.g. Q or Left Arrow)
    /// </summary>
    public partial class SpectatorCameraController : Node
    {
        private Camera3D _camera;
        private List<Player> _targets = new();
        private int _currentIndex = 0;
        private bool _active = false;

        private Vector3 _offset = new(0, 5, 7);
        private float _smoothSpeed = 5f;

        // How often to refresh the target list (seconds)
        private float _refreshTimer = 0f;
        private const float RefreshInterval = 0.5f;

        public void Activate(Camera3D camera)
        {
            _camera = camera;
            _active = true;
            _currentIndex = 0;
            _camera.MakeCurrent();
            Input.MouseMode = Input.MouseModeEnum.Visible;
            RefreshTargets();
            GD.Print($"[Spectator] Activated. {_targets.Count} target(s).");
        }

        public void Deactivate()
        {
            _active = false;
        }

        public override void _Process(double delta)
        {
            if (!_active || _camera == null) return;

            // Periodic refresh so we pick up deaths/respawns
            _refreshTimer += (float)delta;
            if (_refreshTimer >= RefreshInterval)
            {
                _refreshTimer = 0f;
                RefreshTargets();
            }

            if (_targets.Count == 0) return;

            // Clamp index in case list shrank
            _currentIndex = Mathf.Clamp(_currentIndex, 0, _targets.Count - 1);
            Player target = _targets[_currentIndex];

            if (!IsInstanceValid(target))
            {
                RefreshTargets();
                return;
            }

            // Smooth follow
            Vector3 desiredPos = target.GlobalPosition + _offset;
            _camera.GlobalPosition = _camera.GlobalPosition.Lerp(
                desiredPos, _smoothSpeed * (float)delta);

            // Look at target's chest area
            _camera.LookAt(target.GlobalPosition + Vector3.Up * 1.5f);
        }

        /// <summary>
        /// Called from Player._UnhandledInput when dead.
        /// We don't override _UnhandledInput because the Player node
        /// already gates it by IsLocalPlayer and _isDead.
        /// </summary>
        public void HandleInput(InputEvent @event)
        {
            if (!_active || _targets.Count <= 1) return;

            if (@event.IsActionPressed("spectate_next"))
            {
                _currentIndex = (_currentIndex + 1) % _targets.Count;
                GD.Print($"[Spectator] Now watching player {_targets[_currentIndex].PlayerId}");
            }
            else if (@event.IsActionPressed("spectate_prev"))
            {
                _currentIndex = (_currentIndex - 1 + _targets.Count) % _targets.Count;
                GD.Print($"[Spectator] Now watching player {_targets[_currentIndex].PlayerId}");
            }
        }

        private void RefreshTargets()
        {
            if (Arena.Instance == null) return;
            _targets = Arena.Instance.GetAlivePlayers();
        }
    }
}
