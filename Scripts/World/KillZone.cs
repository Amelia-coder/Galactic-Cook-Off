using Godot;
using System;
using Scripts.Game;

public partial class KillZone : Area3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnPlayerEntered(Node3D body)
	{
		if (body.IsInGroup("Player"))
		{
			GD.Print("Playered entered the box area!");
			((IEntity)body).GetComponent<GenericHealthComponent>().DealDamage(10000f);
			///applyEffect();
		}
	}
}
