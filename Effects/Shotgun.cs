using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Shotgun : IEffect
    {
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Spawning a shotgun!";
        public void Use()
        {
            Networker.Instance.ShotgunServerRPC();
        }

        public static void SpawnShotgun()
        {
            List<Item> items = UnityEngine.Resources.FindObjectsOfTypeAll<Item>().ToList();
            Item shotgun = items.FirstOrDefault(item => item.name.Equals("Shotgun"));
            Item ammo = items.FirstOrDefault(item => item.name.Equals("GunAmmo"));

            GameObject obj = UnityEngine.Object.Instantiate(shotgun.spawnPrefab,
               GameNetworkManager.Instance.localPlayerController.transform.position,
               Quaternion.identity,
               RoundManager.Instance.playersManager.propsContainer);

            obj.GetComponent<GrabbableObject>().fallTime = 0f;
            obj.GetComponent<NetworkObject>().Spawn();

            int ammoAmount = UnityEngine.Random.Range(2, 6);
            for (int i = 0; i < ammoAmount; i++)
            {
                GameObject ammoObj = UnityEngine.Object.Instantiate(ammo.spawnPrefab,
                                   GameNetworkManager.Instance.localPlayerController.transform.position,
                                   Quaternion.identity,
                                   RoundManager.Instance.playersManager.propsContainer);

                ammoObj.GetComponent<GrabbableObject>().fallTime = 0f;
                ammoObj.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
