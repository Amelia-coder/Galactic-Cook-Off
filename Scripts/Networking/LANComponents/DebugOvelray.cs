using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Networking.LANComponents
{
    using Godot;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// On-screen debug log. Add this node to your scene tree (e.g. as child of AppRoot)
    /// and call DebugOverlay.Log("message") from anywhere.
    /// </summary>
    public partial class DebugOverlay : CanvasLayer
    {
        private static DebugOverlay _instance;
        private RichTextLabel _label;
        private readonly List<string> _lines = new();
        private const int MaxLines = 30;

        public override void _Ready()
        {
            _instance = this;

            // Always on top
            Layer = 128;

            var panel = new PanelContainer();
            panel.AnchorLeft = 0f;
            panel.AnchorTop = 0f;
            panel.AnchorRight = 1f;
            panel.AnchorBottom = 0.45f;
            // Make it semi-transparent so it doesn't fully block the game
            panel.Modulate = new Color(1, 1, 1, 0.85f);
            panel.MouseFilter = Control.MouseFilterEnum.Ignore;
            AddChild(panel);

            _label = new RichTextLabel();
            _label.BbcodeEnabled = true;
            _label.ScrollFollowing = true;
            _label.MouseFilter = Control.MouseFilterEnum.Ignore;
            _label.AddThemeFontSizeOverride("normal_font_size", 14);
            panel.AddChild(_label);

            Log("[DebugOverlay] Ready — logs will appear here");
        }

        public static void Log(string msg)
        {
            string time = DateTime.Now.ToString("HH:mm:ss.ff");
            string line = $"[{time}] {msg}";

            // Also print to console/log file in case someone checks there
            GD.Print(line);

            if (_instance == null) return;

            _instance._lines.Add(line);
            while (_instance._lines.Count > MaxLines)
                _instance._lines.RemoveAt(0);

            _instance._label.Text = string.Join("\n", _instance._lines);
        }

        public static void Warn(string msg)
        {
            Log($"[color=yellow]⚠ {msg}[/color]");
        }

        public static void Err(string msg)
        {
            Log($"[color=red]✖ {msg}[/color]");
        }

        public static void Ok(string msg)
        {
            Log($"[color=green]✔ {msg}[/color]");
        }
    }
}
