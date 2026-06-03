using Godot;
using Scripts.Enemy.Components;
using Scripts.Game;
using Scripts.Player.Components; //remove this and intorice genrilzed component is Scripts.Game namespace!
namespace Scripts.Enemy.States
{
	public partial class AttackState : State<IEntity>
	{
		public override void Enter()
		{
			//GD.Print("Enmey entered attck state");
			//y play animation
		}


		public override void PhysicsUpdate(double delta)
		{
			var detector = Entity.GetComponent<TargetDetectorComponent>();
			var selector = Entity.GetComponent<TargetSelectorComponent>();
			var attack = Entity.GetComponent<EnemyAttackComponent>();

			if (detector == null ||
				selector == null ||
				attack == null)
			{
				TransitionTo("ChaseState");
				return;
			}

			// IMPORTANT:
			// update cooldowns
			attack.UpdateStrategies(delta);

			if (!detector.HasTargets())
			{
				TransitionTo("ChaseState");
				return;
			}

			Node3D target = selector.SelectTarget(
				detector.Targets,
				(Node3D)Entity);

			if (target == null)
			{
				TransitionTo("ChaseState");
				return;
			}

			AttackStrategy strategy =
				attack.GetAvailableAttack(
					(Node3D)Entity,
					target);

			if (strategy == null)
			{
				TransitionTo("ChaseState");
				return;
			}

			strategy.Execute(
				(Node3D)Entity,
				target);
		}
	}
}
