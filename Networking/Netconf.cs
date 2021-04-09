using UnityEngine;
using RoR2;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using BepInEx.Logging;
using BepInEx;
using ArtifactOfDoom;

    /// <summary>
    /// Provides automatic network syncing and mismatch kicking for the AutoConfig module.
    /// </summary>
    //[R2APISubmoduleDependency(nameof(CommandHelper), nameof(PrefabAPI))]
    public class NetworkClass{
        public NetworkClass()
        {
            SetupConfig();
        }
        public void SetupConfig() {
            var netOrchPrefabPrefab = new GameObject("TILER2NetworkClassOrchestratorPrefabPrefab");
            netOrchPrefabPrefab.AddComponent<NetworkIdentity>();
            NetworkClass.CentralNetworkObject = EnigmaticThunder.Modules.Prefabs.InstantiateClone(netOrchPrefabPrefab,"TILER2NetworkClassOrchestratorPrefab",true);
            //NetworkClass.netOrchPrefab = netOrchPrefabPrefab.InstantiateClone("TILER2NetworkClassOrchestratorPrefab");
            
            NetworkClass.CentralNetworkObject.AddComponent<Networking>();
            
            On.RoR2.Networking.GameNetworkManager.OnServerAddPlayerInternal += (orig, self, conn, pcid, extraMsg) => {
                orig(self, conn, pcid, extraMsg);
                NetworkClass.EnsureNetworking();            
            };
        }

        internal static GameObject CentralNetworkObject;
        internal static GameObject _centralNetworkObjectSpawned;
        internal static void EnsureNetworking() {

            if(!_centralNetworkObjectSpawned) {
                _centralNetworkObjectSpawned = UnityEngine.Object.Instantiate(CentralNetworkObject);
                NetworkServer.Spawn(_centralNetworkObjectSpawned);
            }
        }
    }
    public class Networking : NetworkBehaviour {
        public static Networking _instance;

        private void Awake() {
            _instance = this;
        }
        public static void ServerEnsureNetworking() {

            NetworkClass.EnsureNetworking();
        }
        internal static readonly List<NetworkConnection> checkedConnections = new List<NetworkConnection>();




        [TargetRpc]
        public void TargetUpdateProgressBar(NetworkConnection target, string killedNeededEnemies) {
            NetworkClass.EnsureNetworking();

        if (killedNeededEnemies == null)
            Debug.LogError("killedNeededEnemies == null");
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

    }
