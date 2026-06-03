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

			_detectionArea.BodyEntered += OnBodyEntered;
			_detectionArea.BodyExited += OnBodyExited;
		}

		public bool HasTargets()
		{
			CleanupInvalidTargets();
			return _targets.Count > 0;
		}

		private void OnBodyEntered(Node3D body)
		{
			if (body == null)
				return;

			//Node3D entity = ResolveEntity(body);

			//if (entity == null)
			//	return;

			if (!body.IsInGroup("Player"))
				return;

			if (_targets.Add(body))
			{
				TargetEntered?.Invoke(body);
				//GD.Print($"[Detector] Entered: {body.Name}");
			}
		}

		private void OnBodyExited(Node3D body)
		{
			if (body == null)
				return;

			//Node3D entity = ResolveEntity(body);

			//if (entity == null)
			//	return;

			if (_targets.Remove(body))
			{
				TargetExited?.Invoke(body);
				//GD.Print($"[Detector] Exited: {body.Name}");
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
				_detectionArea.BodyEntered -= OnBodyEntered;
				_detectionArea.BodyExited -= OnBodyExited;
			}

			base._ExitTree();
		}
	}
}
