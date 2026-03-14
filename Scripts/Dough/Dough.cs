using Godot;
using System;

public partial class Dough : RigidBody3D, IInteractable
{
	[Export] public float Damage = 20f;
	[Export] public float StunDuration = 1.5f;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetTree().CreateTimer(11115f).Timeout += QueueFree;
	}

	public void OnBodyEntered(Node body)
	{
		if (body.HasMethod("TakeDamage"))
			body.Call("TakeDamage", Damage);
		GD.Print("cumTaste? sugar!");
		// Приземлилось на пол — "прилипает"
		if (body is StaticBody3D)
		{
			Freeze = true; // отключаем физику
			// TODO: проиграть анимацию шлепка
		}

		//QueueFree();
	}

    public bool CanPickup(IEntity actor)
    {
		return true;
    }

    public void Pickup(IEntity actor)
    {
        return;
    }
}
