using Godot;
using Scripts.Game;



namespace Scripts.Enemy.Components
{
	public partial class PathFindingComponent : Component
	{
		[Export] public float RepathInterval = 0.5f;
		[Export] public float ArrivalDistance = 0.3f;

		public Vector3 Target { get; set; }

		private Vector3[] _path;
		private int _currentIndex;

		private double _repathTimer;

		private Node3D _owner;

		public void Initialize(Node3D owner)
		{
			_owner = owner;
		}

		public override void _PhysicsProcess(double delta)
		{
			_repathTimer -= delta;

			if (_repathTimer <= 0)
			{
				_repathTimer = RepathInterval;
				RecalculatePath();
			}
		}

		/// <summary>
		/// Core API used by Navigation / State Machine
		/// </summary>
		public Vector3 GetNextDirection()
		{
			if (_owner == null)
				return Vector3.Zero;

			if (_path == null || _path.Length == 0 || _currentIndex >= _path.Length)
				return (Target - _owner.GlobalPosition).NormalizedSafe();

			Vector3 currentWaypoint = _path[_currentIndex];

			if (_owner.GlobalPosition.DistanceTo(currentWaypoint) < ArrivalDistance)
			{
				_currentIndex++;
				if (_currentIndex >= _path.Length)
				{
					return Vector3.Zero;
				}
				currentWaypoint = _path[_currentIndex];
			}

			return (currentWaypoint - _owner.GlobalPosition).NormalizedSafe();
		}

		private void RecalculatePath()
		{
			if (_owner == null)
				return;

			// For now: direct fallback (no navmesh yet)
			// Later: replace this with NavigationServer3D or AStarGrid2D/3D
			_path = new Vector3[1] { Target };
			_currentIndex = 0;
		}
	}

	public static class Vector3Extensions
	{
		public static Vector3 NormalizedSafe(this Vector3 v)
		{
			return v.LengthSquared() < 0.0001f ? Vector3.Zero : v.Normalized();
		}
	}
}
