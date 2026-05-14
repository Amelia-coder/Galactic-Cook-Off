using Godot;
using Scripts.Enemy.Components;
using Scripts.Enemy.States;
using Scripts.Enemy.Strategies;
using Scripts.Game;
using Scripts.Game.GenericComponents;
using Scripts.Player.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Enemy.Bosses.EvilRamsy
{
    public partial class EvilRamsy : CharacterBody3D
    {
        private AttackComponent _attackComponent;
        private GenericHealthComponent _healthComponent;
        




        public override void _Ready()
        {
            //_movementComponent = GetNode<GenericMovementComponent>("ComponentRegistry/MovementComponent");
            //_movementComponent.Initialize(this);
            //RegisterComponent(_movementComponent);
            //GD.Print("Movement component is null: ", _movementComponent == null);

            ////_cameraControllerComponent = GetNode<CameraControllerComponent>("ComponentRegistry/CameraControllerComponent");
            ////_cameraControllerComponent.Initialize(this, _camera, GetNode<Node3D>("CameraPivot"), GetNode<SpringArm3D>("CameraPivot/SpringArm3D"), true);
            ////RegisterComponent(_cameraControllerComponent);
            //_healthComponent = GetNode<GenericHealthComponent>("ComponentRegistry/HealthComponent");
            //_healthComponent.Died += OnDied;
            //RegisterComponent(_healthComponent);

            //var targetDetector = GetNode<Area3D>("DetectionArea");
            //_targetDetectorComponent = GetNode<TargetDetectorComponent>("ComponentRegistry/TargetDetectorComponent");
            //_targetDetectorComponent.Initialize(targetDetector);
            //RegisterComponent(_targetDetectorComponent);

            //_pathfindingComponent = GetNode<PathFindingComponent>("ComponentRegistry/NavigationComponent");
            //_pathfindingComponent.Initialize(this);
            //RegisterComponent(_pathfindingComponent);

            //_targetSelectorComponent = GetNode<TargetSelectorComponent>("ComponentRegistry/TargetSelectorComponent");
            //RegisterComponent(_targetSelectorComponent);


            //_attackComponent = GetNode<EnemyAttackComponent>("ComponentRegistry/AttackComponent");
            //_attackComponent.RegisterStrategy(new MeleeAttackStrategy());
            //RegisterComponent(_attackComponent);

            //_lootDropComponent = GetNode<LootDropComponent>("ComponentRegistry/LootDropComponent");
            //_lootDropComponent.Initilaize(this);
            //RegisterComponent(_lootDropComponent);
            //var fsm = GetNode<EnemyStateMachine>("StateMachine");
            //fsm.InitialState = GetNode<ChaseState>("StateMachine/ChaseState");
            //GD.Print($"Initial emy state is: {fsm.InitialState}, fsm is null: {fsm == null}");

        }
    }
}