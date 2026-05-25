using Godot;
using System;
using System.Collections.Generic;
using Scripts.Game;
using Scripts.Game.GenericComponents;
using Scripts.Player.Components;
using Scripts.Player.Abilities;
using Scripts.Player.States;

namespace Scripts.Player
{
	public partial class Player : CharacterBody3D, IEntity, IThrowable
	{


		private MultiplayerSynchronizer _playerInput;

		// Является ли этот игрок локальным (управляемым с этого компьютера)
		[Export] public bool IsLocalPlayer;

		private int _playerId = 1;
		[Export]
		public int PlayerId
		{
			get => _playerId;
			set => _playerId = value;
		}

		// --- IThrowable ---
		public event Action<bool> PickupAvailabilityChanged;

		// Состояние "в руках у другого игрока"
		private bool _isHeld = false;
		// Импульс, который добавляется при броске
		private Vector3 _throwVelocity = Vector3.Zero;

		// --- Подбор предметов ---
		private IThrowable _heldObject; // will be used for determination of where visually shall picked object be
		private readonly List<IThrowable> _itemsInRange = new();

		// --- Зарядка броска ---
		private ProgressBar _chargeBar;

		private Dictionary<Type, Component> _components = new();
		private GenericHealthComponent _healthComponent;
		private PlayerMovementComponent _movementComponent;
		private InputComponent _inputComponent;
		private ThrowableDetectorComponent _detectionComponent;
		private CameraComponent _cameraComponent;
		private CameraControllerComponent _cameraControllerComponent;
		private ItemHolderComponent _itemHolderComponent;
		private StaminaComponent _staminaComponent;
		private PlayerInteractionComponent _playerInteractionComponent;
		//private GenericAnimationComponent _animationComponent;

		private Camera3D _camera;
		private Area3D _bodyDetector;

		private MovementStateMachine _movementStateMachine;
		private AbilitySystem _abilitySystem;


		// =========================================================
		// Lifecycle
		// =========================================================


		public override void _Ready()
		{
			if (int.TryParse(Name, out int id))
				_playerId = id;

			IsLocalPlayer = (_playerId == Multiplayer.GetUniqueId());

			GD.Print($"[Player] layer: {CollisionLayer}, mask: {CollisionMask}");

			InitAndRegisterComponents();


			List<Ability> abilities = new List<Ability>();
			PickupAbility pickupAbility = GetNode<PickupAbility>("AbilitySystem/PickupAbility");
			pickupAbility.Initialize(this);
			abilities.Add(pickupAbility);


			ThrowAbility throwAbility = GetNode<ThrowAbility>("AbilitySystem/ThrowAbility");
			throwAbility.Initialize(this);
			throwAbility.ChargeStarted += () => _chargeBar.Visible = true;
			throwAbility.ChargeUpdated += ratio => _chargeBar.Value = ratio * 100f; //нужно соректировлоать, т к при дляительном заряде некорреткно ооюрадается шкала
			throwAbility.ChargeReleased += () => _chargeBar.Visible = false;
			throwAbility.ChargeCancelled += () => _chargeBar.Visible = false;

			abilities.Add(throwAbility);

			_abilitySystem = GetNode<AbilitySystem>("AbilitySystem");
			_abilitySystem.Initialize(abilities);
			if (!IsLocalPlayer)
			{
				_abilitySystem.SetPhysicsProcess(false);
			}

			_movementStateMachine = GetNode<MovementStateMachine>("MovementStateMachine"); // or whatever path
			_movementStateMachine.ManualInitialize();

			_chargeBar = GetNode<ProgressBar>("CanvasLayer/ChargeBar");
			_chargeBar.Visible = false;
			if (!IsLocalPlayer)
			{
				_inputComponent.SetProcess(false);
				_inputComponent.SetPhysicsProcess(false);
				_cameraControllerComponent.SetProcess(false);
				_cameraControllerComponent.SetPhysicsProcess(false);

			}
			
			var staminaUI = GetNode<StaminaUIComponent>("CanvasLayer/Stamina");
			if (IsLocalPlayer)
			{
				staminaUI.Bind(_staminaComponent);
				staminaUI.Visible = true;
			}

			else
			{
				staminaUI.Visible = false;
			}

			SetupCamera();
			GD.Print($"[Player] Name={Name}, PlayerId={PlayerId}, MyId={Multiplayer.GetUniqueId()}, IsLocal={IsLocalPlayer}");
		}
		public override void _EnterTree()
		{
			if (int.TryParse(Name, out int id))
			{
				_playerId = id;

				var input = GetNodeOrNull<MultiplayerSynchronizer>("PlayerInput");
				if (input != null)
					input.SetMultiplayerAuthority(id);

				var serverSync = GetNodeOrNull<MultiplayerSynchronizer>("ServerSync");
				if (serverSync != null)
					serverSync.SetMultiplayerAuthority(id);
			}
		}

		private void SetupCamera()
		{
			if (_camera == null)
				_camera = GetNode<Camera3D>("CameraPivot/SpringArm3D/Camera3D");

			_camera.Current = IsLocalPlayer;
		}

		private void InitAndRegisterComponents()
		{
			_staminaComponent = GetNode<StaminaComponent>("ComponentRegistry/StaminaComponent");
			GD.Print("Stamina component is null: ", _staminaComponent == null);
			RegisterComponent(_staminaComponent);


			_movementComponent = GetNode<PlayerMovementComponent>("ComponentRegistry/MovementComponent");
			_movementComponent.Initialize(this, _staminaComponent);
			RegisterComponent(_movementComponent);
			GD.Print("Movement component is null: ", _movementComponent == null);


			_inputComponent = GetNode<InputComponent>("ComponentRegistry/InputComponent");
			_inputComponent.Initialize(this);
			RegisterComponent(_inputComponent);
			GD.Print("Input component is null: ", _inputComponent == null);


			_detectionComponent = GetNode<ThrowableDetectorComponent>("ComponentRegistry/ThrowableDetectorComponent");
			// Get the Area3D child node and pass it to component
			_bodyDetector = GetNode<Area3D>("BodyDetector");
			_detectionComponent.Initialize(_bodyDetector);
			RegisterComponent(_detectionComponent);

			_healthComponent = GetNode<GenericHealthComponent>("ComponentRegistry/HealthComponent");
			_healthComponent.Died += OnDied;
			_healthComponent.Died += () => _chargeBar.Visible = false;

			RegisterComponent(_healthComponent);

			_itemHolderComponent = GetNode<ItemHolderComponent>("ComponentRegistry/ItemHolderComponent");
			RegisterComponent(_itemHolderComponent);

			_cameraComponent = GetNode<CameraComponent>("ComponentRegistry/CameraComponent");
			_camera = GetNode<Camera3D>("CameraPivot/SpringArm3D/Camera3D");
			_cameraComponent.Initialize(_camera);
			RegisterComponent(_cameraComponent);

			_cameraControllerComponent = GetNode<CameraControllerComponent>("ComponentRegistry/CameraControllerComponent");
			_cameraControllerComponent.Initialize(this, _camera, GetNode<Node3D>("CameraPivot"), GetNode<SpringArm3D>("CameraPivot/SpringArm3D"), IsLocalPlayer);
			RegisterComponent(_cameraControllerComponent);

			_playerInteractionComponent = GetNode<PlayerInteractionComponent>("ComponentRegistry/PlayerInteractionComponent");
			_playerInteractionComponent.Initialize(this, _itemHolderComponent);
			RegisterComponent(_playerInteractionComponent);

			//_animationComponent = GetNode<GenericAnimationComponent>("ComponentRegistry/AnimationComponent");
			//var animPlayer = GetNode<AnimationPlayer>("3DGodotRobot/AnimationPlayer");
			//GD.Print("Weill dianple animations");
			//foreach (var anim in animPlayer.GetAnimationList())
			//{
			//	GD.Print(anim);
			//}
			//_animationComponent.Init(
			//	animPlayer,
			//	new Dictionary<EntityAnimation, string>
			//	{
			//		[EntityAnimation.Idle] = "Idle",
			//		[EntityAnimation.Walk] = "Run",
			//		[EntityAnimation.Run] = "Sprint",
			//		[EntityAnimation.Jump] = "Jump",
			////		[EntityAnimation.Fall] = "fall",
			//		[EntityAnimation.Attack] = "Attack1",
			//		[EntityAnimation.Hurt] = "Hurt",
			////		[EntityAnimation.Death] = "death",
			//	}
			//);
			//RegisterComponent(_animationComponent);

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

		public override void _UnhandledInput(InputEvent @event)
		{
			if (!IsLocalPlayer) return;
			//GD.Print("[Player] _UnhandledInput fired"); // Add this
			_cameraControllerComponent.HandleInput(@event);
		}

		private void OnDied()
		{
			if (!Multiplayer.IsServer()) return;
			_chargeBar.Visible = false;
			//temporary; emit signals taht will increase counter and if it reaches limit, game finishes
			QueueFree();
		}

		public override void _PhysicsProcess(double delta)
		{
			if (!IsLocalPlayer || _isHeld) return;

			_inputComponent.Update();
			_cameraControllerComponent.Update((float)delta);
			_movementComponent.Update((float)delta);
			//_animationComponent.Update();
			_playerInteractionComponent.Update(_inputComponent);

		}

		public override void _Process(double delta)
		{
			if (!IsLocalPlayer || _isHeld) return;
		}



		// Подбор и бросок(самого игрока)
		private void ShowPickupLabel(bool visible) //должны быть сигналом в рамкх UI
		{
			// TODO: показать/скрыть UI-подсказку "Нажми <клавиша для подбора> для подбора"
		}


		public void UnregisterNearbyThrowable(IThrowable throwable)
		{
			_itemsInRange.Remove(throwable);
			GD.Print($"[Player] убран предмет, всего: {_itemsInRange.Count}");
		}


		// =========================================================
		// IThrowable — этого игрока можно подобрать
		// =========================================================
		private void OnPickupAreaBodyEntered(Node3D body)
		{
			if (body is not Player otherPlayer || otherPlayer == this) return;
			GD.Print($"[PickupArea] вошёл игрок: {body.Name}");
			//RegisterNearbyThrowable(otherPlayer);
		}

		private void OnPickupAreaBodyExited(Node3D body)
		{
			if (body is not Player otherPlayer || otherPlayer == this) return;
			UnregisterNearbyThrowable(otherPlayer);
		}

		public bool CanBePickedUpBy(IEntity actor) => actor is Player p && p != this;

		public void Drop()
		{
			//DetachFromCarrier();
		}
		
		public void Consume(){}

		public void PlayAnimation(string name) { } //уйдет в AnmationComponent
		public void Throw(Vector3 impulse) // TODO: rename, bacues ethis actually describes
										   //jow player is THROWN, not how they themslves throw
		{

		}

		public void PickUp(IEntity actor)
		{
			throw new NotImplementedException();
		}


	}
}
