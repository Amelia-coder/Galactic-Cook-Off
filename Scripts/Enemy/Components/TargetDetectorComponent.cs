using Godot;
using System;
using System.Collections.Generic;
using Scripts.Game;

namespace Scripts.Enemy.Components
{
	/// <summary>
	/// Responsible ONLY for detecting nearby entities.
	/// Does not decide target priority or navigation.
	/// </summary>
	public partial class TargetDetectorComponent : Component
	{
		private Area3D _detectionArea;

		private readonly HashSet<Node3D> _targets = new();

		public event Action<Node3D> TargetEntered;
		public event Action<Node3D> TargetExited;

		/// <summary>
		/// Current detected targets.
		/// </summary>
		public IReadOnlyCollection<Node3D> Targets => _targets;

		public void Initialize(Area3D detectionArea)
		{
			_detectionArea = detectionArea;

			_detectionArea.AreaEntered += OnAreaEntered;
			_detectionArea.AreaExited += OnAreaExited;
		}

		public bool HasTargets()
		{
			CleanupInvalidTargets();
			return _targets.Count > 0;
		}

		private void OnAreaEntered(Area3D area)
		{
			Node owner = area.GetOwner();

			if (owner is not Node3D node3D)
				return;

			// Replace with your actual faction/team logic later
			if (!owner.IsInGroup("PlayerTeam"))
				return;

			if (_targets.Add(node3D))
			{
				TargetEntered?.Invoke(node3D);

				GD.Print($"[TargetDetector] Entered: {node3D.Name}");
			}
		}

		private void OnAreaExited(Area3D area)
		{
			Node owner = area.GetOwner();

			if (owner is not Node3D node3D)
				return;

			if (_targets.Remove(node3D))
			{
				TargetExited?.Invoke(node3D);

				GD.Print($"[TargetDetector] Exited: {node3D.Name}");
			}
		}

		/// <summary>
		/// Removes freed or invalid targets.
		/// Useful in multiplayer and dynamic scenes.
		/// </summary>
		private void CleanupInvalidTargets()
		{
			_targets.RemoveWhere(target =>
				target == null || !GodotObject.IsInstanceValid(target));
		}

		public override void _ExitTree()
		{
			if (_detectionArea != null)
			{
				_detectionArea.AreaEntered -= OnAreaEntered;
				_detectionArea.AreaExited -= OnAreaExited;
			}

			base._ExitTree();
		}
	}
}
