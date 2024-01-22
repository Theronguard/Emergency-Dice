using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class ZombieApocalypse : IEffect
    {
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Zombie apocalypse!";
        public void Use()
        {
            int amountToSpawn = UnityEngine.Random.Range(8, 20);
            HUDManager.Instance.DisplayTip(":)","");
            if(GetEnemies.Masked == null)
                return;

            Misc.SpawnEnemyForced(GetEnemies.Masked, amountToSpawn, true);
            
        }
    }
}
