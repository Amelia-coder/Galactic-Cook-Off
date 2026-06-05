using Godot;
using Scripts.Game;
using Scripts.Game.RecipeSystem.Ingredients;
using Scripts.Player.Abilities;
using Scripts.Player.Components;
using Scripts.Player.States;
using Scripts.World;
using System;
using System.Collections.Generic;

namespace Scripts.Player
{
	public partial class Player : CharacterBody3D, IEntity, IThrowable, IPlayerLifecycle
	{


		private MultiplayerSynchronizer _playerInput;

		private SpectatorCameraController _spectatorController;
		private bool _isDead = false;
		public event Action<int> PlayerDied;

		// Является ли этот игрок локальным (управляемым с этого компьютера)
		[Export] public bool IsLocalPlayer;


		private uint _originalCollisionLayer;
		private uint _originalCollisionMask;

		private Label _pickupHint;

		private Label _interactHint;

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
		private PickableDetectorComponent _detectionComponent;
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

			IsLocalPlayer = _playerId == Multiplayer.GetUniqueId();
			_originalCollisionLayer = CollisionLayer;
			_originalCollisionMask = CollisionMask;


			_pickupHint = GetNode<Label>("CanvasLayer/PickupHint");
			_pickupHint.Visible = false;
			GD.Print($"[Player] layer: {CollisionLayer}, mask: {CollisionMask}");

			InitAndRegisterComponents();

			_interactHint = GetNode<Label>("CanvasLayer/InteractHint");
			_interactHint.Visible = false;

			if (IsLocalPlayer)
			{
				_detectionComponent.ThrowableEntered += OnThrowableNearby;
				_detectionComponent.ThrowableExited += OnThrowableLeft;
				_playerInteractionComponent.InteractableAvailable += OnInteractableNearby;
				_playerInteractionComponent.InteractableCleared += OnInteractableLeft;
			}


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

			_spectatorController = new SpectatorCameraController();
			AddChild(_spectatorController);

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


			_detectionComponent = GetNode<PickableDetectorComponent>("ComponentRegistry/PickableDetectorComponent");
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
			if (_isDead)
			{
				// Forward input to spectator camera when dead
				_spectatorController.HandleInput(@event);
				return;
			}
			_cameraControllerComponent.HandleInput(@event);
		}

		public void Disable()
		{
			Rpc(MethodName.RpcDisablePlayer);
		}

		public void Respawn(Vector3 position)
		{
			Rpc(MethodName.RpcRespawn, position);
		}

		public void EnterSpectator()
		{
			Rpc(MethodName.RpcEnterSpectator);
		}


		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
		public void RpcDisablePlayer()
		{
			_isDead = true;
			_chargeBar.Visible = false;
			_pickupHint.Visible = false;
			Visible = false;
			CollisionLayer = 0;
			CollisionMask = 0;
			SetPhysicsProcess(false);

			_abilitySystem.SetPhysicsProcess(false);

			if (IsLocalPlayer)
			{
				_inputComponent.SetProcess(false);
				_inputComponent.SetPhysicsProcess(false);
			}

			GD.Print($"[Player {PlayerId}] Disabled (dead).");
		}

		/// <summary>
		/// Restores the player at a new position with full health.
		/// </summary>
		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
		public void RpcRespawn(Vector3 spawnPos)
		{
			GlobalPosition = spawnPos;
			Velocity = Vector3.Zero;

			_healthComponent.ResetHealth();
			_isDead = false;

			Visible = true;
			CollisionLayer = _originalCollisionLayer;
			CollisionMask = _originalCollisionMask;
			SetPhysicsProcess(true);

			_abilitySystem.SetPhysicsProcess(IsLocalPlayer);

			if (IsLocalPlayer)
			{
				_inputComponent.SetProcess(true);
				_inputComponent.SetPhysicsProcess(true);
				_cameraControllerComponent.SetProcess(true);
				_cameraControllerComponent.SetPhysicsProcess(true);

				// Make sure our own camera is current again
				_camera.TopLevel = false;
				_camera.MakeCurrent();
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}

			GD.Print($"[Player {PlayerId}] Respawned at {spawnPos}.");
		}

		/// <summary>
		/// Permanent death — disable body, enter spectator camera on local client.
		/// </summary>
		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
		public void RpcEnterSpectator()
		{
			// Reuse disable logic
			RpcDisablePlayer();

			_cameraControllerComponent.SetProcess(false);
			_cameraControllerComponent.SetPhysicsProcess(false);

			if (IsLocalPlayer)
			{
				// Detach camera from the player's spring arm
				_camera.TopLevel = true;
				var respawnManager = GetTree().Root.FindChild("RespawnManager", true, false) as RespawnManager;
				_spectatorController.Activate(_camera, respawnManager);
				GD.Print($"[Player {PlayerId}] Entered spectator mode.");
			}
		}


		private void OnDied()
		{
			if (!Multiplayer.IsServer()) return;
			PlayerDied?.Invoke(PlayerId);  // ← no Arena.Instance anywhere
		}

		public override void _PhysicsProcess(double delta)
		{
			if (!IsLocalPlayer || _isHeld) return;
			//_animationComponent.Update();
		}

		public override void _Process(double delta)
		{
			if (!IsLocalPlayer || _isHeld) return;
		}

		private void OnThrowableNearby(IThrowable throwable)
		{
			if (!IsLocalPlayer || _isDead) return;

			// Show hint with context-appropriate text
			if (throwable is IIngredient ingredient)
				_pickupHint.Text = $"Press E to pick up {ingredient.getIngredientIdentData?.Id ?? "item"}";
			else
				_pickupHint.Text = "Press E to pick up";

			_pickupHint.Visible = true;
		}

		private void OnThrowableLeft(IThrowable throwable)
		{
			if (!IsLocalPlayer) return;

			// Only hide if nothing else is in range
			if (!_detectionComponent.HasNearbyThrowables())
				_pickupHint.Visible = false;
		}

		private void OnInteractableNearby(IInteractable interactable)
		{
			if (_isDead) return;
			_interactHint.Text = "Press E to interact";
			_interactHint.Visible = true;

			// Hide the pickup hint so they don't overlap
			_pickupHint.Visible = false;
		}

		private void OnInteractableLeft()
		{
			_interactHint.Visible = false;

			// Restore pickup hint if something throwable is still nearby
			if (_detectionComponent.HasNearbyThrowables())
				_pickupHint.Visible = true;
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


		// IThrowable — этого игрока можно подобрать
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
