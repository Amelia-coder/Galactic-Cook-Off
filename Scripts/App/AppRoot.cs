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

			// -------------------------
			// MENU
			// -------------------------
			if (scene is Menu menu)
			{
				menu.HostRequested += StartHost;
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

			NetworkManager.Host();

			LanDiscovery.StartHostBroadcast(
				"Cooking Chaos",
				7777
			);

			SceneManager.LoadSceneAsync(ArenaScene);
		}

		private void StartClient(string ip)
		{
			GD.Print($"Joining {ip}");

			NetworkManager.Join(ip, 7777);

			SceneManager.LoadSceneAsync(ArenaScene);
		}

		private void ExitGame()
		{
			GetTree().Quit();
		}
	}
}
