using GameNetcodeStuff;
using HarmonyLib;
using LethalLib;
using MysteryDice.Dice;
using MysteryDice.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class ChatPatch
    {
        [HarmonyPatch("SubmitChat_performed")]
        [HarmonyPrefix]
        private static void ChatTesting(HUDManager __instance)
        {
            
        }
    }
}
