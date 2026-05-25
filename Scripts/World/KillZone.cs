using Godot;
using Scripts.Game;

namespace Scripts.World
{
	public partial class KillZone : Area3D
	{
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{

		}

		public override void _Process(double delta)
		{
		}

		private void OnPlayerEntered(Node3D body)
		{
			if (body.IsInGroup("Player") && body is IEntity entity)
			{
				var health = entity.GetComponent<GenericHealthComponent>();
				if (health != null)
					health.DealDamage(10000f);
			}
		}
	}
}
