using Godot;
using System.Collections.Generic;

namespace Scripts.Game.RecipeSystem.Dishes
{
    public static class DishRegistry
    {
        private static readonly Dictionary<string, string> _scenePaths = new()
        {
            ["pizza_item"] = "res://Scenes/Dishes/Pizza.tscn",
            //["soup_item"] = "res://Scenes/Dishes/Soup.tscn",
            //["burger_item"] = "res://Scenes/Dishes/Burger.tscn",
        };

        private static readonly Dictionary<string, PackedScene> _cache = new();

        public static PackedScene GetScene(string resultId)
        {
            if (_cache.TryGetValue(resultId, out var cached))
                return cached;

            if (!_scenePaths.TryGetValue(resultId, out var path))
            {
                GD.PrintErr($"[DishRegistry] No scene for '{resultId}'");
                return null;
            }

            var scene = GD.Load<PackedScene>(path);
            _cache[resultId] = scene;
            return scene;
        }
    }
}
