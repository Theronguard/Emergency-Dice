using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class JumpscareGlitch : IEffect
    {
        public string Name => "JumpscareGlitch";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Boo!";

        public void Use()
        {
            Networker.Instance.StartCoroutine(Networker.Instance.DelayJumpscare());
        }
    }
}
