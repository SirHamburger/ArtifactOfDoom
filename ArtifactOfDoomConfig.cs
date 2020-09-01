using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using System.Reflection;
using TILER2;
using UnityEngine;
using static TILER2.MiscUtil;
using Path = System.IO.Path;

namespace ArtifactOfDoom
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(TILER2Plugin.ModGuid, "1.3.0")]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(PlayerAPI), nameof(PrefabAPI))]
    public class ArtifactOfDoomConfig : BaseUnityPlugin
    {
        public const string ModVer = "1.2.0";
        public const string ModName = "ArtifactOfDoom";
        public const string ModGuid = "com.SirHamburger.ArtifactOfDoom";

        public static GameObject GameObjectReference;

        private static ConfigFile cfgFile;

        internal static FilingDictionary<ItemBoilerplate> masterItemList = new FilingDictionary<ItemBoilerplate>();

        internal static BepInEx.Logging.ManualLogSource _logger;

        public static ConfigEntry<int> averageItemsPerStage;
        public static ConfigEntry<int> minItemsPerStage;
        public static ConfigEntry<int> maxItemsPerStage;
        public static ConfigEntry<double> exponentailFactorIfYouAreUnderAverageItemsPerStage;
        public static ConfigEntry<double> exponentailFactorToCalculateSumOfLostItems;
        public static ConfigEntry<bool> artifactOfSwarmNerf;
        public static ConfigEntry<string> CustomChars;

        public static ConfigEntry<bool> useArtifactOfSacrificeCalculation;
        public static ConfigEntry<double> multiplayerForArtifactOfSacrificeDropRate;

        public static ConfigEntry<bool> disableItemProgressBar;

        public static ConfigEntry<double> timeAfterHitToNotLoseItemMonsoon;
        public static ConfigEntry<double> timeAfterHitToNotLoseItemDrizzly;
        public static ConfigEntry<double> timeAfterHitToNotLoseItemRainstorm;
        public static ConfigEntry<double> CommandoBonusItems;
        public static ConfigEntry<double> CommandoMultiplierForTimedBuff;
        public static ConfigEntry<double> HuntressBonusItems;
        public static ConfigEntry<double> HuntressMultiplierForTimedBuff;
        public static ConfigEntry<double> MULTBonusItems;
        public static ConfigEntry<double> MULTMultiplierForTimedBuff;
        public static ConfigEntry<double> EngineerBonusItems;
        public static ConfigEntry<double> EngineerMultiplierForTimedBuff;
        public static ConfigEntry<double> ArtificerBonusItems;
        public static ConfigEntry<double> ArtificerMultiplierForTimedBuff;
        public static ConfigEntry<double> MercenaryBonusItems;
        public static ConfigEntry<double> MercenaryMultiplierForTimedBuff;
        public static ConfigEntry<double> RexBonusItems;
        public static ConfigEntry<double> RexMultiplierForTimedBuff;
        public static ConfigEntry<double> LoaderBonusItems;
        public static ConfigEntry<double> LoaderMultiplierForTimedBuff;
        public static ConfigEntry<double> AcridBonusItems;
        public static ConfigEntry<double> AcridMultiplierForTimedBuff;
        public static ConfigEntry<double> CaptainBonusItems;
        public static ConfigEntry<double> CaptainMultiplierForTimedBuff;
        public static ConfigEntry<double> CustomSurvivorBonusItems;
        public static ConfigEntry<double> CustomSurvivorMultiplierForTimedBuff;
        public static ConfigEntry<double> exponentTriggerItems;

        public static BuffIndex buffIndexDidLoseItem;

        public static ConfigEntry<double> sizeOfSideBars;
        public static ConfigEntry<bool> disableSideBars;
        public static ConfigEntry<bool> enableChatItemOutput;

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

            averageItemsPerStage = cfgFile.Bind(new ConfigDefinition("Gameplay Settings", "averageItemsPerStage"), 3, new ConfigDescription(
                "Base chance in percent that enemys steal items from you ((totalItems - currentStage * averageItemsPerStage) ^ exponentTriggerItems; \nIf that value is lower you'll need to kill more enemies to get an item"));
            exponentTriggerItems = cfgFile.Bind(new ConfigDefinition("Gameplay Settings", "exponentTriggerItems"), 2.0, new ConfigDescription(
                "The exponent for calculation when you'll get an item. If it's 1 you have a linear increase. Default is 2"));

            minItemsPerStage = cfgFile.Bind(new ConfigDefinition("Gameplay Settings", "minItemsPerStage"), 2, new ConfigDescription(
                "The expected minimum item count per stage. If you have less Items than that you'll have a decreased chance that you lose items"));
            maxItemsPerStage = cfgFile.Bind(new ConfigDefinition("Gameplay Settings", "maxItemsPerStage"), 7, new ConfigDescription(
                "The expected maximum item count per stage. If you have more Items than that you'll have a chance to lose more than one item per hit"));
            exponentailFactorToCalculateSumOfLostItems = cfgFile.Bind(new ConfigDefinition("Gameplay Settings", "exponentailFactorToCalculateSumOfLostItems"), 1.5, new ConfigDescription(
                "The exponent to Calculate how many items you'll lose if you're over maxItemsPerStage"));
            exponentailFactorIfYouAreUnderAverageItemsPerStage = cfgFile.Bind(new ConfigDefinition("Gameplay Settings", "exponentailFactorIfYouAreUnderAverageItemsPerStage"), 0.0, new ConfigDescription(
                "The exponent to Calculate how many kills you'll need if you're under averageItemsPerStage. The formula is totalitems^exponentailFactorIfYouAreUnderAverageItemsPerStage. Default is 0 so you'll need always two kills."));

            artifactOfSwarmNerf = cfgFile.Bind(new ConfigDefinition("Gameplay Settings", "artifactOfSwarmNerf"), false, new ConfigDescription(
                "Enable the nerf for Artifact of Swarm where you've to kill double as many enemies"));

            useArtifactOfSacrificeCalculation = cfgFile.Bind(new ConfigDefinition("Gameplay Settings", "useArtifactOfSacreficeCalculation"), false, new ConfigDescription(
                "Chance the item gain to a specific drop rate of enemys"));
            multiplayerForArtifactOfSacrificeDropRate = cfgFile.Bind(new ConfigDefinition("Gameplay Settings", "multiplayerForArtifactOfSacrificeDropRate"), 2.0, new ConfigDescription(
                "Multiplier for the drop rate (base Chance is 5)"));

            disableItemProgressBar = cfgFile.Bind(new ConfigDefinition("UI Settings", "disableItemProgressBar"), false, new ConfigDescription(
                "If true it disables the Progress bar in the bottom of the UI"));
            disableSideBars = cfgFile.Bind(new ConfigDefinition("UI Settings", "disableSideBars"), false, new ConfigDescription(
                "Disables the item Sidebars"));
            enableChatItemOutput = cfgFile.Bind(new ConfigDefinition("UI Settings", "enableChatItemOutput"), false, new ConfigDescription(
                "Enables the chat output for gained/lost Items. This setting is not synced."));
            sizeOfSideBars = cfgFile.Bind(new ConfigDefinition("UI Settings", "sizeOfSideBars"), 0.02, new ConfigDescription(
            "Spezifies the size of the sidebars. 1 is whole window 0 is invisible (but for that plase use the disable setting)."));
            timeAfterHitToNotLoseItemDrizzly = cfgFile.Bind(new ConfigDefinition("Gameplay Settings", "timeAfterHitToNotLooseItemDrizzly"), 0.8, new ConfigDescription(
                     "The time in seconds where you will not lose items after you lost one on drizzly"));
            timeAfterHitToNotLoseItemRainstorm = cfgFile.Bind(new ConfigDefinition("Gameplay Settings", "timeAfterHitToNotLooseItemRainstorm"), 0.2, new ConfigDescription(
                "The time in seconds where you will not lose items after you lost one on rainstorm"));
            timeAfterHitToNotLoseItemMonsoon = cfgFile.Bind(new ConfigDefinition("Gameplay Settings", "timeAfterHitToNotLooseItemMonsoon"), 0.05, new ConfigDescription(
                "The time in seconds where you will not lose items after you lost one on monsoon"));

            CommandoBonusItems = cfgFile.Bind(new ConfigDefinition("Character specific settings", "CommandoBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            CommandoMultiplierForTimedBuff = cfgFile.Bind(new ConfigDefinition("Character specific settings", "commandoMultiplyerForTimedBuff"), 1.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            HuntressBonusItems = cfgFile.Bind(new ConfigDefinition("Character specific settings", "HuntressBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            HuntressMultiplierForTimedBuff = cfgFile.Bind(new ConfigDefinition("Character specific settings", "HuntressMultiplierForTimedBuff"), 1.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            MULTBonusItems = cfgFile.Bind(new ConfigDefinition("Character specific settings", "MULTBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            MULTMultiplierForTimedBuff = cfgFile.Bind(new ConfigDefinition("Character specific settings", "MULTMultiplierForTimedBuff"), 1.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            EngineerBonusItems = cfgFile.Bind(new ConfigDefinition("Character specific settings", "EngineerBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            EngineerMultiplierForTimedBuff = cfgFile.Bind(new ConfigDefinition("Character specific settings", "EngineerMultiplierForTimedBuff"), 1.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            ArtificerBonusItems = cfgFile.Bind(new ConfigDefinition("Character specific settings", "ArtificerBonusItems"), 2.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            ArtificerMultiplierForTimedBuff = cfgFile.Bind(new ConfigDefinition("Character specific settings", "ArtificerMultiplierForTimedBuff"), 1.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            MercenaryBonusItems = cfgFile.Bind(new ConfigDefinition("Character specific settings", "MercenaryBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            MercenaryMultiplierForTimedBuff = cfgFile.Bind(new ConfigDefinition("Character specific settings", "MercenaryMultiplierForTimedBuff"), 4.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            RexBonusItems = cfgFile.Bind(new ConfigDefinition("Character specific settings", "RexBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            RexMultiplierForTimedBuff = cfgFile.Bind(new ConfigDefinition("Character specific settings", "RexMultiplierForTimedBuff"), 1.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            LoaderBonusItems = cfgFile.Bind(new ConfigDefinition("Character specific settings", "LoaderBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            LoaderMultiplierForTimedBuff = cfgFile.Bind(new ConfigDefinition("Character specific settings", "LoaderMultiplierForTimedBuff"), 4.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            AcridBonusItems = cfgFile.Bind(new ConfigDefinition("Character specific settings", "AcridBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            AcridMultiplierForTimedBuff = cfgFile.Bind(new ConfigDefinition("Character specific settings", "AcridMultiplierForTimedBuff"), 4.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            CaptainBonusItems = cfgFile.Bind(new ConfigDefinition("Character specific settings", "CaptainBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            CaptainMultiplierForTimedBuff = cfgFile.Bind(new ConfigDefinition("Character specific settings", "CaptainMultiplierForTimedBuff"), 1.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLooseItems"));
            CustomSurvivorBonusItems = cfgFile.Bind(new ConfigDefinition("Character specific settings", "CustomSurvivorBonusItems"), 1.0, new ConfigDescription(
                "The count of items which you get if you kill enough enemies"));
            CustomSurvivorMultiplierForTimedBuff = cfgFile.Bind(new ConfigDefinition("Character specific settings", "CustomSurvivorMultiplierForTimedBuff"), 1.0, new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLoseItems"));
            CustomChars= cfgFile.Bind(new ConfigDefinition("Character specific settings", "CustomCharacters"), "[{\"Name\": \"CUSTOM_CHAR_BODY_NAME1\", \"MultiplierForTimedBuff\": 1.0, \"BonusItems\": 1.0},{\"Name\": \"CUSTOM_CHAR_BODY_NAME2\", \"MultiplierForTimedBuff\": 2.0, \"BonusItems\": 2.0}]", new ConfigDescription(
                "The Multiplier for that specific character for the length of timeAfterHitToNotLoseItems"));
            



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
                    Logger.LogMessage("Other ADOOM" + x.itemCodeName.PadRight(longestName) + " / N/A");
            }

            var didLoseItem = new R2API.CustomBuff("didLoseItem", "", Color.black, false, false);
            buffIndexDidLoseItem = BuffAPI.Add(didLoseItem);
            foreach (ItemBoilerplate x in masterItemList)
            {
                x.SetupBehavior();
            }
            //On.RoR2.UI.HUD.Awake +=myFunc
            // ArtifactOfDoomUI test = new ArtifactOfDoomUI();
        }
    }
}
