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


/*
	Disclaimer:
	Despite it all
	I have no idea what I'm doing to be very honest.
*/

#region TODO:
/*

*/
#endregion

namespace ArtifactOfDoom
{
    public class ArtifactOfDoomUI : BaseUnityPlugin
    {
        public GameObject ModCanvas = new GameObject();
        public GameObject ItemContainer = new GameObject();
        public GameObject ItemGainBar = new GameObject();
        public GameObject ItemGainFrame = new GameObject();
        public static List<GameObject> listGainedImages = new List<GameObject>();
        public static List<GameObject> listLostImages = new List<GameObject>();
        public HealthBar HealthBar;
        public ExpBar ExpBar;
        private static bool ArtifactIsActive = false;
        private static bool calculationSacrifice = false;

        public void Awake()
        {
            try
            {
                SetUpMiniRPC();
            }
            catch (Exception)
            {
                Debug.Log($"[SirHamburger] Error in SetUpMiniRPC");
            }

            On.RoR2.UI.HUD.Awake += HudAwake;

            On.RoR2.Inventory.RemoveItem += RemoveItem;
        }

        private void HudAwake(On.RoR2.UI.HUD.orig_Awake self, HUD orig)
        {
            self(orig);
            initializeItemUI();
            ExpBar = orig.expBar;
            ItemGainFrame.transform.SetParent(orig.healthBar.transform, false);
            HealthBar = orig.healthBar;
        }

        private void initializeItemUI()
        {
            if (!ArtifactIsActive)
                return;

            try
            {
                SetUpModCanvas();

                ItemContainer = new GameObject();
                ItemContainer.name = "Item Container";
                ItemContainer.transform.SetParent(ModCanvas.transform, false);
                ItemContainer.transform.localPosition = new Vector2(0, -100f);
                ItemContainer.GetComponent<GridLayoutGroup>().cellSize = new Vector2(50f, 50f);
                ItemContainer.GetComponent<GridLayoutGroup>().spacing = new Vector2(8f, 8f);
                ItemContainer.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
                ItemContainer.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
                ItemContainer.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                ItemContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(-16f, 0);
                ItemContainer.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                listGainedImages.Clear();
                listLostImages.Clear();

                for (int i = 0; i < 10; i++)
                {
                    var gainedItems = new GameObject("GainedItems" + i);

                    gainedItems.transform.SetParent(ItemContainer.transform, false);
                    //ModExpBarGroup.transform.position = new Vector3(0,0,0);

                    gainedItems.AddComponent<RectTransform>();

                    gainedItems.GetComponent<RectTransform>().anchorMin = new Vector2(0.0f, (float)(0.20 + ((float)i * 0.04)));
                    gainedItems.GetComponent<RectTransform>().anchorMax = new Vector2(0.03f, (float)(0.24 + ((float)i * 0.04)));
                    gainedItems.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                    gainedItems.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    //gainedItems.AddComponent<Image>();
                    //gainedItems.GetComponent<Image>().sprite = Resources.Load<Sprite>("textures/itemicons/bg");
                    gainedItems.AddComponent<NetworkIdentity>().serverOnly = false;
                    listGainedImages.Add(gainedItems);

                    var lostItems = new GameObject("LostItems" + i);

                    lostItems.transform.SetParent(ModCanvas.transform, false);
                    //ModExpBarGroup.transform.position = new Vector3(0,0,0);

                    lostItems.AddComponent<RectTransform>();
                    lostItems.GetComponent<RectTransform>().anchorMin = new Vector2(0.97f, (float)(0.20 + ((float)i * 0.04)));
                    lostItems.GetComponent<RectTransform>().anchorMax = new Vector2(1.00f, (float)(0.24 + ((float)i * 0.04)));
                    lostItems.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                    lostItems.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    lostItems.AddComponent<NetworkIdentity>().serverOnly = false;

                    //lostItems.AddComponent<Image>();
                    //lostItems.GetComponent<Image>().sprite = Resources.Load<Sprite>("textures/itemicons/bg");
                    listLostImages.Add(lostItems);

                    //ModExpBarGroup.AddComponent<Text
                }
                //Debug.LogError("i'm here");
                //Debug.LogError ("ArtifactOfDoomConfig.useArtifactOfSacreficeCalculation.Value"+ArtifactOfDoomConfig.useArtifactOfSacreficeCalculation.Value);
                //Debug.LogError("ArtifactOfDoomConfig.disableItemProgressBar.Value"+ArtifactOfDoomConfig.disableItemProgressBar.Value);
                //Debug.LogError("ArtifactOfDoom.artifactIsActive " +ArtifactOfDoom.artifactIsActive);

                if (!ArtifactOfDoomConfig.disableItemProgressBar.Value && !calculationSacrifice)
                {
                    ItemGainFrame = new GameObject("ItemGainFrame");
                    ItemGainFrame.AddComponent<RectTransform>();
                    ItemGainFrame.GetComponent<RectTransform>().anchorMin = new Vector2(0.35f, 0.05f);
                    ItemGainFrame.GetComponent<RectTransform>().anchorMax = new Vector2(0.65f, 0.06f);
                    ItemGainFrame.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                    ItemGainFrame.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    ItemGainFrame.AddComponent<NetworkIdentity>().serverOnly = false;
                    ItemGainFrame.AddComponent<Image>();
                    ItemGainFrame.GetComponent<Image>().color = new Color(255, 0, 0, 0.1f);

                    //Debug.LogError("!ArtifactOfDoomConfig.useArtifactOfSacreficeCalculation.Value && !ArtifactOfDoomConfig.disableItemProgressBar.Value");
                    ItemGainBar = new GameObject("ItemGainBar");
                    ItemGainBar.transform.SetParent(ItemGainFrame.transform, false);
                    ItemGainBar.AddComponent<RectTransform>();
                    ItemGainBar.GetComponent<RectTransform>().anchorMin = new Vector2(0.35f, 0.05f);
                    ItemGainBar.GetComponent<RectTransform>().anchorMax = new Vector2(0.35f, 0.06f);
                    ItemGainBar.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                    ItemGainBar.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    ItemGainBar.AddComponent<NetworkIdentity>().serverOnly = false;
                    ItemGainBar.AddComponent<Image>();
                    ItemGainBar.GetComponent<Image>().color = new Color(255, 255, 255, 0.3f);
                }
            }
            catch (Exception)
            {
                Debug.Log($"[SirHamburger Error] while Adding UI elements");
            }
        }

        private void SetUpModCanvas()
        {
            ModCanvas = new GameObject("UIModifierCanvas");
            ModCanvas.layer = 5;
            ModCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            ModCanvas.GetComponent<Canvas>().sortingOrder = -1; // Required or the UI will render over pause and tooltips.
            ModCanvas.AddComponent<GraphicRaycaster>();
            ModCanvas.AddComponent<MPEventSystemProvider>().fallBackToMainEventSystem = true;
            ModCanvas.AddComponent<MPEventSystemLocator>();
        }

        private void RemoveItem(On.RoR2.Inventory.orig_RemoveItem orig, Inventory self, ItemIndex itemindex1, int ItemIndex2)
        {
            orig(self, itemindex1, ItemIndex2);
        }

        public static IRpcFunc<string, string> AddGainedItemsToPlayers { get; set; }
        public static IRpcFunc<string, string> AddLostItemsOfPlayers { get; set; }
        public static IRpcFunc<string, string> UpdateProgressBar { get; set; }
        public static IRpcFunc<bool, string> IsArtifactActive { get; set; }
        public static IRpcFunc<bool, string> IsCalculationSacrifice { get; set; }

        public const string ModVer = "1.0.0";
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
                string[] QueueGainedItemSprite = QueueGainedItemSpriteToString.Split(' ');

                int i = 0;
                foreach (var element in QueueGainedItemSprite)
                {
                    if (element != "")
                    {

                        if (listGainedImages[i].GetComponent<Image>() == null)
                            listGainedImages[i].AddComponent<Image>();
                        listGainedImages[i].GetComponent<Image>().sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(element)).pickupIconSprite;

                        i++;
                    }
                }
                return "dummie";
            });

            AddLostItemsOfPlayers = miniRpc.RegisterFunc(Target.Client, (NetworkUser user, string QueueLostItemSpriteToString) => //--------------------HierSTuffMachen!!
            {
                string[] QueueLostItemSprite = QueueLostItemSpriteToString.Split(' ');

                int i = 0;
                foreach (var element in QueueLostItemSprite)
                {
                    if (element != "")
                    {
                        if (listLostImages[i].GetComponent<Image>() == null)
                            listLostImages[i].AddComponent<Image>();
                        listLostImages[i].GetComponent<Image>().sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(element)).pickupIconSprite;

                        i++;
                    }

                }
                return "dummie";
            });

            UpdateProgressBar = miniRpc.RegisterFunc(Target.Client, (NetworkUser user, string killedNeededEnemies) => //--------------------HierSTuffMachen!!
            {
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
                int enemiesKilled = Convert.ToInt32(stringkilledNeededEnemies[0]);
                int enemiesNeeded = Convert.ToInt32(stringkilledNeededEnemies[1]) + 2;
                //Debug.LogError("in line 279");
                double progress = enemiesKilled / enemiesNeeded;
                //                  Debug.LogError("in line 2282");
                if ((0.35f + (float)(progress * 0.3)) > 0.65f)
                    ItemGainBar.GetComponent<RectTransform>().anchorMax = new Vector2(0.65f, 0.06f);
                else
                {
                    ItemGainBar.GetComponent<RectTransform>().anchorMin = new Vector2(0.35f, 0.05f);
                    //                    Debug.LogError("in line 288");
                    ItemGainBar.GetComponent<RectTransform>().anchorMax = new Vector2(0.35f + (float)(progress * 0.3), 0.06f);
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
            On.RoR2.UI.HUD.Awake -= HudAwake;
            On.RoR2.Inventory.RemoveItem -= RemoveItem;
        }
    }
}
