using System.Reflection;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/*
	Disclaimer:
	Despite it all
	I have no idea what I'm doing to be very honest.
*/

#region TODO:
/*

*/
#endregion

namespace ThinkInvisible.TinkersSatchel
{

    [R2API.Utils.R2APISubmoduleDependency("ResourcesAPI")]
    [BepInPlugin("com.ohway.UIMod", "UI Modifier", "1.0")]
    public class MainUIMod : BaseUnityPlugin
    {
        public GameObject ModCanvas = null;
        void Awake()
        {
            RoR2.Console.print("-------------------------yea i'm here----------------------------------------------");
            On.RoR2.UI.ExpBar.Awake += ExpBarAwakeAddon;
            //On.RoR2.LocalUserManager.
        }

        private void SetUpModCanvas()
        {
            if (ModCanvas == null)
            {
                ModCanvas = new GameObject("UIModifierCanvas");
                ModCanvas.AddComponent<Canvas>();
                ModCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                if (VanillaExpBarRoot != null)
                {
                    ModCanvas.GetComponent<Canvas>().worldCamera = VanillaExpBarRoot.transform.root.gameObject.GetComponent<Canvas>().worldCamera;
                }

                ModCanvas.AddComponent<CanvasScaler>();
                ModCanvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                ModCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
                ModCanvas.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            }
        }


        #region Exp bar GameObjects
        public GameObject VanillaExpBarRoot = null;
        public GameObject ModExpBarGroup = null;
        public static List<GameObject> listGainedImages = new List<GameObject>();
		public static List<GameObject> listLostImages = new List<GameObject>();
        #endregion
        public void ExpBarAwakeAddon(On.RoR2.UI.ExpBar.orig_Awake orig, RoR2.UI.ExpBar self)
        {
            orig(self);
            var currentRect = self.gameObject.GetComponentsInChildren<RectTransform>();
            if (currentRect != null && VanillaExpBarRoot == null)
            {
                for (int i = 0; i < currentRect.Length; ++i)
                {
                    if (currentRect[i].name == "ExpBarRoot")
                    {
                        VanillaExpBarRoot = currentRect[i].gameObject;
                        MainExpBarStart();
                    }
                }
            }
        }

        private void MainExpBarStart()
        {
            if (VanillaExpBarRoot != null)
            {
                RoR2.Console.print("----------------MainEXPBarStat-------------------------------------");
                SetUpModCanvas();

                for (int i = 0; i < 10; i++)
                {
                    ModExpBarGroup = new GameObject("GainedItems" + i);

                    ModExpBarGroup.transform.SetParent(ModCanvas.transform);
                    //ModExpBarGroup.transform.position = new Vector3(0,0,0);
                    
                    
                    ModExpBarGroup.AddComponent<RectTransform>();
                    
                    ModExpBarGroup.GetComponent<RectTransform>().anchorMin = new Vector2(0.0f, (float)(0.20+((float)i*0.04)));
                    ModExpBarGroup.GetComponent<RectTransform>().anchorMax = new Vector2(0.03f, (float)(0.24+((float)i*0.04)));
                    ModExpBarGroup.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                    ModExpBarGroup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    //ModExpBarGroup.AddComponent<Image>();
                    //ModExpBarGroup.GetComponent<Image>().sprite = Resources.Load<Sprite>("textures/itemicons/bg");
					listGainedImages.Add(ModExpBarGroup);

                    ModExpBarGroup = new GameObject("LostItems" + i);

                    ModExpBarGroup.transform.SetParent(ModCanvas.transform);
                    //ModExpBarGroup.transform.position = new Vector3(0,0,0);

                    ModExpBarGroup.AddComponent<RectTransform>();
                    ModExpBarGroup.GetComponent<RectTransform>().anchorMin = new Vector2(0.97f, (float)(0.20+((float)i*0.04)));
                    ModExpBarGroup.GetComponent<RectTransform>().anchorMax = new Vector2(1.00f, (float)(0.24+((float)i*0.04)));
                    ModExpBarGroup.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                    ModExpBarGroup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    //ModExpBarGroup.AddComponent<Image>();
                    //ModExpBarGroup.GetComponent<Image>().sprite = Resources.Load<Sprite>("textures/itemicons/bg");
					listLostImages.Add(ModExpBarGroup);

//                    ModExpBarGroup.AddComponent<Text
                }



                //GameObjectReference = new GameObject("blablabla");
                //GameObjectReference.transform.SetParent(HUDroot);
                //GameObjectReference.AddComponent<RectTransform>();
                //GameObjectReference.GetComponent<RectTransform>().anchorMin = new Vector2(0,0);
                //GameObjectReference.GetComponent<RectTransform>().anchorMax = new Vector2((float)0.1,(float)0.1);
                //GameObjectReference.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                //GameObjectReference.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;



            }
        }


        void OnDestroy()
        {
            On.RoR2.UI.ExpBar.Awake -= ExpBarAwakeAddon;
        }
    }
}
