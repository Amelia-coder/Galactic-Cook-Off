using Godot;

namespace Scripts.Player.States
{
	public partial class NotAttackingState : CombatState
	{


		public override void Enter()
		{
		}


		public override void PhysicsUpdate(double delta)
		{
			//TODO: добаваить переходы с клаившами аткаи и подорбным
			if (Input.IsActionJustPressed("attack"))
			{
				TransitionTo("AttackState");
			}
			//if (Input.IsActionJustPressed("jump") && Entity.CanJump)
			//{

			//    GD.Print($"We jumped!", Entity.CanJump);
			//    TransitionTo("JumpState");
			//}
			//Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
			//if (inputDir != Vector2.Zero)
			//{
			//    TransitionTo("WalkState");
			//}

			//if (Input.IsActionJustPressed("run"))
			//{
			//    GD.Print($"We are running!", Entity.CanJump);
			//    TransitionTo("RunState");
			//}
		}
	}
}