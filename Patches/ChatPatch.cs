using GameNetcodeStuff;
using HarmonyLib;
using LethalLib;
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
        private static void GameMasterCommands(HUDManager __instance)
        {
            /*
               if (__instance.chatTextField.text == "enemies")
               {
                   MysteryDice.CustomLogger.LogMessage("ENEMIES CURRENT:");
                   foreach (var enemy in RoundManager.Instance.currentLevel.Enemies)
                   {
                       MysteryDice.CustomLogger.LogMessage(enemy.enemyType.enemyName);
                   }
               }
               if (__instance.chatTextField.text == "dice1")
               {
                   GameObject obj = UnityEngine.Object.Instantiate(MysteryDice.DebugEmergencyDie.spawnPrefab,
                   GameNetworkManager.Instance.localPlayerController.transform.position,
                   Quaternion.identity,
                   RoundManager.Instance.playersManager.propsContainer);

                   obj.GetComponent<GrabbableObject>().fallTime = 0f;
                   obj.GetComponent<NetworkObject>().Spawn();
               }
               if (__instance.chatTextField.text == "dice2")
               {
                   GameObject obj = UnityEngine.Object.Instantiate(MysteryDice.DebugGamblerDie.spawnPrefab,
                   GameNetworkManager.Instance.localPlayerController.transform.position,
                   Quaternion.identity,
                   RoundManager.Instance.playersManager.propsContainer);

                   obj.GetComponent<GrabbableObject>().fallTime = 0f;
                   obj.GetComponent<NetworkObject>().Spawn();
               }
               if (__instance.chatTextField.text == "checkSpawn")
               {
                   foreach (var level in RoundManager.Instance.currentLevel.spawnableScrap)
                   {
                       Plugin.logger.LogInfo($"Item: {level.spawnableItem.itemName}, Rarity: {level.rarity}");
                   }
               }
               if (__instance.chatTextField.text == "test2")
               {
                   MysteryDice.CustomLogger.LogInfo(StartOfRound.Instance.currentLevel.PlanetName);
               }
               if (__instance.chatTextField.text == "test")
               {
                   (new RebeliousCoilHeads()).Use();
               }
               if (__instance.chatTextField.text == "test3")
               {
                   (new ScrapJackpot()).Use();
               }

               if (__instance.chatTextField.text == "test4")
               {
                   (new Swap()).Use();
               }
               if (__instance.chatTextField.text == "test5")
               {
                   (new FireExitBlock()).Use();
               }
               if (__instance.chatTextField.text == "test6")
               {
                   PlayerControllerB p1 = null;
                   PlayerControllerB p2 = null;
                   foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
                   {
                       PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();

                       if (player == null) continue;
                       if (!Networker.IsPlayerAliveAndControlled(player)) continue;

                       if (p1 == null)
                       {
                           p1 = player;
                           continue;
                       }
                       if (p2 == null)
                       {
                           p2 = player;
                           continue;
                       }
                   }

                   MysteryDice.CustomLogger.LogInfo(Vector3.Distance(p1.transform.position, p2.transform.position));
               }
               if (__instance.chatTextField.text == "test7")
               {
                   (new ReturnToShipTogether()).Use();
               }
               if(__instance.chatTextField.text == "bee")
               {
                  (new Beepocalypse()).Use();
               }
               if(__instance.chatTextField.text == "spawnable")
               {
                   RoundManager RM = RoundManager.Instance;
                   MysteryDice.CustomLogger.LogInfo("ENEMIES");
                   foreach (var item in RM.currentLevel.Enemies)
                   {
                       MysteryDice.CustomLogger.LogInfo(item.enemyType.enemyName);
                   }
                   MysteryDice.CustomLogger.LogInfo("DAYTIME ENEMIES");
                   foreach (var item in RM.currentLevel.DaytimeEnemies)
                   {
                       MysteryDice.CustomLogger.LogInfo(item.enemyType.enemyName);
                   }
                   MysteryDice.CustomLogger.LogInfo("OUTSIDE OBJ");

                   foreach (var item in RM.currentLevel.spawnableOutsideObjects)
                   {
                       MysteryDice.CustomLogger.LogInfo(item.spawnableObject.name);
                   }
                   MysteryDice.CustomLogger.LogInfo("SPAWNABLE MAP OBJS");
                   foreach (var item in RM.currentLevel.spawnableMapObjects)
                   {
                       MysteryDice.CustomLogger.LogInfo(item.prefabToSpawn.name);

                   }
                   MysteryDice.CustomLogger.LogInfo("Outsidenemies");
                   foreach (var item in RM.currentLevel.OutsideEnemies)
                   {
                       MysteryDice.CustomLogger.LogInfo(item.enemyType.name);
                   }
               }
               if (__instance.chatTextField.text == "worm")
               {
                   (new Wormageddon()).Use();
               }

               if (__instance.chatTextField.text == "arma")
               {
                   (new Armageddon()).Use();
               }
               if (__instance.chatTextField.text == "boo")
               {
                   (new JumpscareGlitch()).Use();
               }
               if (__instance.chatTextField.text == "alarm")
               {
                   (new AlarmCurse()).Use();
               }
               if (__instance.chatTextField.text == "door")
               {
                   (new InvertDoorLock()).Use();
               }
               if (__instance.chatTextField.text == "zomb")
               {
                   (new ZombieToShip()).Use();
               }
            if (__instance.chatTextField.text == "mines")
            {
                (new SilentMine()).Use();
            }
            if (__instance.chatTextField.text == "overf")
            {
                (new MineOverflow()).Use();
            }
            if (__instance.chatTextField.text == "turrh")
            {
                (new TurretHell()).Use();
            }
            if (__instance.chatTextField.text == "shiptur")
            {
                (new ShipTurret()).Use();
            }
            */
        }
            

    }
}
