using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(NetworkObject), typeof(XRGrabInteractable))]
public class NetworkedGrab : NetworkBehaviour
{
    private NetworkObject networkObject;
    private XRGrabInteractable interactable;
    public NetworkedSocket socket = null;
    private NetworkVariable<bool> isGrabbed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
        interactable = GetComponent<XRGrabInteractable>();
        interactable.selectEntered.AddListener((args) =>{RequestOwnership();});

        interactable.selectExited.AddListener((args) =>{ReleaseOwnership();});
    }

    private void RequestOwnership()
    {
        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestOwnershipRpc(clientID);
    }

    private void ReleaseOwnership()
    {
        ReleaseOwnershipRpc();
    }

    [ClientRpc]
    private void SetWeaponTriggerClientRpc(bool trigger)
    {
        gameObject.GetComponent<Collider>().isTrigger = trigger;
    }

    [Rpc(SendTo.Server)]
    private void RequestOwnershipRpc(ulong clientID)
    {
        if(gameObject.TryGetComponent<WeaponController>(out _))
        {
            SetWeaponTriggerClientRpc(true);
        }
        isGrabbed.Value = true;

        if (socket != null)
        {
            socket.DetachObject();
        }
        if (networkObject.OwnerClientId != clientID)
        {
            networkObject.ChangeOwnership(clientID);
        }
    }

    [Rpc(SendTo.Server)]
    private void ReleaseOwnershipRpc()
    {
        if (gameObject.TryGetComponent<WeaponController>(out _))
        {
            SetWeaponTriggerClientRpc(false);
        }
        isGrabbed.Value = false;
        if (socket == null)
        {
            var objRb = gameObject.GetComponent<Rigidbody>();
            if (objRb != null)
            {
                objRb.isKinematic = false;
                objRb.useGravity = true;
            }
        }
        networkObject.RemoveOwnership();
    }

    public void SetSocket(NetworkedSocket socket)
    {
        this.socket = socket;
    }
    public bool IsGrabbed()
    {
        return isGrabbed.Value;
    }
}