
using RoR2;
using System;
using TILER2;
using UnityEngine;
using System.Collections.Generic;





using RoR2.Stats;






namespace ArtifactOfDoom
{
    public class ArtifactOfDoom : Artifact<ArtifactOfDoom>
    {
        private const string GrayColor = "7e91af";
        private const string ErrorColor = "ff0000";
        public static bool debug = false;
        public override string displayName => "Artifact of Doom";

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangDesc(string langid = null) => "You get items on enemy kills but loose items every time you take damage.";
        private static List<CharacterBody> Playername = new List<CharacterBody>();
        private static List<int> counter = new List<int>();
        private int currentStage = 0;

        private Dictionary<NetworkUser, bool> LockNetworkUser = new Dictionary<NetworkUser, bool>();
        private Dictionary<NetworkUser, bool> LockItemGainNetworkUser = new Dictionary<NetworkUser, bool>();

        private static RoR2.Stats.StatDef statsLostItems;
        private static RoR2.Stats.StatDef statsGainItems;

        //public static Dictionary<CharacterBody, Queue<Sprite>>  PlayerItems = new Dictionary<CharacterBody, Queue<Sprite>>();
        //private static Queue<Sprite>  QueueLostItemSprite = new Queue<Sprite>();
        //private static Queue<Sprite>  QueueGainedItemSprite = new Queue<Sprite>();
        public static Dictionary<uint, Queue<ItemDef>> QueueLostItemSprite = new Dictionary<uint, Queue<ItemDef>>();
        public static Dictionary<uint, Queue<ItemDef>> QueueGainedItemSprite = new Dictionary<uint, Queue<ItemDef>>();

        private static double timeForBuff = 0.0;

        public static bool artifactIsActive = false;


        public void Awake()
        {
            Chat.AddMessage("Loaded MyModName!");
        }


        public ArtifactOfDoom()
        {

            iconPathName = "@ArtifactOfDoom:Assets/Import/artifactofdoom_icon/ArtifactDoomEnabled.png";
            iconPathNameDisabled = "@ArtifactOfDoom:Assets/Import/artifactofdoom_icon/ArtifactDoomDisabled.png";



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
            On.RoR2.PreGameController.StartRun += (orig, self) =>
              {
                  orig(self);
                  //Debug.LogError("PreGAmeController");
                  //artifactIsActive = this.IsActiveAndEnabled();
              };
            On.RoR2.SceneDirector.PopulateScene += (orig, self) =>
                {
                    orig(self);
                    currentStage = RoR2.Run.instance.stageClearCount + 1;
                    //                Debug.LogError("PopulateScene");

                    artifactIsActive = this.IsActiveAndEnabled();


                    if (Run.instance.selectedDifficulty == DifficultyIndex.Easy)
                        timeForBuff = ArtifactOfDoomConfig.timeAfterHitToNotLooseItemDrizzly.Value;
                    if (Run.instance.selectedDifficulty == DifficultyIndex.Normal)
                        timeForBuff = ArtifactOfDoomConfig.timeAfterHitToNotLooseItemRainstorm.Value;
                    if (Run.instance.selectedDifficulty == DifficultyIndex.Hard)
                        timeForBuff = ArtifactOfDoomConfig.timeAfterHitToNotLooseItemMonsoon.Value;
                    QueueLostItemSprite = new Dictionary<uint, Queue<ItemDef>>();
                    QueueGainedItemSprite = new Dictionary<uint, Queue<ItemDef>>();
                    Playername = new List<CharacterBody>();
                    counter = new List<int>();
                    LockNetworkUser.Clear();
                };
            On.RoR2.Run.Start += (orig, self) =>
            {
                orig(self);
                //Debug.LogError("-------------------here----------------------------------");
                ArtifactOfDoomUI.isArtifactActive.Invoke(this.IsActiveAndEnabled(), result =>
                    {
                        //Debug.LogError("Got Message Of IsArtifactActive");
                    }, null);
                ArtifactOfDoomUI.isCalculationSacrifice.Invoke(ArtifactOfDoomConfig.useArtifactOfSacreficeCalculation.Value, result =>
{
    //Debug.LogError("Got Message Of IsArtifactActive");
}, null);
            };
            On.RoR2.CharacterBody.OnInventoryChanged += (orig, self) =>
            {
                orig(self);
                try
                {
                    if (!this.IsActiveAndEnabled())
                        return;
                    if (!self.isPlayerControlled)
                        return;
                    NetworkUser tempNetworkUser = getNetworkUserOfCharacterBody(self);
                    int calculatesEnemyCountToTrigger = calculateEnemyCountToTrigger(self);
                    if (!Playername.Contains(self))
                    {
                        Playername.Add(self);
                        counter.Add(0);
                    }
                    if (tempNetworkUser != null)
                    {
                        //Debug.LogError("Network user == null");
                        string tempString = counter[Playername.IndexOf(self)] + "," + calculatesEnemyCountToTrigger;
                        ArtifactOfDoomUI.UpdateProgressBar.Invoke(tempString, result =>
                        {
                        }, tempNetworkUser);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error while inventory changed");
                }

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
                if (RoR2.Run.instance.isGameOverServer)
                    return;
                if (damageReport.victimBody.isPlayerControlled)
                    return;
                if (damageReport.attackerBody == null)
                    return;
                if (damageReport.attackerBody.inventory == null)
                    return;
                if (damageReport.victimBody.inventory == null)
                    return;

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
                }
                else
                {
                    currentBody = damageReport.attackerBody;
                }
                if (!currentBody.isPlayerControlled)
                {
                    return;
                }

                uint pos = 0;

                int calculatesEnemyCountToTrigger = calculateEnemyCountToTrigger(currentBody);
                bool enemyTrigger = getEnemyDropRate(damageReport);
                if (counter[Playername.IndexOf(currentBody)] <= calculatesEnemyCountToTrigger && !ArtifactOfDoomConfig.useArtifactOfSacreficeCalculation.Value)
                {
                    counter[Playername.IndexOf(currentBody)]++;

                    NetworkUser tempNetworkUser = getNetworkUserOfDamageReport(damageReport, true);
                    string temp = counter[Playername.IndexOf(currentBody)] + "," + calculatesEnemyCountToTrigger;

                    //Debug.LogWarning("currentBody für rpc: " + currentBody.name);
                    ArtifactOfDoomUI.UpdateProgressBar.Invoke(temp, result =>
                           {
                           }, tempNetworkUser);

                }
                else
                {
                    if (ArtifactOfDoomConfig.useArtifactOfSacreficeCalculation.Value && !enemyTrigger)
                        return;
                    if (damageReport.attackerOwnerMaster != null)
                    {
                        double chanceToTrigger = getCharacterSpezificBuffLengthMultiplier(damageReport.attackerOwnerMaster.GetBody());
                        chanceToTrigger *= 100;
                        var rand = new System.Random();
                        while (chanceToTrigger > rand.Next(0, 99))
                        {
                            ItemIndex addedItem = GiveAndReturnRandomItem(damageReport.attackerOwnerMaster.GetBody().inventory);
                            if (ArtifactOfDoomConfig.enableChatItemOutput.Value)
                            {
                                
                                //Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                                //{
                                //    
                                //    baseToken = damageReport.attackerOwnerMaster.GetBody().GetColoredUserName() + " got " + Language.GetString(ItemCatalog.GetItemDef(addedItem).pickupToken)
                                //});
                                var pickupDef = ItemCatalog.GetItemDef(addedItem);
                                var pickupName = Language.GetString(pickupDef.nameToken);
                                var playerColor = damageReport.attackerOwnerMaster.GetBody().GetColoredUserName();
                                var itemCount = damageReport.attackerOwnerMaster.GetBody().inventory.GetItemCount(pickupDef.itemIndex);                                
                                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                                {
                                    baseToken =
                                    damageReport.attackerOwnerMaster.GetBody().GetColoredUserName() +  $"<color=#{GrayColor}> gained</color> " +
                                    $"{pickupName ?? "???"} ({itemCount})</color> <color=#{GrayColor}></color>"
                                    
                                    //baseToken = self.body.GetColoredUserName() + " lost " + Language.GetString(ItemCatalog.GetItemDef(itemToRemove).pickupToken)
                                });                            }
                            PlayerStatsComponent.FindBodyStatSheet(damageReport.attackerOwnerMaster.GetBody()).PushStatValue(statsGainItems, 1UL);
                            if (QueueGainedItemSprite.ContainsKey(damageReport.attackerOwnerMaster.GetBody().netId.Value))
                                pos = damageReport.attackerOwnerMaster.GetBody().netId.Value;
                            else
                            {
                                QueueGainedItemSprite.Add(damageReport.attackerOwnerMaster.GetBody().netId.Value, new Queue<ItemDef>());
                                pos = damageReport.attackerOwnerMaster.GetBody().netId.Value;
                            }
                            QueueGainedItemSprite[pos].Enqueue(ItemCatalog.GetItemDef(addedItem));
                            chanceToTrigger -= 100;
                        }
                    }
                    else
                    {
                        double chanceToTrigger = getCharacterSpezificItemCount(damageReport.attackerBody);
                        chanceToTrigger *= 100;
                        var rand = new System.Random();
                        while (chanceToTrigger > rand.Next(0, 99))
                        {
                            ItemIndex addedItem = GiveAndReturnRandomItem(damageReport.attackerBody.inventory);
                            if (ArtifactOfDoomConfig.enableChatItemOutput.Value)
                            {
                                 var pickupDef = ItemCatalog.GetItemDef(addedItem);
                                var pickupName = Language.GetString(pickupDef.nameToken);
                                var playerColor = damageReport.attackerBody.GetColoredUserName();
                                var itemCount = damageReport.attackerBody.inventory.GetItemCount(pickupDef.itemIndex);                                
                                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                                {
                                    baseToken =
                                    damageReport.attackerBody.GetColoredUserName() +  $"<color=#{GrayColor}> gained</color> " +
                                    $"{pickupName ?? "???"} ({itemCount})</color> <color=#{GrayColor}></color>"
                                    
                                    //baseToken = self.body.GetColoredUserName() + " lost " + Language.GetString(ItemCatalog.GetItemDef(itemToRemove).pickupToken)
                                });  
                            }
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
                                    Debug.Log($"[SirHamburger ArtifactOfDoom] Error while excecuting : QueueGainedItemSprite.Add(damageReport.attackerBody.netId.Value, new Queue<Sprite>()); (line 203)");
                                }
                            }
                            QueueGainedItemSprite[pos].Enqueue(ItemCatalog.GetItemDef(addedItem));
                            chanceToTrigger -= 100;
                        }

                    }

                    if (QueueGainedItemSprite[pos].Count > 10)
                        QueueGainedItemSprite[pos].Dequeue();


                    string temp = "";
                    foreach (var element in ArtifactOfDoom.QueueGainedItemSprite[pos])
                    {
                        temp += element.name + " ";
                    }

                    NetworkUser tempNetworkUser = getNetworkUserOfDamageReport(damageReport, true);

                    if (!LockItemGainNetworkUser.ContainsKey(tempNetworkUser))
                        LockItemGainNetworkUser.Add(tempNetworkUser, false);
                    counter[Playername.IndexOf(currentBody)]++;


                    if (!LockItemGainNetworkUser[tempNetworkUser])
                    {
                        LockItemGainNetworkUser[tempNetworkUser] = true;
                        if (tempNetworkUser == null)
                            Debug.LogError("--------------------------------tempNetworkUser==null---------------------------");

                        ArtifactOfDoomUI.AddGainedItemsToPlayers.Invoke(temp, result =>
                            {
                                LockItemGainNetworkUser[tempNetworkUser] = false;
                            }, tempNetworkUser);
                        string tempString = counter[Playername.IndexOf(currentBody)] + "," + calculatesEnemyCountToTrigger;
                        ArtifactOfDoomUI.UpdateProgressBar.Invoke(tempString, result =>
                               {
                               }, tempNetworkUser);
                    }



                    counter[Playername.IndexOf(currentBody)] = 0;

                }


            };
            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageinfo) =>
            {

                //For adding possibility to dont loose items for some time: characterBody.AddTimedBuff(BuffIndex.Immune, duration);
                orig(self, damageinfo);

                if (!this.IsActiveAndEnabled())
                {

                    return;
                }
                if (damageinfo.rejected)
                {
                    //Debug.Log("Teddie?");
                    return;
                }

                if (debug) Debug.LogWarning("Line 287");
                if (self.body == null)
                { if (debug) Debug.LogWarning("self.body == null)"); return; }
                if (self.body.inventory == null)
                { if (debug) Debug.LogWarning("self.body.inventory == null)"); return; }
                if (RoR2.Run.instance.isGameOverServer)
                { if (debug) Debug.LogWarning("RoR2.Run.instance.isGameOverServer)"); return; }
                if (damageinfo == null)
                { if (debug) Debug.LogWarning("damageinfo == null)"); return; }
                if (damageinfo.attacker == null)
                { if (debug) Debug.LogWarning("damageinfo.attacker.name==null)"); return; }
                if (self.body.HasBuff(ArtifactOfDoomConfig.buffIndexDidLooseItem))
                {
                    if (debug) Debug.LogWarning("you did loose an item not long ago so you don't loose one now");
                    return;
                }
                if (debug) Debug.LogWarning("Line 294");
                int totalItems = getTotalItemCountOfPlayer(self.body);
                if (debug) Debug.LogWarning("Line 298");
                if (self.body.isPlayerControlled && (totalItems > 0) && self.name != damageinfo.attacker.name)
                {

                    if (debug) Debug.LogWarning("Line 301");
                    Dictionary<ItemIndex, int> lstItemIndex = new Dictionary<ItemIndex, int>();
                    List<ItemIndex> index = new List<ItemIndex>();
                    foreach (var element in ItemCatalog.allItems)
                    {
                        if (self.body.inventory.GetItemCount(element) > 0)
                        {
                            lstItemIndex.Add(element, self.body.inventory.GetItemCount(element));
                            index.Add(element);
                        }
                    }
                    if (debug) Debug.LogWarning("Line 312");

                    double chanceToTrigger = 100.0;
                    if (totalItems <= (ArtifactOfDoomConfig.minItemsPerStage.Value * currentStage))
                    {
                        //chanceToTrigger = 1.0 - (double)(ArtifactOfDoomConfig.minItemsPerStage.Value * currentStage - totalItems) / ((double)ArtifactOfDoomConfig.minItemsPerStage.Value * currentStage);
                        chanceToTrigger = (double)Math.Sqrt((double)totalItems / ((double)currentStage * (double)ArtifactOfDoomConfig.minItemsPerStage.Value));
                        chanceToTrigger *= 100;
                    }
                    //Debug.LogError("ChanceToTriggerLoose_Item"+ chanceToTrigger);
                    if (debug) Debug.LogWarning("Line 320");

                    var rand = new System.Random();
                    for (int i = 0; i < self.body.inventory.GetItemCount(ItemIndex.Clover) + 1; i++)

                    {
                        int randomValue = rand.Next(1, 100);

                        if (chanceToTrigger < (double)randomValue)
                        {
                            return;
                        }
                    }
                    chanceToTrigger = 100.0;
                    if (debug) Debug.LogWarning("Line 329");

                    if (totalItems > (ArtifactOfDoomConfig.maxItemsPerStage.Value * currentStage))
                    {
                        chanceToTrigger = Math.Pow((double)(totalItems) / ((double)ArtifactOfDoomConfig.maxItemsPerStage.Value * currentStage), ArtifactOfDoomConfig.exponentailFactorToCalculateSumOfLostItems.Value);
                        chanceToTrigger *= 100;

                    }
                    if (debug) Debug.LogWarning("Line 337");

                    int lostItems = 0;

                    uint pos = 50000;
                    if (debug) Debug.LogWarning("Line 342");

                    while (chanceToTrigger > 0)
                    {
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
                                Debug.Log($"[SirHamburger ArtifactOfDoom] Error in Line 311");

                            }
                        }
                        if (chanceToTrigger < rand.Next(0, 99))
                        {
                            break;
                        }
                        lostItems++;
                        int randomPosition = rand.Next(0, lstItemIndex.Count - 1);
                        ItemIndex itemToRemove = index[randomPosition];
                        if (debug) Debug.LogWarning("Line 394");
                        while ((lstItemIndex[itemToRemove] == 0))
                        {
                            randomPosition = rand.Next(0, lstItemIndex.Count - 1);
                            itemToRemove = index[randomPosition];
                        }
                        lstItemIndex[itemToRemove]--;
                        if (debug) Debug.LogWarning("Line 401");

                        if (!ItemCatalog.lunarItemList.Contains(itemToRemove) && (ItemCatalog.GetItemDef(itemToRemove).tier != ItemTier.NoTier && itemToRemove != ItemIndex.CaptainDefenseMatrix))
                        {
                            if (debug) Debug.LogWarning("Line 405");

                            self.body.inventory.RemoveItem(itemToRemove, 1);

                            //Chat.AddPickupMessage(self.body,itemToRemove,self.body.GetColoredUserName,PickupCatalog.GetPickupDef(itemToRemove).)

                            if (ArtifactOfDoomConfig.enableChatItemOutput.Value)
                            {
                                var pickupDef = ItemCatalog.GetItemDef(itemToRemove);
                                var pickupName = Language.GetString(pickupDef.nameToken);
                                var playerColor = self.body.GetColoredUserName();
                                var itemCount = self.body.inventory.GetItemCount(pickupDef.itemIndex);                                
                                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                                {
                                    baseToken =
                                    self.body.GetColoredUserName() +  $"<color=#{GrayColor}> lost</color> " +
                                    $"{pickupName ?? "???"} ({itemCount})</color> <color=#{GrayColor}></color>"
                                    
                                    //baseToken = self.body.GetColoredUserName() + " lost " + Language.GetString(ItemCatalog.GetItemDef(itemToRemove).pickupToken)
                                });
                            }
                            PlayerStatsComponent.FindBodyStatSheet(self.body).PushStatValue(statsLostItems, 1UL);

                            if (debug) Debug.LogWarning("Line 411");

                            QueueLostItemSprite[pos].Enqueue(ItemCatalog.GetItemDef(itemToRemove));
                            if (QueueLostItemSprite[pos].Count > 10)
                                QueueLostItemSprite[pos].Dequeue();

                            double buffLengthMultiplier = getCharacterSpezificBuffLengthMultiplier(self.body);
                            self.body.AddTimedBuff(ArtifactOfDoomConfig.buffIndexDidLooseItem, (float)(timeForBuff * (float)buffLengthMultiplier));


                        }
                        else
                        {

                        }
                        chanceToTrigger -= 100;
                    }
                    if (debug) Debug.LogWarning("Line 431");

                    string temp = "";
                    foreach (var element in ArtifactOfDoom.QueueLostItemSprite[pos])
                    {
                        temp += element.name + " ";
                    }
                    NetworkUser tempNetworkUser = getNetworkUserOfCharacterBody(self.body);
                    if (debug) Debug.LogWarning("Line 444");

                    if (tempNetworkUser == null)
                        Debug.LogError("--------------------------------tempNetworkUser(lostitems)==null---------------------------");
                    if (!LockNetworkUser.ContainsKey(tempNetworkUser))
                        LockNetworkUser.Add(tempNetworkUser, false);
                    if (LockNetworkUser[tempNetworkUser] == false)
                    {
                        LockNetworkUser[tempNetworkUser] = true;
                        ArtifactOfDoomUI.AddLostItemsOfPlayers.Invoke(temp, result =>
                        {
                            LockNetworkUser[tempNetworkUser] = false;
                        }, tempNetworkUser);
                        int calculatesEnemyCountToTrigger = calculateEnemyCountToTrigger(self.body);
                        string tempString = counter[Playername.IndexOf(self.body)] + "," + calculatesEnemyCountToTrigger;
                        ArtifactOfDoomUI.UpdateProgressBar.Invoke(tempString, result =>
                               {
                               }, tempNetworkUser);
                    }
                }


            };
        }
        private NetworkUser getNetworkUserOfDamageReport(DamageReport report, bool withMaster)
        {
            NetworkUser tempNetworkUser = null;
            foreach (var element in NetworkUser.readOnlyInstancesList)
            {
                if (report.attackerOwnerMaster != null && withMaster)
                {
                    if (element.GetCurrentBody() != null)
                    {
                        if (element.GetCurrentBody().netId == report.attackerOwnerMaster.GetBody().netId)
                        {
                            tempNetworkUser = element;
                        }
                    }


                }
                else
                {
                    if (element.GetCurrentBody() != null)
                    {
                        if (element.GetCurrentBody().netId == report.attackerBody.netId)
                        {
                            tempNetworkUser = element;
                        }
                    }

                }
            }
            return tempNetworkUser;
        }
        private NetworkUser getNetworkUserOfCharacterBody(CharacterBody body)
        {
            NetworkUser tempNetworkUser = null;
            foreach (var element in NetworkUser.readOnlyInstancesList)
            {
                if (element.GetCurrentBody() != null)
                {
                    if (element.GetCurrentBody().netId == body.netId)
                        tempNetworkUser = element;
                }
            }
            return tempNetworkUser;
        }
        private int getTotalItemCountOfPlayer(CharacterBody body)
        {
            return body.inventory.GetTotalItemCountOfTier(ItemTier.Tier1) +
            body.inventory.GetTotalItemCountOfTier(ItemTier.Tier2) +
            body.inventory.GetTotalItemCountOfTier(ItemTier.Tier3);
        }
        private int calculateEnemyCountToTrigger(CharacterBody body)
        {
            double totalItems = getTotalItemCountOfPlayer(body);
            double calculatedValue = ((double)totalItems - (double)currentStage * (double)ArtifactOfDoomConfig.averageItemsPerStage.Value);
            int calculatesEnemyCountToTrigger = 0;
            if (calculatedValue >= 0)
                calculatesEnemyCountToTrigger = (int)Math.Pow(calculatedValue, ArtifactOfDoomConfig.exponentTriggerItems.Value);
            else
                calculatesEnemyCountToTrigger = (int)Math.Pow(totalItems, ArtifactOfDoomConfig.exponentailFactorIfYouAreUnderAverageItemsPerStage.Value);
            //calculatesEnemyCountToTrigger =1;

            if (calculatesEnemyCountToTrigger < 1)
                calculatesEnemyCountToTrigger = 1;

            if (RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.swarmsArtifactDef) && ArtifactOfDoomConfig.artifactOfSwarmNerf.Value)
                calculatesEnemyCountToTrigger *= 2;
            return calculatesEnemyCountToTrigger;
        }

        private double getCharacterSpezificItemCount(CharacterBody body)
        {
            if (body.name.Contains("Commando"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Commando"); }
                return ArtifactOfDoomConfig.commandoBonusItems.Value;
            }
            if (body.name.Contains("Huntress"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Huntress"); }

                return ArtifactOfDoomConfig.HuntressBonusItems.Value;
            }
            if (body.name.Contains("Toolbot"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: MUL"); }

                return ArtifactOfDoomConfig.MULTBonusItems.Value;
            }
            if (body.name.Contains("Engi"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Engineer"); }

                return ArtifactOfDoomConfig.EngineerBonusItems.Value;
            }
            if (body.name.Contains("Mage"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Artificer"); }

                return ArtifactOfDoomConfig.ArtificerBonusItems.Value;
            }
            if (body.name.Contains("Merc"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Mercenary"); }

                return ArtifactOfDoomConfig.MercenaryBonusItems.Value;
            }

            if (body.name.Contains("Treebot"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Rex"); }

                return ArtifactOfDoomConfig.RexBonusItems.Value;
            }
            if (body.name.Contains("Loader"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Loader"); }

                return ArtifactOfDoomConfig.LoaderBonusItems.Value;
            }
            if (body.name.Contains("Croco"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Acrid"); }

                return ArtifactOfDoomConfig.AcridBonusItems.Value;
            }
            if (body.name.Contains("Captain"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Captain"); }

                return ArtifactOfDoomConfig.CaptainBonusItems.Value;
            }
            Debug.LogWarning("Character BodyName = " + body.name + " Didnt find valid Body. \n If the character is NOT a custom character report this please to SirHamburger");
            return 1.0;
        }
        private double getCharacterSpezificBuffLengthMultiplier(CharacterBody body)
        {

            if (body.name.Contains("Commando"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Commando"); }
                return ArtifactOfDoomConfig.commandoMultiplyerForTimedBuff.Value;
            }
            if (body.name.Contains("Huntress"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Huntress"); }

                return ArtifactOfDoomConfig.HuntressMultiplyerForTimedBuff.Value;
            }
            if (body.name.Contains("Toolbot"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: MUL"); }

                return ArtifactOfDoomConfig.MULTMultiplyerForTimedBuff.Value;
            }
            if (body.name.Contains("Engi"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Engineer"); }

                return ArtifactOfDoomConfig.EngineerMultiplyerForTimedBuff.Value;
            }
            if (body.name.Contains("Mage"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Artificer"); }

                return ArtifactOfDoomConfig.ArtificerMultiplyerForTimedBuff.Value;
            }
            if (body.name.Contains("Merc"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Mercenary"); }

                return ArtifactOfDoomConfig.MercenaryMultiplyerForTimedBuff.Value;
            }

            if (body.name.Contains("Treebot"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Rex"); }

                return ArtifactOfDoomConfig.RexMultiplyerForTimedBuff.Value;
            }
            if (body.name.Contains("Loader"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Loader"); }

                return ArtifactOfDoomConfig.LoaderMultiplyerForTimedBuff.Value;
            }
            if (body.name.Contains("Croco"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Acrid"); }

                return ArtifactOfDoomConfig.AcridMultiplyerForTimedBuff.Value;
            }
            if (body.name.Contains("Captain"))
            {
                if (debug) { Debug.LogWarning("Character BodyName = " + body.name + " returning: Captain"); }

                return ArtifactOfDoomConfig.CaptainMultiplyerForTimedBuff.Value;
            }
            Debug.LogWarning("Character BodyName = " + body.name + " Didnt find valid Body. \n If the character is NOT a custom character report this please to SirHamburger");
            return 1.0;
        }
        public ItemIndex GiveAndReturnRandomItem(Inventory inventory)
        {

            WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
            weightedSelection.AddChoice(Run.instance.availableTier1DropList, 80f);
            weightedSelection.AddChoice(Run.instance.availableTier2DropList, 19f);
            weightedSelection.AddChoice(Run.instance.availableTier3DropList, 1f);

            List<PickupIndex> list = weightedSelection.Evaluate(UnityEngine.Random.value);
            ItemIndex givenItem = list[UnityEngine.Random.Range(0, list.Count)].itemIndex;
            inventory.GiveItem(givenItem, 1);
            return givenItem;

        }

        protected override void UnloadBehavior()
        {

            ArtifactOfDoom.QueueLostItemSprite = new Dictionary<uint, Queue<ItemDef>>();
            ArtifactOfDoom.QueueGainedItemSprite = new Dictionary<uint, Queue<ItemDef>>();
            ArtifactOfDoom.statsLostItems = null;
            ArtifactOfDoom.statsGainItems = null;
            ArtifactOfDoom.Playername = new List<CharacterBody>();
            ArtifactOfDoom.counter = new List<int>();

        }

        private bool getEnemyDropRate(DamageReport damageReport)
        {
            if (!ArtifactOfDoomConfig.useArtifactOfSacreficeCalculation.Value)
                return false;
            if (!damageReport.victimMaster)
            {
                return false;
            }
            if (damageReport.attackerTeamIndex == damageReport.victimTeamIndex && damageReport.victimMaster.minionOwnership.ownerMaster)
            {
                return false;
            }
            float expAdjustedDropChancePercent = Util.GetExpAdjustedDropChancePercent(5f * (float)ArtifactOfDoomConfig.multiplayerForArtifactOfSacrificeDropRate.Value, damageReport.victim.gameObject);
            //Debug.LogFormat("Drop chance from {0}: {1}", new object[]
            //{
            //	damageReport.victimBody,
            //	expAdjustedDropChancePercent
            //});
            if (Util.CheckRoll(expAdjustedDropChancePercent, 0f, null))
            {
                return true;
            }
            return false;
        }

    }
}