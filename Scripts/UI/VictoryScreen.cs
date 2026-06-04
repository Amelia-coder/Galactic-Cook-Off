using Godot;
using System;

namespace Scripts.UI
{
	public partial class VictoryScreen : Control
	{
		[Signal]
		public delegate void ExitPressedEventHandler();
		private Button _exitButton; 

		public override void _Ready()
		{
			_exitButton = GetNode<Button>("VBoxContainer/ExitButton");
			_exitButton.Pressed += () => EmitSignal(SignalName.ExitPressed);
			//var button = GetNode<Button>("Button");
			//button.Pressed += 
		}
	}
}
