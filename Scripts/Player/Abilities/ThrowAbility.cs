using Godot;

public partial class ThrowAbility : Ability
{
	// =========================================================
	// Exports — tweak in editor
	// =========================================================
	[Export] public float MinForce = 1f;
	[Export] public float MaxForce = 900f;
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
		// Lost the item mid-charge (dropped by something else, etc.)
		if (_isCharging && !(_context.HeldItem != null) || Input.IsActionPressed("cancel_charge"))
		{
			Reset(cancelled: true);
			return;
		}

		if (!(_context.HeldItem != null)) return;

		// Begin / continue charging
		if (Input.IsActionPressed("throw"))
		{
			if (!_isCharging)
			{
				_isCharging = true;
				EmitSignal(SignalName.ChargeStarted);
			}

			_chargeTime = Mathf.Min(_chargeTime + (float)delta, MaxChargeTime);
			EmitSignal(SignalName.ChargeUpdated, ChargeRatio);
		}

		// Release — actually throw
		if (Input.IsActionJustReleased("throw") && _isCharging)
		{
			float force = Mathf.Lerp(MinForce, MaxForce, ChargeRatio);
			Vector3 direction = _context.ForwardDir;

			_context.TryThrow(direction * force);
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
