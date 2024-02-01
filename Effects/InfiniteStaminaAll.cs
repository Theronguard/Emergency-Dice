using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class InfiniteStaminaAll : IEffect
    {
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Infinite stamina for everyone";
        public void Use()
        {
            Networker.Instance.InfiniteStaminaAllServerRPC();
        }
    }
}
