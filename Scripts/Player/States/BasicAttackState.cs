using Godot;
using System;

public partial class BasicAttackState : CombatState
{
    public override void Enter()
    {
        //Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
        //Vector3 moveDirection = Entity.GetMovementDirection(inputDir);

        //Entity.Velocity = new Vector3(
        //    moveDirection.X * ,
        //    Entity.Velocity.Y,
        //    moveDirection.Z * Speed
        //);
    }


    public override void PhysicsUpdate(double delta)
    {
        ///таймер для перехода 
        //TODO: добаваить переходы с клаившами аткаи и подорбным
        //if (Input.IsActionJustPressed("attack"))
        //{
        //    TransitionTo("AttackState");
        //}
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
