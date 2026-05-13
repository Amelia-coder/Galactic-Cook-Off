using Godot;
using System;
using Scripts.Player.Components;
using Scripts.Game;

public partial class GlobalScript : Node3D
{
	[Export] public PackedScene PlayerScene;
	[Export] public PackedScene EnemyScene;
	[Export] public PackedScene ThrowableScene;
	//Note: spawn postiton of player is temporary and will be removed in multuplayer - or left for convinience
	//of testingд. Consider making is exportable
	private Vector3 SpawnPoint = new Vector3(0, 3.657f, 0);  
	[Export] public StaminaUIComponent StaminaUI;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var player = PlayerScene.Instantiate<Player>();
		AddChild(player);
		player.GlobalPosition = SpawnPoint;
		//if (player.IsMultiplayerAuthority())
		StaminaUI =  GetNode<StaminaUIComponent>("UIElements/Stamina");
		StaminaUI.Bind(player.GetComponent<StaminaComponent>());
		
		//TODO: replacer with correct initialization logic
		//var enemy = EnemyScene.Instantiate<MelleEnemy>();
		//enemy.SetTarget(player);
		//enemy.GlobalPosition = new Vector3(2, 5, -9);
		//AddChild(enemy);
		//
		//enemy = EnemyScene.Instantiate<MelleEnemy>();
		//enemy.GlobalPosition = new Vector3(4, 5, -7);
		//enemy.SetTarget(player);
		//AddChild(enemy);
		//
		//enemy = EnemyScene.Instantiate<MelleEnemy>();
		//enemy.SetTarget(player);
		//enemy.GlobalPosition = new Vector3(-4, 5, -8);
		//AddChild(enemy);
		
		
		//TODO: spawn enemies on sthe start OR on timer timeout. TImer can be seen in tutorial project!
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
