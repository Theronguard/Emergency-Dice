﻿using BepInEx;
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
using MysteryDice.Patches;
using System.Collections.Generic;

namespace MysteryDice
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class MysteryDice : BaseUnityPlugin
    {
        private const string modGUID = "Theronguard.EmergencyDice";
        private const string modName = "Emergency Dice";
        private const string modVersion = "1.1.15";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource CustomLogger;
        public static AssetBundle LoadedAssets;

        public static GameObject NetworkerPrefab, JumpscareCanvasPrefab, JumpscareOBJ, PathfinderPrefab;
        public static Jumpscare JumpscareScript;

        public static AudioClip ExplosionSFX, DetonateSFX, MineSFX, AwfulEffectSFX, BadEffectSFX, GoodEffectSFX, JumpscareSFX, AlarmSFX, PurrSFX;
        public static Sprite WarningBracken, WarningJester, WarningDeath, WarningLuck;

        public static Item DieEmergency, DieGambler, DieChronos, DieSacrificer, DieSaint, DieRusty, PathfinderSpawner;

        public static ConfigFile BepInExConfig = null;
        void Awake()
        {
            CustomLogger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            BepInExConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "Emergency Dice.cfg"), true);

            ModConfig();
            DieBehaviour.Config();

            NetcodeWeaver();

            LoadedAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mysterydice"));

            ExplosionSFX = LoadedAssets.LoadAsset<AudioClip>("MineDetonate");
            MineSFX = LoadedAssets.LoadAsset<AudioClip>("MineTrigger");
            AwfulEffectSFX = LoadedAssets.LoadAsset<AudioClip>("Bell2");
            BadEffectSFX = LoadedAssets.LoadAsset<AudioClip>("Bad1");
            GoodEffectSFX = LoadedAssets.LoadAsset<AudioClip>("Good2");
            JumpscareSFX = LoadedAssets.LoadAsset<AudioClip>("glitch");
            PurrSFX = LoadedAssets.LoadAsset<AudioClip>("purr");
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

            PathfinderSpawner = LoadedAssets.LoadAsset<Item>("Pathblob");

            Pathfinder.BlobspawnerBehaviour scriptBlobspawner = PathfinderSpawner.spawnPrefab.AddComponent<Pathfinder.BlobspawnerBehaviour>();
            scriptBlobspawner.grabbable = true;
            scriptBlobspawner.grabbableToEnemies = true;
            scriptBlobspawner.itemProperties = PathfinderSpawner;

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(PathfinderSpawner.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(PathfinderPrefab);

            LoadDice();

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

        public static void ModConfig()
        {
            ConfigEntry<bool> pussyMode = BepInExConfig.Bind<bool>(
                "Clientside",
                "Pussy mode",
                true,
                "Changes the jumpscare effect to a less scary one");

            JumpscareGlitch.PussyMode = pussyMode.Value;


            ConfigEntry<bool> debugDice = BepInExConfig.Bind<bool>(
                "Admin",
                "Show effects in the console",
                false,
                "Shows what effect has been rolled by the dice in the console. For debug purposes.");

            DieBehaviour.LogEffectsToConsole = debugDice.Value;


            ConfigEntry<bool> allowChatCommands = BepInExConfig.Bind<bool>(
                "Admin",
                "Allow chat commands",
                false,
                "Enables chat commands for the admin. Mainly for debugging.");

            ChatPatch.AllowChatDebug = allowChatCommands.Value;

        }

        public static Dictionary<string, Levels.LevelTypes> RegLevels = new Dictionary<string, Levels.LevelTypes>
        {
            {Consts.Experimentation,Levels.LevelTypes.ExperimentationLevel},
            {Consts.Assurance,Levels.LevelTypes.AssuranceLevel},
            {Consts.Vow,Levels.LevelTypes.VowLevel},
            {Consts.Offense,Levels.LevelTypes.OffenseLevel},
            {Consts.March,Levels.LevelTypes.MarchLevel},
            {Consts.Rend,Levels.LevelTypes.RendLevel},
            {Consts.Dine,Levels.LevelTypes.DineLevel},
            {Consts.Titan,Levels.LevelTypes.TitanLevel}
        };

        public static List<Item> RegisteredDice = new List<Item>();

        public static void LoadDice()
        {
            Item DieGambler = LoadedAssets.LoadAsset<Item>("MysteryDiceItem");
            DieGambler.minValue = 100;
            DieGambler.maxValue = 130;

            GamblerDie scriptMystery = DieGambler.spawnPrefab.AddComponent<GamblerDie>();
            scriptMystery.grabbable = true;
            scriptMystery.grabbableToEnemies = true;
            scriptMystery.itemProperties = DieGambler;

            RegisteredDice.Add(DieGambler);

            ///

            Item DieEmergency = LoadedAssets.LoadAsset<Item>("Emergency Dice Script");
            DieEmergency.highestSalePercentage = 80;

            EmergencyDie scriptEmergency = DieEmergency.spawnPrefab.AddComponent<EmergencyDie>();
            scriptEmergency.grabbable = true;
            scriptEmergency.grabbableToEnemies = true;
            scriptEmergency.itemProperties = DieEmergency;

            RegisteredDice.Add(DieEmergency);

            ///

            Item DieChronos = LoadedAssets.LoadAsset<Item>("Chronos");
            DieChronos.minValue = 120;
            DieChronos.maxValue = 140;

            ChronosDie scriptChronos = DieChronos.spawnPrefab.AddComponent<ChronosDie>();
            scriptChronos.grabbable = true;
            scriptChronos.grabbableToEnemies = true;
            scriptChronos.itemProperties = DieChronos;

            RegisteredDice.Add(DieChronos);

            ///

            Item DieSacrificer = LoadedAssets.LoadAsset<Item>("Sacrificer");
            DieSacrificer.minValue = 170;
            DieSacrificer.maxValue = 230;

            SacrificerDie scriptSacrificer = DieSacrificer.spawnPrefab.AddComponent<SacrificerDie>();
            scriptSacrificer.grabbable = true;
            scriptSacrificer.grabbableToEnemies = true;
            scriptSacrificer.itemProperties = DieSacrificer;

            RegisteredDice.Add(DieSacrificer);

            ///

            Item DieSaint = LoadedAssets.LoadAsset<Item>("Saint");
            DieSaint.minValue = 210;
            DieSaint.maxValue = 280;

            SaintDie scriptSaint = DieSaint.spawnPrefab.AddComponent<SaintDie>();
            scriptSaint.grabbable = true;
            scriptSaint.grabbableToEnemies = true;
            scriptSaint.itemProperties = DieSaint;

            RegisteredDice.Add(DieSaint);

            ///

            Item DieRusty = LoadedAssets.LoadAsset<Item>("Rusty");
            DieRusty.minValue = 90;
            DieRusty.maxValue = 160;

            RustyDie scriptRusty = DieRusty.spawnPrefab.AddComponent<RustyDie>();
            scriptRusty.grabbable = true;
            scriptRusty.grabbableToEnemies = true;
            scriptRusty.itemProperties = DieRusty;

            RegisteredDice.Add(DieRusty);

            ///

            Dictionary<(string, string), int> DefaultSpawnRates = new Dictionary<(string, string), int>();

            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Experimentation), 13);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Assurance), 13);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Vow), 15);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Offense), 17);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.March), 17);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Rend), 33);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Dine), 46);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Titan), 30);

            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Experimentation), 17);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Assurance), 17);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Vow), 17);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Offense), 25);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.March), 25);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Rend), 22);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Dine), 41);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Titan), 33);

            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Experimentation), 20);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Assurance), 20);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Vow), 20);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Offense), 20);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.March), 20);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Rend), 35);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Dine), 38);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Titan), 23);

            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Experimentation), 10);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Assurance), 10);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Vow), 10);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Offense), 10);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.March), 10);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Rend), 12);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Dine), 15);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Titan), 12);

            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Experimentation), 15);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Assurance), 15);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Vow), 5);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Offense), 18);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.March), 5);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Rend), 16);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Dine), 26);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Titan), 14);

            foreach (Item die in RegisteredDice)
            {
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(die.spawnPrefab);
                Utilities.FixMixerGroups(die.spawnPrefab);
            }

            foreach (Item die in RegisteredDice)
            {
                ConfigEntry<Boolean> inShop = BepInExConfig.Bind<Boolean>(
                    die.itemName + " Shop",
                    "CanBuy",
                    die == DieEmergency
                );

                ConfigEntry<Int32> cost = BepInExConfig.Bind<Int32>(
                    die.itemName + " Shop",
                    "Cost",
                    200
                );

                if (inShop.Value)
                {
                    TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
                    node.clearPreviousText = true;
                    node.displayText = GetDisplayText(die);

                    Items.RegisterShopItem(die, null, null, node, cost.Value);
                }

                if (die == DieEmergency) continue;

                foreach (KeyValuePair<string, Levels.LevelTypes> level in RegLevels)
                {
                    ConfigEntry<int> rate = BepInExConfig.Bind<int>(
                        die.itemName + " Spawn rates",
                        level.Key,
                        DefaultSpawnRates[(die.itemName, level.Key)],
                        "Sets how often this item spawns on this level. 0-10 is very rare, 10-25 is rare, 25+ is common. This is only from my observations."
                    );

                    Items.RegisterScrap(die, rate.Value, level.Value);
                }
            }
        }

        private static string GetDisplayText(Item die)
        {
            string text = string.Empty;

            switch (die.itemName)
            {
                case "Emergency Die":
                    text = @"This handy, unstable device might be your last chance to save yourself.

 Rolls a number from 1 to 6:
 -Rolling 6 teleports you and players standing closely near you to the ship with all your items.
 -Rolling 4 or 5 teleports you to the ship with all your items.
 -Rolling 3 might be bad, or might be good. You decide? 
 -Rolling 2 will causes some problems
 -You dont want to roll a 1
";
                    break;
                case "The Gambler":
                    text = @"This item with rolling outcomes similar to the Emergency die, but with a larger pool of postivie effects and a bigger chance for a bad outcome.

-Rolling 6 will cause a Great effect.
-Rolling 5 will cause a Good effect.
-Rolling 4 will cause either a Good or a Mixed effect.
-Rolling 3 will cause a Bad effect.
-Rolling 2 will cause either a Bad or an Awful effect.
-Rolling 1 will cause an Awful effect.
";
                    break;
                case "Chronos":
                    text = @"This item has a higher chance of getting a great effect, but the outcomes change when the time passes. As the night comes, the chances to roll a bad/awful effect increase.

It's probably better to use it earlier in the day, but this can make your whole day troublesome.

-Rolling 6 will cause a Great effect.
-Rolling 5 will cause a Good or a Great effect.
-Rolling 4 will cause either a Bad, Good or a Great effect.
-Rolling 3 will cause a Bad effect.
-Rolling 2 will cause either a Bad or an Awful effect.
-Rolling 1 will cause an Awful effect.
";
                    break;
                case "The Sacrificer":
                    text = @"This item guarantees a return to the ship, and a bad/awful effect.

-Rolling 6 will cause a Bad effect.
-Rolling 5 will cause a Bad effect.
-Rolling 4 will cause a Bad effect.
-Rolling 3 will cause a Bad or an Awful effect.
-Rolling 2 will cause an Awful effect.
-Rolling 1 will cause two Awful effects.
";
                    break;
                case "The Saint":
                    text = @"This item will never roll a bad or an awful effect. It's the rarest die in this mod, so don't expect to see it a lot.

-Rolling 6 will cause a Great effect.
-Rolling 5 will cause a Great effect.
-Rolling 4 will cause a Good effect.
-Rolling 3 will cause a Good effect.
-Rolling 2 will cause a Good effect.
-Rolling 1 will waste the die.
";
                    break;
                case "Rusty":
                    text = @"This item spawns scrap. Only scrap. Higher rolls mean more scrap. Has negative outcomes.

-Rolling 6 will spawn 7-8 scrap.
-Rolling 5 will spawn 5-6 scrap
-Rolling 4 will spawn 3-4 scrap.
-Rolling 3 will spawn 1-2 scrap.
-Rolling 2 will cause a Bad effect.
-Rolling 1 will cause an Awful effect
";
                    break;
                default:
                    text = "";
                    break;
            }

            return text;
        }
    }
}
