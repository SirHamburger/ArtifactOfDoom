// using UnityEngine.Networking;
// using UnityEngine;
// using BepInEx;
// using EnigmaticThunder;
// using RoR2;
// using ArtifactOfDoom;
// using UnityEngine.UI;
// using System;
// using System.Collections.Generic;


// //Commandhelper is only needed for this example.
// //PrefabAPI is needed for the InstantiateClone Method contained within.
// public class NetworkClass
// {
//     //Static references so we do not need to do tricky things with passing references.
//     internal static GameObject CentralNetworkObject;
//     private static GameObject _centralNetworkObjectSpawned;

//     public NetworkClass()
//     {
//         var netOrchPrefabPrefab = new GameObject("ArtifactOfDoomNetworkingPrefab");
//         netOrchPrefabPrefab.AddComponent<NetworkIdentity>();
//         NetworkClass.CentralNetworkObject = EnigmaticThunder.Modules.Prefabs.InstantiateClone(netOrchPrefabPrefab, "ArtifactOfDoomClassNetworkingPrefab", true);
//         //NetworkClass.netOrchPrefab = netOrchPrefabPrefab.InstantiateClone("TILER2NetworkClassOrchestratorPrefab");

//         NetworkClass.CentralNetworkObject.AddComponent<Networking>();

//         On.RoR2.Networking.GameNetworkManager.OnServerAddPlayerInternal += (orig, self, conn, pcid, extraMsg) =>
//         {
//             orig(self, conn, pcid, extraMsg);
//             if (Util.ConnectionIsLocal(conn) || Networking.checkedConnections.Contains(conn)) return;
//             Networking.checkedConnections.Add(conn);
//             NetworkClass.EnsureNetworking();
//         };
//     }
//     internal static void EnsureNetworking()
//     {

//         if (!_centralNetworkObjectSpawned)
//         {
//             _centralNetworkObjectSpawned = UnityEngine.Object.Instantiate(CentralNetworkObject);
//             NetworkServer.Spawn(_centralNetworkObjectSpawned);
//         }
//     }
// }

// //Important to note that these NetworkBehaviour classes must not be nested for UNetWeaver to find them.
// internal class Networking : NetworkBehaviour
// {
//     internal static readonly List<NetworkConnection> checkedConnections = new List<NetworkConnection>();

//     // We only ever have one instance of the networked behaviour here.
//     public static Networking _instance;
//     private void Awake()
//     {
//         _instance = this;
//     }
//     [Server]
//     public void AddGainedItemsToPlayers(NetworkUser user, string msg)
//     {
//         TargetAddGainedItemsToPlayers(user.connectionToClient, msg);
//     }
//     [Server]
//     public void CallUpdateProgressBar(NetworkUser target, string msg)
//     {
//         Debug.LogWarning("in callupdateProgressBar");
//         TargetUpdateProgressBar(target.connectionToClient, msg);
//     }
//     [Server]
//     public void AddLostItemsOfPlayers(NetworkUser user, string msg)
//     {
//         TargetAddLostItemsOfPlayers(user.connectionToClient, msg);
//     }

//     [Server]
//     public void IsArtifactActive(NetworkUser user, bool msg)
//     {
//         RpcIsArtifactActive(msg);
//     }
//     [Server]
//     public void IsCalculationSacrifice(NetworkUser user, bool msg)
//     {
//         RpcIsCalculationSacrifice(msg);
//     }

//     // While we can't find the entirety of the Unity Script API in here, we can provide links to them.
//     // This attribute is explained here: https://docs.unity3d.com/2017.3/Documentation/ScriptReference/Networking.TargetRpcAttribute.html
//     [TargetRpc]
//     private void TargetAddGainedItemsToPlayers(NetworkConnection target, string QueueGainedItemSpriteToString)
//     {
//         NetworkClass.EnsureNetworking();


//         if (!ArtifactOfDoomConfig.disableSideBars.Value)
//         {
//             string[] QueueGainedItemSprite = QueueGainedItemSpriteToString.Split(' ');
//             int i = 0;
//             foreach (var element in QueueGainedItemSprite)
//             {
//                 if (element != "")
//                 {
//                     if (ArtifactOfDoomUI.listGainedImages[i].GetComponent<Image>() == null)
//                         ArtifactOfDoomUI.listGainedImages[i].AddComponent<Image>();
//                     ArtifactOfDoomUI.listGainedImages[i].GetComponent<Image>().sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(element)).pickupIconSprite;
//                     i++;
//                 }
//             }
//         }
//     }
//     [TargetRpc]
//     private void TargetAddLostItemsOfPlayers(NetworkConnection target, string QueueLostItemSpriteToString)
//     {
//         if (!ArtifactOfDoomConfig.disableSideBars.Value)
//         {
//             string[] QueueLostItemSprite = QueueLostItemSpriteToString.Split(' ');

//             int i = 0;
//             foreach (var element in QueueLostItemSprite)
//             {
//                 if (element != "")
//                 {

//                     if (ArtifactOfDoomUI.listLostImages[i].GetComponent<Image>() == null)
//                         ArtifactOfDoomUI.listLostImages[i].AddComponent<Image>();
//                     ArtifactOfDoomUI.listLostImages[i].GetComponent<Image>().sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(element)).pickupIconSprite;

//                     i++;
//                 }

//             }
//         }

//     }
//     [TargetRpc]
//     public void TargetUpdateProgressBar(NetworkConnection target, string killedNeededEnemies)
//     {
//         CliUpdateProgress(killedNeededEnemies);
//     }
//     [Client]
//     private void CliUpdateProgress(string killedNeededEnemies)
//     {
//         Debug.LogError("-------------------------------AHAAAAAAAAAAAAAA------------------------------------");
//         if (killedNeededEnemies == null)
//             Debug.LogError("killedNeededEnemies == null");
//         if (!ArtifactOfDoomConfig.disableItemProgressBar.Value && !ArtifactOfDoomUI.calculationSacrifice)
//         {
//             string[] stringkilledNeededEnemies = killedNeededEnemies.Split(',');
//             if (stringkilledNeededEnemies == null)
//                 Debug.LogError("stringkilledneededEnemies=null");

//             int enemiesKilled = Convert.ToInt32(stringkilledNeededEnemies[0]);
//             int enemiesNeeded = Convert.ToInt32(stringkilledNeededEnemies[1]) + 2;


//             double progress = (double)enemiesKilled / ((double)enemiesNeeded);

//             if ((0.35f + (float)(progress * 0.3)) > 0.65f)
//             {

//                 if (ArtifactOfDoomUI.itemGainBar.GetComponent<RectTransform>().anchorMax == null)
//                     Debug.LogError("itemGainBar.GetComponent<RectTransform>().anchorMax==null");

//                 ArtifactOfDoomUI.itemGainBar.GetComponent<RectTransform>().anchorMax = new Vector2(0.65f, 0.06f);
//             }
//             else
//             {

//                 ArtifactOfDoomUI.itemGainBar.GetComponent<RectTransform>().anchorMin = new Vector2(0.35f, 0.05f);

//                 ArtifactOfDoomUI.itemGainBar.GetComponent<RectTransform>().anchorMax = new Vector2(0.35f + (float)(progress * 0.3), 0.06f);
//             }
//         }
//     }
//     [ClientRpc]
//     private void RpcIsArtifactActive(bool isActive)
//     {
//         Debug.LogWarning("TargetIsArtifactActive: " + isActive);
//         ArtifactOfDoomUI.ArtifactIsActive = isActive;
//     }
//     [ClientRpc]
//     private void RpcIsCalculationSacrifice(bool isActive)
//     {
//         //Debug.LogError("Set CalculationSacrifice to " + isActive);
//         ArtifactOfDoomUI.calculationSacrifice = isActive;
//     }

// }