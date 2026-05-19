using Godot;
using Scripts.Game;
using Scripts.Game.RecipeSystem.Recipes;
using Scripts.Player.Components;
using System;
using Scripts.Networking;

public partial class Arena : Node3D
{
	[Export] public PackedScene EnemyScene;
	[Export] public PackedScene ThrowableScene;
	//[Export] public PackedScene BossScene;
	//Note: spawn postiton of player is temporary and will be removed in multuplayer - or left for convinience
	//of testingд. Consider making is exportable
	private Vector3 SpawnPoint = new Vector3(0, 3.657f, 0);  
	[Export] public StaminaUIComponent StaminaUI;
	
	private MultiplayerSpawner _playerManager;
	
	private int _currentStage = 0;
	private int _dishesCooked = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		//var player = PlayerScene.Instantiate<Player>();
		//AddChild(player);
		//player.GlobalPosition = SpawnPoint;
		////((IEntity)player).GetComponent<GenericHealthComponent>().Died += () => StaminaUI.Hide(); //dity; place this in player later
		//if (player.IsMultiplayerAuthority())
		StaminaUI =  GetNode<StaminaUIComponent>("UIElements/Stamina"); //link wit player's death signla
		//StaminaUI.Bind(player.GetComponent<StaminaComponent>());

		var station = GetNode<CookStation>("CookStation");

		station.DishCooked += OnDishCooked;

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
				{
					SummonBoss();
				//StartBossPhase();
				}
				break;
		}
	}
	private void SummonBoss()
	{
		///var boss = BossScene.Instantiate<EvilRamsy>();
		///AddChild(boss);
		///boss.GlobalPostion = <some vector 3d>
	} 
}
