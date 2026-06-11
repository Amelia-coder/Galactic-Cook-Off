using Godot;
using Scripts.Game;

namespace Scripts.Enemy
{
	public abstract class AttackStrategy
	{
		public float Range { get; protected set; }

		public float Cooldown { get; protected set; }

		protected double CooldownRemaining;

		//подумать в сторону замены на IEntity
		public virtual bool CanAttack(Node3D self, Node3D target)
		{
			GD.Print("CooldownRemaining is", CooldownRemaining);
			if (CooldownRemaining > 0)
				return false;

			float rangeSq = Range * Range;

			GD.Print("Is distance fine? ", self.GlobalPosition.DistanceSquaredTo(
				target.GlobalPosition) <= Range);
			GD.Print("Distance sueared is ", self.GlobalPosition.DistanceSquaredTo(target.GlobalPosition));
			GD.Print("Range is  ", self.GlobalPosition.DistanceSquaredTo(target.GlobalPosition));
			return self.GlobalPosition.DistanceSquaredTo(
				target.GlobalPosition) <= Range;
		}

		public virtual void Update(double delta)
		{
			if (CooldownRemaining > 0)
				CooldownRemaining -= delta;
		}

		public void Execute(Node3D self, Node3D target)
		{
			CooldownRemaining = Cooldown;

			OnExecute(self, target);
		}

		protected abstract void OnExecute(Node3D self, Node3D target);
	}
}
