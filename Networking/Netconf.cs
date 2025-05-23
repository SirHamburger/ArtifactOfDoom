using UnityEngine;
using RoR2;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using ArtifactOfDoom;
using UnityEngine.UI;

using R2API;


/// <summary>
/// Provides automatic network syncing and mismatch kicking for the AutoConfig module.
/// </summary>
//[R2APISubmoduleDependency(nameof(CommandHelper), nameof(PrefabAPI))]
public class NetworkClass
{
    public NetworkClass()
    {
        SetupConfig();
    }
    public void SetupConfig()
    {
        var artifactOfDoomNetworkingPrefab = new GameObject("ArtifactOfDoomNetworkingPrefab");
        artifactOfDoomNetworkingPrefab.AddComponent<NetworkIdentity>();

        NetworkClass.CentralNetworkObject = artifactOfDoomNetworkingPrefab.InstantiateClone("ArtifactOfDoomNetworking", true);

        NetworkClass.CentralNetworkObject.AddComponent<Networking>();

    }

    internal static GameObject CentralNetworkObject;
    internal static GameObject _centralNetworkObjectSpawned;
    internal static void EnsureNetworking()
    {

        if (!_centralNetworkObjectSpawned)
        {
            _centralNetworkObjectSpawned = UnityEngine.Object.Instantiate(CentralNetworkObject);
            NetworkServer.Spawn(_centralNetworkObjectSpawned);
        }
    }
}
public class Networking : NetworkBehaviour
{
    public static Networking _instance;
    [SyncVar]
    public bool IsArtifactEnabled = false;
    [SyncVar]
    public bool IsCalculationSacrifice = false;
    private void Awake()
    {
        _instance = this;
    }
    public static void ServerEnsureNetworking()
    {

        NetworkClass.EnsureNetworking();
    }
    internal static readonly List<NetworkConnection> checkedConnections = new List<NetworkConnection>();




    [TargetRpc]
    public void TargetUpdateProgressBar(NetworkConnection target, string killedNeededEnemies)
    {
        NetworkClass.EnsureNetworking();

        if (killedNeededEnemies == null)
            Debug.LogError("killedNeededEnemies == null");
        if (!ArtifactOfDoomConfig.disableItemProgressBar.Value && !Networking._instance.IsCalculationSacrifice)
        {
            string[] stringkilledNeededEnemies = killedNeededEnemies.Split(',');
            if (stringkilledNeededEnemies == null)
                Debug.LogError("stringkilledneededEnemies=null");

            int enemiesKilled = Convert.ToInt32(stringkilledNeededEnemies[0]);
            int enemiesNeeded = Convert.ToInt32(stringkilledNeededEnemies[1]) + 2;


            double progress = (double)enemiesKilled / ((double)enemiesNeeded);

            if ((0.35f + (float)(progress * 0.3)) > 0.65f)
            {

                if (ArtifactOfDoomUI.itemGainBar.GetComponent<RectTransform>().anchorMax == null)
                    Debug.LogError("itemGainBar.GetComponent<RectTransform>().anchorMax==null");

                ArtifactOfDoomUI.itemGainBar.GetComponent<RectTransform>().anchorMax = new Vector2(0.65f, 0.06f);
            }
            else
            {

                ArtifactOfDoomUI.itemGainBar.GetComponent<RectTransform>().anchorMin = new Vector2(0.35f, 0.05f);

                ArtifactOfDoomUI.itemGainBar.GetComponent<RectTransform>().anchorMax = new Vector2(0.35f + (float)(progress * 0.3), 0.06f);
            }
        }
    }

    [TargetRpc]
    public void TargetAddGainedItemsToPlayers(NetworkConnection target, string QueueGainedItemSpriteToString)
    {
        NetworkClass.EnsureNetworking();


        if (!ArtifactOfDoomConfig.disableSideBars.Value)
        {
            string[] QueueGainedItemSprite = QueueGainedItemSpriteToString.Split(' ');
            int i = 0;
            foreach (var element in QueueGainedItemSprite)
            {
                if (element != "")
                {
                    if(ArtifactOfDoomUI.listGainedImages.Count<=i)
                        return;
                    if (ArtifactOfDoomUI.listGainedImages[i].GetComponent<Image>() == null)
                        ArtifactOfDoomUI.listGainedImages[i].AddComponent<Image>();
                    ArtifactOfDoomUI.listGainedImages[i].GetComponent<Image>().sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(element)).pickupIconSprite;
                    i++;
                }
            }
        }
    }
    [TargetRpc]
    public void TargetAddLostItemsOfPlayers(NetworkConnection target, string QueueLostItemSpriteToString)
    {
        if (!ArtifactOfDoomConfig.disableSideBars.Value)
        {
            string[] QueueLostItemSprite = QueueLostItemSpriteToString.Split(' ');

            int i = 0;
            foreach (var element in QueueLostItemSprite)
            {
                if (element != "")
                {

                    if (ArtifactOfDoomUI.listLostImages[i].GetComponent<Image>() == null)
                        ArtifactOfDoomUI.listLostImages[i].AddComponent<Image>();
                    ArtifactOfDoomUI.listLostImages[i].GetComponent<Image>().sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(element)).pickupIconSprite;

                    i++;
                }

            }
        }

    }
}
