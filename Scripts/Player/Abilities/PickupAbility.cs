using Godot;
using System;

public partial class PickupAbility : Ability
{
	private IPlayerContext _context;

	public override bool IsActive() { return true;  }

	public void Initialize(IPlayerContext context)
	{
		_context = context;
	}

	public override void Update(double delta)
	{
		if (!Input.IsActionJustPressed("pickup")) return;

		if (_context.HeldItem != null)
			_context.TryDrop();
		else
			_context.TryPickUp(_context.ForwardDir);
	}

	//public void PhysicsProcess(double delta) { }
}
