using Godot;
using System;

namespace Scripts.UI
{
    public partial class VictoryScreen : Control
    {
        [Signal]
        public delegate void ContinuePressedEventHandler();

        public override void _Ready()
        {
            //var button = GetNode<Button>("Button");
            //button.Pressed += () => EmitSignal(SignalName.ContinuePressed);
        }
    }
}