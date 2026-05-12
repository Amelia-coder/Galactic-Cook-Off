using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Game
{
    public partial class EnemyWavesController
    {
        //[Export] public PackedScene EnemyScene;

        //public async Task RunWave(WaveDefinition wave)
        //{
        //    for (int i = 0; i < wave.EnemyCount; i++)
        //    {
        //        SpawnEnemy(wave);

        //        await ToSignal(
        //            GetTree().CreateTimer(wave.SpawnInterval),
        //            "timeout");
        //    }
        //}

        //private void SpawnEnemy(WaveDefinition wave)
        //{
        //    var enemy = EnemyScene.Instantiate<Node3D>();

        //    AddChild(enemy);

        //    enemy.GlobalPosition = GetRandomSpawnPoint();

        //    // later:
        //    // enemy.Health *= wave.HealthMultiplier;
        //}
    }
}
