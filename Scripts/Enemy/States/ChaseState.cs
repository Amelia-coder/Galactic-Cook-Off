using Scripts.Game;

namespace Scripts.Enemy.Enemy
{
	public partial class ChaseState : State<IEnemyEntity>
	{
		public override void Enter()
		{
			//GD.Print("");
			//_jumpReleased = false;
		}

		public override void PhysicsUpdate(double delta)
		{
			//var _movement = Entity.GetComponent<MovementComponent>();
			//var _attackComponent = Entity.GetComponent<AttackComponent>();


			//var target = Entity.GetTarget<Node3D>();

			//if (target == null)
			//{
			//	StateMachine.ChangeState("Idle");
			//	return;
			//}

			//if (_attack.CanAttack(target))
			//{
			//	StateMachine.ChangeState("Attack");
			//	return;
			//}

			//_pathfinding.SetDestination(target.GlobalPosition);

			//Vector3 next =
			//	_pathfinding.GetNextPoint();

			//Vector3 dir =
			//	(next - Entity.GlobalPosition).Normalized();

			//_movement.SetDesiredDirection(dir);
			//// Apply physics
			//_movement.Update((float)delta);

			//// Transition when landing
			//if (_movement.IsGrounded)
			//{
			//	if (_input.MoveDirection.LengthSquared() > 0.01f)
			//	{
			//		if (_input.SprintPressed)
			//			TransitionTo("RunState");
			//		else
			//			TransitionTo("WalkState");
			//	}
			//	else
			//	{
			//		TransitionTo("IdleState");
			//	}
			//}
		}
	}
}
