using BepInEx;
using R2API;
using R2API.Utils;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

using BepInEx.Configuration;
using Path = System.IO.Path;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.TinkersSatchel {
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(TILER2Plugin.ModGuid, "1.3.0")]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(PlayerAPI), nameof(PrefabAPI))]
    public class TinkersSatchelPlugin:BaseUnityPlugin {
        public const string ModVer = "1.1.2";
        public const string ModName = "TinkersSatchel";
        public const string ModGuid = "com.ThinkInvisible.TinkersSatchel";
        private static Transform HUDroot = null;

        public static GameObject GameObjectReference;

        private static ConfigFile cfgFile;
        
        internal static FilingDictionary<ItemBoilerplate> masterItemList = new FilingDictionary<ItemBoilerplate>();
        
        internal static BepInEx.Logging.ManualLogSource _logger;




        private void Awake() {
            _logger = Logger;

            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TinkersSatchel.tinkerssatchel_assets")) {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider("@TinkersSatchel", bundle);
                ResourcesAPI.AddProvider(provider);
            }
            cfgFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            masterItemList = ItemBoilerplate.InitAll("TinkersSatchel");
            foreach(ItemBoilerplate x in masterItemList) {
                x.SetupConfig(cfgFile);
            }
            
            int longestName = 0;
            foreach(ItemBoilerplate x in masterItemList) {
                x.SetupAttributes("TINKSATCH", "TKSCH");
                if(x.itemCodeName.Length > longestName) longestName = x.itemCodeName.Length;
            }

            Logger.LogMessage("Index dump follows (pairs of name / index):");
            foreach(ItemBoilerplate x in masterItemList) {
                if(x is Artifact afct)
                    Logger.LogMessage(" Artifact TKSCH"+x.itemCodeName.PadRight(longestName) + " / "+((int)afct.regIndex).ToString());
                else
                    Logger.LogMessage("    Other TKSCH"+x.itemCodeName.PadRight(longestName) + " / N/A");
            }

            foreach(ItemBoilerplate x in masterItemList) {
                x.SetupBehavior();
            }
            //On.RoR2.UI.HUD.Awake +=myFunc
            MainUIMod test = new MainUIMod();


        }

        private void OnDestroy()
        {
        }
    }
}
