using Godot;

public partial class MelleEnemy : CharacterBody3D
{
	private Player _target;

	[Export] public float Speed = 1f;
	[Export] public float Gravity = 9.8f;
	[Export] public float StoppingDistance = 1.5f;
	[Export] public float MaxHealth = 100f;  

	private float _currentHealth;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
	}

	// =========================================================
	// Called by Dough's OnImpact via body.Call("TakeDamage", ...)
	// =========================================================
	public void TakeDamage(float amount)
	{
		_currentHealth -= amount;
		GD.Print($"Enemy hit! HP: {_currentHealth}/{MaxHealth}");

		if (_currentHealth <= 0f)
			Die();
	}

	private void Die()
	{
		// Optional: spawn death effect, drop loot, play animation, etc.
		QueueFree();
	}

	// =========================================================
	// Movement (unchanged from before)
	// =========================================================
	public void SetTarget(Player player) => _target = player;

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
			velocity.Y -= Gravity * (float)delta;
		else
			velocity.Y = 0f;

		if (_target != null)
		{
			Vector3 toPlayer = _target.GlobalPosition - GlobalPosition;
			toPlayer.Y = 0f;

			if (toPlayer.Length() > StoppingDistance)
			{
				Vector3 direction = toPlayer.Normalized();
				velocity.X = direction.X * Speed;
				velocity.Z = direction.Z * Speed;
			}
			else
			{
				velocity.X = 0f;
				velocity.Z = 0f;
			}
		}
		else
		{
			velocity.X = 0f;
			velocity.Z = 0f;
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
