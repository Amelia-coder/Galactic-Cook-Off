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
        [Export]
        PackedScene LanBroadcasterScene;

        [Export]
        PackedScene LanListenerScene;

        private LanBroadcaster _broadcaster;
        private LanListener _listener;

        public void Host()
        {
            // start ENet server

            _broadcaster =
                LanBroadcasterScene
                    .Instantiate<LanBroadcaster>();

            AddChild(_broadcaster);
        }

        public void StartSearching()
        {
            _listener =
                LanListenerScene
                    .Instantiate<LanListener>();

            AddChild(_listener);
        }

        public void StopSearching()
        {
            _listener?.QueueFree();
            _listener = null;
        }
    }
}
