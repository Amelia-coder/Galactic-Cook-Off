using Godot;
using Scripts.Game.RecipeSystem.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.UI
{
    public partial class RecipeSelectionUI : Control
    {
        [Export] public VBoxContainer RecipeList { get; set; }
        [Export] public PackedScene RecipeButtonScene { get; set; } // a Button scene

        public event Action<string> RecipeSelected;

        public override void _Ready()
        {
            Visible = false;
        }

        public void Show(List<Recipe> recipes)
        {
            // Clear old buttons
            foreach (var child in RecipeList.GetChildren())
                child.QueueFree();

            // Create a button per recipe
            foreach (var recipe in recipes)
            {
                var btn = new Button();
                btn.Text = recipe.Id; // or recipe.DisplayName
                var capturedId = recipe.Id;
                btn.Pressed += () =>
                {
                    RecipeSelected?.Invoke(capturedId);
                    Hide();
                };
                RecipeList.AddChild(btn);
            }

            Visible = true;
        }
    }
}
