﻿using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Purge : IEffect
    {
        public string Name => "Purge";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "All living enemies explode";
        public void Use()
        {
            Networker.Instance.PurgeServerRPC();
        }

        public static void PurgeAllEnemies()
        {
            foreach(var enemy in RoundManager.Instance.SpawnedEnemies)
            {
                Landmine.SpawnExplosion(enemy.transform.position, true, 2, 5);
                enemy.KillEnemy(true);
            }
        }
    }
}
