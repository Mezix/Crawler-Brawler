using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    [HideInInspector] private bool _serverAuth;

    private NetworkVariable<PlayerNetworkData> _playerState;


    struct PlayerNetworkData : INetworkSerializable
    {
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            throw new System.NotImplementedException();
        }
    }
    private void Awake()
    {
        var permission = _serverAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        //_playerState = new NetworkVariable<PlayerNetworkState>(writePerm:permission);
    }
}
