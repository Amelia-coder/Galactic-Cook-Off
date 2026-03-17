// JumpState.cs
using Godot;

public partial class JumpState : MovementState
{
	[Export] public float JumpVelocity { get; set; } = 5.0f;
	[Export] public float AirControl { get; set; } = 0.3f;
	[Export] public float AirAcceleration { get; set; } = 15f;
	[Export] public float VariableJumpCut { get; set; } = 0.5f;
	public override float StaminaConsumptionPerSecond => 10f;

	private int _enterFrames;

	private bool _justEntered;

	//public override void Enter()
	//{
	//	Entity.Velocity = Vector3.Up * 5.0f;
	//	Entity.Stamina.TryConsume(StaminaConsumptionPerSecond);
	//}

	public override void Enter()
	{
		_enterFrames = 2;
		//_justEntered = true;
		// Сохраняем горизонтальную скорость, меняем только Y
		Entity.Velocity = new Vector3(
			Entity.Velocity.X,
			JumpVelocity,
			Entity.Velocity.Z
		);
		Entity.Stamina.TryConsume(StaminaConsumptionPerSecond);
		GD.Print($"[JumpState.Enter] Velocity.Y установлен в {JumpVelocity}");
	}

	public override void Exit() { }

	public override void PhysicsUpdate(double delta)
	{
		if (_enterFrames > 0)
		{
			_enterFrames--;
			return; // ← выходим, не читаем инпут вообще
		}
		
		// Двойной прыжок — мы уже в воздухе, просто повторяем Enter()
		if (Input.IsActionJustPressed("jump") && Entity.TryJump())
		{
			Enter();
		}

		Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
		Vector3 moveDirection = Entity.GetMovementDirection(inputDir);

		Entity.Velocity = new Vector3(
			moveDirection.X * JumpVelocity,
			Entity.Velocity.Y,
			moveDirection.Z * JumpVelocity
		);

		// ground transition
		if (Entity.IsTouchingFloor)
		{
			if (inputDir.Length() > 0.1f)
				TransitionTo("WalkState");
			else
				TransitionTo("IdleState");
		}
	}
}
