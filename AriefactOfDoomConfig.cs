using BepInEx;
using R2API;
using R2API.Utils;
using System.Reflection;
using UnityEngine;

using BepInEx.Configuration;
using Path = System.IO.Path;
using TILER2;
using static TILER2.MiscUtil;

namespace ArtifactOfDoom {
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(TILER2Plugin.ModGuid, "1.3.0")]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(PlayerAPI), nameof(PrefabAPI))]
    public class ArtifactOfDoomConfig:BaseUnityPlugin {
        public const string ModVer = "0.8.1";
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






        private void Awake() {
            _logger = Logger;

            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ArtifactOfDoom.tinkerssatchel_assets")) {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider("@TinkersSatchel", bundle);
                ResourcesAPI.AddProvider(provider);
            }
            cfgFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            masterItemList = ItemBoilerplate.InitAll("ArtifactOfDoom");
            foreach(ItemBoilerplate x in masterItemList) {
                x.SetupConfig(cfgFile);
            }

            averageItemsPerStage = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "averageItemsPerStage"), 3, new ConfigDescription(
                "Base chance in percent that enemys steal items from you ((totalItems - currentStage * averageItemsPerStage) * 2; \nIf that value is lower you'll need faster more enemies to get an item")); 
            minItemsPerStage = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "minItemsPerStage"), 2, new ConfigDescription(
                "The expected minimum item count per stage. If you have less Items than that you'll have a decreased chance that you loose items")); 
            maxItemsPerStage = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "maxItemsPerStage"), 7, new ConfigDescription(
                "The expected maximum item count per stage. If you have more Items than that you'll have a chance to loose more than one item per hit")); 
            exponentailFactorToCalculateSumOfLostItems = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "exponentailFactorToCalculateSumOfLostItems"), 1.5, new ConfigDescription(
                "The exponent to Calculate how many items you'll loose if you're over maxItemsPerStage")); 
            
            int longestName = 0;
            foreach(ItemBoilerplate x in masterItemList) {
                x.SetupAttributes("ARTDOOM", "ADOOM");
                if(x.itemCodeName.Length > longestName) longestName = x.itemCodeName.Length;
            }

            Logger.LogMessage("Index dump follows (pairs of name / index):");
            foreach(ItemBoilerplate x in masterItemList) {
                if(x is Artifact afct)
                    Logger.LogMessage(" Artifact ADOOM"+x.itemCodeName.PadRight(longestName) + " / "+((int)afct.regIndex).ToString());
                else
                    Logger.LogMessage("    Other ADOOM"+x.itemCodeName.PadRight(longestName) + " / N/A");
            }

            foreach(ItemBoilerplate x in masterItemList) {
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
