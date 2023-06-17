using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public abstract class AThrowable : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Collider2D col;
    protected NetworkObject _netObj;
    public float _throwForce;
    public NetworkTransform _networkTransform;

    //public bool _pickedUp; //TODO: network variable
    //public NetworkVariable<bool> _pickedUp = new NetworkVariable<bool>(false);

    private NetworkVariable<PackageData> _packageData = new NetworkVariable<PackageData>(writePerm: NetworkVariableWritePermission.Owner);

    public Transform _parentToTrack;

    public virtual void Awake()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        col = GetComponentInChildren<Collider2D>();
        _netObj = GetComponentInChildren<NetworkObject>();
        _networkTransform = GetComponentInChildren<NetworkTransform>();
    }
    private void Update()
    {
        if(_packageData.Value.PackagePickedUp)
        {
            _networkTransform.enabled = false;
            col.isTrigger = true;
            transform.position = _parentToTrack.position;
            HM.RotateTransformToAngle(transform, _parentToTrack.rotation.eulerAngles);
        }
        else
        {
            _networkTransform.enabled = true;
            col.isTrigger = false;
        }
    }
    public void PickUp(Transform t, bool pickedUp)
    {
        _packageData.Value = new PackageData() { PackagePickedUp = pickedUp };
        if (pickedUp) _parentToTrack = t;
        else _parentToTrack = null;
    }

    public virtual void Throw(Vector3 dir)
    {
        Debug.LogWarning("Throw not implemented yet");
    }

    //  Network Stuff

    struct PackageData : INetworkSerializable
    {
        private bool _pickedUp;
        internal bool PackagePickedUp
        {
            get => _pickedUp;
            set => _pickedUp = value;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _pickedUp);
        }
    }
}
