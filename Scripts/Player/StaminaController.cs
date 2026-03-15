//using Godot;

//public partial class StaminaController : Node
//{
//	[Export] public float MaxStamina = 100f;
//	[Export] public float RegenRate = 15f;      // в покое
//	[Export] public float SprintCost = 20f;     // в секунду
//	[Export] public float JumpCost = 15f;
//	[Export] public float FightCost = 10f;

//	public float Current { get; private set; }
//	private bool _isConsuming = false;

//	public override void _Ready() => Current = MaxStamina;

//	public override void _Process(double delta)
//	{
//		if (!_isConsuming)
//			Current = Mathf.Min(Current + RegenRate * (float)delta, MaxStamina);
//		_isConsuming = false; // сбрасываем каждый кадр
//	}

//	public bool Consume(float amount)
//	{
//		if (Current < amount) return false;
//		Current -= amount;
//		_isConsuming = true;
//		return true;
//	}

//	public void ConsumePerFrame(float ratePerSecond, double delta)
//	{
//		_isConsuming = true;
//		Current = Mathf.Max(0, Current - ratePerSecond * (float)delta);
//	}
//}
