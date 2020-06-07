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
        private static List<short> Playername = new List<short>();
        private static List<int> counter = new List<int>();
        private int currentStage=0;

        public void Awake()
        {
            Chat.AddMessage("Loaded MyModName!");



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
                    currentStage++;

                    orig(self);
              };
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
            {




                if(damageReport.attackerOwnerMaster!=null)
                {
                    if (!Playername.Contains(damageReport.attackerBody.playerControllerId))
                {
                    Playername.Add(damageReport.attackerOwnerMaster.GetBody().playerControllerId);
                    counter.Add(0);
                }
                
                }
                if (!Playername.Contains(damageReport.attackerBody.playerControllerId))
                {
                    Playername.Add(damageReport.attackerBody.playerControllerId);
                    counter.Add(0);
                }
                short currentPlayerID = -1;

                if(damageReport.attackerOwnerMaster!=null)
                {
                    currentPlayerID = damageReport.attackerOwnerMaster.GetBody().playerControllerId;
                    RoR2.Console.print("pet master : "+ currentPlayerID);
                }
                else
                {
                    currentPlayerID = damageReport.attackerBody.playerControllerId;
                    RoR2.Console.print("master : "+ currentPlayerID);
                }
                
                int totalItems = damageReport.attackerBody.inventory.GetTotalItemCountOfTier(ItemTier.Tier1);
                totalItems += damageReport.attackerBody.inventory.GetTotalItemCountOfTier(ItemTier.Tier2);
                 totalItems += damageReport.attackerBody.inventory.GetTotalItemCountOfTier(ItemTier.Tier3);
                int calculatesEnemyCountToTrigger =  totalItems-currentStage*2;
                if(calculatesEnemyCountToTrigger < 1)
                    calculatesEnemyCountToTrigger =1;
                RoR2.Console.print ("stage: " + currentStage);
                RoR2.Console.print ("calculatesEnemyCountToTrigger: " + calculatesEnemyCountToTrigger);
                if (counter[Playername.IndexOf(currentPlayerID)] != calculatesEnemyCountToTrigger)
                {
                    counter[Playername.IndexOf(currentPlayerID)]++;
                }
                else
                {
                    if(damageReport.attackerOwnerMaster!=null)
                        damageReport.attackerOwnerMaster.GetBody().inventory.GiveRandomItems(1);
                    else
                        damageReport.attackerBody.inventory.GiveRandomItems(1);
                    counter[Playername.IndexOf(currentPlayerID)] = 0;
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
                    //TinkersSatchelPlugin.GameObjectReference.AddComponent<Image>();
                    //TinkersSatchelPlugin.GameObjectReference.GetComponent<Image>().sprite = ItemCatalog.GetItemDef(itemToRemove).pickupIconSprite;

                    
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