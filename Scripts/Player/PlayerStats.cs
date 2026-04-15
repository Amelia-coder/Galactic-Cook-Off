using Godot;
using System;

public partial class PlayerStats : Node
{
	public float Speed = 5f;
	public float JumpVelocity = 5f;
	public float Gravity = 9.8f;
	
	public float MaxStamina = 100f;
	public float RegenRate = 15f;      // в покое
	public float SprintCost = 20f;     // в секунду
	public float JumpCost = 15f;
	public float FightCost = 10f;

}
