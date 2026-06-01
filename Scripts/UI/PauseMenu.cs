using System;
using Godot;

namespace Scripts.UI
{
	public partial class PauseMenu : CanvasLayer
	{
		public event Action ExitRequested;

		private Control _panel;
		private bool _isOpen;
		private bool _enabled; // only allow opening during gameplay

		public bool IsOpen => _isOpen;

		public override void _Ready()
		{
			Layer = 100;
			_panel = GetNode<Control>("PanelContainer");
			ProcessMode = ProcessModeEnum.Always;
			_panel.Visible = false;

			GetNode<Button>("PanelContainer/ColorRect/VBoxContainer/ResumeButton").Pressed += Close;
			GetNode<Button>("PanelContainer/ColorRect/VBoxContainer/ExitButton").Pressed += OnExit;
		}

		public void Enable() => _enabled = true;
		public void Disable() { _enabled = false; Close(); }

		public override void _UnhandledInput(InputEvent @event)
		{
			if (!_enabled) return;
			if (!@event.IsActionPressed("ui_cancel")) return;

			if (_isOpen) Close();
			else Open();

			GetViewport().SetInputAsHandled();
		}

		private void Open()
		{
			_isOpen = true;
			_panel.Visible = true;
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}

		private void Close()
		{
			if (!_isOpen) return;
			_isOpen = false;
			_panel.Visible = false;
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}

		private void OnExit()
		{
			Close();
			ExitRequested?.Invoke();
		}
	}
}
