using GameNetcodeStuff;
using LethalLib.Modules;
using MysteryDice.Effects;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice
{
    internal class Networker : NetworkBehaviour
    {
        public static Networker Instance;
        public static float RebelTimer = 0f;
        public static bool CoilheadIgnoreStares = false;
        public override void OnNetworkSpawn()
        {
            Instance = this;
            base.OnNetworkSpawn();
        }
        public override void OnNetworkDespawn()
        {
            StartOfRoundPatch.ResetSettingsShared();
            base.OnNetworkDespawn();
        }
        void FixedUpdate()
        {
            UpdateMineTimers();
            Armageddon.BoomTimer();
        }
        void Update()
        {
            ModifyPitch.PitchFluctuate();
            RebelCoilheads();
            AlarmCurse.TimerUpdate();
        }

        #region Detonate
        private static Vector2 TimerRange = new Vector2(3f, 6f);
        private static ulong PlayerIDToExplode;
        private static float ExplosionTimer = 0f;

        public static bool IsPlayerAliveAndControlled(PlayerControllerB player)
        {
            return !player.isPlayerDead &&
                    player.isActiveAndEnabled &&
                    player.IsSpawned &&
                    player.isPlayerControlled;
        }

        public static bool IsPlayerAlive(PlayerControllerB player)
        {
            return !player.isPlayerDead &&
                    player.isActiveAndEnabled &&
                    player.IsSpawned;
        }

        public void UpdateMineTimers()
        {
            if (ExplosionTimer >= 0f)
            {
                ExplosionTimer -= Time.fixedDeltaTime;

                if (ExplosionTimer < 0f)
                    DetonatePlayerClientRpc(PlayerIDToExplode);
            }
        }
        public void StartDoomCountdown(ulong playerID)
        {
            PlayerIDToExplode = playerID;
            ExplosionTimer = UnityEngine.Random.Range(TimerRange.x, TimerRange.y);
        }

        [ClientRpc]
        public void OnStartRoundClientRPC()
        {
            StartOfRoundPatch.StartGameOnClient();
        }

        [ServerRpc(RequireOwnership = false)]
        public void DetonateRandomPlayerServerRpc()
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            List<PlayerControllerB> validPlayers = new List<PlayerControllerB>();

            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();
                if (IsPlayerAliveAndControlled(player))
                    validPlayers.Add(player);
            }

            PlayerControllerB theUnluckyOne = validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)];
            StartDoomCountdown(theUnluckyOne.playerClientId);
        }

        [ClientRpc]
        public void DetonatePlayerClientRpc(ulong clientID)
        {
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();
                if (player.playerClientId == clientID &&
                    IsPlayerAliveAndControlled(player))
                {
                    AudioSource.PlayClipAtPoint(MysteryDice.MineSFX, player.transform.position);
                    StartCoroutine(SpawnExplosionAfterSFX(player.transform.position));
                    break;
                }
            }
        }

        IEnumerator SpawnExplosionAfterSFX(Vector3 position)
        {
            yield return new WaitForSeconds(0.5f);
            Landmine.SpawnExplosion(position, true, 1, 5);
        }
        #endregion

        #region Revive
        [ServerRpc(RequireOwnership = false)]
        public void ReviveAllPlayersServerRpc()
        {
            if (StartOfRound.Instance == null) return;

            ReviveAllPlayersClientRpc();
        }
        [ClientRpc]
        public void ReviveAllPlayersClientRpc()
        {
            if (StartOfRound.Instance == null) return;

            StartOfRound.Instance.ReviveDeadPlayers();
        }
        #endregion

        #region TeleportInside

        [ServerRpc(RequireOwnership = false)]
        public void TeleportInsideServerRPC(ulong clientID, Vector3 teleportPos)
        {
            TeleportInsideClientRPC(clientID, teleportPos);
        }

        [ClientRpc]
        public void TeleportInsideClientRPC(ulong clientID, Vector3 teleportPos)
        {
            TeleportInside.TeleportPlayerInside(clientID, teleportPos);
        }
        #endregion

        #region TeleportToShip

        [ServerRpc(RequireOwnership = false)]
        public void TeleportToShipServerRPC(ulong clientID)
        {
            TeleportToShipClientRPC(clientID);
        }

        [ClientRpc]
        public void TeleportToShipClientRPC(ulong clientID)
        {
            ReturnToShip.TeleportPlayerToShip(clientID);
        }
        #endregion

        #region FireExitBlock

        [ServerRpc(RequireOwnership = false)]
        public void BlockFireExitsServerRPC()
        {
            BlockFireExitsClientRPC();
        }

        [ClientRpc]
        public void BlockFireExitsClientRPC()
        {
            FireExitPatch.AreFireExitsBlocked = true;
        }
        #endregion

        #region FakeFireExits

        [ServerRpc(RequireOwnership = false)]
        public void FakeFireExitsServerRPC()
        {
            FakeFireExitsClientRPC();
        }

        [ClientRpc]
        public void FakeFireExitsClientRPC()
        {
            //this is a bit inefficient
            GameObject[] potentialFireExitSlots = GameObject.FindObjectsOfType<GameObject>(true);
            for(int i =0; i < potentialFireExitSlots.Length; i++)
            {
                if (potentialFireExitSlots[i].name.Contains("AlleyExitDoorContainer"))
                    potentialFireExitSlots[i].SetActive(true);
            }
        }
        #endregion

        #region InstaJester
        [ServerRpc(RequireOwnership = false)]
        public void InstaJesterServerRPC()
        {
            InstaJester.SpawnInstaJester();
        }
        #endregion

        #region OutsideBracken
        [ServerRpc(RequireOwnership = false)]
        public void OutsideBrackenServerRPC()
        {
            OutsideBracken.SpawnOutsideBracken();
        }

        [ClientRpc]
        public void SetNavmeshBrackenClientRPC()
        {
            OutsideBracken.SetNavmeshBrackenClient();
        }
        #endregion

        #region MineOverflow
        [ServerRpc(RequireOwnership = false)]
        public void MineOverflowServerRPC()
        {
            MineOverflow.SpawnMoreMines();
        }
        #endregion

        #region MineOverflowOutside
        [ServerRpc(RequireOwnership = false)]
        public void MineOverflowOutsideServerRPC()
        {
            MineOverflowOutside.SpawnMoreMinesOutside();
        }
        #endregion

        #region ModifyPitch
        [ServerRpc(RequireOwnership = false)]
        public void ModifyPitchNotifyServerRPC()
        {
            ModifyPitchNotifyClientRPC();
        }

        [ClientRpc]
        public void ModifyPitchNotifyClientRPC()
        {
            ModifyPitch.FluctuatePitch = true;
        }
        #endregion

        #region Swap
        [ServerRpc(RequireOwnership = false)]
        public void SwapPlayersServerRPC(ulong userID)
        {
            List<PlayerControllerB> validPlayers = new List<PlayerControllerB>();
            PlayerControllerB callingPlayer = null;

            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();

                if (IsPlayerAliveAndControlled(player) && player.playerClientId != userID)
                    validPlayers.Add(player);
                if (IsPlayerAliveAndControlled(player) && player.playerClientId == userID)
                    callingPlayer = player;
            }

            if (callingPlayer == null ||
                validPlayers.Count == 0)
                return;

            ulong randomPlayer = validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)].playerClientId;
            Swap.SwapPlayers(callingPlayer.playerClientId, randomPlayer);
            SwapPlayerClientRPC(callingPlayer.playerClientId, randomPlayer);
        }

        [ClientRpc]
        public void SwapPlayerClientRPC(ulong userID, ulong otherUserID)
        {
            if (IsServer) return;
            Swap.SwapPlayers(userID, otherUserID);
        }
        #endregion

        #region ScrapJackpot
        [ServerRpc(RequireOwnership = false)]
        public void JackpotServerRPC(ulong userID)
        {
            ScrapJackpot.JackpotScrap(userID);
        }

        [ClientRpc]
        public void SyncItemWeightsClientRPC(NetworkObjectReference[] netObjs, float[] scrapWeights)
        {
            for(int i=0;i<netObjs.Length; i++)
            {
                if (netObjs[i].TryGet(out var networkObject))
                {
                    GrabbableObject component = networkObject.GetComponent<GrabbableObject>();
                    if (component == null) return;
                    component.itemProperties.weight = scrapWeights[i];
                }
            }
        }
        #endregion

        #region HealAndRestore
        [ServerRpc(RequireOwnership = false)]
        public void HealAllServerRPC()
        {
            HealAllClientRPC();
        }

        [ClientRpc]
        public void HealAllClientRPC()
        {
            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();

                if (player == null) continue;
                if (!IsPlayerAliveAndControlled(player)) continue;

                Heal(player);
                HUDManager.Instance.UpdateHealthUI(player.health,false);

                foreach (var item in player.ItemSlots)
                {
                    if (item == null) continue;
                    if (item.insertedBattery == null) continue;

                    item.insertedBattery.charge = 1f;
                    item.insertedBattery.empty = false;
                }
            }
        }

        public static void Heal(PlayerControllerB player)
        {
            player.bleedingHeavily = false;
            player.criticallyInjured = false;
            player.health = 100;
        }
        #endregion

        #region TurnOffLights

        [ServerRpc(RequireOwnership =false)]
        public void TurnOffAllLightsServerRPC()
        {
            TurnOffAllLightsClientRPC();
        }

        [ClientRpc]
        public void TurnOffAllLightsClientRPC()
        {
            RoundManager.Instance.TurnOnAllLights(false);
            BreakerBox breakerBox = UnityEngine.Object.FindObjectOfType<BreakerBox>();
            if (breakerBox != null)
                breakerBox.gameObject.SetActive(false);
        }
        #endregion

        #region CoilheadRebel

        void RebelCoilheads()
        {
            if (!RebeliousCoilHeads.IsEnabled)
            {
                CoilheadIgnoreStares = false;
                return;
            }

            RebelTimer -= Time.deltaTime;
            if (RebelTimer <= 0f)
            {
                CoilheadIgnoreStares = !CoilheadIgnoreStares;
                if (CoilheadIgnoreStares)
                {
                    RebelTimer = UnityEngine.Random.Range(2f, 3f);
                }
                else
                {
                    RebelTimer = UnityEngine.Random.Range(12f, 20f);
                }
            }
        }

        [ServerRpc(RequireOwnership =false)]
        public void EnableRebelServerRPC()
        {
            RebeliousCoilHeads.IsEnabled = true;
            Misc.SpawnEnemyForced(GetEnemies.Coilhead, 1, true);
        }

        #endregion

        #region TeleportToShipTogether

        [ServerRpc(RequireOwnership = false)]
        public void ReturnPlayerToShipServerRPC(ulong clientID)
        {
            TeleportToShipClientRPC(clientID);
        }
        #endregion

        #region Beepocalypse

        [ServerRpc(RequireOwnership = false)]
        public void SpawnBeehivesServerRPC()
        {
            Beepocalypse.SpawnBeehives();
        }

        [ClientRpc]
        public void ZeroOutBeehiveScrapClientRPC()
        {
            StartCoroutine(ZeroScrapDelay());
        }

        public IEnumerator ZeroScrapDelay()
        {
            yield return new WaitForSeconds(4f);
            Beepocalypse.ZeroAllBeehiveScrap();
        }
        #endregion

        #region Wormageddon

        [ServerRpc(RequireOwnership = false)]
        public void SpawnWormssServerRPC()
        {
            Wormageddon.SpawnWorms();
        }
        #endregion

        #region Armageddon

        [ServerRpc(RequireOwnership = false)]
        public void SetArmageddonServerRPC(bool enable)
        {
            Armageddon.IsEnabled = enable;
        }

        [ClientRpc]
        public void DetonateAtPosClientRPC(Vector3 position)
        {
            Landmine.SpawnExplosion(position, true, 1, 5);
        }
        #endregion

        #region Jumpscare

        [ServerRpc(RequireOwnership = false)]
        public void JumpscareAllServerRPC()
        {
            JumpscareAllClientRPC();
        }

        [ClientRpc]
        public void JumpscareAllClientRPC()
        {
            MysteryDice.JumpscareScript.Scare();
        }

        public IEnumerator DelayJumpscare()
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(15f, 111f));
            JumpscareAllServerRPC();
        }
        #endregion


        #region AlarmCurse

        [ServerRpc(RequireOwnership=false)]
        public void AlarmCurseServerRPC(Vector3 position)
        {
            AlarmCurse.AlarmAudio(position);
            AlarmCurseClientRPC(position);
        }

        [ClientRpc]
        public void AlarmCurseClientRPC(Vector3 position)
        {
            if (IsServer) return;
            AlarmCurse.AlarmAudio(position);
        }

        #endregion

        #region DoorLock

        [ServerRpc(RequireOwnership = false)]
        public void DoorlockServerRPC()
        {
            InvertDoorLock.InvertDoors();
            DoorlockClientRPC();
        }

        [ClientRpc]
        public void DoorlockClientRPC()
        {
            if (IsServer) return;
            InvertDoorLock.InvertDoors();
        }

        #endregion

        #region ZombieToShip

        [ServerRpc(RequireOwnership=false)]
        public void ZombieToShipServerRPC(ulong userID)
        {
            ZombieToShip.ZombieUseServer(userID);
        }

        [ClientRpc]
        public void ZombieSuitClientRPC(NetworkObjectReference netObj,int suitID)
        {
            ZombieToShip.ZombieSetSuit(netObj, suitID);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestToSyncSuitIDServerRPC(NetworkObjectReference zombieNet)
        {
            ZombieToShip.ZombieSyncData(zombieNet);
        }

        [ClientRpc]
        public void SyncSuitIDClientRPC(NetworkObjectReference zombieNet, int zombieSuitID)
        {
            ZombieToShip.ZombieSetSuit(zombieNet,zombieSuitID);
        }
        #endregion
    }
}
