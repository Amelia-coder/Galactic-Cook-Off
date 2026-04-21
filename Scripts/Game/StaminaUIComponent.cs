using Godot;
using System;

public partial class StaminaUIComponent : Control
{
	private ProgressBar _bar;
	private Label _label;

	public override void _Ready()
	{
		_bar = GetNode<ProgressBar>("StaminaProgressBar");
		_label = GetNode<Label>("StaminaLabel");

		_label.Text = "Выносливость";
	}

	public void Bind(StaminaComponent stamina)
	{
		stamina.StaminaChanged += OnStaminaChanged;
		stamina.StaminaConsumed += OnStaminaConsumed;
	}

	private void OnStaminaChanged(float current, float max)
	{
		_bar.Value = (current / max) * 100f;
		//GD.Print($"We actually caaanged ui!", _bar.Value);
	}

	private void OnStaminaConsumed(float amount)
	{
		// optional feedback later
	}
}
