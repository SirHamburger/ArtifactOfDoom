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
        private static List<short> Playername = new List<short>();
        private static List<int> counter = new List<int>();
        private int currentStage = 0;

        private static RoR2.Stats.StatDef statsLostItems;
        private static RoR2.Stats.StatDef statsGainItems;

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

        Playername = new List<short>();
        counter = new List<int>();
        currentStage = 0;

        statsLostItems=null;
       statsGainItems=null;


                        statsLostItems= RoR2.Stats.StatDef.Register("Lostitems",RoR2.Stats.StatRecordType.Sum,RoR2.Stats.StatDataType.ULong,0,null);
            statsGainItems= RoR2.Stats.StatDef.Register("Gainitems",RoR2.Stats.StatRecordType.Sum,RoR2.Stats.StatDataType.ULong,0,null);


            On.RoR2.UI.GameEndReportPanelController.Awake+=(orig,self)=>
            {
                orig(self);
                if(!this.IsActiveAndEnabled())
                {
                    return;
                }
                string[] information = new string[self.statsToDisplay.Length+2];
                self.statsToDisplay.CopyTo(information,0);
                information[information.Length-2] = "Lostitems";
                information[information.Length-1] = "Gainitems";
                self.statsToDisplay = information;
            };
            On.RoR2.SceneDirector.PopulateScene += (orig, self) =>
              {
                  currentStage=RoR2.Run.instance.stageClearCount;

                  orig(self);
              };
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
            {

                if(!this.IsActiveAndEnabled())
                {
                    orig(self,damageReport);
                    return;
                }




                if (damageReport.attackerOwnerMaster != null)
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

                if (damageReport.attackerOwnerMaster != null)
                {
                    currentPlayerID = damageReport.attackerOwnerMaster.GetBody().playerControllerId;
                    RoR2.Console.print("pet master : " + currentPlayerID);
                }
                else
                {
                    currentPlayerID = damageReport.attackerBody.playerControllerId;
                    RoR2.Console.print("master : " + currentPlayerID);
                }

                int totalItems = damageReport.attackerBody.inventory.GetTotalItemCountOfTier(ItemTier.Tier1);
                totalItems += damageReport.attackerBody.inventory.GetTotalItemCountOfTier(ItemTier.Tier2);
                totalItems += damageReport.attackerBody.inventory.GetTotalItemCountOfTier(ItemTier.Tier3);
                int calculatesEnemyCountToTrigger = totalItems - currentStage * 2;
                if (calculatesEnemyCountToTrigger < 1)
                    calculatesEnemyCountToTrigger = 1;
                RoR2.Console.print("calculatesEnemyCountToTrigger: " + calculatesEnemyCountToTrigger);
                if (counter[Playername.IndexOf(currentPlayerID)] != calculatesEnemyCountToTrigger)
                {
                    counter[Playername.IndexOf(currentPlayerID)]++;
                }
                else
                {
                    if (damageReport.attackerOwnerMaster != null)
                    {
                        damageReport.attackerOwnerMaster.GetBody().inventory.GiveRandomItems(1);
                        PlayerStatsComponent.FindBodyStatSheet(damageReport.attackerOwnerMaster.GetBody()).PushStatValue(statsGainItems, 1UL);
                    }

                    else
                    {
                        damageReport.attackerBody.inventory.GiveRandomItems(1);
                        PlayerStatsComponent.FindBodyStatSheet(damageReport.attackerBody).PushStatValue(statsGainItems, 1UL);
                    }

                    counter[Playername.IndexOf(currentPlayerID)] = 0;

                }
                orig(self, damageReport);

            };
            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageinfo) =>
            {
                 if(!this.IsActiveAndEnabled())
                {
                    orig(self,damageinfo);
                    return;
                }
                int totalItems = self.body.inventory.GetTotalItemCountOfTier(ItemTier.Tier1);
                totalItems += self.body.inventory.GetTotalItemCountOfTier(ItemTier.Tier2);
                totalItems += self.body.inventory.GetTotalItemCountOfTier(ItemTier.Tier3);

                RoR2.Console.print("totalItems " + totalItems);
                RoR2.Console.print("self.body.isPlayerControlled " + self.body.isPlayerControlled);
                RoR2.Console.print("damageinfo.inflictor.name " + self.name);
                RoR2.Console.print("damageinfo.attacker.name " + damageinfo.attacker.name);

                //RoR2.Console.print("intakedamage");
                if (self.body.isPlayerControlled && (totalItems > 0) && self.name != damageinfo.attacker.name)
                {
                    RoR2.Console.print("inif");
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

                    //RoR2.Console.print("preparing to remove");
                    if (!ItemCatalog.lunarItemList.Contains(itemToRemove))
                    {
                        //RoR2.Console.print("remove item");
                        self.body.inventory.RemoveItem(itemToRemove, 1);
                        PlayerStatsComponent.FindBodyStatSheet(self.body).PushStatValue(statsLostItems, 1UL);
                    }
                    else
                    {
                        RoR2.Console.print("lunar");
                    }


                }
                orig(self, damageinfo);
            };
        }

        protected override void UnloadBehavior()
        {


        }
    }
}