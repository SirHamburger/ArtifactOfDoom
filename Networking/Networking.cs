using UnityEngine.Networking;
using UnityEngine;
using BepInEx;
using EnigmaticThunder;
using RoR2;
using ArtifactOfDoom;
using UnityEngine.UI;
using System;
using System.Collections.Generic;


//Commandhelper is only needed for this example.
//PrefabAPI is needed for the InstantiateClone Method contained within.
public class NetworkClass
{
    //Static references so we do not need to do tricky things with passing references.
    internal static GameObject CentralNetworkObject;
    private static GameObject _centralNetworkObjectSpawned;

    public NetworkClass()
    {
        //We create an empty gameobject to hold all our networked components. The name of this GameObject is largely irrelevant.
        var tmpGo = new GameObject("tmpGo");
        //Add the networkidentity so that Unity knows which Object it's going to be networking all about.
        tmpGo.AddComponent<NetworkIdentity>();
        //Thirdly, we use InstantiateClone from the PrefabAPI to make sure we have full control over our GameObject.
        CentralNetworkObject = EnigmaticThunder.Modules.Prefabs.InstantiateClone(tmpGo, "Blob", true);
        // Delete the now useless temporary GameObject
        GameObject.Destroy(tmpGo);
        //Finally, we add a specific component that we want networked. In this example, we will only be adding one, but you can add as many components here as you like. Make sure these components inherit from NetworkBehaviour.
        CentralNetworkObject.AddComponent<Networking>();
        //In this specific example, we use a console command. You can look at https://github.com/risk-of-thunder/R2Wiki/wiki/Console-Commands for more information on that.
    }
    public static void SpawnNetworkObject()
    {
        if (!_centralNetworkObjectSpawned)
        {
            _centralNetworkObjectSpawned =
                UnityEngine.Object.Instantiate(CentralNetworkObject);
            NetworkServer.Spawn(_centralNetworkObjectSpawned);
        }
    }
}

//Important to note that these NetworkBehaviour classes must not be nested for UNetWeaver to find them.
internal class Networking : NetworkBehaviour
{
    // We only ever have one instance of the networked behaviour here.
    private static Networking _instance;

    private void Awake()
    {
        _instance = this;
    }
    [Server]
    public static void InvokeAddGainedItemsToPlayers(NetworkUser user, string msg)
    {
        _instance.TargetAddGainedItemsToPlayers(user, msg);
    }
    [Server]
    public static void InvokeAddLostItemsOfPlayers(NetworkUser user, string msg)
    {
        _instance.TargetAddLostItemsOfPlayers(user, msg);
    }
    [Server]
    public static void InvokeUpdateProgressBar(NetworkUser user, string msg)
    {
        _instance.TargetUpdateProgressBar(user, msg);
    }
    [Server]
    public static void InvokeIsArtifactActive(bool msg)
    {
        _instance.TargetIsArtifactActive(msg);
    }
    [Server]
    public static void InvokeIsCalculationSacrifice(bool msg)
    {
        _instance.TargetIsCalculationSacrifice(msg);
    }

    // While we can't find the entirety of the Unity Script API in here, we can provide links to them.
    // This attribute is explained here: https://docs.unity3d.com/2017.3/Documentation/ScriptReference/Networking.TargetRpcAttribute.html
    [TargetRpc]
    //Note that the doc explictly says "These functions [-> Functions with the TargetRPC attribute] must begin with the prefix "Target" and cannot be static." 
    private void TargetLog(NetworkConnection target, string msg)
    {

    }
    [TargetRpc]
    private void TargetAddGainedItemsToPlayers(NetworkUser user, string QueueGainedItemSpriteToString)
    {

        if (!ArtifactOfDoomConfig.disableSideBars.Value)
        {
            string[] QueueGainedItemSprite = QueueGainedItemSpriteToString.Split(' ');
            int i = 0;
            foreach (var element in QueueGainedItemSprite)
            {
                if (element != "")
                {
                    if (ArtifactOfDoomUI.listGainedImages[i].GetComponent<Image>() == null)
                        ArtifactOfDoomUI.listGainedImages[i].AddComponent<Image>();
                    ArtifactOfDoomUI.listGainedImages[i].GetComponent<Image>().sprite = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(element)).pickupIconSprite;
                    i++;
                }
            }
        }
    }
    [TargetRpc]
    private void TargetAddLostItemsOfPlayers(NetworkUser user, string QueueLostItemSpriteToString)
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
    [TargetRpc]
    private void TargetUpdateProgressBar(NetworkUser user, string killedNeededEnemies)
    {

        if (!ArtifactOfDoomConfig.disableItemProgressBar.Value && !ArtifactOfDoomUI.calculationSacrifice)
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
    [Client]
    private void TargetIsArtifactActive(bool isActive)
    {

        ArtifactOfDoomUI.ArtifactIsActive = isActive;
    }
    [Client]
    private void TargetIsCalculationSacrifice(bool isActive)
    {
        //Debug.LogError("Set CalculationSacrifice to " + isActive);
        ArtifactOfDoomUI.calculationSacrifice = isActive;
    }

}