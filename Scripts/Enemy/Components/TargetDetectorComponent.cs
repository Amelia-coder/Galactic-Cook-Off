using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TargetDetectorComponent : Component
{
	private Area3D _detectionArea;
	
	// We store Node3D because we need position for calculations. 
	// You can cast to specific interfaces (like IDamageable) when acting on them.
	private readonly List<Node3D> _targetsInRange = new();

	// Events for AI state machines (e.g., switch from Patrol to Chase)
	public event Action<Node3D> TargetEntered;
	public event Action<Node3D> TargetExited;

	/// <summary>
	/// Initialize the detector with the Area3D defined in the Enemy scene.
	/// Ensure the Area3D has a CollisionShape and appropriate Collision Layer/Mask.
	/// </summary>
	public void Initialize(Area3D detectionArea)
	{
		_detectionArea = detectionArea;
		
		// Connect signals
		_detectionArea.AreaEntered += OnAreaEntered;
		_detectionArea.AreaExited += OnAreaExited;
	}

	/// <summary>
	/// Returns all currently detected targets (Player, Companions, etc.)
	/// </summary>
	public IEnumerable<Node3D> GetTargets() => _targetsInRange.AsReadOnly();

	/// <summary>
	/// Checks if any target is currently detected.
	/// </summary>
	public bool HasTargets() => _targetsInRange.Count > 0;

	/// <summary>
	/// Gets the closest target to the enemy's current position.
	/// </summary>
	public Node3D GetClosest(Vector3 fromPosition)
	{
		Node3D closest = null;
		float bestDistSq = float.MaxValue;

		foreach (var target in _targetsInRange)
		{
			if (target == null) continue; // Safety check for freed nodes

			float distSq = fromPosition.DistanceSquaredTo(target.GlobalPosition);
			if (distSq < bestDistSq)
			{
				bestDistSq = distSq;
				closest = target;
			}
		}

		return closest;
	}

	/// <summary>
	/// Gets the target that is most directly in front of the enemy.
	/// Useful for ranged attacks or cone-of-vision checks.
	/// </summary>
	public Node3D GetBestInDirection(Vector3 fromPosition, Vector3 lookDirection, float minDot = 0.5f)
	{
		Node3D best = null;
		float bestDot = minDot;

		foreach (var target in _targetsInRange)
		{
			if (target == null) continue;

			Vector3 toTarget = (target.GlobalPosition - fromPosition).Normalized();
			float dot = lookDirection.Dot(toTarget);

			if (dot > bestDot)
			{
				bestDot = dot;
				best = target;
			}
		}

		return best;
	}

	/// <summary>
	/// Advanced selection: Combines distance and direction to pick the "most threatening" or "easiest to hit" target.
	/// </summary>
	public Node3D GetBestWeighted(Vector3 fromPosition, Vector3 lookDirection, float directionWeight = 0.7f)
	{
		Node3D best = null;
		float bestScore = float.MinValue;

		// Adjust this max distance based on your game scale (e.g., enemy view range)
		float maxViewDistance = 20.0f; 

		foreach (var target in _targetsInRange)
		{
			if (target == null) continue;

			Vector3 targetPos = target.GlobalPosition;
			
			// 1. Distance Score (Closer is better)
			float dist = fromPosition.DistanceTo(targetPos);
			if (dist > maxViewDistance) continue; // Ignore too far
			
			float distScore = 1.0f - (dist / maxViewDistance); // 1.0 at pos, 0.0 at max

			// 2. Direction Score (In front is better)
			Vector3 toTarget = (targetPos - fromPosition).Normalized();
			float dirScore = (lookDirection.Dot(toTarget) + 1) / 2.0f; // Map -1..1 to 0..1

			// 3. Weighted Combination
			float score = (dirScore * directionWeight) + (distScore * (1 - directionWeight));

			if (score > bestScore)
			{
				bestScore = score;
				best = target;
			}
		}

		return best;
	}

	private void OnAreaEntered(Area3D area)
	{
		// STRATEGY: How do we identify a "Player-related" entity?
		// Option A: Check Physics Layer (Fastest)
		// Option B: Check Group (Flexible)
		// Option C: Check Interface (Strict)
		
		// Example: Checking if the owner is in the "Player" group
		Node owner = area.GetOwner();
		
		if (owner == null) return;

		// Assuming you add your Player and Companions to a group called "PlayerTeam" or "HostileToEnemy"
		if (owner.IsInGroup("PlayerTeam")) 
		{
			if (owner is Node3D node3D)
			{
				if (!_targetsInRange.Contains(node3D))
				{
					_targetsInRange.Add(node3D);
					TargetEntered?.Invoke(node3D);
					GD.Print($"[Enemy] Target Detected: {node3D.Name}");
				}
			}
		}
	}

	private void OnAreaExited(Area3D area)
	{
		Node owner = area.GetOwner();
		if (owner == null) return;

		if (owner.IsInGroup("PlayerTeam"))
		{
			if (owner is Node3D node3D)
			{
				if (_targetsInRange.Remove(node3D))
				{
					TargetExited?.Invoke(node3D);
					GD.Print($"[Enemy] Target Lost: {node3D.Name}");
				}
			}
		}
	}
	
	public override void _ExitTree()
	{
		// Cleanup connections to prevent memory leaks
		if (_detectionArea != null)
		{
			_detectionArea.AreaEntered -= OnAreaEntered;
			_detectionArea.AreaExited -= OnAreaExited;
		}
		base._ExitTree();
	}
}
