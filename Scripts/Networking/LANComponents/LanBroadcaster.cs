using Godot;
using System.Text;

namespace Scripts.Networking.LANComponents
{
    public partial class LanBroadcaster : Node
    {
        [Export] public int BroadcastPort = 9999;

        [Export] public string ServerName = "Cooking Server";

        [Export] public int GamePort = 9999;

        private PacketPeerUdp _udp;

        private Timer _timer;


        public override void _Ready()
        {
            _udp = new PacketPeerUdp();

            // Required for LAN broadcast packets
            _udp.SetBroadcastEnabled(true);

            // Internal periodic broadcaster
            _timer = new Timer();

            _timer.WaitTime = 1.0;
            _timer.Autostart = false;
            _timer.OneShot = false;

            _timer.Timeout += Broadcast;

            AddChild(_timer);
        }

        public void Init(string serverName, int gamePort)
        {
            ServerName = serverName;
            GamePort = gamePort;
        }

        public void StartBroadcasting()
        {
            GD.Print("[LAN] Broadcasting started");

            _timer.Start();
        }

        public void StopBroadcasting()
        {
            GD.Print("[LAN] Broadcasting stopped");

            _timer.Stop();
        }

        
        private void Broadcast()
        {
            // Example:
            // GAME_SERVER|KitchenWar|9999

            string message =
                $"GAME_SERVER|{ServerName}|{GamePort}";

            byte[] bytes =
                Encoding.UTF8.GetBytes(message);

            _udp.SetDestAddress(
                "255.255.255.255",
                BroadcastPort);

            _udp.PutPacket(bytes);

            GD.Print($"[LAN] Broadcasted: {message}");
        }
    }
}
