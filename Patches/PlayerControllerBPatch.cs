using HarmonyLib;
using LethalLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using MysteryDice.Effects;
using GameNetcodeStuff;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        public static bool HasInfiniteStamina = false;

        [HarmonyPrefix]
        [HarmonyPatch("HasLineOfSightToPosition")]
        public static bool OverrideLineOfSight(ref bool __result)
        {
            if(RebeliousCoilHeads.IsEnabled && Networker.CoilheadIgnoreStares)
            {
                __result = false;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void InfiniteSprint(ref float ___sprintMeter)
        {
            if (HasInfiniteStamina)
                ___sprintMeter = 1f;
        }
    }
}
