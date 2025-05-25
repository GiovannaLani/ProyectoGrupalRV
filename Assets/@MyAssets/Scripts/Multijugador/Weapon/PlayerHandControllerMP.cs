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

    // Referencias locales de objetos en las manos
    private GameObject leftHandItem;
    private GameObject rightHandItem;

    // Referencias de armas guardadas
    private GameObject weaponRight;
    private GameObject weaponLeft;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Solo configurar eventos para el propietario local
        if (!IsOwner) return;

        SetupHandInteractors();
        SetupInputControls();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        CleanupInputControls();
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

    #region Hand Selection Events
    private void OnLeftHandSelect(SelectEnterEventArgs args)
    {
        leftHandItem = args.interactableObject.transform.gameObject;
        Debug.Log("Left hand grabbed: " + leftHandItem.name);

        // Actualizar estado del arma si es necesario
        UpdateWeaponGrabbedState();
    }

    private void OnLeftHandDeselect(SelectExitEventArgs args)
    {
        Debug.Log("Left hand released: " + (leftHandItem != null ? leftHandItem.name : "Nothing"));
        leftHandItem = null;

        // Actualizar estado del arma si es necesario
        UpdateWeaponGrabbedState();
    }

    private void OnRightHandSelect(SelectEnterEventArgs args)
    {
        rightHandItem = args.interactableObject.transform.gameObject;
        Debug.Log("Right hand grabbed: " + rightHandItem.name);

        // Actualizar estado del arma si es necesario
        UpdateWeaponGrabbedState();
    }

    private void OnRightHandDeselect(SelectExitEventArgs args)
    {
        Debug.Log("Right hand released: " + (rightHandItem != null ? rightHandItem.name : "Nothing"));
        rightHandItem = null;

        // Actualizar estado del arma si es necesario
        UpdateWeaponGrabbedState();
    }
    #endregion

    #region Weapon Management
    private void UpdateWeaponGrabbedState()
    {
        // Actualizar estado de WeaponControllerMP si existe
        bool hasWeapon = false;
        WeaponControllerMP weaponController = null;

        if (rightHandItem != null && rightHandItem.TryGetComponent(out weaponController))
        {
            hasWeapon = true;
        }
        else if (leftHandItem != null && leftHandItem.TryGetComponent(out weaponController))
        {
            hasWeapon = true;
        }

        if (weaponController != null)
        {
            weaponController.SetGrabbedStateServerRpc(hasWeapon);
        }
    }

    private void HideKnife(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        // Intentar ocultar/mostrar arma en mano derecha
        if (TryHideShowWeapon(ref rightHandItem, ref weaponRight, rightHandInteractor))
            return;

        // Intentar ocultar/mostrar arma en mano izquierda
        if (TryHideShowWeapon(ref leftHandItem, ref weaponLeft, leftHandInteractor))
            return;
    }

    private bool TryHideShowWeapon(ref GameObject handItem, ref GameObject storedWeapon, XRDirectInteractor handInteractor)
    {
        // Si hay un arma en la mano, ocultarla
        if (handItem != null && handItem.TryGetComponent<WeaponControllerMP>(out _))
        {
            storedWeapon = handItem;
            handItem.SetActive(false);
            handItem = null;
            return true;
        }

        // Si no hay nada en la mano pero hay un arma guardada, mostrarla
        if (handItem == null && storedWeapon != null)
        {
            storedWeapon.transform.position = handInteractor.transform.position;
            storedWeapon.transform.rotation = handInteractor.transform.rotation;
            storedWeapon.SetActive(true);
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

        var interactable = weapon.GetComponent<XRGrabInteractable>();
        if (interactable != null && handInteractor.interactionManager != null)
        {
            handInteractor.interactionManager.SelectEnter(handInteractor, interactable);
        }
        else
        {
            Debug.LogWarning("No se pudo forzar el agarre del arma.");
        }
    }
    #endregion

    #region Utility Methods
    public bool HasBodyPart()
    {
        bool leftHasBodyPart = leftHandItem != null && leftHandItem.layer == LayerMask.NameToLayer("BodyParts");
        bool rightHasBodyPart = rightHandItem != null && rightHandItem.layer == LayerMask.NameToLayer("BodyParts");

        return leftHasBodyPart || rightHasBodyPart;
    }

    public GameObject GetLeftHandItem()
    {
        return leftHandItem;
    }

    public GameObject GetRightHandItem()
    {
        return rightHandItem;
    }

    public bool HasWeaponInHand()
    {
        bool leftHasWeapon = leftHandItem != null && leftHandItem.TryGetComponent<WeaponControllerMP>(out _);
        bool rightHasWeapon = rightHandItem != null && rightHandItem.TryGetComponent<WeaponControllerMP>(out _);

        return leftHasWeapon || rightHasWeapon;
    }
    #endregion

    private void OnDestroy()
    {
        CleanupInputControls();
    }
}