using System.Collections;
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

    // Estado de agarre sincronizado
    public NetworkVariable<bool> isGrabbed = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    // Durabilidad sincronizada
    public NetworkVariable<int> currentDurability = new NetworkVariable<int>(
        10,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    // Componentes
    private Renderer weaponRenderer;
    private XRGrabInteractable grabInteractable;

    #region Unity Lifecycle

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        weaponRenderer = GetComponent<Renderer>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (IsServer)
        {
            currentDurability.Value = durability;
            isGrabbed.Value = false;
        }

        // Suscribirse a eventos de red
        currentDurability.OnValueChanged += OnDurabilityChanged;
        isGrabbed.OnValueChanged += OnGrabbedChanged;

        // Eventos locales de agarre (solo si es Owner)
        if (IsOwner && grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnWeaponGrabbed);
            grabInteractable.selectExited.AddListener(OnWeaponReleased);
        }

        UpdateWeaponAppearance();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        currentDurability.OnValueChanged -= OnDurabilityChanged;
        isGrabbed.OnValueChanged -= OnGrabbedChanged;

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnWeaponGrabbed);
            grabInteractable.selectExited.RemoveListener(OnWeaponReleased);
        }
    }

    private void OnValidate()
    {
        durability = Mathf.Clamp(durability, 0, maxDurability);
    }

    #endregion

    #region Interaction Events

    private void OnWeaponGrabbed(SelectEnterEventArgs args)
    {
        if (!IsOwner) return;
        SetGrabbedStateServerRpc(true);
        Debug.Log($" Weapon grabbed by client {OwnerClientId}");
    }

    private void OnWeaponReleased(SelectExitEventArgs args)
    {
        if (!IsOwner) return;
        SetGrabbedStateServerRpc(false);
        Debug.Log($"Weapon released by client {OwnerClientId}");
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
            OnWeaponBrokenClientRpc();
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

    [ServerRpc(RequireOwnership = false)]
    public void ToggleVisibilityServerRpc(bool visible)
    {
        ToggleVisibilityClientRpc(visible);
    }

    [ClientRpc]
    private void ToggleVisibilityClientRpc(bool visible)
    {
        Debug.Log($"Visibilidad cambiada a {(visible ? "ACTIVO" : "OCULTO")} en cliente {NetworkManager.Singleton.LocalClientId}");
        gameObject.SetActive(visible);
    }

    #endregion

    #region Network Variable Callbacks

    private void OnDurabilityChanged(int previousValue, int newValue)
    {
        Debug.Log($"Durabilidad cambió de {previousValue} a {newValue}");
        UpdateWeaponAppearance();
    }

    private void OnGrabbedChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"Estado de agarre cambió de {previousValue} a {newValue}");
        // Aquí puedes poner efectos de sonido, vibración, etc.
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

    public bool CanUse() => currentDurability.Value > 0;
    public bool IsWeaponGrabbed() => isGrabbed.Value;
    public int GetCurrentDurability() => currentDurability.Value;
    public int GetMaxDurability() => maxDurability;
    public int GetRepairCost() => repairCost;

    #endregion

    #region Collider and Visual Updates

    public void SetColliderTrigger(bool isTrigger)
    {
        if (TryGetComponent(out Collider weaponCollider))
        {
            weaponCollider.isTrigger = isTrigger;
        }
    }

    private void UpdateWeaponAppearance()
    {
        if (weaponRenderer == null || knifeMaterial == null) return;

        if (currentDurability.Value <= 0)
        {
            Color brokenColor = new Color(1f, 0f, 0f, 0.5f); // rojo con transparencia
            weaponRenderer.material.color = brokenColor;
        }
        else
        {
            float durabilityPercent = (float)currentDurability.Value / maxDurability;
            Color normalColor = Color.Lerp(Color.yellow, Color.white, durabilityPercent);
            weaponRenderer.material.color = normalColor;
        }
    }

    #endregion
}
