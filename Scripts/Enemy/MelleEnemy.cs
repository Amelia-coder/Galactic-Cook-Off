using Godot;
using System;
using System.Collections.Generic;

using Scripts.Enemy;
using Scripts.Enemy.Components;
using Scripts.Enemy.States;
using Scripts.Game;
using Scripts.Player.Components;
using Scripts.Game.GenericComponents;

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
		//_attackComponent.Initialize(this);
		RegisterComponent(_attackComponent);

		var fsm = GetNode<EnemyStateMachine>("StateMachine");
		fsm.InitialState = GetNode<ChaseState>("StateMachine/ChaseState");
		GD.Print($"Initial emy state is: {fsm.InitialState}, fsm is null: {fsm == null}");

	}


	private void Die()
	{
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
