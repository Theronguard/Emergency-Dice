using GameNetcodeStuff;
using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice.Effects
{
    internal class MineOverflow : IEffect
    {
        public static int MaxMinesToSpawn = 50;
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "More mines spawned inside!";

        public void Use()
        {
            Networker.Instance.MineOverflowServerRPC();
        }

        public static void SpawnMoreMines()
        {
            List<Vector3> positions = new List<Vector3>();
            int spawnedMines = 0;
            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);
            
            foreach(Vector3 pos in RoundManagerPatch.MapObjectsPositions)
            {
                for(int i = 0; i < 8; i++)
                {
                    if (spawnedMines > MaxMinesToSpawn) return;

                    Vector3 position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(
                        pos,
                        10,
                        default,
                        random,
                        -5);

                    if (i > 3)
                        position = RoundManager.Instance.insideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position;

                    if (GetShortestDistanceSqr(position, positions) < 1)
                        continue;

                    GameObject gameObject = UnityEngine.Object.Instantiate(
                    StartOfRoundPatch.SpawnableLandmine.prefabToSpawn,
                    position,
                    Quaternion.identity,
                    RoundManager.Instance.mapPropsContainer.transform);

                    positions.Add(position);
                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, UnityEngine.Random.Range(0, 360), gameObject.transform.eulerAngles.z);
                    gameObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                    spawnedMines++;
                }
            }
        }

        public static float GetShortestDistanceSqr(Vector3 position, List<Vector3> positions)
        {
            float shortestLength = float.MaxValue;
            foreach(Vector3 pos in positions)
            {
                float distance = (position - pos).sqrMagnitude;
                if (distance < shortestLength)
                    shortestLength = distance;
            }
            return shortestLength;
        }
    }
}
