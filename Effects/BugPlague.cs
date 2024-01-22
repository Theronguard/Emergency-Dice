using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class BugPlague : IEffect
    {
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Spawned a lot of bugs!";

        public void Use()
        {
            int centipedeToSpawn = UnityEngine.Random.Range(15, 30);
            int bugToSpawn = UnityEngine.Random.Range(15, 30);
            if (GetEnemies.HoardingBug == null || GetEnemies.Centipede == null)
                return;

            Misc.SpawnEnemyForced(GetEnemies.HoardingBug, bugToSpawn, true);
            Misc.SpawnEnemyForced(GetEnemies.Centipede, centipedeToSpawn, true);
        }
    }
}
