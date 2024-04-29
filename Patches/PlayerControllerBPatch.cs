﻿using HarmonyLib;
using MysteryDice.Effects;
using GameNetcodeStuff;
using UnityEngine;

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

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void FlyMode(PlayerControllerB __instance)
        {
            if (!Fly.CanFly) return;

            if (IngamePlayerSettings.Instance.playerInput.actions.FindAction("Sprint").ReadValue<float>() > 0.5f)
            {
                __instance.externalForces += Vector3.Lerp(__instance.externalForces, Vector3.ClampMagnitude(__instance.transform.up * 10, 400f), Time.deltaTime * 50f);
                __instance.fallValue = 0f;
                __instance.ResetFallGravity();
            }

        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void BreakNeckUpdate(PlayerControllerB __instance)
        {
            if (!NeckBreak.IsNeckBroken) return;

            Transform cam = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform;
            cam.eulerAngles = new Vector3(cam.eulerAngles.x, cam.eulerAngles.y, 90f);
        }
    }
}
