using Godot;
using Scripts.UI;
using Scripts.Networking;
using Scripts;

namespace Scripts.App
{
	public partial class AppRoot : Node
	{
		[Export] public PackedScene MenuScene;
		[Export] public PackedScene ArenaScene;

		[Export] public SceneManager SceneManager;
		[Export] public NetworkManager NetworkManager;
		[Export] public LanDiscovery LanDiscovery;

		private Node _currentScene;

		public override void _Ready()
		{
			SceneManager.SceneLoaded += OnSceneLoaded;
			SceneManager.LoadFailed += OnSceneLoadFailed;

			SceneManager.LoadSceneAsync(MenuScene);
		}

		private void OnSceneLoaded(Node scene)
		{
			GD.Print($"Loaded scene: {scene.Name}");

			_currentScene = scene;

			if (scene is Menu menu)
			{
				GD.Print($"[AppRoot] wiring menu: {menu.GetInstanceId()}");
				menu.HostRequested += StartHost;
				GD.Print("[AppRoot] subscribed to HostRequested");
				menu.JoinRequested += StartClient;
				menu.ExitRequested += ExitGame;

				LanDiscovery.StartClientDiscovery();
			}

			if (scene is Arena)
			{
				GD.Print("Arena initialized");

				LanDiscovery.StopClientDiscovery();
			}
		}

		private void OnSceneLoadFailed(string path)
		{
			GD.PrintErr($"Failed to load scene: {path}");
		}

		private void StartHost()
		{
			GD.Print("Starting host");


			LanDiscovery.StartHostBroadcast(
				"Cooking Chaos",
				65000
			);

			NetworkManager.Host();



			//load lobby instead
			SceneManager.LoadSceneAsync(ArenaScene);
		}

		private void StartClient(string ip)
		{
			GD.Print($"Joining {ip}");

			NetworkManager.Connected += OnClientConnected;

			NetworkManager.Join(ip, 65000);

			
			//SceneManager.LoadSceneAsync(ArenaScene);
		}

		private void OnClientConnected()
		{
			NetworkManager.Connected -= OnClientConnected;

			SceneManager.LoadSceneAsync(ArenaScene);
		}

		private void ExitGame()
		{
			GetTree().Quit();
		}
	}
}
