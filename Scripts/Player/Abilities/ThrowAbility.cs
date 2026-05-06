using Godot;
using Scripts.Player.Components;
using Scripts.Game;

namespace Scripts.Player.Abilities
{
	public partial class ThrowAbility : Ability
	{
		private InputComponent _input;
		private CameraComponent _camera;
		private StaminaComponent _stamina;
		private ItemHolderComponent _itemHolder;

		[Export] public float MinForce { get; set; } = 1f;
		[Export] public float MaxForce { get; set; } = 90f;
		[Export] public float MaxChargeTime { get; set; } = 2.0f;
		[Export] public float StaminaCostPercentage { get; set; } = 1.0f; // 100% charge = 100 stamina
		[Export] public float CancelCooldown { get; set; } = 0.3f; // Can't charge for 0.3s after cancel
		private float _cancelCooldownTimer = 0f;

		// Signals for HUD
		[Signal] public delegate void ChargeStartedEventHandler();
		[Signal] public delegate void ChargeUpdatedEventHandler(float ratio);
		[Signal] public delegate void ChargeReleasedEventHandler();
		[Signal] public delegate void ChargeCancelledEventHandler();

		private float _chargeTime;
		private bool _isCharging;

		public float ChargeRatio => _chargeTime / MaxChargeTime;
		public override bool IsActive() => _isCharging;
		public override bool BlocksOtherAbilities() => _isCharging;

		public void Initialize(IEntity entity)
		{
			_input = entity.GetComponent<InputComponent>();
			_camera = entity.GetComponent<CameraComponent>();
			_stamina = entity.GetComponent<StaminaComponent>();
			_itemHolder = entity.GetComponent<ItemHolderComponent>();
		}

		public override void Update(double delta)
		{
			_input.Update();

			if (_cancelCooldownTimer > 0)
			{
				_cancelCooldownTimer -= (float)delta;
			}
			// Cancel if lost item or cancel pressed
			if (_isCharging && (!_itemHolder.IsHoldingItem || Input.IsActionPressed("cancel_charge")))
			{
				_cancelCooldownTimer = CancelCooldown; // Start cooldown
				Reset(cancelled: true);
				return;
			}

			// Can't throw without item
			if (!_itemHolder.IsHoldingItem) return;

			// Start/continue charging
			if (_input.ThrowHeld && _cancelCooldownTimer <= 0)
			{
				if (!_isCharging)
				{
					_isCharging = true;
					EmitSignal(SignalName.ChargeStarted);
				}

				_chargeTime = Mathf.Min(_chargeTime + (float)delta, MaxChargeTime);

				// Clamp charge by available stamina
				float maxStaminaCost = StaminaCostPercentage * 100f;
				float staminaRatio = _stamina.CurrentStamina / maxStaminaCost;
				float effectiveRatio = Mathf.Min(ChargeRatio, staminaRatio);

				EmitSignal(SignalName.ChargeUpdated, effectiveRatio);
			}

			// Release throw
			if (_input.ThrowReleased && _isCharging)
			{
				// Calculate effective force based on stamina limit
				float maxStaminaCost = StaminaCostPercentage * 100f;
				float staminaRatio = _stamina.CurrentStamina / maxStaminaCost;
				float effectiveRatio = Mathf.Min(ChargeRatio, staminaRatio);

				float force = Mathf.Lerp(MinForce, MaxForce, effectiveRatio);
				float staminaCost = effectiveRatio * maxStaminaCost;

				// Get camera forward and add upward bias
				Vector3 cameraForward = _camera.GetForwardDirection();
				Vector3 horizontalDir = new Vector3(cameraForward.X, 0, cameraForward.Z).Normalized();

				// Mix: 70% horizontal, 30% upward (adjust to taste)
				Vector3 throwDirection = _camera.GetForwardDirection();
				Vector3 impulse = throwDirection * force;

				IThrowable item = _itemHolder.HeldItem;
				_itemHolder.ClearHeldItem();

				// Consume stamina before throwing
				_stamina.TryConsume(staminaCost);

				item.Throw(impulse);

				Reset(cancelled: false);
			}
		}

		private void Reset(bool cancelled)
		{
			_isCharging = false;
			_chargeTime = 0f;

			if (cancelled)
				EmitSignal(SignalName.ChargeCancelled);
			else
				EmitSignal(SignalName.ChargeReleased);
		}
	}
}
