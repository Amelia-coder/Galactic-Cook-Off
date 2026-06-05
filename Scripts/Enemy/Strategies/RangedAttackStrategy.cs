using Godot;
using Scripts.Game;

namespace Scripts.Enemy.Strategies
{
	public class RangedAttackStrategy : AttackStrategy
	{
		private readonly PackedScene _projectileScene;
		private readonly float _minRange;
		private readonly float _maxRange;

		public RangedAttackStrategy(PackedScene projectile, float minRange,
									 float maxRange, float cooldown)
		{
			_projectileScene = projectile;
			_minRange = minRange;
			_maxRange = maxRange;
			Cooldown = cooldown;
		}

		public override bool CanAttack(Node3D self, Node3D target)
		{
			//if (CooldownRemaining > 0) return false;
			float dist = self.GlobalPosition.DistanceTo(target.GlobalPosition);
			return dist >= _minRange && dist <= _maxRange;
		}

		protected override void OnExecute(Node3D self, Node3D target)
		{

			GD.Print("Executng ranged attack!");
			var proj = _projectileScene.Instantiate<Node3D>();
			
			self.GetTree().Root.AddChild(proj, true);
			proj.GlobalPosition = self.GlobalPosition + Vector3.Up * 1.5f;

			Vector3 dir = (target.GlobalPosition - self.GlobalPosition).Normalized();

			GD.Print("dir is ", dir);
			if (proj is IThrowable throwable)
			{
				GD.Print("we are in");
				throwable.Throw(dir * 20f);
			}
				
			else
				GD.PrintErr($"[RangedAttack] Projectile does not implement IThrowable");
		}
	}
}
