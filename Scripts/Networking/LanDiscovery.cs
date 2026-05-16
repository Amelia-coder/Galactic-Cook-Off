using Godot;

namespace Scripts.Networking
{
    public partial class LanDiscovery : Node
    {
        private PacketPeerUdp _listener;

        public override void _Ready()
        {
            _listener = new PacketPeerUdp();

            var err = _listener.Bind(8910);

            if (err != Error.Ok)
            {
                GD.PrintErr("Failed to bind LAN listener");
                return;
            }

            GD.Print("LAN discovery listening...");
        }

        public override void _Process(double delta)
        {
            while (_listener.GetAvailablePacketCount() > 0)
            {
                byte[] packet = _listener.GetPacket();

                string msg =
                    System.Text.Encoding.UTF8
                        .GetString(packet);

                string ip =
                    _listener.GetPacketIP();

                GD.Print(
                    $"Found server: {msg} at {ip}");
            }
        }
    }
}
