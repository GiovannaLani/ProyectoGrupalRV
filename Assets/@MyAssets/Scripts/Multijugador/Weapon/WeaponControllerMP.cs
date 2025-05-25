using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WeaponControllerMP : NetworkBehaviour
{
    [Header("Weapon Stats")]
    public int durability = 10;
    public int maxDurability = 10;
    public int repairCost = 10;
    public Material knifeMaterial;

    [Header("Weapon State")]
    public bool isSpawnedWeapon = false;

    // Network Variables
    public NetworkVariable<bool> isGrabbed = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> currentDurability = new NetworkVariable<int>(10,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    // Components
    private Renderer weaponRenderer;
    private XRGrabInteractable grabInteractable;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Inicializar componentes
        weaponRenderer = GetComponent<Renderer>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Configurar valores iniciales solo en el servidor
        if (IsServer)
        {
            currentDurability.Value = durability;
            isGrabbed.Value = false;
        }

        // Suscribirse a cambios en las variables de red
        currentDurability.OnValueChanged += OnDurabilityChanged;
        isGrabbed.OnValueChanged += OnGrabbedChanged;

        // Configurar eventos de interacción solo para el propietario
        if (IsOwner && grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnWeaponGrabbed);
            grabInteractable.selectExited.AddListener(OnWeaponReleased);
        }

        // Aplicar estado inicial
        UpdateWeaponAppearance();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // Limpiar suscripciones
        currentDurability.OnValueChanged -= OnDurabilityChanged;
        isGrabbed.OnValueChanged -= OnGrabbedChanged;

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnWeaponGrabbed);
            grabInteractable.selectExited.RemoveListener(OnWeaponReleased);
        }
    }

    #region Interaction Events
    private void OnWeaponGrabbed(SelectEnterEventArgs args)
    {
        if (!IsOwner) return;

        SetGrabbedStateServerRpc(true);
        Debug.Log($"Weapon grabbed by {OwnerClientId}");
    }

    private void OnWeaponReleased(SelectExitEventArgs args)
    {
        if (!IsOwner) return;

        SetGrabbedStateServerRpc(false);
        Debug.Log($"Weapon released by {OwnerClientId}");
    }
    #endregion

    #region Network RPCs
    [ServerRpc(RequireOwnership = false)]
    public void SetGrabbedStateServerRpc(bool grabbed)
    {
        isGrabbed.Value = grabbed;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetDurabilityServerRpc(int newDurability)
    {
        currentDurability.Value = Mathf.Clamp(newDurability, 0, maxDurability);

        if (currentDurability.Value <= 0)
        {
            OnWeaponBrokenClientRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RepairWeaponServerRpc()
    {
        currentDurability.Value = maxDurability;
        OnWeaponRepairedClientRpc();
    }

    [ClientRpc]
    private void OnWeaponBrokenClientRpc()
    {
        Debug.Log("Weapon is broken!");
        UpdateWeaponAppearance();
    }

    [ClientRpc]
    private void OnWeaponRepairedClientRpc()
    {
        Debug.Log("Weapon has been repaired!");
        UpdateWeaponAppearance();
    }
    #endregion

    #region Network Variable Callbacks
    private void OnDurabilityChanged(int previousValue, int newValue)
    {
        Debug.Log($"Weapon durability changed from {previousValue} to {newValue}");
        UpdateWeaponAppearance();
    }

    private void OnGrabbedChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"Weapon grabbed state changed from {previousValue} to {newValue}");
        // Aquí puedes agregar efectos visuales o de sonido cuando el arma es agarrada/soltada
    }
    #endregion

    #region Public Methods
    public void ReduceDurability(int amount = 1)
    {
        if (!IsServer) return;

        int newDurability = Mathf.Max(0, currentDurability.Value - amount);
        SetDurabilityServerRpc(newDurability);
    }

    public void RepairKnife()
    {
        if (!IsServer) return;

        RepairWeaponServerRpc();
    }

    public bool CanUse()
    {
        return currentDurability.Value > 0;
    }

    public bool IsWeaponGrabbed()
    {
        return isGrabbed.Value;
    }

    public int GetCurrentDurability()
    {
        return currentDurability.Value;
    }

    public int GetMaxDurability()
    {
        return maxDurability;
    }

    public int GetRepairCost()
    {
        return repairCost;
    }
    #endregion

    #region Collider Control
    public void setColliderTrigger(bool isTrigger)
    {
        Collider weaponCollider = GetComponent<Collider>();
        if (weaponCollider != null)
        {
            weaponCollider.isTrigger = isTrigger;
        }
    }
    #endregion

    #region Visual Updates
    private void UpdateWeaponAppearance()
    {
        if (weaponRenderer == null || knifeMaterial == null) return;

        // Cambiar apariencia basada en la durabilidad
        if (currentDurability.Value <= 0)
        {
            // Arma rota - más opaca o diferente color
            Color brokenColor = Color.red;
            brokenColor.a = 0.5f;
            weaponRenderer.material.color = brokenColor;
        }
        else
        {
            // Arma funcional - color normal
            float durabilityPercent = (float)currentDurability.Value / maxDurability;
            Color normalColor = Color.Lerp(Color.yellow, Color.white, durabilityPercent);
            weaponRenderer.material.color = normalColor;
        }
    }
    #endregion

    private void OnValidate()
    {
        // Asegurar que los valores sean válidos en el editor
        if (durability > maxDurability)
            durability = maxDurability;

        if (durability < 0)
            durability = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetWeaponVisibleServerRpc(bool visible)
    {
        SetWeaponVisibleClientRpc(visible);
    }

    [ClientRpc]
    private void SetWeaponVisibleClientRpc(bool visible)
    {
        Debug.Log($" SetActive({visible}) en cliente {NetworkManager.Singleton.LocalClientId}");
        gameObject.SetActive(visible);
    }
}