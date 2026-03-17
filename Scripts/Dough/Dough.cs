using Godot;
using System;

public partial class Dough : RigidBody3D, IThrowable
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
		if (body.HasMethod("TakeDamage")) /// сделать разделение: игрока мы оглушаем, моба - дамажим
		///один из вариантов - по тому, к какой группе принадлежит сущность. Это точно можно натстроить через editor графически
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


	public bool CanBePickedUpBy(IEntity actor)
	{
		return actor is Player;
	}

	public void PickUp(IEntity actor)
	{
		//// remove form scene, add another object
	}

	public void Throw(Vector3 impulse)
	{
		//player gives it
	}
}
