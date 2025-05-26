using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Netcode;

public class PlayerHandControllerMP : NetworkBehaviour
{
    [Header("Hand Interactors")]
    public XRDirectInteractor leftHandInteractor;
    public XRDirectInteractor rightHandInteractor;

    [Header("Input Actions")]
    private PlayerControls playerControls;

    // Objetos actualmente en las manos
    private GameObject leftHandItem;
    private GameObject rightHandItem;

    // Armas ocultas temporalmente
    private GameObject weaponRight;
    private GameObject weaponLeft;


    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        SetupHandInteractors();
        SetupInputControls();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        CleanupInputControls();
        CleanupHandInteractors();
    }

    private void OnDestroy()
    {
        CleanupInputControls();
        CleanupHandInteractors();
    }

 
    private void SetupHandInteractors()
    {
        if (leftHandInteractor != null)
        {
            leftHandInteractor.selectEntered.AddListener(OnLeftHandSelect);
            leftHandInteractor.selectExited.AddListener(OnLeftHandDeselect);
        }

        if (rightHandInteractor != null)
        {
            rightHandInteractor.selectEntered.AddListener(OnRightHandSelect);
            rightHandInteractor.selectExited.AddListener(OnRightHandDeselect);
        }
    }

    private void CleanupHandInteractors()
    {
        if (leftHandInteractor != null)
        {
            leftHandInteractor.selectEntered.RemoveListener(OnLeftHandSelect);
            leftHandInteractor.selectExited.RemoveListener(OnLeftHandDeselect);
        }

        if (rightHandInteractor != null)
        {
            rightHandInteractor.selectEntered.RemoveListener(OnRightHandSelect);
            rightHandInteractor.selectExited.RemoveListener(OnRightHandDeselect);
        }
    }

    private void SetupInputControls()
    {
        playerControls.Player.ClickB.performed += HideKnife;
        playerControls.Enable();
    }

    private void CleanupInputControls()
    {
        if (playerControls != null)
        {
            playerControls.Player.ClickB.performed -= HideKnife;
            playerControls.Disable();
        }
    }



    private void OnLeftHandSelect(SelectEnterEventArgs args)
    {
        leftHandItem = args.interactableObject.transform.gameObject;
        Debug.Log($"Left hand grabbed: {leftHandItem.name}");
        UpdateWeaponGrabbedState();
    }

    private void OnLeftHandDeselect(SelectExitEventArgs args)
    {
        Debug.Log($"Left hand released: {(leftHandItem != null ? leftHandItem.name : "Nothing")}");
        leftHandItem = null;
        UpdateWeaponGrabbedState();
    }

    private void OnRightHandSelect(SelectEnterEventArgs args)
    {
        rightHandItem = args.interactableObject.transform.gameObject;
        Debug.Log($"Right hand grabbed: {rightHandItem.name}");
        UpdateWeaponGrabbedState();
    }

    private void OnRightHandDeselect(SelectExitEventArgs args)
    {
        Debug.Log($"Right hand released: {(rightHandItem != null ? rightHandItem.name : "Nothing")}");
        rightHandItem = null;
        UpdateWeaponGrabbedState();
    }


    private void HideKnife(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        // Primero intenta con mano derecha
        if (TryHideShowWeapon(ref rightHandItem, ref weaponRight, rightHandInteractor))
            return;

        // Luego intenta con mano izquierda
        if (TryHideShowWeapon(ref leftHandItem, ref weaponLeft, leftHandInteractor))
            return;

        Debug.Log("No hay arma que ocultar o mostrar");
    }

    private bool TryHideShowWeapon(ref GameObject handItem, ref GameObject storedWeapon, XRDirectInteractor handInteractor)
    {
        // Ocultar arma si está en mano
        if (handItem != null && handItem.TryGetComponent<WeaponControllerMP>(out var grabbedWeapon))
        {
            storedWeapon = handItem;
            grabbedWeapon.ToggleVisibilityServerRpc(false);
            handItem = null;
            return true;
        }

        // Mostrar arma guardada si no hay nada en la mano
        if (handItem == null && storedWeapon != null && storedWeapon.TryGetComponent<WeaponControllerMP>(out var storedWeaponController))
        {
            storedWeaponController.ToggleVisibilityServerRpc(true);

            storedWeapon.transform.position = handInteractor.transform.position;
            storedWeapon.transform.rotation = handInteractor.transform.rotation;

            StartCoroutine(ForceGrabWeapon(storedWeapon, handInteractor));
            handItem = storedWeapon;
            storedWeapon = null;
            return true;
        }

        return false;
    }

    private IEnumerator ForceGrabWeapon(GameObject weapon, XRDirectInteractor handInteractor)
    {
        yield return new WaitForEndOfFrame();

        if (weapon.TryGetComponent<XRGrabInteractable>(out var interactable) && handInteractor.interactionManager != null)
        {
            handInteractor.interactionManager.SelectEnter(handInteractor, interactable);
        }
        else
        {
            Debug.LogWarning("No se pudo forzar el agarre del arma.");
        }
    }


    private void UpdateWeaponGrabbedState()
    {
        WeaponControllerMP weaponController = null;

        if (rightHandItem != null && rightHandItem.TryGetComponent(out weaponController))
        {
            weaponController.SetGrabbedStateServerRpc(true);
        }
        else if (leftHandItem != null && leftHandItem.TryGetComponent(out weaponController))
        {
            weaponController.SetGrabbedStateServerRpc(true);
        }
    }



    public bool HasBodyPart()
    {
        return (leftHandItem != null && leftHandItem.layer == LayerMask.NameToLayer("BodyParts")) ||
               (rightHandItem != null && rightHandItem.layer == LayerMask.NameToLayer("BodyParts"));
    }

    public GameObject GetLeftHandItem() => leftHandItem;

    public GameObject GetRightHandItem() => rightHandItem;

    public bool HasWeaponInHand()
    {
        return (leftHandItem != null && leftHandItem.TryGetComponent<WeaponControllerMP>(out _)) ||
               (rightHandItem != null && rightHandItem.TryGetComponent<WeaponControllerMP>(out _));
    }

}
