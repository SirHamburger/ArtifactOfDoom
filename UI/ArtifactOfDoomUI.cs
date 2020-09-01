using BepInEx;
using MiniRpcLib;
using MiniRpcLib.Func;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;



#region TODO:
/*

*/
#endregion

namespace ArtifactOfDoom
{

    [R2API.Utils.R2APISubmoduleDependency("ResourcesAPI")]
    [BepInPlugin("com.ohway.UIMod", "UI Modifier", "1.0")]
    [BepInDependency(MiniRpcPlugin.Dependency)]
    public class ArtifactOfDoomUI : BaseUnityPlugin
    {
        public GameObject ModCanvas = null;
        private static bool ArtifactIsActive = false;
        private static bool calculationSacrifice = false;
        void Awake()
        {
            //On.RoR2.UI.HealthBar.Awake += ExpBarAwakeAddon;
            On.RoR2.UI.HUD.Awake += HUDAwake;


            try
            {
                SetUpMiniRPC();
            }
            catch (Exception)
            {
                Debug.LogError($"[SirHamburger] Error in SetUpMiniRPC");
            }
            On.RoR2.Inventory.RemoveItem += (orig, self, itemindex1, ItemIndex2) =>
            {
                orig(self, itemindex1, ItemIndex2);

            };
        }
        private void SetUpModCanvas()
        {
            if (ModCanvas == null)
            {
                ModCanvas = new GameObject("UIModifierCanvas");
                ModCanvas.AddComponent<Canvas>();
                ModCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                if (HUDRoot != null)
                {
                    ModCanvas.GetComponent<Canvas>().worldCamera = HUDRoot.transform.root.gameObject.GetComponent<Canvas>().worldCamera;
                }

                ModCanvas.AddComponent<CanvasScaler>();
                ModCanvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                ModCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
                ModCanvas.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            }
        }


        #region Exp bar GameObjects
        public Transform HUDRoot = null;
        public GameObject ModExpBarGroup = null;
        public static List<GameObject> listGainedImages = new List<GameObject>();
        public static List<GameObject> listLostImages = new List<GameObject>();
        public static GameObject itemGainBar;
        public static GameObject itemGainFrame;

        #endregion
        public void HUDAwake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);

            if (!ArtifactIsActive)
                return;

            HUDRoot = self.transform.root;


            MainExpBarStart();

        }

        private void MainExpBarStart()
        {
            //Debug.LogError("MainExpBarStart");
            //Debug.LogError("AArtifactIsActiv " + ArtifactIsActiv);
            if (HUDRoot != null)
            {
                try
                {
                    SetUpModCanvas();
                    listGainedImages.Clear();
                    listLostImages.Clear();
                    float baseSize = (Convert.ToSingle(ArtifactOfDoomConfig.sizeOfSideBars.Value));
                    
                   
                    float screenResultuionMultiplier= (float)Screen.currentResolution.width/(float)Screen.currentResolution.height;
                    //Debug.LogError("screenResultuionMultiplier " +screenResultuionMultiplier);
                    //Debug.LogError("float screenResultuionMultiplier=Screen.currentResolution.width/Screen.currentResolution.height;" +Screen.currentResolution.width/Screen.currentResolution.height);
                     float baseSizeY = baseSize * screenResultuionMultiplier;
                     float baseSizeYPlusMargin = baseSizeY + (float)0.01;
                    //Debug.LogError("baseSizeY " +baseSizeY);
                    //Debug.LogError("baseSize " +baseSize);

                    for (int i = 0; i < 10; i++)
                    {
                        ModExpBarGroup = new GameObject("GainedItems" + i);

                        ModExpBarGroup.transform.SetParent(ModCanvas.transform);

                        ModExpBarGroup.AddComponent<RectTransform>();

                        ModExpBarGroup.GetComponent<RectTransform>().anchorMin = new Vector2(0.0f, (float)(0.20 + ((float)i * baseSizeYPlusMargin)));
                        ModExpBarGroup.GetComponent<RectTransform>().anchorMax = new Vector2(baseSize, (float)(0.2+ baseSizeY + ((float)i * baseSizeYPlusMargin)));
                        ModExpBarGroup.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                        ModExpBarGroup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                        ModExpBarGroup.AddComponent<NetworkIdentity>().serverOnly = false;
                        listGainedImages.Add(ModExpBarGroup);


                        ModExpBarGroup = new GameObject("LostItems" + i);

                        ModExpBarGroup.transform.SetParent(ModCanvas.transform);

                        ModExpBarGroup.AddComponent<RectTransform>();
                        ModExpBarGroup.GetComponent<RectTransform>().anchorMin = new Vector2((float)1.0-baseSize, (float)(0.20 + ((float)i * baseSizeYPlusMargin)));
                        ModExpBarGroup.GetComponent<RectTransform>().anchorMax = new Vector2(1.00f, (float)(0.2+baseSizeY + ((float)i * baseSizeYPlusMargin)));
                        ModExpBarGroup.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                        ModExpBarGroup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                        ModExpBarGroup.AddComponent<NetworkIdentity>().serverOnly = false;

                        listLostImages.Add(ModExpBarGroup);
                    }

                    if (!ArtifactOfDoomConfig.disableItemProgressBar.Value && !calculationSacrifice)
                    {
                        ModExpBarGroup = new GameObject("ItemGainBar");
                        ModExpBarGroup.transform.SetParent(ModCanvas.transform);
                        ModExpBarGroup.AddComponent<RectTransform>();
                        ModExpBarGroup.GetComponent<RectTransform>().anchorMin = new Vector2(0.35f, 0.05f);
                        ModExpBarGroup.GetComponent<RectTransform>().anchorMax = new Vector2(0.35f, 0.06f);
                        ModExpBarGroup.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                        ModExpBarGroup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                        ModExpBarGroup.AddComponent<NetworkIdentity>().serverOnly = false;

                        itemGainBar = ModExpBarGroup;
                        itemGainBar.AddComponent<Image>();
                        itemGainBar.GetComponent<Image>().color = new Color(255, 255, 255, 0.3f);



                        ModExpBarGroup = new GameObject("ItemGainFrame");
                        ModExpBarGroup.transform.SetParent(ModCanvas.transform);
                        ModExpBarGroup.AddComponent<RectTransform>();
                        ModExpBarGroup.GetComponent<RectTransform>().anchorMin = new Vector2(0.35f, 0.05f);
                        ModExpBarGroup.GetComponent<RectTransform>().anchorMax = new Vector2(0.65f, 0.06f);
                        ModExpBarGroup.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                        ModExpBarGroup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                        ModExpBarGroup.AddComponent<NetworkIdentity>().serverOnly = false;
                        itemGainFrame = ModExpBarGroup;
                        itemGainFrame.AddComponent<Image>();
                        itemGainFrame.GetComponent<Image>().color = new Color(255, 0, 0, 0.1f);
                    }



                }
                catch (Exception)
                {
                    Debug.Log($"[SirHamburger Error] while Adding UI elements");
                }
            }
            else
            {
                Debug.LogError("HUDRoot == null");
            }

        }
        public static void updateItemProgressBar(int enemiesKilled, int enemiesNeeded)
        {


        }



        public static IRpcFunc<string, string> AddGainedItemsToPlayers { get; set; }
        public static IRpcFunc<string, string> AddLostItemsOfPlayers { get; set; }
        public static IRpcFunc<string, string> UpdateProgressBar { get; set; }
        public static IRpcFunc<bool, string> IsArtifactActive { get; set; }
        public static IRpcFunc<bool, string> IsCalculationSacrifice { get; set; }

        public const string ModVer = "1.2.0";
        public const string ModName = "ArtifactOfDoom";
        public const string ModGuid = "com.SirHamburger.ArtifactOfDoom";

        private void SetUpMiniRPC()
        {
            // Fix the damn in-game console stealing our not-in-game consoles output.
            // Not related to the demo, just very useful.
            //On.RoR2.RoR2Application.UnitySystemConsoleRedirector.Redirect += orig => { };

            // Create a MiniRpcInstance that automatically registers all commands to our ModGuid
            // This lets us support multiple mods using the same command ID
            // We could also just generate new command ID's without "isolating" them by mod as well, so it would break if mod load order was different for different clients
            // I opted for the ModGuid instead of an arbitrary number or GUID to encourage mods not to set the same ID
            var miniRpc = MiniRpc.CreateInstance(ModGuid);

            AddGainedItemsToPlayers = miniRpc.RegisterFunc(Target.Client, (NetworkUser user, string QueueGainedItemSpriteToString) => //--------------------HierSTuffMachen!!
            {
                if (!ArtifactOfDoomConfig.disableSideBars.Value)
                {
                    string[] QueueGainedItemSprite = QueueGainedItemSpriteToString.Split(' ');

                    int i = 0;
                    foreach (var element in QueueGainedItemSprite)
                    {
                        if (element != "")
                        {

                            if (ArtifactOfDoomUI.listGainedImages[i].GetComponent<Image>() == null)
                                ArtifactOfDoomUI.listGainedImages[i].AddComponent<Image>();
                            ArtifactOfDoomUI.listGainedImages[i].GetComponent<Image>().sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(element)).pickupIconSprite;

                            i++;

                        }

                    }
                }
                return "dummie";
            });
            AddLostItemsOfPlayers = miniRpc.RegisterFunc(Target.Client, (NetworkUser user, string QueueLostItemSpriteToString) => //--------------------HierSTuffMachen!!
            {
                if (!ArtifactOfDoomConfig.disableSideBars.Value)
                {
                    string[] QueueLostItemSprite = QueueLostItemSpriteToString.Split(' ');

                    int i = 0;
                    foreach (var element in QueueLostItemSprite)
                    {
                        if (element != "")
                        {

                            if (ArtifactOfDoomUI.listLostImages[i].GetComponent<Image>() == null)
                                ArtifactOfDoomUI.listLostImages[i].AddComponent<Image>();
                            ArtifactOfDoomUI.listLostImages[i].GetComponent<Image>().sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(element)).pickupIconSprite;

                            i++;
                        }

                    }
                }
                return "dummie";
            });
            UpdateProgressBar = miniRpc.RegisterFunc(Target.Client, (NetworkUser user, string killedNeededEnemies) => //--------------------HierSTuffMachen!!
            {
                //Debug.LogWarning("in UpdateProgressBar");
                //Debug.LogError("ArtifactOfDoomConfig.disableItemProgressBar.Value"+ ArtifactOfDoomConfig.disableItemProgressBar.Value);
                //Debug.LogError("ArtifactOfDoomConfig.useArtifactOfSacreficeCalculation.Value"+ ArtifactOfDoomConfig.useArtifactOfSacreficeCalculation.Value);
                if (ArtifactOfDoomConfig.disableItemProgressBar.Value || calculationSacrifice)
                    return "Disabled Progress Bar";

                if (killedNeededEnemies == null)
                {
                    Debug.Log("killedNeededEnemies == null");
                    return "error";
                }

                //Debug.LogWarning("string killedNeededEnemies für rpc: " + killedNeededEnemies);
                string[] stringkilledNeededEnemies = killedNeededEnemies.Split(',');
                //Debug.LogError("in line 276");
                if (stringkilledNeededEnemies == null)
                    Debug.LogError("stringkilledneededEnemies=null");

                int enemiesKilled = Convert.ToInt32(stringkilledNeededEnemies[0]);
                int enemiesNeeded = Convert.ToInt32(stringkilledNeededEnemies[1]) + 2;

                //Debug.LogError("in line 279");
                if (itemGainBar == null)
                    return "error";
                double progress = (double)enemiesKilled / ((double)enemiesNeeded);

                //                  Debug.LogError("in line 2282");
                if (itemGainBar.GetComponent<RectTransform>() == null)
                    return "Error while excecuting Update progress bar";

                if ((0.35f + (float)(progress * 0.3)) > 0.65f)
                {

                    if (itemGainBar.GetComponent<RectTransform>().anchorMax == null)
                        Debug.LogError("itemGainBar.GetComponent<RectTransform>().anchorMax==null");

                    itemGainBar.GetComponent<RectTransform>().anchorMax = new Vector2(0.65f, 0.06f);
                }
                else
                {

                    itemGainBar.GetComponent<RectTransform>().anchorMin = new Vector2(0.35f, 0.05f);
                    //                    Debug.LogError("in line 288");

                    itemGainBar.GetComponent<RectTransform>().anchorMax = new Vector2(0.35f + (float)(progress * 0.3), 0.06f);
                }
                return "dummie";
            });
            IsArtifactActive = miniRpc.RegisterFunc(Target.Client, (NetworkUser user, bool isActive) => //--------------------HierSTuffMachen!!
            {
                ArtifactIsActive = isActive;
                return "";
            });
            IsCalculationSacrifice = miniRpc.RegisterFunc(Target.Client, (NetworkUser user, bool isActive) => //--------------------HierSTuffMachen!!
            {
                //Debug.LogError("Set CalculationSacrifice to " + isActive);
                calculationSacrifice = isActive;
                return "";
            });

            Debug.LogWarning("minirpc succsessfull set up");

            // The "_ ="'s above mean that the return value will be ignored. In your code you should assign the return value to something to be able to call the function.
        }
        enum CommandId
        {
            //                     ----|    This number is only needed because we already created an RpcFunc with ID 0 (the first one we made without an ID).
            SomeCommandName = 2345, // If you use IDs in your own code, you will most likely want to give all commands explicit IDs, which will avoid this issue.
            SomeOtherCommandName,
        }

        private void OnDestroy()
        {
            On.RoR2.UI.HUD.Awake -= HUDAwake;
           // On.RoR2.Inventory.RemoveItem -= RemoveItem;
        }
    }
}
