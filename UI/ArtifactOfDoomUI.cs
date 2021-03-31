using BepInEx;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



#region TODO:
/*

*/
#endregion

namespace ArtifactOfDoom
{
    public class ArtifactOfDoomUI
    {
        public GameObject ModCanvas = null;
        public static bool ArtifactIsActive = false;
        public static bool calculationSacrifice = false;
        public ArtifactOfDoomUI()
        {
            //On.RoR2.UI.HealthBar.Awake += ExpBarAwakeAddon;
            On.RoR2.UI.HUD.Awake += HUDAwake;


            try
            {
                //SetUpMiniRPC();
            }
            catch (Exception)
            {
                Debug.LogError($"[SirHamburger] Error in SetUpMiniRPC");
            }
            On.RoR2.Inventory.RemoveItem_ItemIndex_int+= (orig, self, itemindex1, ItemIndex2) =>
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
                        listGainedImages.Add(ModExpBarGroup);


                        ModExpBarGroup = new GameObject("LostItems" + i);

                        ModExpBarGroup.transform.SetParent(ModCanvas.transform);

                        ModExpBarGroup.AddComponent<RectTransform>();
                        ModExpBarGroup.GetComponent<RectTransform>().anchorMin = new Vector2((float)1.0-baseSize, (float)(0.20 + ((float)i * baseSizeYPlusMargin)));
                        ModExpBarGroup.GetComponent<RectTransform>().anchorMax = new Vector2(1.00f, (float)(0.2+baseSizeY + ((float)i * baseSizeYPlusMargin)));
                        ModExpBarGroup.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                        ModExpBarGroup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

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



        // public static IRpcFunc<string, string> AddGainedItemsToPlayers { get; set; }
        // public static IRpcFunc<string, string> AddLostItemsOfPlayers { get; set; }
        // public static IRpcFunc<string, string> UpdateProgressBar { get; set; }
        // public static IRpcFunc<bool, string> IsArtifactActive { get; set; }
        // public static IRpcFunc<bool, string> IsCalculationSacrifice { get; set; }

        public const string ModVer = "1.2.0";
        public const string ModName = "ArtifactOfDoom";
        public const string ModGuid = "com.SirHamburger.ArtifactOfDoom";


        private void OnDestroy()
        {
            On.RoR2.UI.HUD.Awake -= HUDAwake;
           // On.RoR2.Inventory.RemoveItem -= RemoveItem;
        }
    }
}
