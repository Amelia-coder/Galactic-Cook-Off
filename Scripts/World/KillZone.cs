using Godot;
using System;

public partial class KillZone : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    private void OnBodyEnered(Node body)
    {
        if (body.IsInGroup("player"))
        {
            GD.Print("Playered entered the box area!");
            ///applyEffect();
        }
        //if body.has_method("die")
        //body.die()
        //if (body is Player)
        //{
        ////body.die();
        //GD.Print("Playered entered the box area!");
        //QueueFree();
        /////applyEffect();
        //}
    }
}
