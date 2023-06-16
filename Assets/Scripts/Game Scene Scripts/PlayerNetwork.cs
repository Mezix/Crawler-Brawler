using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    private readonly NetworkVariable<PlayerNetworkData> _playerNetworkData = new NetworkVariable<PlayerNetworkData>(writePerm: NetworkVariableWritePermission.Owner);
    private Vector3 _vel;
    private float _rotVel;
    [SerializeField] private float cheapInterpolationTime = 0.1f;
    private void Update()
    {
        if (IsOwner)
        {
            OwnerBehaviour();
        }
        else
        {
            NotOwnerBehaviour();
        }
    }

    private void OwnerBehaviour()
    {
        _playerNetworkData.Value = new PlayerNetworkData()
        {
            Position = transform.position,
            Rotation = transform.rotation.eulerAngles
        };
    }
    private void NotOwnerBehaviour()
    {
        transform.position = Vector3.SmoothDamp(transform.position, _playerNetworkData.Value.Position, ref _vel, cheapInterpolationTime);
        transform.rotation = Quaternion.Euler(0, 0, Mathf.SmoothDampAngle(transform.rotation.eulerAngles.z, _playerNetworkData.Value.Rotation.z, ref _rotVel, cheapInterpolationTime));
    }
    struct PlayerNetworkData : INetworkSerializable
    {
        private float _x;
        private float _y;
        private float _zrot;

        private float _directionalCursorX;
        private float _directionalCursorY;
        private float _directionCursorRotation;

        internal Vector3 Position
        {
            get => new Vector3(_x, _y, 0);
            set
            {
                _x = value.x;
                _y = value.y;
            }
        }

        internal Vector3 Rotation
        {
            get => new Vector3(0, 0, _zrot);
            set => _zrot = value.z;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _x);
            serializer.SerializeValue(ref _y);
            serializer.SerializeValue(ref _zrot);
        }
    }
}
