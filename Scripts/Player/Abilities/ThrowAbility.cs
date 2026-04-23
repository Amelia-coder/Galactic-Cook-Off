using Godot;

public partial class ThrowAbility : Ability
{
	// =========================================================
	// Exports — tweak in editor
	// =========================================================
	[Export] public float MinForce = 1f;
	[Export] public float MaxForce = 90f;
	[Export] public float MaxChargeTime = 18.5f;

	// =========================================================
	// Signals — HUD subscribes to these
	// =========================================================
	[Signal] public delegate void ChargeStartedEventHandler();
	[Signal] public delegate void ChargeUpdatedEventHandler(float ratio);  // 0..1
	[Signal] public delegate void ChargeReleasedEventHandler();
	[Signal] public delegate void ChargeCancelledEventHandler();           // dropped without throwing

	// =========================================================
	// State
	// =========================================================
	private float _chargeTime;
	private bool _isCharging;

	public float ChargeRatio => _chargeTime / MaxChargeTime;
	public override bool IsActive() { return _isCharging; }
	public override bool BlocksOtherAbilities() { return _isCharging; }

	private IPlayerContext _context;
	// =========================================================
	// Setup — matches PickupAbility pattern
	// =========================================================
	public void Initialize(IPlayerContext context)
	{
		_context = context;
	}

	// =========================================================
	// Process
	// =========================================================
	public override void Update(double delta)
	{
		if (_isCharging && !(_context.HeldItem != null) || Input.IsActionPressed("cancel_charge"))
		{
			Reset(cancelled: true);
			return;
		}

		if (!(_context.HeldItem != null)) return;

		if (Input.IsActionPressed("throw"))
		{
			if (!_isCharging)
			{
				_isCharging = true;
				EmitSignal(SignalName.ChargeStarted);
			}

			_chargeTime = Mathf.Min(_chargeTime + (float)delta, MaxChargeTime);

			// Clamp charge to whatever stamina can actually afford
			// e.g. 60 stamina left → bar can only fill to 0.6
			float staminaRatio = _context.Stamina.CurrentStamina / 100f;
			float effectiveRatio = Mathf.Min(ChargeRatio, staminaRatio);

			EmitSignal(SignalName.ChargeUpdated, effectiveRatio);
		}

		if (Input.IsActionJustReleased("throw") && _isCharging)
		{
			// Use the same effective ratio for force, not raw ChargeRatio
			float staminaRatio = _context.Stamina.CurrentStamina / 100f;
			float effectiveRatio = Mathf.Min(ChargeRatio, staminaRatio);

			float force = Mathf.Lerp(MinForce, MaxForce, effectiveRatio);
			_context.TryThrow(_context.ForwardDir * force);
			_context.Stamina.TryConsume(effectiveRatio * 100f);
			Reset(cancelled: false);
		}
	}

	// =========================================================
	// Helpers
	// =========================================================
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
