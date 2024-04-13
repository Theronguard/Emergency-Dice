using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice
{
    internal class Misc
    {
        private static bool hasOldLandimeCode = HasOldLandmineCode();

        public static void SpawnEnemy(SpawnableEnemyWithRarity enemy, int amount, bool isInside)
        {
            if (!Networker.Instance.IsHost) return;
            RoundManager RM = RoundManager.Instance;

            if (isInside)
            {
                for (int i = 0; i < amount; i++)
                {
                    EnemyVent randomVent = RM.allEnemyVents[UnityEngine.Random.Range(0, RM.allEnemyVents.Length)];
                    RM.SpawnEnemyOnServer(randomVent.floorNode.position, randomVent.floorNode.eulerAngles.y, RM.currentLevel.Enemies.IndexOf(enemy));
                }
            }
        }

        public static void SpawnOutsideEnemy(SpawnableEnemyWithRarity enemy)
        {
            RoundManager RM = RoundManager.Instance;

            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);
            GameObject[] aiNodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
            aiNodes = aiNodes.OrderBy(x => Vector3.Distance(x.transform.position, Vector3.zero)).ToArray();

            Vector3 position = RM.outsideAINodes[UnityEngine.Random.Range(0, RM.outsideAINodes.Length)].transform.position;
            position = RM.GetRandomNavMeshPositionInBoxPredictable(position, 30f, default(NavMeshHit), random) + Vector3.up;

            GameObject enemyObject = UnityEngine.Object.Instantiate(
                enemy.enemyType.enemyPrefab,
                position,
                Quaternion.Euler(new Vector3(0f, 0f, 0f)));

            enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
            RM.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
        }

        /// <summary>
        /// Allows an enemy to spawn on a moon which he isnt native to.
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="amount"></param>
        /// <param name="isInside"></param>
        public static void SpawnEnemyForced(SpawnableEnemyWithRarity enemy, int amount, bool isInside)
        {
            if (!RoundManager.Instance.currentLevel.Enemies.Contains(enemy))
            {
                RoundManager.Instance.currentLevel.Enemies.Add(enemy);
                SpawnEnemy(enemy, amount, isInside);
                RoundManager.Instance.currentLevel.Enemies.Remove(enemy);
            }
            else
            {
                SpawnEnemy(enemy, amount, isInside);
            }
        }

        public static float Map(float x, float inMin, float inMax, float outMin, float outMax)
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }

        public static PlayerControllerB GetPlayerByUserID(ulong userID)
        {
            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();
                if (player.playerClientId == userID)
                    return player;
            }
            return null;
        }

        public static NetworkObjectReference SpawnEnemyOnServer(Vector3 spawnPosition, float yRot, SpawnableEnemyWithRarity enemy)
        {
            NetworkObjectReference emptyReference = new NetworkObjectReference();
            if (!Networker.Instance.IsServer)
                return emptyReference;

            GameObject gameObject = UnityEngine.Object.Instantiate(enemy.enemyType.enemyPrefab, spawnPosition, Quaternion.Euler(new Vector3(0f, yRot, 0f)));
            gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
            RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
            return gameObject.GetComponentInChildren<NetworkObject>();
        }

        private static bool HasOldLandmineCode()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];

                if (assembly.GetName().Name != "Assembly-CSharp")
                {
                    continue;
                }

                Type landmine = assembly.GetType("Landmine");

                if (landmine == null)
                {
                    MysteryDice.CustomLogger.LogMessage($"[HasOldLandmineCode] Landmine not found");
                    return false;
                }

                MethodInfo[] methods = landmine.GetMethods();

                for (int j = 0; j < methods.Length; j++)
                {
                    MethodInfo method = methods[j];
                    MysteryDice.CustomLogger.LogMessage($"[HasOldLandmineCode] {method.Name} found");

                    if (method.Name != "SpawnExplosion")
                    {
                        MysteryDice.CustomLogger.LogMessage($"[HasOldLandmineCode] {method.Name} found");
                        continue;
                    }

                    ParameterInfo[] parameters = method.GetParameters();

                    if (parameters.Length == 4)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void SpawnExplosion(Vector3 explosionPosition, bool spawnExplosionEffect = false, float killRange = 1f, float damageRange = 1f, int nonLethalDamage = 50, float physicsForce = 0f, GameObject overridePrefab = null)
        {
            if (hasOldLandimeCode)
            {
                SpawnExplosion(explosionPosition, spawnExplosionEffect, killRange, damageRange);
            }
            else
            {
                // Example: Landmine.SpawnExplosion(explosionPosition, spawnExplosionEffect, killRange, damageRange, nonLethalDamage, physicsForce, overridePrefab);
            }
        }

        public static void SpawnExplosion(Vector3 explosionPosition, bool spawnExplosionEffect = false, float killRange = 1f, float damageRange = 1f)
        {
            Debug.Log("Spawning explosion at pos: {explosionPosition}");
            if (spawnExplosionEffect)
            {
                UnityEngine.Object.Instantiate(StartOfRound.Instance.explosionPrefab, explosionPosition, Quaternion.Euler(-90f, 0f, 0f), RoundManager.Instance.mapPropsContainer.transform).SetActive(value: true);
            }

            float num = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, explosionPosition);
            if (num < 14f)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            }
            else if (num < 25f)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
            }

            Collider[] array = Physics.OverlapSphere(explosionPosition, 6f, 2621448, QueryTriggerInteraction.Collide);
            PlayerControllerB playerControllerB = null;
            for (int i = 0; i < array.Length; i++)
            {
                float num2 = Vector3.Distance(explosionPosition, array[i].transform.position);
                if (num2 > 4f && Physics.Linecast(explosionPosition, array[i].transform.position + Vector3.up * 0.3f, 256, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                if (array[i].gameObject.layer == 3)
                {
                    playerControllerB = array[i].gameObject.GetComponent<PlayerControllerB>();
                    if (playerControllerB != null && playerControllerB.IsOwner)
                    {
                        if (num2 < killRange)
                        {
                            Vector3 bodyVelocity = (playerControllerB.gameplayCamera.transform.position - explosionPosition) * 80f / Vector3.Distance(playerControllerB.gameplayCamera.transform.position, explosionPosition);
                            playerControllerB.KillPlayer(bodyVelocity, spawnBody: true, CauseOfDeath.Blast);
                        }
                        else if (num2 < damageRange)
                        {
                            playerControllerB.DamagePlayer(50);
                        }
                    }
                }
                else if (array[i].gameObject.layer == 21)
                {
                    Landmine componentInChildren = array[i].gameObject.GetComponentInChildren<Landmine>();
                    if (componentInChildren != null && !componentInChildren.hasExploded && num2 < 6f)
                    {
                        Debug.Log("Setting off other mine");
                        componentInChildren.StartCoroutine(TriggerOtherMineDelayed(componentInChildren));
                    }
                }
                else if (array[i].gameObject.layer == 19)
                {
                    EnemyAICollisionDetect componentInChildren2 = array[i].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
                    if (componentInChildren2 != null && componentInChildren2.mainScript.IsOwner && num2 < 4.5f)
                    {
                        componentInChildren2.mainScript.HitEnemyOnLocalClient(6);
                    }
                }
            }

            int num3 = ~LayerMask.GetMask("Room");
            num3 = ~LayerMask.GetMask("Colliders");
            array = Physics.OverlapSphere(explosionPosition, 10f, num3);
            for (int j = 0; j < array.Length; j++)
            {
                Rigidbody component = array[j].GetComponent<Rigidbody>();
                if (component != null)
                {
                    component.AddExplosionForce(70f, explosionPosition, 10f);
                }
            }
        }

        private static IEnumerator TriggerOtherMineDelayed(Landmine mine)
        {
            if (!mine.hasExploded)
            {
                mine.mineAudio.pitch = UnityEngine.Random.Range(0.75f, 1.07f);
                mine.hasExploded = true;
                yield return new WaitForSeconds(0.2f);
                mine.SetOffMineAnimation();
            }
        }
    }
}
