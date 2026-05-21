using Godot;
using Scripts.Enemy;
using Scripts.Enemy.Components;
using Scripts.Enemy.States;
using Scripts.Enemy.Strategies;
using Scripts.Game;
using Scripts.Game.GenericComponents;
using Scripts.Player;
using Scripts.Player.Components;
using Scripts.Player.States;
using System;
using System.Collections.Generic;
using Scripts.Player;

public partial class MelleEnemy : CharacterBody3D, IEntity
{
	private Player _target; // reconsider

	[Export] public float Speed = 1f;
	[Export] public float Gravity = 9.8f;
	[Export] public float StoppingDistance = 1.5f;
	private Dictionary<Type, Component> _components = new();
	
	public Node3D CurrentTarget; // TODD: wtore implnention
	private GenericMovementComponent _movementComponent;
	private EnemyStateMachine _enemyStateMachine;
	private GenericHealthComponent _healthComponent;
	private TargetDetectorComponent _targetDetectorComponent;
	private TargetSelectorComponent _targetSelectorComponent;
	private PathFindingComponent _pathfindingComponent;
	private EnemyAttackComponent _attackComponent;
	private LootDropComponent _lootDropComponent;

	public override void _Ready()
	{
		_movementComponent = GetNode<GenericMovementComponent>("ComponentRegistry/MovementComponent");
		_movementComponent.Initialize(this);
		RegisterComponent(_movementComponent);
		GD.Print("Movement component is null: ", _movementComponent == null);

		//_cameraControllerComponent = GetNode<CameraControllerComponent>("ComponentRegistry/CameraControllerComponent");
		//_cameraControllerComponent.Initialize(this, _camera, GetNode<Node3D>("CameraPivot"), GetNode<SpringArm3D>("CameraPivot/SpringArm3D"), true);
		//RegisterComponent(_cameraControllerComponent);
		_healthComponent = GetNode<GenericHealthComponent>("ComponentRegistry/HealthComponent");
		_healthComponent.Died += OnDied;
		RegisterComponent(_healthComponent);

		var targetDetector = GetNode<Area3D>("DetectionArea");
		_targetDetectorComponent = GetNode<TargetDetectorComponent>("ComponentRegistry/TargetDetectorComponent");
		_targetDetectorComponent.Initialize(targetDetector);
		RegisterComponent(_targetDetectorComponent);

		_pathfindingComponent = GetNode<PathFindingComponent>("ComponentRegistry/NavigationComponent");
		_pathfindingComponent.Initialize(this);
		RegisterComponent(_pathfindingComponent);
		
		_targetSelectorComponent = GetNode<TargetSelectorComponent>("ComponentRegistry/TargetSelectorComponent");
		RegisterComponent(_targetSelectorComponent);


		_attackComponent = GetNode<EnemyAttackComponent>("ComponentRegistry/AttackComponent");
		_attackComponent.RegisterStrategy(new MeleeAttackStrategy());
		RegisterComponent(_attackComponent);

		_lootDropComponent= GetNode<LootDropComponent>("ComponentRegistry/LootDropComponent");
		var itemsContainer = GetTree().Root.GetNode<Node>("AppRoot/Level/Arena/Items");
		_lootDropComponent.Initilaize(this, itemsContainer);
		RegisterComponent(_lootDropComponent);
		if (!Multiplayer.IsServer())
		{
			SetPhysicsProcess(false);
			// Disable the state machine on clients
			return; // skip FSM init on client
		}

		var fsm = GetNode<EnemyStateMachine>("StateMachine");
		fsm.InitialState = GetNode<ChaseState>("StateMachine/ChaseState");
		fsm.ManualInitialize();

	}

	private void OnDied()
	{
		Die();
	}

	private void Die()
	{
		if (!Multiplayer.IsServer()) return;
		_lootDropComponent.Drop();
		// Optional: spawn death effect, drop loot, play animation, etc.
		QueueFree();
	}

	//// =========================================================
	//// Movement (unchanged from before)
	//// =========================================================
	//public void SetTarget(Player player) => _target = player;

	public override void _PhysicsProcess(double delta)
	{
		_movementComponent.Update((float)delta);
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
