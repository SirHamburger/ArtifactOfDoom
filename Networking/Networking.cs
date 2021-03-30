using UnityEngine.Networking;
using UnityEngine;
using BepInEx;
using EnigmaticThunder;
using RoR2;


[BepInPlugin("com.networktest","ifISeethisInTechSupportIwillBerateYou","0.0.1")]
//Commandhelper is only needed for this example.
//PrefabAPI is needed for the InstantiateClone Method contained within.
class MyPluginClass : BaseUnityPlugin
{
    //Static references so we do not need to do tricky things with passing references.
    internal static GameObject CentralNetworkObject;
    private static GameObject _centralNetworkObjectSpawned;

    public void Awake()
    {
        //We create an empty gameobject to hold all our networked components. The name of this GameObject is largely irrelevant.
        var tmpGo = new GameObject("tmpGo");
        //Add the networkidentity so that Unity knows which Object it's going to be networking all about.
        tmpGo.AddComponent<NetworkIdentity>();
        //Thirdly, we use InstantiateClone from the PrefabAPI to make sure we have full control over our GameObject.
        CentralNetworkObject= EnigmaticThunder.Modules.Prefabs.InstantiateClone(tmpGo,"Blob",true);
        // Delete the now useless temporary GameObject
        GameObject.Destroy(tmpGo);
        //Finally, we add a specific component that we want networked. In this example, we will only be adding one, but you can add as many components here as you like. Make sure these components inherit from NetworkBehaviour.
        CentralNetworkObject.AddComponent<MyNetworkComponent>();

        //In this specific example, we use a console command. You can look at https://github.com/risk-of-thunder/R2Wiki/wiki/Console-Commands for more information on that.
    }

    // ExecuteOnServer as a concommand flag makes sure it's always exectuted on the... you guessed it, server.
    private static void CCNetworkLog(ConCommandArgs args)
    {
        //Although here it's not relevant, you can ensure you are the server by checking if the NetworkServer is active.
        if (NetworkServer.active)
        {
            //Before we can Invoke our NetworkMessage, we need to make sure our centralized networkobject is spawned.
            // For doing that, we Instantiate the CentralNetworkObject, we obviously check if we don't already have one that is already instantiated and activated in the current scene.
            // Note : Make sure you Instantiate the gameobject, and not spawn it directly, it would get deleted otherwise on scene change, even with DontDestroyOnLoad.
            if (!_centralNetworkObjectSpawned)
            {
	            _centralNetworkObjectSpawned = 
		            Object.Instantiate(CentralNetworkObject);
	            NetworkServer.Spawn(_centralNetworkObjectSpawned);
            }
            //This readOnlyInstancesList is great for going over all players in general, 
            // so it might be worth commiting to memory.
            foreach (NetworkUser user in NetworkUser.readOnlyInstancesList)
            {
                //Args.userArgs is a list of all words in the command arguments.
                MyNetworkComponent.Invoke(user,string.Join(" ", args.userArgs));
                //MyNetworkComponent._instance.Invoke(user, string.Join(" ", args.userArgs));
            }
        }
    }
}

//Important to note that these NetworkBehaviour classes must not be nested for UNetWeaver to find them.
internal class MyNetworkComponent : NetworkBehaviour
{
	// We only ever have one instance of the networked behaviour here.
	private static MyNetworkComponent _instance;
	
	private void Awake()
	{
		_instance = this;
	}
    public static void Invoke(NetworkUser user, string msg)
    {
        _instance.TargetLog(user.connectionToClient, msg);
    }

    // While we can't find the entirety of the Unity Script API in here, we can provide links to them.
    // This attribute is explained here: https://docs.unity3d.com/2017.3/Documentation/ScriptReference/Networking.TargetRpcAttribute.html
    [TargetRpc]
    //Note that the doc explictly says "These functions [-> Functions with the TargetRPC attribute] must begin with the prefix "Target" and cannot be static." 
    private void TargetLog(NetworkConnection target, string msg)
    {
        Debug.Log(msg);
    }
}