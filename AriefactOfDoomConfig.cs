using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;

using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;
using BepInEx.Configuration;
using Path = System.IO.Path;
using TILER2;
using static TILER2.MiscUtil;

namespace ArtifactOfDoom
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(TILER2Plugin.ModGuid, "1.3.0")]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(PlayerAPI), nameof(PrefabAPI))]
    public class ArtifactOfDoomConfig : BaseUnityPlugin
    {
        public const string ModVer = "0.9.4";
        public const string ModName = "ArtifactOfDoom";
        public const string ModGuid = "com.SirHamburger.ArtifactOfDoom";
        private static Transform HUDroot = null;

        public static GameObject GameObjectReference;

        private static ConfigFile cfgFile;

        internal static FilingDictionary<ItemBoilerplate> masterItemList = new FilingDictionary<ItemBoilerplate>();

        internal static BepInEx.Logging.ManualLogSource _logger;

        public static ConfigEntry<int> averageItemsPerStage;
        public static ConfigEntry<int> minItemsPerStage;
        public static ConfigEntry<int> maxItemsPerStage;
        public static ConfigEntry<double> exponentailFactorToCalculateSumOfLostItems;
        public static ConfigEntry<double> timeAfterHitToNotLooseItemMonsoon;
        public static ConfigEntry<double> timeAfterHitToNotLooseItemDrizzly;
        public static ConfigEntry<double> timeAfterHitToNotLooseItemRainstorm;
        public static ConfigEntry<double> commandoBonusItems;
        public static ConfigEntry<double> commandoMultiplyerForTimedBuff;
        public static ConfigEntry<double> HuntressBonusItems;
        public static ConfigEntry<double> HuntressMultiplyerForTimedBuff;
        public static ConfigEntry<double> MULTBonusItems;
        public static ConfigEntry<double> MULTMultiplyerForTimedBuff;
        public static ConfigEntry<double> EngineerBonusItems;
        public static ConfigEntry<double> EngineerMultiplyerForTimedBuff;
        public static ConfigEntry<double> ArtificerBonusItems;
        public static ConfigEntry<double> ArtificerMultiplyerForTimedBuff;
        public static ConfigEntry<double> MercenaryBonusItems;
        public static ConfigEntry<double> MercenaryMultiplyerForTimedBuff;
        public static ConfigEntry<double> RexBonusItems;
        public static ConfigEntry<double> RexMultiplyerForTimedBuff;
        public static ConfigEntry<double> LoaderBonusItems;
        public static ConfigEntry<double> LoaderMultiplyerForTimedBuff;
        public static ConfigEntry<double> AcridBonusItems;
        public static ConfigEntry<double> AcridMultiplyerForTimedBuff;
        public static ConfigEntry<double> exponentTriggerItems;




        public static BuffIndex buffIndexDidLooseItem;





        private void Awake()
        {
            _logger = Logger;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ArtifactOfDoom.artifactofdoom"))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider("@ArtifactOfDoom", bundle);
                ResourcesAPI.AddProvider(provider);
            }




        
            cfgFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            masterItemList = ItemBoilerplate.InitAll("ArtifactOfDoom");
            foreach (ItemBoilerplate x in masterItemList)
            {
                x.SetupConfig(cfgFile);
            }

            averageItemsPerStage = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "averageItemsPerStage"), 3, new ConfigDescription(
                "Base chance in percent that enemys steal items from you ((totalItems - currentStage * averageItemsPerStage) * 2; \nIf that value is lower you'll need faster more enemies to get an item"));
            exponentTriggerItems = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "exponentTriggerItems"), 2.0, new ConfigDescription(
                "The exponent for calculation when you'll get an item. If it's 1 you have a linear increase. Default is 2"));
            
            minItemsPerStage = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "minItemsPerStage"), 2, new ConfigDescription(
                "The expected minimum item count per stage. If you have less Items than that you'll have a decreased chance that you loose items"));
            maxItemsPerStage = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "maxItemsPerStage"), 7, new ConfigDescription(
                "The expected maximum item count per stage. If you have more Items than that you'll have a chance to loose more than one item per hit"));
            exponentailFactorToCalculateSumOfLostItems = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "exponentailFactorToCalculateSumOfLostItems"), 1.5, new ConfigDescription(
                "The exponent to Calculate how many items you'll loose if you're over maxItemsPerStage"));

            timeAfterHitToNotLooseItemDrizzly = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "timeAfterHitToNotLooseItemDrizzly"), 0.8, new ConfigDescription(
                "The exponent to Calculate how many items you'll loose if you're over maxItemsPerStage"));
            timeAfterHitToNotLooseItemRainstorm = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "timeAfterHitToNotLooseItemRainstorm"), 0.2, new ConfigDescription(
                "The exponent to Calculate how many items you'll loose if you're over maxItemsPerStage"));
            timeAfterHitToNotLooseItemMonsoon = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "timeAfterHitToNotLooseItemMonsoon"), 0.05, new ConfigDescription(
                "The exponent to Calculate how many items you'll loose if you're over maxItemsPerStage"));

            commandoBonusItems = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "CommandoBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            commandoMultiplyerForTimedBuff = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "commandoMultiplyerForTimedBuff"), 1.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            HuntressBonusItems = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "HuntressBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            HuntressMultiplyerForTimedBuff = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "HuntressMultiplyerForTimedBuff"), 1.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            MULTBonusItems = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "MULTBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            MULTMultiplyerForTimedBuff = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "MULTMultiplyerForTimedBuff"), 1.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            EngineerBonusItems = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "EngineerBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            EngineerMultiplyerForTimedBuff = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "EngineerMultiplyerForTimedBuff"), 1.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            ArtificerBonusItems = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "ArtificerBonusItems"), 2.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            ArtificerMultiplyerForTimedBuff = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "ArtificerMultiplyerForTimedBuff"), 1.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            MercenaryBonusItems = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "MercenaryBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            MercenaryMultiplyerForTimedBuff = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "MercenaryMultiplyerForTimedBuff"), 4.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            RexBonusItems = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "RexBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            RexMultiplyerForTimedBuff = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "RexMultiplyerForTimedBuff"), 1.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            LoaderBonusItems = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "LoaderBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            LoaderMultiplyerForTimedBuff = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "LoaderMultiplyerForTimedBuff"), 4.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            AcridBonusItems = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "AcridBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            AcridMultiplyerForTimedBuff = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "AcridMultiplyerForTimedBuff"), 4.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));


            int longestName = 0;
            foreach (ItemBoilerplate x in masterItemList)
            {
                x.SetupAttributes("ARTDOOM", "ADOOM");
                if (x.itemCodeName.Length > longestName) longestName = x.itemCodeName.Length;
            }

            Logger.LogMessage("Index dump follows (pairs of name / index):");
            foreach (ItemBoilerplate x in masterItemList)
            {
                if (x is Artifact afct)
                    Logger.LogMessage(" Artifact ADOOM" + x.itemCodeName.PadRight(longestName) + " / " + ((int)afct.regIndex).ToString());
                else
                    Logger.LogMessage("    Other ADOOM" + x.itemCodeName.PadRight(longestName) + " / N/A");
            }

            var didLooseItem = new R2API.CustomBuff("didLooseItem", "", Color.black, false, false);
            buffIndexDidLooseItem = BuffAPI.Add(didLooseItem);
            foreach (ItemBoilerplate x in masterItemList)
            {
                x.SetupBehavior();
            }
            //On.RoR2.UI.HUD.Awake +=myFunc
            ArtifactOfDoomUI test = new ArtifactOfDoomUI();
        }

        private void OnDestroy()
        {
        }
    }
}
