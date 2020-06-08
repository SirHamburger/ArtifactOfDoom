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

using RoR2.Stats;
using System.Collections;
using static TILER2.MiscUtil;

using static TILER2.StatHooks;



using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using System.IO;

using UnityEngine.Networking;
using R2API;





namespace ThinkInvisible.TinkersSatchel
{
    public class Danger : Artifact<Danger>
    {
        public override string displayName => "Artifact of Danger";

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangDesc(string langid = null) => "Players can be killed in one hit.";
        private static List<CharacterBody> Playername = new List<CharacterBody>();
        private static List<int> counter = new List<int>();
        private int currentStage = 0;

        private static RoR2.Stats.StatDef statsLostItems;
        private static RoR2.Stats.StatDef statsGainItems;
        private static Queue<Sprite>  QueueLostItemSprite = new Queue<Sprite>();
        private static Queue<Sprite>  QueueGainedItemSprite = new Queue<Sprite>();


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

            Playername = new List<CharacterBody>();
            counter = new List<int>();
            currentStage = 0;

            statsLostItems = null;
            statsGainItems = null;


            statsLostItems = RoR2.Stats.StatDef.Register("Lostitems", RoR2.Stats.StatRecordType.Sum, RoR2.Stats.StatDataType.ULong, 0, null);
            statsGainItems = RoR2.Stats.StatDef.Register("Gainitems", RoR2.Stats.StatRecordType.Sum, RoR2.Stats.StatDataType.ULong, 0, null);


            
            On.RoR2.UI.GameEndReportPanelController.Awake += (orig, self) =>
              {
                  orig(self);
                  if (!this.IsActiveAndEnabled())
                  {
                      return;
                  }
                  string[] information = new string[self.statsToDisplay.Length + 2];
                  self.statsToDisplay.CopyTo(information, 0);
                  information[information.Length - 2] = "Lostitems";
                  information[information.Length - 1] = "Gainitems";
                  self.statsToDisplay = information;
              };
            On.RoR2.SceneDirector.PopulateScene += (orig, self) =>
              {
                  currentStage = RoR2.Run.instance.stageClearCount+1;

                  orig(self);
              };
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
            {
                try
                {
                    if (!this.IsActiveAndEnabled())
                    {
                        orig(self, damageReport);
                        return;
                    }




                    if (damageReport.attackerOwnerMaster != null)
                    {
                        if (!Playername.Contains(damageReport.attackerBody))
                        {
                            Playername.Add(damageReport.attackerOwnerMaster.GetBody());
                            counter.Add(0);
                        }

                    }
                    if (!Playername.Contains(damageReport.attackerBody))
                    {
                        Playername.Add(damageReport.attackerBody);
                        counter.Add(0);
                    }
                    CharacterBody currentBody;

                    if (damageReport.attackerOwnerMaster != null)
                    {
                        currentBody = damageReport.attackerOwnerMaster.GetBody();
                        // //Ror2.console.print("pet master : " + currentPlayerID);
                    }
                    else
                    {
                        currentBody = damageReport.attackerBody;
                        // //Ror2.console.print("master : " + currentPlayerID);
                    }

                    int totalItems = damageReport.attackerBody.inventory.GetTotalItemCountOfTier(ItemTier.Tier1);
                    totalItems += damageReport.attackerBody.inventory.GetTotalItemCountOfTier(ItemTier.Tier2);
                    totalItems += damageReport.attackerBody.inventory.GetTotalItemCountOfTier(ItemTier.Tier3);
                    int calculatesEnemyCountToTrigger = totalItems - currentStage * 2;
                    if (calculatesEnemyCountToTrigger < 1)
                        calculatesEnemyCountToTrigger = 1;
                    //Ror2.console.print("calculatesEnemyCountToTrigger: " + calculatesEnemyCountToTrigger);
                    if (counter[Playername.IndexOf(currentBody)] <= calculatesEnemyCountToTrigger)
                    {
                        counter[Playername.IndexOf(currentBody)]++;
                    }
                    else
                    {
                        if (damageReport.attackerOwnerMaster != null)
                        {
                            damageReport.attackerOwnerMaster.GetBody().inventory.GiveRandomItems(1);
                            
                            PlayerStatsComponent.FindBodyStatSheet(damageReport.attackerOwnerMaster.GetBody()).PushStatValue(statsGainItems, 1UL);
                            QueueGainedItemSprite.Enqueue(ItemCatalog.GetItemDef(damageReport.attackerOwnerMaster.GetBody().inventory.itemAcquisitionOrder[damageReport.attackerOwnerMaster.GetBody().inventory.itemAcquisitionOrder.Count-1]).pickupIconSprite);
                        }

                        else
                        {
                            damageReport.attackerBody.inventory.GiveRandomItems(1);
                            PlayerStatsComponent.FindBodyStatSheet(damageReport.attackerBody).PushStatValue(statsGainItems, 1UL);

                            QueueGainedItemSprite.Enqueue(ItemCatalog.GetItemDef(damageReport.attackerBody.inventory.itemAcquisitionOrder[damageReport.attackerBody.inventory.itemAcquisitionOrder.Count-1]).pickupIconSprite);

                        }
                        
                        //RoR2.Console.print("ListGainItems.Count" + ListGainItems.Count);
                        //int j =9;
                        //for(int i = ListGainItems.Count-1; i > ListGainItems.Count-10 && i >=0; i--)
                        //{
                        //    MainUIMod.listGainedImages[j].GetComponent<Image>().sprite = ItemCatalog.GetItemDef(ListGainItems[i]).pickupIconSprite;
                        //    j--;
                        //}

                            if(QueueGainedItemSprite.Count >10)
                                QueueGainedItemSprite.Dequeue();
                            //int i=QueueGainedItemSprite.Count -1;
                            int i= 0;
                            foreach(var element in QueueGainedItemSprite)
                            {
                                //ModExpBarGroup.AddComponent<Image>();
                                if( MainUIMod.listGainedImages[i].GetComponent<Image>()== null )
                                    MainUIMod.listGainedImages[i].AddComponent<Image>();
                                MainUIMod.listGainedImages[i].GetComponent<Image>().sprite = element;
                                i++;
                            }

                        counter[Playername.IndexOf(currentBody)] = 0;

                    }
                }
                catch (Exception e)
                {
                    RoR2.Console.print("Error " + e.Message);
                }
                orig(self, damageReport);

            };
            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageinfo) =>
            {
                try
                {
                    if (!this.IsActiveAndEnabled())
                    {
                        orig(self, damageinfo);
                        return;
                    }
                    int totalItems = self.body.inventory.GetTotalItemCountOfTier(ItemTier.Tier1);
                    totalItems += self.body.inventory.GetTotalItemCountOfTier(ItemTier.Tier2);
                    totalItems += self.body.inventory.GetTotalItemCountOfTier(ItemTier.Tier3);

                    //Ror2.console.print("totalItems " + totalItems);
                    //Ror2.console.print("self.body.isPlayerControlled " + self.body.isPlayerControlled);
                    //Ror2.console.print("damageinfo.inflictor.name " + self.name);
                    //Ror2.console.print("damageinfo.attacker.name " + damageinfo.attacker.name);

                    ////Ror2.console.print("intakedamage");
                    if (self.body.isPlayerControlled && (totalItems > 0) && self.name != damageinfo.attacker.name)
                    {
                        //Ror2.console.print("inif");
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

                        ////Ror2.console.print("preparing to remove");
                        if (!ItemCatalog.lunarItemList.Contains(itemToRemove) && (ItemCatalog.GetItemDef(itemToRemove).tier!=ItemTier.NoTier))
                        { 
                            ////Ror2.console.print("remove item");
                            self.body.inventory.RemoveItem(itemToRemove, 1);
                            PlayerStatsComponent.FindBodyStatSheet(self.body).PushStatValue(statsLostItems, 1UL);
                            QueueLostItemSprite.Enqueue(ItemCatalog.GetItemDef(itemToRemove).pickupIconSprite);
                            if(QueueLostItemSprite.Count >10)
                                QueueLostItemSprite.Dequeue();
                            //int i=QueueLostItemSprite.Count -1;
                            int i= 0;
                            foreach(var element in QueueLostItemSprite)
                            {
                                if( MainUIMod.listLostImages[i].GetComponent<Image>()== null )
                                    MainUIMod.listLostImages[i].AddComponent<Image>();
                                MainUIMod.listLostImages[i].GetComponent<Image>().sprite = element;
                                i++;
                            }

                        }
                        else
                        {
                            //Ror2.console.print("lunar");
                        }


                    }
                }
                catch (Exception e)
                {

                }
                orig(self, damageinfo);
            };
        }

        protected override void UnloadBehavior()
        {


        }
    }
}