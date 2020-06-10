using System.Reflection;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.PlayerConnection;
using MiniRpcLib;
using MiniRpcLib.Action;
using MiniRpcLib.Func;

using UnityEditor;



using System;

using RoR2;


/*
	Disclaimer:
	Despite it all
	I have no idea what I'm doing to be very honest.
*/

#region TODO:
/*

*/
#endregion

namespace ArtefactOfDoom
{

    [R2API.Utils.R2APISubmoduleDependency("ResourcesAPI")]
    [BepInPlugin("com.ohway.UIMod", "UI Modifier", "1.0")]
    [BepInDependency(MiniRpcPlugin.Dependency)]
    public class ArtefactOfDoomUI : BaseUnityPlugin
    {
        public GameObject ModCanvas = null;
        void Awake()
        {
            On.RoR2.UI.ExpBar.Awake += ExpBarAwakeAddon;


            try
            {
                SetUpMiniRPC();
            }
            catch (Exception e)
            {
                Debug.Log($"[SirHamburger] Error in SetUpMiniRPC");
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
                try
                {
                    SetUpModCanvas();
                    listGainedImages.Clear();
                    listLostImages.Clear();


                    for (int i = 0; i < 10; i++)
                    {
                        ModExpBarGroup = new GameObject("GainedItems" + i);

                        ModExpBarGroup.transform.SetParent(ModCanvas.transform);
                        //ModExpBarGroup.transform.position = new Vector3(0,0,0);


                        ModExpBarGroup.AddComponent<RectTransform>();

                        ModExpBarGroup.GetComponent<RectTransform>().anchorMin = new Vector2(0.0f, (float)(0.20 + ((float)i * 0.04)));
                        ModExpBarGroup.GetComponent<RectTransform>().anchorMax = new Vector2(0.03f, (float)(0.24 + ((float)i * 0.04)));
                        ModExpBarGroup.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                        ModExpBarGroup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                        //ModExpBarGroup.AddComponent<Image>();
                        //ModExpBarGroup.GetComponent<Image>().sprite = Resources.Load<Sprite>("textures/itemicons/bg");
                        ModExpBarGroup.AddComponent<NetworkIdentity>().serverOnly = false;
                        listGainedImages.Add(ModExpBarGroup);

                        ModExpBarGroup = new GameObject("LostItems" + i);

                        ModExpBarGroup.transform.SetParent(ModCanvas.transform);
                        //ModExpBarGroup.transform.position = new Vector3(0,0,0);

                        ModExpBarGroup.AddComponent<RectTransform>();
                        ModExpBarGroup.GetComponent<RectTransform>().anchorMin = new Vector2(0.97f, (float)(0.20 + ((float)i * 0.04)));
                        ModExpBarGroup.GetComponent<RectTransform>().anchorMax = new Vector2(1.00f, (float)(0.24 + ((float)i * 0.04)));
                        ModExpBarGroup.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                        ModExpBarGroup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                        ModExpBarGroup.AddComponent<NetworkIdentity>();

                        //ModExpBarGroup.AddComponent<Image>();
                        //ModExpBarGroup.GetComponent<Image>().sprite = Resources.Load<Sprite>("textures/itemicons/bg");
                        listLostImages.Add(ModExpBarGroup);

                        //                    ModExpBarGroup.AddComponent<Text
                    }



                }
                catch (Exception e)
                {
                    Debug.Log($"[SirHamburger Error] while Adding UI elements");
                }
            }


        }

        // Define two actions sending/receiving a single string
        public IRpcAction<string> ExampleCommandHost { get; set; }
        public IRpcAction<string> ExampleCommandClient { get; set; }

        // Define two actions that manages reading/writing messages themselves
        public IRpcAction<Action<NetworkWriter>> ExampleCommandHostCustom { get; set; }
        public IRpcAction<Action<NetworkWriter>> ExampleCommandClientCustom { get; set; }

        // Define two functions of type `string Function(bool);`
        public IRpcFunc<bool, string> ExampleFuncClient { get; set; }
        public IRpcFunc<bool, string> ExampleFuncHost { get; set; }

        // Define two functions of type `ExampleObject Function(ExampleObject);`
        public static IRpcFunc<string, string> AddGainedItemsToPlayers { get; set; }
        public static IRpcFunc<string, string> AddLostItemsOfPlayers { get; set; }

        public const string ModVer = "0.8.0";
        public const string ModName = "ArtefactOfDoom";
        public const string ModGuid = "com.SirHamburger.ArtefactOfDoom";

        private void SetUpMiniRPC()
        {
            // Fix the damn in-game console stealing our not-in-game consoles output.
            // Not related to the demo, just very useful.
            On.RoR2.RoR2Application.UnitySystemConsoleRedirector.Redirect += orig => { };

            // Create a MiniRpcInstance that automatically registers all commands to our ModGuid
            // This lets us support multiple mods using the same command ID
            // We could also just generate new command ID's without "isolating" them by mod as well, so it would break if mod load order was different for different clients
            // I opted for the ModGuid instead of an arbitrary number or GUID to encourage mods not to set the same ID
            var miniRpc = MiniRpc.CreateInstance(ModGuid);

            // Define two commands, both transmitting a single string
            ExampleCommandHost = miniRpc.RegisterAction(Target.Server, (NetworkUser user, string x) => Debug.Log($"[Host] {user?.userName} sent us: {x}"));
            ExampleCommandClient = miniRpc.RegisterAction(Target.Client, (NetworkUser user, string x) => Debug.Log($"[Client] Host sent us: {x}"));


            AddGainedItemsToPlayers = miniRpc.RegisterFunc(Target.Client, (NetworkUser user, string QueueGainedItemSpriteToString) => //--------------------HierSTuffMachen!!
            {

                string[] QueueGainedItemSprite = QueueGainedItemSpriteToString.Split(' ');

                int i = 0;
                foreach (var element in QueueGainedItemSprite)
                {
                    if (element != "")
                    {

                        if (ArtefactOfDoomUI.listGainedImages[i].GetComponent<Image>() == null)
                            ArtefactOfDoomUI.listGainedImages[i].AddComponent<Image>();
                        ArtefactOfDoomUI.listGainedImages[i].GetComponent<Image>().sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(element)).pickupIconSprite;

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

                        if (ArtefactOfDoomUI.listLostImages[i].GetComponent<Image>() == null)
                            ArtefactOfDoomUI.listLostImages[i].AddComponent<Image>();
                        ArtefactOfDoomUI.listLostImages[i].GetComponent<Image>().sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(element)).pickupIconSprite;

                        i++;
                    }

                }
                return "dummie";
            });



            // The "_ ="'s above mean that the return value will be ignored. In your code you should assign the return value to something to be able to call the function.
        }
        enum CommandId
        {
            //                     ----|    This number is only needed because we already created an RpcFunc with ID 0 (the first one we made without an ID).
            SomeCommandName = 2345, // If you use IDs in your own code, you will most likely want to give all commands explicit IDs, which will avoid this issue.
            SomeOtherCommandName,
        }


    }
}
