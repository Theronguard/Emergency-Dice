using HarmonyLib;
using MysteryDice.Dice;
using MysteryDice.Effects;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class ChatPatch
    {
        public static bool AllowChatDebug = false;
        const string BaseCommand = "~edice ";
        [HarmonyPatch("SubmitChat_performed")]
        [HarmonyPrefix]
        private static void ChatTesting(HUDManager __instance)
        {
            if (!AllowChatDebug) return;

            string txt = __instance.chatTextField.text.ToLower();

            if (txt == BaseCommand + "enemies")
            {
                MysteryDice.CustomLogger.LogMessage("Registered enemies:");
                foreach (var enemy in RoundManager.Instance.currentLevel.Enemies)
                    MysteryDice.CustomLogger.LogMessage(enemy.enemyType.enemyName);
            }

            if (txt.Contains(BaseCommand + "dice"))
            {
                string[] args = txt.Split(' ');
                if (args.Length < 3) return;
                
                if(Int32.TryParse(args[2], out int diceNum))
                {
                    GameObject selectedDie = MysteryDice.DebugEmergencyDie.spawnPrefab;
                    switch (diceNum)
                    {
                        case 1:
                            selectedDie = MysteryDice.DebugEmergencyDie.spawnPrefab;
                            break;
                        case 2:
                            selectedDie = MysteryDice.DebugGamblerDie.spawnPrefab;
                            break;
                        case 3:
                            selectedDie = MysteryDice.DebugChronosDie.spawnPrefab;
                            break;
                        case 4:
                            selectedDie = MysteryDice.DebugSacrificerDie.spawnPrefab;
                            break;
                    }

                    GameObject obj = UnityEngine.Object.Instantiate
                    (
                        selectedDie,
                        GameNetworkManager.Instance.localPlayerController.transform.position,
                        Quaternion.identity,
                        RoundManager.Instance.playersManager.propsContainer
                    );

                    obj.GetComponent<GrabbableObject>().fallTime = 0f;
                    obj.GetComponent<NetworkObject>().Spawn();
                }
            }

            foreach (IEffect effect in DieBehaviour.AllEffects)
            {
                if (txt.Contains(BaseCommand + "effect " + effect.Name.ToLower()) || txt.Contains(BaseCommand + "e " + effect.Name.ToLower()))
                {
                    effect.Use();
                }
            }
        }
        
    }

    
}
