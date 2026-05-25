using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Game
{
    /// <summary>
    /// Data-only description of a single wave.
    /// Create these as .tres files in the editor or build them in code.
    /// </summary>
    [GlobalClass]
    public partial class WaveDefinition : Resource
    {
        /// <summary>
        /// Which enemy scene to spawn. Later this becomes an enum
        /// that a factory resolves — for now, direct scene reference.
        /// </summary>
        [Export] public PackedScene EnemyScene;

        /// <summary>Total enemies in this wave.</summary>
        [Export] public int EnemyCount = 3;

        /// <summary>Seconds between each spawn.</summary>
        [Export] public float SpawnInterval = 1.5f;

        /// <summary>Health multiplier applied to every enemy in this wave.</summary>
        [Export] public float HealthMultiplier = 1.0f;

        /// <summary>Speed multiplier applied to every enemy in this wave.</summary>
        [Export] public float SpeedMultiplier = 1.0f;

        /// <summary>
        /// If true, the next wave won't start until all enemies
        /// from this wave are dead.
        /// </summary>
        [Export] public bool WaitForClear = true;

        /// <summary>Optional delay before the first spawn.</summary>
        [Export] public float PreWaveDelay = 2.0f;
    }
}
