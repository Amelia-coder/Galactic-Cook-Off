using Godot;
using Scripts.Game;
using Scripts.Game.RecipeSystem.Recipes;
using Scripts.Player.Components;
using System;


public partial class GlobalScript : Node3D
{
	[Export] public PackedScene PlayerScene;
	[Export] public PackedScene EnemyScene;
	[Export] public PackedScene ThrowableScene;
	//Note: spawn postiton of player is temporary and will be removed in multuplayer - or left for convinience
	//of testingд. Consider making is exportable
	private Vector3 SpawnPoint = new Vector3(0, 3.657f, 0);  
	[Export] public StaminaUIComponent StaminaUI;
	
	private int _currentStage = 0;
	private int _dishesCooked = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var player = PlayerScene.Instantiate<Player>();
		AddChild(player);
		player.GlobalPosition = SpawnPoint;
		((IEntity)player).GetComponent<GenericHealthComponent>().Died += () => StaminaUI.Hide(); //dity; place this in player later
		//if (player.IsMultiplayerAuthority())
		StaminaUI =  GetNode<StaminaUIComponent>("UIElements/Stamina"); //link wit player's death signla
		StaminaUI.Bind(player.GetComponent<StaminaComponent>());

		var station = GetNode<CookStation>("CookStation");

		station.DishCooked += OnDishCooked;

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

	private void OnDishCooked(Recipe recipe)
	{
		_dishesCooked++;

		GD.Print(
			$"Dish cooked: {recipe.Id} " +
			$"({_dishesCooked} total)");

		switch (_dishesCooked)
		{
			case 1:
				{
					GD.Print("Wave 1 is starting!");


				//StartWave1();
				}
				break;
				
			case 3:
				//StartWave2();
				break;

			case 5:
				//StartBossPhase();
				break;
		}
	}
}
