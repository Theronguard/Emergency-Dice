using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Unity.Netcode;
using System.IO;
using LethalLib.Modules;
using System.Collections.Generic;
using MysteryDice.Effects;
using System.Linq;
using MysteryDice.Patches;
using System.Collections;
using DunGen;
using MysteryDice.Visual;

namespace MysteryDice
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class MysteryDice : BaseUnityPlugin
    {
        private const string modGUID = "Theronguard.EmergencyDice";
        private const string modName = "Emergency Dice";
        private const string modVersion = "1.1.3";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource CustomLogger;
        public static AssetBundle LoadedAssets;

        public static GameObject NetworkerPrefab, JumpscareCanvasPrefab, JumpscareOBJ;
        public static Jumpscare JumpscareScript;

        public static AudioClip ExplosionSFX,DetonateSFX, MineSFX, AwfulEffectSFX, BadEffectSFX, GoodEffectSFX, JumpscareSFX, AlarmSFX;
        public static Sprite WarningBracken, WarningJester, WarningDeath, WarningLuck;

        public static Item DebugEmergencyDie, DebugGamblerDie;
        void Awake()
        {
            NetcodeWeaver();
            
            CustomLogger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            LoadedAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mysterydice"));

            ExplosionSFX = LoadedAssets.LoadAsset<AudioClip>("MineDetonate");
            MineSFX = LoadedAssets.LoadAsset<AudioClip>("MineTrigger");
            AwfulEffectSFX = LoadedAssets.LoadAsset<AudioClip>("Bell2");
            BadEffectSFX = LoadedAssets.LoadAsset<AudioClip>("Bad1");
            GoodEffectSFX = LoadedAssets.LoadAsset<AudioClip>("Good2");
            JumpscareSFX = LoadedAssets.LoadAsset<AudioClip>("glitch");
            AlarmSFX = LoadedAssets.LoadAsset<AudioClip>("alarmcurse");

            WarningBracken = LoadedAssets.LoadAsset<Sprite>("bracken");
            WarningJester = LoadedAssets.LoadAsset<Sprite>("jester");
            WarningDeath = LoadedAssets.LoadAsset<Sprite>("death");
            WarningLuck = LoadedAssets.LoadAsset<Sprite>("luck");

            NetworkerPrefab = LoadedAssets.LoadAsset<GameObject>("Networker");
            NetworkerPrefab.AddComponent<Networker>();

            JumpscareCanvasPrefab = LoadedAssets.LoadAsset<GameObject>("JumpscareCanvas");
            JumpscareCanvasPrefab.AddComponent<Jumpscare>();

            Item mysteryDie = LoadedAssets.LoadAsset<Item>("MysteryDiceItem");
            mysteryDie.minValue = 100;
            mysteryDie.maxValue = 130;

            GamblerDie scriptMystery = mysteryDie.spawnPrefab.AddComponent<GamblerDie>();
            scriptMystery.grabbable = true;
            scriptMystery.grabbableToEnemies = true;
            scriptMystery.itemProperties = mysteryDie;

            GameObject dieMysteryModel = mysteryDie.spawnPrefab.transform.Find("Model").gameObject;
            dieMysteryModel.AddComponent<Spinner>();
            dieMysteryModel.AddComponent<CycleSigns>();

            DebugGamblerDie = mysteryDie;

            Item emergencyDie = LoadedAssets.LoadAsset<Item>("Emergency Dice Script");
            emergencyDie.highestSalePercentage = 80;
            EmergencyDie scriptEmergency = emergencyDie.spawnPrefab.AddComponent<EmergencyDie>();
            scriptEmergency.grabbable = true;
            scriptEmergency.grabbableToEnemies = true;
            scriptEmergency.itemProperties = emergencyDie;

            DebugEmergencyDie = emergencyDie;

            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "This handy, unstable device might be your last chance to save yourself.\n\n" +
                "Rolls a number from 1 to 6:\n" +
                "-Rolling 6 teleports you and players standing closely near you to the ship with all your items.\n" +
                "-Rolling 4 or 5 teleports you to the ship with all your items.\n" +
                "-Rolling 3 might be bad, or might be good. You decide? \n" +
                "-Rolling 2 will causes some problems\n" +
                "-You dont want to roll a 1\n";

            Items.RegisterShopItem(emergencyDie, null, null, node, 200);

            GameObject dieModel = emergencyDie.spawnPrefab.transform.Find("Model").gameObject;
            dieModel.AddComponent<Spinner>();
            dieModel.AddComponent<Blinking>();

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(mysteryDie.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(emergencyDie.spawnPrefab);

            Utilities.FixMixerGroups(mysteryDie.spawnPrefab); //fixes audio
            Items.RegisterScrap(mysteryDie, 13, Levels.LevelTypes.ExperimentationLevel | Levels.LevelTypes.AssuranceLevel);
            Items.RegisterScrap(mysteryDie, 15, Levels.LevelTypes.VowLevel);
            Items.RegisterScrap(mysteryDie, 17, Levels.LevelTypes.OffenseLevel | Levels.LevelTypes.MarchLevel);
            Items.RegisterScrap(mysteryDie, 25, Levels.LevelTypes.RendLevel);
            Items.RegisterScrap(mysteryDie, 28, Levels.LevelTypes.DineLevel);
            Items.RegisterScrap(mysteryDie, 23, Levels.LevelTypes.TitanLevel);

            harmony.PatchAll();
            CustomLogger.LogInfo("The Emergency Dice mod was initialized!");
        }
        private static void NetcodeWeaver()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
        public abstract class DieBehaviour : PhysicsProp
        {
            protected GameObject DiceModel;
            public List<IEffect> Effects = new List<IEffect>();
            public Dictionary<int, EffectType[]> RollToEffect = new Dictionary<int, EffectType[]>();

            public virtual void SetupDiceEffects()
            {
                Effects.Add(new SilentMine());
                Effects.Add(new ZombieToShip());
                Effects.Add(new InvertDoorLock());
                Effects.Add(new AlarmCurse());
                Effects.Add(new JumpscareGlitch());
                Effects.Add(new Armageddon());
                Effects.Add(new Beepocalypse());
                Effects.Add(new RebeliousCoilHeads());
                Effects.Add(new TurnOffLights());
                Effects.Add(new HealAndRestore());
                Effects.Add(new ScrapJackpot());
                Effects.Add(new Swap());
                Effects.Add(new ModifyPitch());
                Effects.Add(new MineOverflow());
                Effects.Add(new MineOverflowOutside());
                Effects.Add(new InstaJester());
                Effects.Add(new FakeFireExits());
                Effects.Add(new FireExitBlock());
                Effects.Add(new ReturnToShip());
                Effects.Add(new TeleportInside());
                Effects.Add(new BugPlague());
                Effects.Add(new ZombieApocalypse());
                Effects.Add(new Revive());
                Effects.Add(new Detonate());
                //Effects.Add(new OutsideBracken()); to fix
            }
            public virtual void SetupRollToEffectMapping()
            {
                RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
                RollToEffect.Add(2, new EffectType[] { EffectType.Bad });
                RollToEffect.Add(3, new EffectType[] { EffectType.Bad });
                RollToEffect.Add(4, new EffectType[] { EffectType.Good });
                RollToEffect.Add(5, new EffectType[] { EffectType.Good });
                RollToEffect.Add(6, new EffectType[] { EffectType.Great });
            }

            public override void Start()
            {
                base.Start();
                DiceModel = gameObject.transform.Find("Model").gameObject;
                SetupDiceEffects();
                SetupRollToEffectMapping();
            }

            public override void PocketItem()
            {
                DiceModel.SetActive(false);
                base.PocketItem();
                
            }
            public override void EquipItem()
            {
                DiceModel.SetActive(true);
                base.EquipItem();
            }

            public override void OnHitGround()
            {
                base.OnHitGround();
                DiceModel.SetActive(true);
            }

            public override void ItemActivate(bool used, bool buttonDown = true)
            {
                base.ItemActivate(used, buttonDown);
                if (buttonDown)
                {
                    if (StartOfRound.Instance == null) return;
                    if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;
                   
                    ulong dropperID = playerHeldBy.playerClientId;
                    GameNetworkManager.Instance.localPlayerController.DiscardHeldObject(true, null, GetItemFloorPosition(DiceModel.transform.parent.position),false);
                    SyncDropServerRPC(dropperID);
                }
            }

            public virtual IEnumerator UseTimer(ulong userID)
            {
                DiceModel.GetComponent<Spinner>().StartHyperSpinning(3f);

                yield return new WaitForSeconds(3f);

                Landmine.SpawnExplosion(gameObject.transform.position, true, 0, 0);
                DestroyObject();

                if (GameNetworkManager.Instance.localPlayerController.playerClientId == userID)
                    Roll();
            }

            [ServerRpc(RequireOwnership = false)]
            public virtual void SyncDropServerRPC(ulong userID)
            {
                if(!IsHost)
                    DropAndBlock(userID);

                SyncDropClientRPC(userID);
            }
            [ClientRpc]
            public virtual void SyncDropClientRPC(ulong userID)
            {
                DropAndBlock(userID);
            }

            public virtual void DropAndBlock(ulong userID)
            {
                grabbable = false;
                grabbableToEnemies = false;
                DiceModel.SetActive(true);
                StartCoroutine(UseTimer(userID));
            }
            public virtual void DestroyObject()
            {
                grabbable = false;
                grabbableToEnemies = false;
                deactivated = true;
                if (radarIcon != null)
                {
                    Object.Destroy(radarIcon.gameObject);
                }
                MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    Object.Destroy(componentsInChildren[i]);
                }
                Collider[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<Collider>();
                for (int j = 0; j < componentsInChildren2.Length; j++)
                {
                    Object.Destroy(componentsInChildren2[j]);
                }
            }
            public virtual void Roll()
            {
                if (StartOfRound.Instance == null) return;
                if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

                if (StartOfRound.Instance.currentLevel.PlanetName == "71 Gordion")
                {
                    HUDManager.Instance.DisplayTip($"Company penalty", "Do not try this again.");
                    (new Detonate()).Use();
                    return;
                }

                int diceRoll = UnityEngine.Random.Range(1, 7);

                IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

                if (randomEffect == null) return;

                PlaySoundBasedOnEffect(randomEffect.Outcome);
                randomEffect.Use();

                if (randomEffect.ShowDefaultTooltip)
                    ShowDefaultTooltip(randomEffect.Outcome, diceRoll);
                else
                    HUDManager.Instance.DisplayTip($"Rolled {diceRoll}", randomEffect.Tooltip);
            }
            public IEffect GetRandomEffect(int diceRoll, List<IEffect> effects)
            {
                List<IEffect> rolledEffects = new List<IEffect>();

                foreach (IEffect effect in Effects)
                    if (RollToEffect[diceRoll].Contains(effect.Outcome))
                        rolledEffects.Add(effect);

                if (rolledEffects.Count == 0) return null;
                int randomEffectID = Random.Range(0, rolledEffects.Count);
                return rolledEffects[randomEffectID];
            }

            public void PlaySoundBasedOnEffect(EffectType effectType)
            {
                switch (effectType)
                {
                    case EffectType.Awful:
                        AudioSource.PlayClipAtPoint(AwfulEffectSFX, GameNetworkManager.Instance.localPlayerController.transform.position);
                        break;
                    case EffectType.Bad:
                        AudioSource.PlayClipAtPoint(BadEffectSFX, GameNetworkManager.Instance.localPlayerController.transform.position);
                        break;
                    default:
                        AudioSource.PlayClipAtPoint(GoodEffectSFX, GameNetworkManager.Instance.localPlayerController.transform.position);
                        break;
                }
                    
            }
        }

        public class EmergencyDie : DieBehaviour
        {
            public override void SetupRollToEffectMapping()
            {
                RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
                RollToEffect.Add(2, new EffectType[] { EffectType.Bad });
                RollToEffect.Add(3, new EffectType[] { EffectType.Mixed });
                RollToEffect.Add(4, new EffectType[] { EffectType.Good });
                RollToEffect.Add(5, new EffectType[] { EffectType.Good });
                RollToEffect.Add(6, new EffectType[] { EffectType.Great });
            }

            public override IEnumerator UseTimer(ulong userID)
            {
                DiceModel.GetComponent<Blinking>().BlinkingTime = 0.1f;
                return base.UseTimer(userID);
            }

            public override void DestroyObject()
            {
                DiceModel.GetComponent<Blinking>().HideSigns();
                base.DestroyObject();
            }

            public override void Roll()
            {
                if (StartOfRound.Instance == null) return;
                if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

                if (StartOfRound.Instance.currentLevel.PlanetName == "71 Gordion")
                {
                    HUDManager.Instance.DisplayTip($"Company penalty", "Do not try this again.");
                    (new Detonate()).Use();
                    return;
                }

                int diceRoll = UnityEngine.Random.Range(1, 7);

                IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

                if (randomEffect == null) return;

                PlaySoundBasedOnEffect(randomEffect.Outcome);

                if (diceRoll > 3) randomEffect = new ReturnToShip();
                if (diceRoll == 6) randomEffect = new ReturnToShipTogether();

                randomEffect.Use();

                if (randomEffect.ShowDefaultTooltip)
                    ShowDefaultTooltip(randomEffect.Outcome,diceRoll);
                else
                    HUDManager.Instance.DisplayTip($"Rolled {diceRoll}", randomEffect.Tooltip);
            }
        }
        public class GamblerDie : DieBehaviour
        {
            public override void SetupRollToEffectMapping()
            {
                RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
                RollToEffect.Add(2, new EffectType[] { EffectType.Awful, EffectType.Bad });
                RollToEffect.Add(3, new EffectType[] { EffectType.Bad });
                RollToEffect.Add(4, new EffectType[] { EffectType.Mixed, EffectType.Good });
                RollToEffect.Add(5, new EffectType[] { EffectType.Good });
                RollToEffect.Add(6, new EffectType[] { EffectType.Great });
            }

            public override IEnumerator UseTimer(ulong userID)
            {
                DiceModel.GetComponent<CycleSigns>().CycleTime = 0.1f;
                return base.UseTimer(userID);
            }
            public override void DestroyObject()
            {
                base.DiceModel.GetComponent<CycleSigns>().HideSigns();
                base.DestroyObject();
            }
        }

        public static void ShowDefaultTooltip(EffectType effectType,int diceRoll)
        {
            switch (effectType)
            {
                case EffectType.Awful:
                    HUDManager.Instance.DisplayTip($"Rolled {diceRoll}", ":)");
                    break;
                case EffectType.Bad:
                    HUDManager.Instance.DisplayTip($"Rolled {diceRoll}", "Uh oh");
                    break;
                case EffectType.Good:
                    HUDManager.Instance.DisplayTip($"Rolled {diceRoll}", "Enjoy.");
                    break;
                case EffectType.Great:
                    HUDManager.Instance.DisplayTip($"Rolled {diceRoll}", "Lucky.");
                    break;
                case EffectType.Mixed:
                    HUDManager.Instance.DisplayTip($"Rolled {diceRoll}", "Debatable");
                    break;
            }
        }
    }
}
