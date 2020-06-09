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

using UnityEditor;




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
        public static bool debug = false;
        public override string displayName => "Artifact of Danger";

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangDesc(string langid = null) => "You get items on enemy kills but loose items every time you take damage.";
        private static List<CharacterBody> Playername = new List<CharacterBody>();
        private static List<int> counter = new List<int>();
        private int currentStage = 0;

        private Dictionary<NetworkUser, bool> LockNetworkUser = new Dictionary<NetworkUser, bool>();

        private static RoR2.Stats.StatDef statsLostItems;
        private static RoR2.Stats.StatDef statsGainItems;

        //public static Dictionary<CharacterBody, Queue<Sprite>>  PlayerItems = new Dictionary<CharacterBody, Queue<Sprite>>();
        //private static Queue<Sprite>  QueueLostItemSprite = new Queue<Sprite>();
        //private static Queue<Sprite>  QueueGainedItemSprite = new Queue<Sprite>();
        public static Dictionary<uint, Queue<ItemDef>> QueueLostItemSprite = new Dictionary<uint, Queue<ItemDef>>();
        public static Dictionary<uint, Queue<ItemDef>> QueueGainedItemSprite = new Dictionary<uint, Queue<ItemDef>>();


        public void Awake()
        {
            Chat.AddMessage("Loaded MyModName!");




        }


        public Danger()
        {
            Debug.Log($"[SirHamburger Danger] Called Constructor");

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
                    currentStage = RoR2.Run.instance.stageClearCount + 1;

                    orig(self);
                    QueueLostItemSprite = new Dictionary<uint, Queue<ItemDef>>();
                    QueueGainedItemSprite = new Dictionary<uint, Queue<ItemDef>>();
                    Playername = new List<CharacterBody>();
                    counter = new List<int>();
                    LockNetworkUser.Clear();

                };
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
            {
                //try
                //{
                orig(self, damageReport);

                if (!this.IsActiveAndEnabled())
                {
                    return;
                }
                if (debug)
                    Debug.LogError("Line 129");
                if (damageReport.attackerBody == null)
                    return;
                if (damageReport.attackerBody.inventory == null)
                    return;
                if (damageReport.victimBody.inventory == null)
                    return;
                if (debug)
                    Debug.LogError("Line 135");
                if (damageReport.attackerOwnerMaster != null)
                {
                    if (!Playername.Contains(damageReport.attackerBody))
                    {
                        Playername.Add(damageReport.attackerOwnerMaster.GetBody());
                        counter.Add(0);
                    }

                    if (debug)
                        Debug.LogError("Line 146");

                }
                if (!Playername.Contains(damageReport.attackerBody))
                {
                    Playername.Add(damageReport.attackerBody);
                    counter.Add(0);
                }
                CharacterBody currentBody;
                if (debug)
                    Debug.LogError("Line 156");
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

                if (debug)
                    Debug.LogError("Line 168");
                uint pos = 0;
                int totalItems = damageReport.attackerBody.inventory.GetTotalItemCountOfTier(ItemTier.Tier1);
                totalItems += damageReport.attackerBody.inventory.GetTotalItemCountOfTier(ItemTier.Tier2);
                totalItems += damageReport.attackerBody.inventory.GetTotalItemCountOfTier(ItemTier.Tier3);
                int calculatesEnemyCountToTrigger = (totalItems - currentStage * 2) * 2;
                if (calculatesEnemyCountToTrigger < 1)
                    calculatesEnemyCountToTrigger = 1;
                if (debug)
                    Debug.LogError("Line 177");
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
                        //for(int j= 0; j< QueueGainedItemSprite.Count; j++)
                        if (QueueGainedItemSprite.ContainsKey(damageReport.attackerOwnerMaster.GetBody().netId.Value))
                            pos = damageReport.attackerOwnerMaster.GetBody().netId.Value;
                        else
                        {

                            QueueGainedItemSprite.Add(damageReport.attackerOwnerMaster.GetBody().netId.Value, new Queue<ItemDef>());
                            pos = damageReport.attackerOwnerMaster.GetBody().netId.Value;

                        }
                        QueueGainedItemSprite[pos].Enqueue(ItemCatalog.GetItemDef(damageReport.attackerOwnerMaster.GetBody().inventory.itemAcquisitionOrder[damageReport.attackerOwnerMaster.GetBody().inventory.itemAcquisitionOrder.Count - 1]));
                    }
                    else
                    {


                        damageReport.attackerBody.inventory.GiveRandomItems(1);
                        PlayerStatsComponent.FindBodyStatSheet(damageReport.attackerBody).PushStatValue(statsGainItems, 1UL);
                        if (QueueGainedItemSprite.ContainsKey(damageReport.attackerBody.netId.Value))
                            pos = damageReport.attackerBody.netId.Value;
                        else
                        {
                            try
                            {
                                QueueGainedItemSprite.Add(damageReport.attackerBody.netId.Value, new Queue<ItemDef>());
                                pos = damageReport.attackerBody.netId.Value;
                            }
                            catch (Exception e)
                            {
                                Debug.Log($"[SirHamburger Danger] Error while excecuting : QueueGainedItemSprite.Add(damageReport.attackerBody.netId.Value, new Queue<Sprite>()); (line 203)");
                            }
                        }



                        QueueGainedItemSprite[pos].Enqueue(ItemCatalog.GetItemDef(damageReport.attackerBody.inventory.itemAcquisitionOrder[damageReport.attackerBody.inventory.itemAcquisitionOrder.Count - 1]));
                        Debug.Log($"[SirHamburger Danger] length of Queue: " + QueueGainedItemSprite[pos].Count);

                    }

                    if (QueueGainedItemSprite[pos].Count > 10)
                        QueueGainedItemSprite[pos].Dequeue();

                    Debug.Log("[--------SirHamburger------] Body net id: " + damageReport.attackerBody.netId.Value);

                    string temp = "";
                    foreach (var element in Danger.QueueGainedItemSprite[pos])
                    {
                        temp += element.name + " ";
                    }

                    NetworkUser tempNetworkUser = null;
                    foreach (var element in NetworkUser.readOnlyInstancesList)
                    {
                        Debug.Log($"[Sirhamburger] Comparing " + element.GetCurrentBody().netId + " to " + damageReport.attackerBody.networkIdentity.netId);
                        if (damageReport.attackerOwnerMaster != null)
                            if (element.GetCurrentBody().netId == damageReport.attackerOwnerMaster.GetBody().netId)
                                tempNetworkUser = element;
                            else
                            if (element.GetCurrentBody().netId == damageReport.attackerBody.netId)
                                tempNetworkUser = element;
                    }
                    if (tempNetworkUser == null)
                        Debug.Log($"[Sirhamburger] TempNetworUser == null");

                    MainUIMod.AddGainedItemsToPlayers.Invoke(temp, result =>
                        {
                            Debug.Log($"[Sirhamburger] addet items: {result}");
                        }, tempNetworkUser);

                    counter[Playername.IndexOf(currentBody)] = 0;

                }
                //}
                //catch (Exception e)
                //{
                //    RoR2.Console.print(e.Message);
                //
                //}

            };
            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageinfo) =>
            {
                orig(self, damageinfo);
                if (!this.IsActiveAndEnabled())
                {

                    return;
                }
                if (self.body.inventory == null)
                    return;

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
                    if (!ItemCatalog.lunarItemList.Contains(itemToRemove) && (ItemCatalog.GetItemDef(itemToRemove).tier != ItemTier.NoTier))
                    {
                        ////Ror2.console.print("remove item");
                        Debug.Log($"[SirHamburger Danger] Pre remove item");

                        Debug.Log($"[SirHamburger Danger] Plan to remove: " + ItemCatalog.GetItemDef(itemToRemove).name);
                        self.body.inventory.RemoveItem(itemToRemove, 1);
                        Debug.Log($"[SirHamburger Danger] PlayerStatsComponent.FindBodyStatSheet");

                        PlayerStatsComponent.FindBodyStatSheet(self.body).PushStatValue(statsLostItems, 1UL);

                        uint pos = 50000;
                        Debug.Log($"[SirHamburger Danger] QueueLostItemSprite.ContainsKey");
                        if (QueueLostItemSprite.ContainsKey(self.body.netId.Value))
                            pos = self.body.netId.Value;
                        else
                        {
                            try
                            {
                                QueueLostItemSprite.Add(self.body.netId.Value, new Queue<ItemDef>());
                                pos = self.body.netId.Value;
                            }
                            catch (Exception e)
                            {
                                Debug.Log($"[SirHamburger Danger] Error in Line 311");

                            }
                        }
                        if (pos == 50000)
                        {
                            Debug.Log($"[SirHamburger Danger] Didnt contain Key");
                            Debug.Log($"[SirHamburger Danger] netid:" + self.body.netId.Value);
                        }



                        QueueLostItemSprite[pos].Enqueue(ItemCatalog.GetItemDef(itemToRemove));
                        if (QueueLostItemSprite.Count > 10)
                            QueueLostItemSprite[pos].Dequeue();
                        //int i=QueueLostItemSprite.Count -1;
                        //int i= 0;
                        //foreach(var element in QueueLostItemSprite[pos])
                        //{
                        //    if( MainUIMod.listLostImages[i].GetComponent<Image>()== null )
                        //        MainUIMod.listLostImages[i].AddComponent<Image>();
                        //    MainUIMod.listLostImages[i].GetComponent<Image>().sprite = element;
                        //    i++;
                        //}
                        Debug.Log($"[--------SirHamburger------] self body user ID: " + self.body.netId.Value);
                        string temp = "";
                        foreach (var element in Danger.QueueLostItemSprite[pos])
                        {
                            temp += element.name + " ";
                        }
                        NetworkUser tempNetworkUser = null;
                        foreach (var element in NetworkUser.readOnlyInstancesList)
                        {
                            if (element.GetCurrentBody().netId == self.body.netId)
                                tempNetworkUser = element;
                        }
                        if (!LockNetworkUser.ContainsKey(tempNetworkUser))
                            LockNetworkUser.Add(tempNetworkUser, false);
                        if (LockNetworkUser[tempNetworkUser] == false)
                        {
                            LockNetworkUser[tempNetworkUser] = true;
                            MainUIMod.AddLostItemsOfPlayers.Invoke(temp, result =>
                            {
                                LockNetworkUser[tempNetworkUser] = false;
                                Debug.Log($"[Sirhamburger] added items: {result}");
                            }, tempNetworkUser);
                        }
                        //else{
                        //    while(LockNetworkUser[tempNetworkUser])
                        //    {
                        //        System.Threading.Thread.Sleep(100);
                        //    }
                        //    LockNetworkUser[tempNetworkUser]=true;
                        //    MainUIMod.AddLostItemsOfPlayers.Invoke(temp, result =>
                        //    {
                        //        LockNetworkUser[tempNetworkUser]=false;
                        //        Debug.Log($"[Sirhamburger] added items: {result}");
                        //    }, tempNetworkUser);
                        //}


                    }
                    else
                    {

                        //Ror2.console.print("lunar");
                    }


                }


            };
        }

        protected override void UnloadBehavior()
        {

            Danger.QueueLostItemSprite = new Dictionary<uint, Queue<ItemDef>>();
            Danger.QueueGainedItemSprite = new Dictionary<uint, Queue<ItemDef>>();
            Danger.statsLostItems = null;
            Danger.statsGainItems = null;
            Danger.Playername = new List<CharacterBody>();
            Danger.counter = new List<int>();

        }
    }
}