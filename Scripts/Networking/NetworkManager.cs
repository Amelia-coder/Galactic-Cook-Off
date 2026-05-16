using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scripts.Networking.LANComponents;

//todo: make universal(so that it will be able to work with both lan and outisde-hosted lobbies
namespace Scripts.Networking
{
    public partial class NetworkManager : Node
    {
        public event Action Connected;
        public event Action ConnectionFailed;
        public void Host()
        {
            var peer = new ENetMultiplayerPeer();
            peer.CreateServer(7777);

            Multiplayer.MultiplayerPeer = peer;
        }

        public void Join(string ip, int port)
        {
            var peer = new ENetMultiplayerPeer();
            peer.CreateClient(ip, port);

            Multiplayer.MultiplayerPeer = peer;
        }
    }
}
