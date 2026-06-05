using Godot;
using System;
using System.Collections.Generic;
using Scripts.Game;

namespace Scripts.Player.Components
{
	public partial class PickableDetectorComponent : Component
	{
		private Area3D _detectionArea;
		private readonly List<IPickable> _itemsInRange = new();

		public event Action<IPickable> ThrowableEntered;
		public event Action<IPickable> ThrowableExited;

		public void Initialize(Area3D detectionArea)
		{
			_detectionArea = detectionArea;
			_detectionArea.AreaEntered += OnAreaEntered;
			_detectionArea.AreaExited += OnAreaExited;
		}

		public IEnumerable<IPickable> GetNearby() => _itemsInRange;

		public IPickable GetClosest(Vector3 fromPosition)
		{
			IPickable closest = null;
			float best = float.MaxValue;

			foreach (var item in _itemsInRange)
			{
				if (item is not Node3D node) continue;
				float d = fromPosition.DistanceSquaredTo(node.GlobalPosition);
				if (d < best)
				{
					best = d;
					closest = item;
				}
			}

			return closest;
		}

		public bool HasNearbyThrowables() => _itemsInRange.Count > 0;

		private void OnAreaEntered(Area3D area)
		{
			if (area.GetOwner() is not IPickable throwable) return;

			_itemsInRange.Add(throwable);
			ThrowableEntered?.Invoke(throwable);
			//GD.Print($"Throwable entered: {throwable}");
		}

		private void OnAreaExited(Area3D area)
		{
			if (area.GetOwner() is not IPickable throwable) return;

			_itemsInRange.Remove(throwable);
			ThrowableExited?.Invoke(throwable);
			//GD.Print($"Throwable exited: {throwable}");
		}

		/// <summary>
		/// Gets the best throwable in a direction (based on dot product)
		/// </summary>
		public IPickable GetBestInDirection(Vector3 fromPosition, Vector3 lookDirection, float minDot = 0.5f)
		{
            IPickable best = null;
			float bestDot = minDot;

			foreach (var item in _itemsInRange)
			{
				if (item is not Node3D node) continue;

				Vector3 toItem = (node.GlobalPosition - fromPosition).Normalized();
				float dot = lookDirection.Dot(toItem);

				if (dot > bestDot)
				{
					bestDot = dot;
					best = item;
				}
			}

			return best;
		}

		/// <summary>
		/// Gets the best throwable combining distance and direction
		/// </summary>
		public IPickable GetBestWeighted(Vector3 fromPosition, Vector3 lookDirection, float directionWeight = 0.7f)
		{
			IPickable best = null;
			float bestScore = float.MinValue;

			foreach (var item in _itemsInRange)
			{
				if (item is not Node3D node) continue;

				// Normalize distance to 0-1 range (closer = higher score)
				float dist = fromPosition.DistanceTo(node.GlobalPosition);
				float distScore = Mathf.Clamp(1.0f - (dist / 10.0f), 0, 1);

				// Direction score (0-1)
				Vector3 toItem = (node.GlobalPosition - fromPosition).Normalized();
				float dirScore = (lookDirection.Dot(toItem) + 1) / 2; // Map -1..1 to 0..1

				// Weighted combination
				float score = (dirScore * directionWeight) + (distScore * (1 - directionWeight));

				if (score > bestScore)
				{
					bestScore = score;
					best = item;
				}
			}

			return best;
		}
	}
}