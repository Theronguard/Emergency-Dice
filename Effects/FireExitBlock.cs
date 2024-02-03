using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class FireExitBlock : IEffect
    {
        public string Name => "FireExitBlocks";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Fire exit blocked!";
        public void Use()
        {
            Networker.Instance.BlockFireExitsServerRPC();
        }

    }
}
