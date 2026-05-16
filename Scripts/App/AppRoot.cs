using Godot;
using Scripts.UI;

namespace Scripts.App
{
    public partial class AppRoot : Node
    {
        [Export] public PackedScene ArenaScene;
        [Export] public PackedScene MenuScene;

        [Export] public SceneManager SceneManager;

        private Menu _menuInstance;

        public override void _Ready()
        {
            ShowMenu();

            SceneManager.SceneLoaded += OnSceneLoaded;
            SceneManager.LoadFailed += OnSceneLoadFailed;
        }

        private void ShowMenu()
        {
            _menuInstance = MenuScene.Instantiate<Menu>();

            SceneManager.SceneContainer.AddChild(_menuInstance);

            _menuInstance.HostRequested += StartHost;
            _menuInstance.JoinRequested += StartClient;
            _menuInstance.ExitRequested += OnExitRequested;
        }

        public void StartHost()
        {
            GD.Print("[AppRoot] Hosting game");

            // NetworkManager.Host();

            SceneManager.LoadSceneAsync(ArenaScene);
        }

        public void StartClient(string ip)
        {
            GD.Print($"[AppRoot] Joining {ip}");

            // NetworkManager.Join(ip);

            SceneManager.LoadSceneAsync(ArenaScene);
        }

        private void OnSceneLoaded(PackedScene scene)
        {
            GD.Print(
                $"[AppRoot] Scene loaded: {scene.ResourcePath}");
        }

        private void OnSceneLoadFailed(string path)
        {
            GD.PrintErr(
                $"[AppRoot] Failed to load scene: {path}");
        }

        private void OnExitRequested()
        {
            GetTree().Quit();
        }
    }
