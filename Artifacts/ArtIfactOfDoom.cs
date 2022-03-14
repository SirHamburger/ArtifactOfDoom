
//using R2API;
//using R2API.Utils;
//using EnigmaticThunder;
using BepInEx;
using BepInEx.Logging;
using System.Reflection;
using RoR2;
using RoR2.Stats;
using System;
using System.Collections.Generic;
//using TILER2;
using UnityEngine;
using ArtifactOfDoomTinyJson;
using UnityEngine.Networking;
using UnityEngine;
using RoR2.Artifacts;
namespace ArtifactOfDoom
{
    public class ArtifactOfDoom
    {


        public static ArtifactDef Transmutation = ScriptableObject.CreateInstance<ArtifactDef>();

        private const string GrayColor = "7e91af";
        private const string ErrorColor = "ff0000";
        public static bool debug = false;
        private static List<CharacterBody> Playername = new List<CharacterBody>();
        private static List<int> counter = new List<int>();
        private int currentStage = 0;

        private Dictionary<NetworkUser, bool> LockNetworkUser = new Dictionary<NetworkUser, bool>();
        private Dictionary<NetworkUser, bool> LockItemGainNetworkUser = new Dictionary<NetworkUser, bool>();

        private static StatDef statsLostItems;
        private static StatDef statsGainItems;



        //public static Dictionary<CharacterBody, Queue<Sprite>>  PlayerItems = new Dictionary<CharacterBody, Queue<Sprite>>();
        //private static Queue<Sprite>  QueueLostItemSprite = new Queue<Sprite>();
        //private static Queue<Sprite>  QueueGainedItemSprite = new Queue<Sprite>();
        public static Dictionary<uint, Queue<ItemDef>> QueueLostItemSprite = new Dictionary<uint, Queue<ItemDef>>();
        public static Dictionary<uint, Queue<ItemDef>> QueueGainedItemSprite = new Dictionary<uint, Queue<ItemDef>>();

        private static double timeForBuff = -1.0;

        public ArtifactOfDoom()
        {
            Transmutation.nameToken = "Artifact of Doom";
            Transmutation.descriptionToken = "You get items on enemy kills but lose items every time you take damage.";

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ArtifactOfDoom.artifactofdoom"))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                Transmutation.smallIconSelectedSprite = bundle.LoadAsset<Sprite>("Assets/Import/artifactofdoom_icon/ArtifactDoomEnabled.png");
                Transmutation.smallIconDeselectedSprite = bundle.LoadAsset<Sprite>("Assets/Import/artifactofdoom_icon/ArtifactDoomDisabled.png");
                R2API.ContentAddition.AddArtifactDef(Transmutation);
            }
            LoadBehavior();
        }
                  


        protected void LoadBehavior()
        {

            Playername = new List<CharacterBody>();
            counter = new List<int>();
            currentStage = 0;

            statsLostItems = null;
            statsGainItems = null;

            statsLostItems = StatDef.Register("Lostitems", StatRecordType.Sum, StatDataType.ULong, 0, null);
            statsGainItems = StatDef.Register("Gainitems", StatRecordType.Sum, StatDataType.ULong, 0, null);

            On.RoR2.UI.GameEndReportPanelController.Awake += (orig, self) =>
                {
                    orig(self);
                    if (!Networking._instance.IsArtifactEnabled)
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
                
            };

            On.RoR2.SceneDirector.PopulateScene += (orig, self) =>
                {
                    orig(self);

                    currentStage = Run.instance.stageClearCount + 1;


                    if (Run.instance.selectedDifficulty == DifficultyIndex.Easy)
                        timeForBuff = ArtifactOfDoomConfig.timeAfterHitToNotLoseItemDrizzly.Value;
                    if (Run.instance.selectedDifficulty == DifficultyIndex.Normal)
                        timeForBuff = ArtifactOfDoomConfig.timeAfterHitToNotLoseItemRainstorm.Value;
                    if (Run.instance.selectedDifficulty == DifficultyIndex.Hard)
                        timeForBuff = ArtifactOfDoomConfig.timeAfterHitToNotLoseItemMonsoon.Value;
                    if (timeForBuff == -1.0)
                    {
                        List<Difficulty> characters = ArtifactOfDoomConfig.timeAfterHitToNotLoseItemOtherDifficulty.Value.FromJson<List<Difficulty>>();
                        foreach (var element in characters)
                        {

                            if (Run.instance.selectedDifficulty == (DifficultyIndex)element.DifficultyIndex)
                                timeForBuff = element.time;
                        }
                        if (timeForBuff == -1.0)
                        {
                            Debug.LogWarning("Didn't find valid Configuration for Selected Difficulty. Falling back to 0.1 seconds for Buff. If you want a own definition fill out timeAfterHitToNotLoseItemOtherDifficulty in the Config. DifficultyIndex=" + Run.instance.selectedDifficulty);
                            timeForBuff = 0.1;
                        }
                    }
                    QueueLostItemSprite = new Dictionary<uint, Queue<ItemDef>>();
                    QueueGainedItemSprite = new Dictionary<uint, Queue<ItemDef>>();
                    Playername = new List<CharacterBody>();
                    counter = new List<int>();
                    LockNetworkUser.Clear();
                };
            On.RoR2.Run.Awake += (orig, self) =>
              {
                  orig(self);
                  //Debug.LogWarning("NetworkClass.SpawnNetworkObject();");

              };
            On.RoR2.Run.Start += (orig, self) =>
            {
                orig(self);

            };
            On.RoR2.CharacterBody.OnInventoryChanged += (orig, self) =>
            {
                orig(self);
                if(Networking._instance==null)
                    return;
                


                if (!Networking._instance.IsArtifactEnabled)
                    return;
                if (!self.isPlayerControlled)
                    return;

                NetworkUser tempNetworkUser = getNetworkUserOfCharacterBody(self);
                int calculatesEnemyCountToTrigger = calculateEnemyCountToTrigger(self.inventory);

                if (!Playername.Contains(self))
                {

                    Playername.Add(self);
                    counter.Add(0);
                }
                if (tempNetworkUser != null)
                {

                    string tempString = counter[Playername.IndexOf(self)] + "," + calculatesEnemyCountToTrigger;
                    if (NetworkServer.active)
                    {
                        Networking.ServerEnsureNetworking();
                        Networking._instance.TargetUpdateProgressBar(tempNetworkUser.connectionToClient, tempString);
                    }
                }

            };
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
            {
                //try
                //{
                orig(self, damageReport);
                Networking._instance.IsArtifactEnabled = RunArtifactManager.instance.IsArtifactEnabled(ArtifactOfDoom.Transmutation.artifactIndex);
                Networking._instance.IsCalculationSacrifice = ArtifactOfDoomConfig.useArtifactOfSacrificeCalculation.Value;

                if (!Networking._instance.IsArtifactEnabled)
                {
                    return;
                }
                if (Run.instance.isGameOverServer)
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

                int calculatesEnemyCountToTrigger = calculateEnemyCountToTrigger(currentBody.inventory);
                bool enemyTrigger = getEnemyDropRate(damageReport);
                if (counter[Playername.IndexOf(currentBody)] <= calculatesEnemyCountToTrigger && !ArtifactOfDoomConfig.useArtifactOfSacrificeCalculation.Value)
                {
                    counter[Playername.IndexOf(currentBody)]++;

                    NetworkUser tempNetworkUser = getNetworkUserOfDamageReport(damageReport, true);
                    string temp = counter[Playername.IndexOf(currentBody)] + "," + calculatesEnemyCountToTrigger;
                    //Debug.LogWarning("tempNetworkUser: " + tempNetworkUser);
                    //Debug.LogWarning("temp: " + temp);
                    if (NetworkServer.active)
                    {
                        Networking.ServerEnsureNetworking();
                        Networking._instance.TargetUpdateProgressBar(tempNetworkUser.connectionToClient, temp);
                    }

                }
                else
                {
                    if (ArtifactOfDoomConfig.useArtifactOfSacrificeCalculation.Value && !enemyTrigger)
                        return;
                    CharacterBody body;

                    if (damageReport.attackerOwnerMaster != null)
                    {
                        body = damageReport.attackerOwnerMaster.GetBody();

                        double chanceToTrigger = getCharacterSpezificBuffLengthMultiplier(body.baseNameToken);
                        chanceToTrigger *= 100;
                        var rand = new System.Random();
                        while (chanceToTrigger > rand.Next(0, 99))
                        {
                            ItemIndex addedItem = GiveAndReturnRandomItem(body.inventory);
                            if (ArtifactOfDoomConfig.enableChatItemOutput.Value)
                            {

                                var pickupDef = ItemCatalog.GetItemDef(addedItem);
                                var pickupName = Language.GetString(pickupDef.nameToken);
                                var playerColor = damageReport.attackerOwnerMaster.GetBody().GetColoredUserName();
                                var itemCount = damageReport.attackerOwnerMaster.GetBody().inventory.GetItemCount(pickupDef.itemIndex);
                                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                                {
                                    baseToken =
                                    damageReport.attackerOwnerMaster.GetBody().GetColoredUserName() + $"<color=#{GrayColor}> gained</color> " +
                                    $"{pickupName ?? "???"} ({itemCount})</color> <color=#{GrayColor}></color>"

                                });
                            }
                            PlayerStatsComponent.FindBodyStatSheet(body).PushStatValue(statsGainItems, 1UL);
                            if (QueueGainedItemSprite.ContainsKey(body.netId.Value))
                                pos = body.netId.Value;
                            else
                            {
                                QueueGainedItemSprite.Add(body.netId.Value, new Queue<ItemDef>());
                                pos = body.netId.Value;
                            }
                            QueueGainedItemSprite[pos].Enqueue(ItemCatalog.GetItemDef(addedItem));
                            chanceToTrigger -= 100;
                        }
                    }
                    else
                    {
                        body = damageReport.attackerBody;
                        double chanceToTrigger = getCharacterSpezificItemCount(body.baseNameToken);
                        chanceToTrigger *= 100;
                        var rand = new System.Random();
                        while (chanceToTrigger > rand.Next(0, 99))
                        {
                            ItemIndex addedItem = GiveAndReturnRandomItem(body.inventory);
                            if (ArtifactOfDoomConfig.enableChatItemOutput.Value)
                            {
                                var pickupDef = ItemCatalog.GetItemDef(addedItem);
                                var pickupName = Language.GetString(pickupDef.nameToken);
                                var playerColor = body.GetColoredUserName();
                                var itemCount = body.inventory.GetItemCount(pickupDef.itemIndex);
                                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                                {
                                    baseToken =
                                    body.GetColoredUserName() + $"<color=#{GrayColor}> gained</color> " +
                                    $"{pickupName ?? "???"} ({itemCount})</color> <color=#{GrayColor}></color>"

                                });
                            }
                            PlayerStatsComponent.FindBodyStatSheet(body).PushStatValue(statsGainItems, 1UL);
                            if (QueueGainedItemSprite.ContainsKey(body.netId.Value))
                                pos = body.netId.Value;
                            else
                            {
                                try
                                {
                                    QueueGainedItemSprite.Add(body.netId.Value, new Queue<ItemDef>());
                                    pos = body.netId.Value;
                                }
                                catch (Exception)
                                {
                                    Debug.LogError("[SirHamburger ArtifactOfDoom] Error while excecuting : QueueGainedItemSprite.Add(body.netId.Value, new Queue<Sprite>()); (line 203)");
                                }
                            }
                            QueueGainedItemSprite[pos].Enqueue(ItemCatalog.GetItemDef(addedItem));
                            chanceToTrigger -= 100;
                        }
                    }

                    if (QueueGainedItemSprite[pos].Count > 10)
                        QueueGainedItemSprite[pos].Dequeue();
                    string temp = "";
                    foreach (var element in QueueGainedItemSprite[pos])
                    {
                        temp += element.name + " ";
                    }

                    NetworkUser tempNetworkUser = getNetworkUserOfDamageReport(damageReport, true);

                    if (!LockItemGainNetworkUser.ContainsKey(tempNetworkUser))
                        LockItemGainNetworkUser.Add(tempNetworkUser, false);
                    counter[Playername.IndexOf(currentBody)]++;


                    if (!LockItemGainNetworkUser[tempNetworkUser])
                    {

                        LockItemGainNetworkUser[tempNetworkUser] = false;

                        LockItemGainNetworkUser[tempNetworkUser] = false;
                        string tempString = counter[Playername.IndexOf(currentBody)] + "," + calculatesEnemyCountToTrigger;
                        if (NetworkServer.active)
                        {
                            Networking._instance.TargetAddGainedItemsToPlayers(tempNetworkUser.connectionToClient, temp);
                            Networking.ServerEnsureNetworking();
                            Networking._instance.TargetUpdateProgressBar(tempNetworkUser.connectionToClient, tempString);
                        }
                    }

                    counter[Playername.IndexOf(currentBody)] = 0;
                }
            };
            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageinfo) =>
            {
                //For adding possibility to dont loose items for some time: characterBody.AddTimedBuff(BuffIndex.Immune, duration);
                orig(self, damageinfo);
                //BuffIndex buff = new BuffIndex();
                //Debug.LogError("buffindex " +buff );
//
                //BuffCatalog.FindBuffIndex("ArtifactOfDoomDidLoseItem");
                //                Debug.LogError("buffindex " +buff );


                if (!Networking._instance.IsArtifactEnabled)
                {
                    return;
                }

                if (damageinfo.rejected)
                {
                    //Debug.Log("Teddie?");
                    return;
                }

                if (debug) Debug.LogWarning("Line 336");

                if (self.body == null)
                {
                    if (debug) Debug.LogWarning("self.body == null)");
                    return;
                }

                if (self.body.inventory == null)
                {
                    if (debug) Debug.LogWarning("self.body.inventory == null)");
                    return;
                }

                if (Run.instance.isGameOverServer)
                {
                    if (debug) Debug.LogWarning("RoR2.Run.instance.isGameOverServer)");
                    return;
                }

                if (damageinfo == null)
                {
                    if (debug) Debug.LogWarning("damageinfo == null)");
                    return;
                }

                if (damageinfo.attacker == null)
                {
                    if (debug) Debug.LogWarning("damageinfo.attacker.name==null)");
                    return;

                }
                if (self.body.HasBuff(ArtifactOfDoomConfig.ArtifactOfDoomBuff))
                {
                    if (debug) Debug.LogWarning("you did lose an item not long ago so you don't lose one now");
                    return;
                }

                int totalItems = getTotalItemCountOfPlayer(self.body.inventory);
                if (self.body.isPlayerControlled && (totalItems > 0) && self.name != damageinfo.attacker.name)
                {

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

                    double chanceToTrigger = 100.0;
                    if (totalItems <= (ArtifactOfDoomConfig.minItemsPerStage.Value * currentStage))
                    {
                        //chanceToTrigger = 1.0 - (double)(ArtifactOfDoomConfig.minItemsPerStage.Value * currentStage - totalItems) / ((double)ArtifactOfDoomConfig.minItemsPerStage.Value * currentStage);
                        chanceToTrigger = (double)Math.Sqrt((double)totalItems / ((double)currentStage * (double)ArtifactOfDoomConfig.minItemsPerStage.Value));
                        chanceToTrigger *= 100;
                    }
                    //Debug.LogError("ChanceToTriggerLoose_Item"+ chanceToTrigger);

                    var rand = new System.Random();

                    for (int i = 0; i < self.body.inventory.GetItemCount(RoR2.RoR2Content.Items.Clover) + 1; i++)
                    {
                        int randomValue = rand.Next(1, 100);

                        if (chanceToTrigger < randomValue)
                        {
                            return;
                        }
                    }

                    chanceToTrigger = 100.0;

                    if (totalItems > (ArtifactOfDoomConfig.maxItemsPerStage.Value * currentStage))
                    {
                        chanceToTrigger = Math.Pow((double)(totalItems) / ((double)ArtifactOfDoomConfig.maxItemsPerStage.Value * currentStage), ArtifactOfDoomConfig.exponentailFactorToCalculateSumOfLostItems.Value);
                        chanceToTrigger *= 100;
                    }

                    int lostItems = 0;

                    uint pos = 50000;

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
                            catch (Exception)
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
                        while ((lstItemIndex[itemToRemove] == 0))
                        {
                            randomPosition = rand.Next(0, lstItemIndex.Count - 1);
                            itemToRemove = index[randomPosition];
                        }
                        lstItemIndex[itemToRemove]--;

                        if (!ItemCatalog.lunarItemList.Contains(itemToRemove) && (ItemCatalog.GetItemDef(itemToRemove).tier != ItemTier.NoTier && itemToRemove != RoR2.RoR2Content.Items.CaptainDefenseMatrix.itemIndex))
                        {

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
                                    self.body.GetColoredUserName() + $"<color=#{GrayColor}> lost</color> " +
                                    $"{pickupName ?? "???"} ({itemCount})</color> <color=#{GrayColor}></color>"

                                    //baseToken = self.body.GetColoredUserName() + " lost " + Language.GetString(ItemCatalog.GetItemDef(itemToRemove).pickupToken)
                                });
                            }
                            PlayerStatsComponent.FindBodyStatSheet(self.body).PushStatValue(statsLostItems, 1UL);


                            QueueLostItemSprite[pos].Enqueue(ItemCatalog.GetItemDef(itemToRemove));
                            if (QueueLostItemSprite[pos].Count > 10)
                                QueueLostItemSprite[pos].Dequeue();

                            double buffLengthMultiplier = getCharacterSpezificBuffLengthMultiplier(self.body.baseNameToken);
                            self.body.AddTimedBuff(ArtifactOfDoomConfig.ArtifactOfDoomBuff, (float)(timeForBuff * (float)buffLengthMultiplier));
                        }

                        chanceToTrigger -= 100;
                    }

                    string temp = "";
                    foreach (var element in ArtifactOfDoom.QueueLostItemSprite[pos])
                    {
                        temp += element.name + " ";
                    }
                    NetworkUser tempNetworkUser = getNetworkUserOfCharacterBody(self.body);

                    if (tempNetworkUser == null)
                        Debug.LogError("--------------------------------tempNetworkUser(lostitems)==null---------------------------");
                    if (!LockNetworkUser.ContainsKey(tempNetworkUser))
                        LockNetworkUser.Add(tempNetworkUser, false);
                    if (LockNetworkUser[tempNetworkUser] == false)
                    {
                        LockNetworkUser[tempNetworkUser] = false;
                        int calculatesEnemyCountToTrigger = calculateEnemyCountToTrigger(self.body.inventory);
                        string tempString = counter[Playername.IndexOf(self.body)] + "," + calculatesEnemyCountToTrigger;
                        if (NetworkServer.active)
                        {
                            Networking._instance.TargetAddLostItemsOfPlayers(tempNetworkUser.connectionToClient, temp);
                            Networking.ServerEnsureNetworking();
                            Networking._instance.TargetUpdateProgressBar(tempNetworkUser.connectionToClient, tempString);
                        }
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
        private int getTotalItemCountOfPlayer(Inventory inventory)
        {
            return inventory.GetTotalItemCountOfTier(ItemTier.Tier1) +
            inventory.GetTotalItemCountOfTier(ItemTier.Tier2) +
            inventory.GetTotalItemCountOfTier(ItemTier.Tier3);
        }
        private int calculateEnemyCountToTrigger(Inventory inventory)
        {
            var totalItems = getTotalItemCountOfPlayer(inventory);
            var calculatedValue = totalItems - currentStage * ArtifactOfDoomConfig.averageItemsPerStage.Value;
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

        private double getCharacterSpezificItemCount(string baseNameToken)
        {
            switch (baseNameToken)
            {
                case "COMMANDO_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Commando"); }
                    return ArtifactOfDoomConfig.CommandoBonusItems.Value;
                case "HUNTRESS_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Huntress"); }
                    return ArtifactOfDoomConfig.HuntressBonusItems.Value;
                case "ENGI_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Engineer"); }
                    return ArtifactOfDoomConfig.EngineerBonusItems.Value;
                case "TOOLBOT_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: MULT"); }
                    return ArtifactOfDoomConfig.MULTBonusItems.Value;
                case "MAGE_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Artificer"); }
                    return ArtifactOfDoomConfig.ArtificerBonusItems.Value;
                case "MERC_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Mercenary"); }
                    return ArtifactOfDoomConfig.MercenaryBonusItems.Value;
                case "TREEBOT_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Rex"); }
                    return ArtifactOfDoomConfig.RexBonusItems.Value;
                case "LOADER_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Loader"); }
                    return ArtifactOfDoomConfig.LoaderBonusItems.Value;
                case "CROCO_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Acrid"); }
                    return ArtifactOfDoomConfig.AcridBonusItems.Value;
                case "CAPTAIN_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Captain"); }
                    return ArtifactOfDoomConfig.CaptainBonusItems.Value;
                case "BANDIT2_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Bandit"); }
                    return ArtifactOfDoomConfig.BanditBonusItems.Value;
                default:
                    string CustomChars = ArtifactOfDoomConfig.CustomChars.Value;

                    //Character characters = TinyJson.JSONParser.FromJson<Character>(CustomChars);
                    List<Character> characters = CustomChars.FromJson<List<Character>>();
                    foreach (var element in characters)
                    {
                        if (baseNameToken == element.Name)
                            return element.BonusItems;
                    }
                    Debug.LogWarning("did not find a valid configuation setting for Character " + baseNameToken + " you can add one in the settings");
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Acrid"); }
                    return ArtifactOfDoomConfig.CustomSurvivorBonusItems.Value;
            }
        }

        private double getCharacterSpezificBuffLengthMultiplier(string baseNameToken)
        {
            switch (baseNameToken)
            {
                case "COMMANDO_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Commando"); }
                    return ArtifactOfDoomConfig.CommandoMultiplierForTimedBuff.Value;
                case "HUNTRESS_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Huntress"); }
                    return ArtifactOfDoomConfig.HuntressMultiplierForTimedBuff.Value;
                case "ENGI_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Engineer"); }
                    return ArtifactOfDoomConfig.EngineerMultiplierForTimedBuff.Value;
                case "TOOLBOT_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: MULT"); }
                    return ArtifactOfDoomConfig.MULTMultiplierForTimedBuff.Value;
                case "MAGE_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Artificer"); }
                    return ArtifactOfDoomConfig.ArtificerMultiplierForTimedBuff.Value;
                case "MERC_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Mercenary"); }
                    return ArtifactOfDoomConfig.MercenaryMultiplierForTimedBuff.Value;
                case "TREEBOT_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Rex"); }
                    return ArtifactOfDoomConfig.RexMultiplierForTimedBuff.Value;
                case "LOADER_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Loader"); }
                    return ArtifactOfDoomConfig.LoaderMultiplierForTimedBuff.Value;
                case "CROCO_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Acrid"); }
                    return ArtifactOfDoomConfig.AcridMultiplierForTimedBuff.Value;
                case "CAPTAIN_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Captain"); }
                    return ArtifactOfDoomConfig.CaptainMultiplierForTimedBuff.Value;
                case "BANDIT2_BODY_NAME":
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Bandit"); }
                    return ArtifactOfDoomConfig.BanditMultiplierForTimedBuff.Value;
                default:
                    string CustomChars = ArtifactOfDoomConfig.CustomChars.Value;

                    //Character characters = TinyJson.JSONParser.FromJson<Character>(CustomChars);
                    List<Character> characters = CustomChars.FromJson<List<Character>>();
                    foreach (var element in characters)
                    {
                        if (baseNameToken == element.Name)
                            return element.MultiplierForTimedBuff;
                    }
                    Debug.LogWarning("did not find a valid configuation setting for Character " + baseNameToken + " you can add one in the settings");
                    if (debug) { Debug.LogWarning($"Character baseNameToken = {baseNameToken} returning: Acrid"); }
                    return ArtifactOfDoomConfig.CustomSurvivorMultiplierForTimedBuff.Value;
            }
        }
        public class Character
        {
            public string Name { get; set; }
            public float MultiplierForTimedBuff { get; set; }
            public float BonusItems { get; set; }
        }
        public class Difficulty
        {
            public int DifficultyIndex { get; set; }
            public float time { get; set; }
        }
        public ItemIndex GiveAndReturnRandomItem(Inventory inventory)
        {

            var tier1 = ItemCatalog.tier1ItemList;
            var tier2 = ItemCatalog.tier2ItemList;
            var tier3 = ItemCatalog.tier3ItemList;

            WeightedSelection<List<ItemIndex>> weightedSelection = new WeightedSelection<List<ItemIndex>>();
            weightedSelection.AddChoice(tier1, 80f);
            weightedSelection.AddChoice(tier2, 19f);
            weightedSelection.AddChoice(tier3, 1f);

            List<ItemIndex> list = weightedSelection.Evaluate(UnityEngine.Random.value);

            var givenItem = list[UnityEngine.Random.Range(0, list.Count)];

            inventory.GiveItem(givenItem);
            return givenItem;
        }

        protected void OnDestroy()
        {
            QueueLostItemSprite = new Dictionary<uint, Queue<ItemDef>>();
            QueueGainedItemSprite = new Dictionary<uint, Queue<ItemDef>>();
            statsLostItems = null;
            statsGainItems = null;
            Playername = new List<CharacterBody>();
            counter = new List<int>();
        }

        private bool getEnemyDropRate(DamageReport damageReport)
        {
            if (!ArtifactOfDoomConfig.useArtifactOfSacrificeCalculation.Value)
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