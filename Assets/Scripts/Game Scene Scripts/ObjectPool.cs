using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class ObjectPoolItem //this is an internal class, which we fill with prefabs of our projectiles
{
    public int _amountToPool; //how many projectiles should be created each time we expand the pool
    public NetworkObject _networkObject; //the actual prefab of the projectile
}
public class ObjectPool : MonoBehaviour
{
    private List<NetworkObject> networkObjectPool; //where all our different bullets are stored

    public List<ObjectPoolItem> _networkObjectsToPool; //the list of all our bullets we need to instantiate and keep track of

    private void Awake()
    {
        REF.ObjPool = this; //set the reference to ourselves
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        //StartCoroutine(InitProjectiles());

        CustomNetworkManager.Singleton.OnServerStarted += InitProjectiles;
    }
    private void InitProjectiles()
    {
        networkObjectPool = new List<NetworkObject>(); //init the list
        foreach (ObjectPoolItem item in _networkObjectsToPool) //at the start of the game make sure we create a few of each projectile
        {
            CustomNetworkManager.Singleton.AddNetworkPrefab(item._networkObject.gameObject);
            GrowBulletPool(item._networkObject);
        }
        Debug.Log("Network Objects Initialized");
    }

    private void GrowBulletPool(NetworkObject netObject)
    {
        for (int i = 0; i < 10; i++)
        {
            NetworkObject spawnedNetObj = Instantiate(netObject); //create an object of this kind
            netObject.Spawn();
            spawnedNetObj.TrySetParent(transform);
            AddToPool(netObject);
        }
    }
    public void AddToPool(NetworkObject netObject)
    {
        netObject.TrySetParent(transform);
        netObject.gameObject.SetActive(false); //disable the gameobjects so we don't need to see them
        networkObjectPool.Add(netObject); //add the projectile to the entire pool
    }

    public NetworkObject GetNetworkObjectFromPool(string tag) //take the tag of the prefab we need, and return an instance of it
    {
        for (int i = 0; i < networkObjectPool.Count; i++) //go through the entire pool of all bullets to find one we need
        {
            if (!networkObjectPool[i].gameObject.activeInHierarchy && networkObjectPool[i].tag == tag) //only pull objects which are currently inactive, and have the same tags
            {
                networkObjectPool[i].gameObject.SetActive(true); //if we found the correct bullet, activate it
                //projectilePool[i].transform.SetParent(null);
                return networkObjectPool[i];          //...and now return it!
            }
        }

        //this is only called if we havent found a correct bullet

        foreach (ObjectPoolItem item in _networkObjectsToPool) //check through all possible types of bullets
        {
            if (item._networkObject.tag == tag) //check the tag of the prefab to the one we got
            {
                GrowBulletPool(item._networkObject); //create more of the prefab
                return GetNetworkObjectFromPool(tag); // now get the new projectile, since we created some
            }
        }
        return null;
    }
}