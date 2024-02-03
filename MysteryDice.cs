using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.IO;
using LethalLib.Modules;
using MysteryDice.Effects;
using MysteryDice.Visual;
using MysteryDice.Dice;
using System;
using BepInEx.Configuration;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;

namespace MysteryDice
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class MysteryDice : BaseUnityPlugin
    {
        private const string modGUID = "Theronguard.EmergencyDice";
        private const string modName = "Emergency Dice";
        private const string modVersion = "1.1.5";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource CustomLogger;
        public static AssetBundle LoadedAssets;

        public static GameObject NetworkerPrefab, JumpscareCanvasPrefab, JumpscareOBJ, PathfinderPrefab;
        public static Jumpscare JumpscareScript;

        public static AudioClip ExplosionSFX, DetonateSFX, MineSFX, AwfulEffectSFX, BadEffectSFX, GoodEffectSFX, JumpscareSFX, AlarmSFX;
        public static Sprite WarningBracken, WarningJester, WarningDeath, WarningLuck;

        public static Item DebugEmergencyDie, DebugGamblerDie, DebugChronosDie, DebugSacrificerDie, PathfinderSpawner;
        public static Config EffectConfig;
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

            PathfinderPrefab = LoadedAssets.LoadAsset<GameObject>("Pathfinder");
            PathfinderPrefab.AddComponent<Pathfinder.PathfindBehaviour>();
            LoadEffectConfig();
            Item mysteryDie = LoadedAssets.LoadAsset<Item>("MysteryDiceItem");
            mysteryDie.minValue = 100;
            mysteryDie.maxValue = 130;

            GamblerDie scriptMystery = mysteryDie.spawnPrefab.AddComponent<GamblerDie>();
            scriptMystery.grabbable = true;
            scriptMystery.grabbableToEnemies = true;
            scriptMystery.itemProperties = mysteryDie;

            DebugGamblerDie = mysteryDie;

            Item emergencyDie = LoadedAssets.LoadAsset<Item>("Emergency Dice Script");
            emergencyDie.highestSalePercentage = 80;

            EmergencyDie scriptEmergency = emergencyDie.spawnPrefab.AddComponent<EmergencyDie>();
            scriptEmergency.grabbable = true;
            scriptEmergency.grabbableToEnemies = true;
            scriptEmergency.itemProperties = emergencyDie;

            DebugEmergencyDie = emergencyDie;

            Item chronosDie = LoadedAssets.LoadAsset<Item>("Chronos");
            chronosDie.minValue = 150;
            chronosDie.maxValue = 200;

            ChronosDie scriptChronos = chronosDie.spawnPrefab.AddComponent<ChronosDie>();
            scriptChronos.grabbable = true;
            scriptChronos.grabbableToEnemies = true;
            scriptChronos.itemProperties = chronosDie;

            DebugChronosDie = chronosDie;

            Item sacrificerDie = LoadedAssets.LoadAsset<Item>("Sacrificer");
            sacrificerDie.minValue = 170;
            sacrificerDie.maxValue = 230;

            SacrificerDie scriptSacrificer = sacrificerDie.spawnPrefab.AddComponent<SacrificerDie>();
            scriptSacrificer.grabbable = true;
            scriptSacrificer.grabbableToEnemies = true;
            scriptSacrificer.itemProperties = sacrificerDie;

            DebugSacrificerDie = sacrificerDie;

            PathfinderSpawner = LoadedAssets.LoadAsset<Item>("Pathblob");



            Pathfinder.BlobspawnerBehaviour scriptBlobspawner = PathfinderSpawner.spawnPrefab.AddComponent<Pathfinder.BlobspawnerBehaviour>();
            scriptBlobspawner.grabbable = true;
            scriptBlobspawner.grabbableToEnemies = true;
            scriptBlobspawner.itemProperties = PathfinderSpawner;

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

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(mysteryDie.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(emergencyDie.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(chronosDie.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(sacrificerDie.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(PathfinderPrefab);

            Utilities.FixMixerGroups(mysteryDie.spawnPrefab); //fixes audio
            Utilities.FixMixerGroups(emergencyDie.spawnPrefab);
            Utilities.FixMixerGroups(chronosDie.spawnPrefab);
            Utilities.FixMixerGroups(sacrificerDie.spawnPrefab);

            Utilities.FixMixerGroups(mysteryDie.spawnPrefab);
            Items.RegisterScrap(mysteryDie, 13, Levels.LevelTypes.ExperimentationLevel | Levels.LevelTypes.AssuranceLevel);
            Items.RegisterScrap(mysteryDie, 15, Levels.LevelTypes.VowLevel);
            Items.RegisterScrap(mysteryDie, 17, Levels.LevelTypes.OffenseLevel | Levels.LevelTypes.MarchLevel);
            Items.RegisterScrap(mysteryDie, 33, Levels.LevelTypes.RendLevel);
            Items.RegisterScrap(mysteryDie, 43, Levels.LevelTypes.DineLevel);
            Items.RegisterScrap(mysteryDie, 30, Levels.LevelTypes.TitanLevel);

            Items.RegisterScrap(chronosDie, 17, Levels.LevelTypes.ExperimentationLevel | Levels.LevelTypes.AssuranceLevel);
            Items.RegisterScrap(chronosDie, 17, Levels.LevelTypes.VowLevel);
            Items.RegisterScrap(chronosDie, 25, Levels.LevelTypes.OffenseLevel | Levels.LevelTypes.MarchLevel);
            Items.RegisterScrap(chronosDie, 22, Levels.LevelTypes.RendLevel);
            Items.RegisterScrap(chronosDie, 20, Levels.LevelTypes.DineLevel);
            Items.RegisterScrap(chronosDie, 33, Levels.LevelTypes.TitanLevel);

            Items.RegisterScrap(sacrificerDie, 20, Levels.LevelTypes.ExperimentationLevel | Levels.LevelTypes.AssuranceLevel);
            Items.RegisterScrap(sacrificerDie, 20, Levels.LevelTypes.VowLevel);
            Items.RegisterScrap(sacrificerDie, 20, Levels.LevelTypes.OffenseLevel | Levels.LevelTypes.MarchLevel);
            Items.RegisterScrap(sacrificerDie, 35, Levels.LevelTypes.RendLevel);
            Items.RegisterScrap(sacrificerDie, 20, Levels.LevelTypes.DineLevel);
            Items.RegisterScrap(sacrificerDie, 23, Levels.LevelTypes.TitanLevel);

            harmony.PatchAll();
            CustomLogger.LogInfo("The Emergency Dice mod was initialized!");
        }

        public void LoadEffectConfig()
        {
            List<IEffect> Effects = new List<IEffect>();
            Effects.Add(new InfiniteStaminaAll());
            Effects.Add(new InfiniteStamina());
            Effects.Add(new Pathfinder());
            Effects.Add(new Shotgun());
            Effects.Add(new ShipTurret());
            Effects.Add(new TurretHell());
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

            EffectConfig = new Config(base.Config, Effects);

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
    }

    public class Config
    {
        List<IEffectConfig> ConfigEntries = new List<IEffectConfig>();
        public Config(ConfigFile cfg, List<IEffect> effects)
        {
            foreach (IEffect effect in effects)
            {
                ConfigEntries.Add(new IEffectConfig()
                {
                    effect = effect,
                    configEntry = cfg.Bind(
                        "Effects",
                        effect.Name,
                        true,
                        effect.Tooltip
                        )
                });
            }
        }

        public List<IEffect> getEnabledEffects()
        {
            return ConfigEntries.Where(e => e.configEntry.Value).Select(e=> e.effect).ToList();
        }
        
    }

    public class IEffectConfig
    {
        public IEffect effect;
        public ConfigEntry<bool> configEntry;
    }


}
