using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using TILER2;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using System.IO;




namespace ThinkInvisible.TinkersSatchel
{
    public class Danger : Artifact<Danger>
    {
        public override string displayName => "Artifact of Danger";

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangDesc(string langid = null) => "Players can be killed in one hit.";
        private static List<string> Playername = new List<string>();
        private static List<int> counter = new List<int>();
        private Transform HUDroot = null;

        private GameObject GameObjectReference;


        public void Awake()
        {
            Chat.AddMessage("Loaded MyModName!");

            On.RoR2.UI.HealthBar.Awake += myFunc;

        }
        private void myFunc(On.RoR2.UI.HealthBar.orig_Awake orig, RoR2.UI.HealthBar self)
        {
            orig(self); // Don't forget to call this, or the vanilla / other mods' codes will not execute!
            HUDroot = self.transform.root; // This will return the canvas that the UI is displaying on!
                                           // Rest of the code is to go here
            
                    }
        private void OnDestroy()
        {
            On.RoR2.UI.HealthBar.Awake -= myFunc;
        }

        public Danger()
        {
            iconPathName = "@TinkersSatchel:Assets/TinkersSatchel/Textures/Icons/danger_on.png";
            iconPathNameDisabled = "@TinkersSatchel:Assets/TinkersSatchel/Textures/Icons/danger_off.png";



        }

        protected override void LoadBehavior()
        {
            On.RoR2.SceneDirector.PopulateScene += (orig, self) =>
              {
                  orig(self);
              };
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
            {

                if (!Playername.Contains(damageReport.attackerBody.name))
                {
                    Playername.Add(damageReport.attackerBody.name);
                    counter.Add(0);
                }
                if (counter[Playername.IndexOf(damageReport.attackerBody.name)] != 3)
                {
                    counter[Playername.IndexOf(damageReport.attackerBody.name)]++;
                }
                else
                {

                    damageReport.attackerBody.inventory.GiveRandomItems(1);
                    counter[Playername.IndexOf(damageReport.attackerBody.name)] = 0;
                }
                orig(self, damageReport);

            };
            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageinfo) =>
            {
                if (self.body.isPlayerControlled&& (self.body.inventory.GetTotalItemCountOfTier(ItemTier.Tier1)>0) && damageinfo.inflictor!=damageinfo.attacker)
                {
                    List<ItemIndex> lstItemIndex = new List<ItemIndex>();
                    foreach (var element in ItemCatalog.allItems)
                    {
                        if (self.body.inventory.GetItemCount(element) > 0)
                        {
                            lstItemIndex.Add(element);
                        }
                    }
                    var rand = new System.Random();
                    int randomPosition = rand.Next(0, lstItemIndex.Count - 1);
                    ItemIndex itemToRemove = lstItemIndex[randomPosition];
                    //try{

                    //RoR2.Console.print("---------------------Startet setting GameObject---------------------");
//
                    //GameObjectReference = new GameObject("GameObjectName");
                    //GameObjectReference.transform.SetParent(HUDroot);
                    //GameObjectReference.AddComponent<RectTransform>();
                    //GameObjectReference.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                    //GameObjectReference.GetComponent<RectTransform>().anchorMax = Vector2.one;
                    //GameObjectReference.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                    //GameObjectReference.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;  
                    //GameObjectReference.AddComponent<Image>();
                    //GameObjectReference.GetComponent<Image>().sprite = Resources.Load<Sprite>("textures/itemicons/texBearIcon");

                    //GameObjectReference.GetComponent<Image>().sprite = ItemCatalog.GetItemDef(itemToRemove).pickupIconSprite;
                    //}
                    //catch(Exception e)
                    //{
                    //    RoR2.Console.print(e.Message);
                    //}
                    
                    if(!ItemCatalog.lunarItemList.Contains(itemToRemove))
                        self.body.inventory.RemoveItem(itemToRemove, 1);
                    //counter[Playername.IndexOf(self.body.name)] = 0; ;


                }
                orig(self, damageinfo);
            };
        }

        protected override void UnloadBehavior()
        {


        }
    }
}