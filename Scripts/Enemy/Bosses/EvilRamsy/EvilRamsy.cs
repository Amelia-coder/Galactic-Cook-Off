using Godot;
using Scripts.Enemy.Components;
using Scripts.Enemy.States;
using Scripts.Enemy.Strategies;
using Scripts.Game;
using Scripts.Game.GenericComponents;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Scripts.Enemy.Bosses.EvilRamsy
{
	public partial class EvilRamsy : CharacterBody3D, IEntity
	{
		[Signal]
		public delegate void BossDefeatedEventHandler();
		
		private Dictionary<Type, Component> _components = new();
		private GenericMovementComponent _movementComponent;
		private GenericHealthComponent _healthComponent;
		private TargetDetectorComponent _targetDetectorComponent;
		private TargetSelectorComponent _targetSelectorComponent;
		private PathFindingComponent _pathfindingComponent;
		private EnemyAttackComponent _attackComponent;
		private LootDropComponent _lootDropComponent;
		
		private float _meleeAttackDamage = 2.5f;
		private float _meleeAttackRange = 1.0f;

		[Export]
		private PackedScene projectile;

		public override void _Ready()
		{
			_movementComponent = GetNode<GenericMovementComponent>("ComponentRegistry/MovementComponent");
			_movementComponent.Initialize(this);
			RegisterComponent(_movementComponent);

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

			_attackComponent = GetNode<EnemyAttackComponent>("ComponentRegistry/BossAttackComponent");
			// Register normal phase attack (rage attack added by BossRageState)
			//    projectile: ProjectileScene,
			//    minRange: 6f, maxRange: 20f,
			//    damage: 25f, cooldown: 4f));
			_attackComponent.RegisterStrategy(new MeleeAttackStrategy(_meleeAttackDamage, _meleeAttackRange));
			_attackComponent.RegisterStrategy(new AoEAttackStrategy(radius: 30f, damage: 20f, cooldown: 8f));
			//_attackComponent.RegisterStrategy(new RangedAttackStrategy(projectile, 8f, 1000f, cooldown: 8f));
			RegisterComponent(_attackComponent);

			_lootDropComponent = GetNode<LootDropComponent>("ComponentRegistry/LootDropComponent");
			var itemsContainer = GetTree().Root.GetNode<Node>("AppRoot/Level/Arena/Items");
			_lootDropComponent.Initilaize(this, itemsContainer);
			RegisterComponent(_lootDropComponent);

			if (!Multiplayer.IsServer())
			{
				SetPhysicsProcess(false);
				return;
			}

			//_attackComponent.RegisterStrategy(new RangedAttackStrategy(


			// Use boss states, not regular ChaseState
			var fsm = GetNode<EnemyStateMachine>("StateMachine");
			fsm.InitialState = GetNode<BossChaseState>("StateMachine/BossChaseState");
			fsm.ManualInitialize();
		}

		private void OnDied()
		{
			if (!Multiplayer.IsServer()) return;
			EmitSignal(SignalName.BossDefeated);
			_lootDropComponent.Drop();
			QueueFree();
		}

		public override void _PhysicsProcess(double delta)
		{

		}

		public void RegisterComponent(Component component)
		{
			_components[component.GetType()] = component;
		}

		public T GetComponent<T>() where T : Component
		{
			if (_components.TryGetValue(typeof(T), out Component component))
				return component as T;
			return null;
		}
	}
}
