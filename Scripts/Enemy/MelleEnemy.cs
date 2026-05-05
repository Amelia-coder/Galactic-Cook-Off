using Godot;
using System;
using System.Collections.Generic;

public partial class MelleEnemy : CharacterBody3D, IEnemyEntity
{
	private Player _target; // reconsider

	[Export] public float Speed = 1f;
	[Export] public float Gravity = 9.8f;
	[Export] public float StoppingDistance = 1.5f;
	[Export] public float MaxHealth = 100f;
	private Dictionary<Type, Component> _components = new();


	private float _currentHealth;

	public Node3D CurrentTarget; // TODD: wtore implnention

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

	public void RegisterComponent(Component component)
	{
		_components[component.GetType()] = component;
	}

	// --- IEntity ---
	public T GetComponent<T>() where T : Component
	{
		if (_components.TryGetValue(typeof(T), out Component component))
			return component as T;

		GD.PrintErr($"[Player] Component {typeof(T).Name} not found in dictionary!");
		GD.PrintErr($"[Player] Call stack: {System.Environment.StackTrace}"); // Shows who called this

		return null;
	}

    // --- IEnemyEntity ---
    public T GetTarget<T>() where T : Node3D
    {
        throw new NotImplementedException();
    }
}
