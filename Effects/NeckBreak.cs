using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class NeckBreak : IEffect
    {
        public string Name => "Neck Break";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Your neck is broken!";
        
        public static bool IsNeckBroken = false;
        public void Use()
        {
            Networker.Instance.NeckBreakRandomPlayerServerRpc();
        }

        public static void BreakNeck()
        {
            IsNeckBroken = true;
        }
        public static void FixNeck()
        {
            IsNeckBroken = false;
            Transform cam = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform;
            cam.eulerAngles = new Vector3(cam.eulerAngles.x, cam.eulerAngles.y, 0f);
        }
    }
}
