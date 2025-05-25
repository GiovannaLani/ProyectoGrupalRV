using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkedSocket : NetworkBehaviour
{
    [SerializeField]
    private InteractionLayerMask acceptedLayers;

    [SerializeField]
    private GameObject attach;

    private bool followSocketPos;
    private Rigidbody objRb;
    public GameObject currentObj;
    private NetworkObject currentNetObj;
    public bool allowSocket;

    public UnityEvent<GameObject> onSocketEnter = new UnityEvent<GameObject>();
    public UnityEvent<GameObject> onSocketExit = new UnityEvent<GameObject>();
    private void Awake()
    {
        followSocketPos = false;
        allowSocket = true;
    }
    private void Update()
    {
        if (IsServer)
        {
            if (currentNetObj != null)
            {
                if (gameObject.transform.parent.gameObject.GetComponent<NetworkObject>().OwnerClientId != currentNetObj.OwnerClientId)
                {
                    currentNetObj.ChangeOwnership(gameObject.transform.parent.gameObject.GetComponent<NetworkObject>().OwnerClientId);
                }

            }
        }

        if(IsOwner)
        {
            if (followSocketPos && currentObj != null)
            {

                float dist = Vector3.Distance(transform.position, currentObj.transform.position);

                if (dist < 0.75f)
                {
                    currentObj.transform.position = transform.position + attach.transform.localPosition;
                    currentObj.transform.rotation = transform.rotation * attach.transform.localRotation;
                }
            }

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        var interactable = other.gameObject.GetComponent<XRGrabInteractable>();
        bool isChild = false;
        if(interactable==null && other.gameObject.transform.parent!=null)
        {
            interactable = other.gameObject.transform.parent.gameObject.GetComponent<XRGrabInteractable>();
            isChild = true;
        }
        var netwGrab = other.gameObject.GetComponent<NetworkedGrab>();
        if(netwGrab == null && other.gameObject.transform.parent != null)
        {
            netwGrab = other.gameObject.transform.parent.gameObject.GetComponent<NetworkedGrab>();
        }

        if (interactable != null && !netwGrab.IsGrabbed() && netwGrab.socket == null && (acceptedLayers.value & interactable.interactionLayers.value) != 0 && allowSocket)
        {
            if (isChild)
            {
                currentObj = other.transform.parent.gameObject;
            }
            else
            {
                currentObj = other.gameObject;
            }
            currentNetObj = currentObj.GetComponent<NetworkObject>();
            objRb = currentObj.GetComponent<Rigidbody>();
            netwGrab.SetSocket(this);
            if (objRb != null)
            {
                objRb.isKinematic = true;
                objRb.useGravity = false;
            }

            Physics.IgnoreCollision(GetComponent<Collider>(), other, true);
            if (currentObj.CompareTag("Cabeza"))
            {
                foreach (Transform child in other.transform)
                {
                    child.gameObject.GetComponent<Collider>().isTrigger = true;
                    Physics.IgnoreCollision(GetComponent<Collider>(), child.gameObject.GetComponent<Collider>(), true);
                }
            }
            followSocketPos = true;
            allowSocket = false;
            SetUpClientRpc(followSocketPos, currentNetObj);

            if(currentObj.CompareTag("Torso"))
            {
                currentObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                attach.transform.localRotation = Quaternion.Euler(0, 90, -90);
            }else if(currentObj.CompareTag("Brazo") || currentObj.CompareTag("Cabeza") || currentObj.CompareTag("Pierna"))
            {
                currentObj.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            }
            onSocketEnter.Invoke(currentObj);
        }
    }

    public void DetachObject()
    {
        if (currentObj == null) return;

        if (currentObj.TryGetComponent<Collider>(out Collider col))
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), col, false);
        }
        else if (currentObj.transform.GetChild(0) != null && currentObj.transform.GetChild(0).TryGetComponent<Collider>(out Collider col2))
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), col2, false);
            if (currentObj.CompareTag("Cabeza"))
            {
                foreach (Transform child in currentObj.transform.GetChild(0).transform)
                {
                    child.gameObject.GetComponent<Collider>().isTrigger = false;
                    Physics.IgnoreCollision(GetComponent<Collider>(), child.gameObject.GetComponent<Collider>(), false);
                }
            }
        }
        if (currentObj.CompareTag("Torso") || currentObj.CompareTag("Brazo") || currentObj.CompareTag("Cabeza") || currentObj.CompareTag("Pierna"))
        {
            currentObj.transform.localScale = Vector3.one;
            if (currentObj.CompareTag("Torso"))
            {
                attach.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            }
        

        currentObj.GetComponent<NetworkedGrab>().SetSocket(null);
        currentObj = null;
        currentNetObj = null;
        objRb = null;

        followSocketPos = false;
        allowSocket = true;
        SetUpClientRpc(followSocketPos, currentNetObj);
        onSocketExit.Invoke(currentObj);
    }
    [ClientRpc]
    private void SetUpClientRpc(bool follow, NetworkObjectReference obj)
    {

        if (obj.TryGet(out NetworkObject currNetObj))
        {
            currentObj = currNetObj.gameObject;
            if(currentObj.TryGetComponent<Collider>(out Collider col))
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), col, true);
            }else if(currentObj.transform.GetChild(0) != null && currentObj.transform.GetChild(0).TryGetComponent<Collider>(out Collider col2))
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), col2, true);
            }
            objRb = currentObj.GetComponent<Rigidbody>();
            if (objRb != null)
            {
                objRb.isKinematic = true;
                objRb.useGravity = false;
            }
        }

        
        else
        {
            if (currentObj != null)
            {
                if (currentObj.TryGetComponent<Collider>(out Collider col))
                {
                    Physics.IgnoreCollision(GetComponent<Collider>(), col, false);
                }
                else if (currentObj.transform.GetChild(0) != null && currentObj.transform.GetChild(0).TryGetComponent<Collider>(out Collider col2))
                {
                    Physics.IgnoreCollision(GetComponent<Collider>(), col2, false);
                }
            }
            currentObj = null;
            if (objRb != null)
            {
                objRb.isKinematic = false;
                objRb.useGravity = true;
            }
            objRb = null;
        }
        followSocketPos = follow;
    }
}
