using Godot;
using System;

public partial class MelleEnemy : CharacterBody3D
{
	private Player _target;

	[Export] public float Speed = 3f;

	public void SetTarget(Player player)
	{
		_target = player;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_target == null)
			return;

		Vector3 direction = (_target.GlobalPosition - GlobalPosition).Normalized();
		Velocity = direction * Speed;

		MoveAndSlide();
	}
}
