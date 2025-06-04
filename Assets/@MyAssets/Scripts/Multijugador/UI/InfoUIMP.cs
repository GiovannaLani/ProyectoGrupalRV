using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class InfoUIMP : NetworkBehaviour
{
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI orderText;
    [SerializeField] private TextMeshProUGUI textCash;
    [SerializeField] private TextMeshProUGUI textWeapon;
    [SerializeField] private Button repairButton;
    [SerializeField] private TextMeshProUGUI repairMessage;
    [SerializeField] private Slider repairSlider;
    [SerializeField] private Image repairSliderFillImage;
    [SerializeField] private Button boxButton;
    [SerializeField] private TextMeshProUGUI textBox;
    [SerializeField] private BoxButtom boxBuyController;

    public OrderController orderController;
    public ClientManagerMP clientManager;
    private MafiaControllerMP currentMafia;
    public WeaponControllerMP weaponController;
    private Coroutine messageCoroutine;

    private void Start()
    {
        if (textBox != null)
        {
            textBox.text = boxBuyController.boxCost.ToString();

        }
    }

    void Update()
    {
        Debug.Log("INFO PANEL NULL?" + infoPanel == null);
        Debug.Log("INFO PANEL ACTIVE? " + infoPanel.activeSelf);
        if (infoPanel != null && infoPanel.activeSelf)
        {
            Debug.Log("host:" + IsHost + " IsServer " + IsServer + " isClient " + IsClient);
            Debug.Log("MAFIA 01");
            if (IsHost)
            {
                Debug.Log("MAFIA 02");

                UpdateMafiaInfo();
                UpdateCashInfo();
                UpdateBoxInfo();
            }
            UpdateWeaponInfo();
        }
    }

    private void UpdateMafiaInfo()
    {
                    Debug.Log("MAFIA: 1");
        if (clientManager.mafia != null)
        {
            currentMafia = clientManager.mafia.GetComponent<MafiaControllerMP>();

                    Debug.Log("MAFIA: 2");
            if (currentMafia != null && !currentMafia.served)
            {
                    Debug.Log("MAFIA: 3");
                if (!descriptionText.text.Equals(currentMafia.AppearanceDescription) || !orderText.text.Equals(currentMafia.orderDescription))
                {
                    Debug.Log("MAFIA4: " + currentMafia.AppearanceDescription);
                    descriptionText.text = currentMafia.AppearanceDescription;
                    orderText.text = currentMafia.orderDescription;
                    UpdateMafiaInfoClientRpc(currentMafia.AppearanceDescription, currentMafia.orderDescription);
                }
            }
            else
            {
                    Debug.Log("MAFIA5: ");
                if (!descriptionText.text.Equals("No hay mafioso presente") || !orderText.text.Equals("Sin pedido"))
                {
                    Debug.Log("MAFIA6: No hay mafioso presente");
                    descriptionText.text = "No hay mafioso presente";
                    orderText.text = "Sin pedido";
                    UpdateMafiaInfoClientRpc("No hay mafioso presente", "Sin pedido");
                }
            }
        }
        else
        {
                    Debug.Log("MAFI75: ");
            if (!descriptionText.text.Equals("No hay mafioso presente") || !orderText.text.Equals("Sin pedido"))
            {
                    Debug.Log("MAFIA8: No hay mafioso presente");
                descriptionText.text = "No hay mafioso presente";
                orderText.text = "Sin pedido";
                UpdateMafiaInfoClientRpc("No hay mafioso presente", "Sin pedido");
            }
        }
    }
    [ClientRpc]
    public void UpdateMafiaInfoClientRpc(string description, string order)
    {
        descriptionText.text = description;
        orderText.text = order;
    }
    public void UpdateCashInfo()
    {
        textCash.text = orderController.cash.ToString();
    }
    public void UpdateBoxInfo()
    {
        boxButton.interactable = orderController.cash >= boxBuyController.boxCost;
    }

    public void UpdateWeaponInfo()
    {
        int value = weaponController.durability;
        textWeapon.text = $"Durabilidad del cuchillo (max " + weaponController.maxDurability + "): " + value;
        repairSlider.value = (float)value / (float)weaponController.maxDurability;
        Debug.Log("durab: " + (float)value / (float)weaponController.maxDurability);
        repairSliderFillImage.color = Color.Lerp(Color.red, Color.green, (float)value / (float)weaponController.maxDurability);
        UpdateRepairButton();
    }

    private void UpdateRepairButton()
    {
        if (repairButton != null)
        {
            bool canRepair = weaponController.durability <= 0 && orderController.cash >= weaponController.repairCost;
            repairButton.interactable = canRepair;
            Debug.Log("es reparable " + canRepair + "   " + (weaponController.durability <= 0) + "    " + (orderController.cash >= weaponController.repairCost));

            if (!canRepair)
            {
                ShowRepairMessage("No tienes suficiente dinero para reparar el cuchillo.");
            }
            else
            {
                HideRepairMessage();
            }

            if (weaponController.durability > 0)
            {
                ShowRepairMessage("Tu arma todavía no necesita reparación.");
            }
            else
            {
                HideRepairMessage();
            }
        }
    }

    private void ShowRepairMessage(string message)
    {
        repairMessage.gameObject.SetActive(true);
        repairMessage.text = message;
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }
        messageCoroutine = StartCoroutine(HideMessageIEnum());
    }
    private IEnumerator HideMessageIEnum()
    {
        yield return new WaitForSeconds(3.0f);
        HideRepairMessage();
    }

    private void HideRepairMessage()
    {
        repairMessage.gameObject.SetActive(false);
    }
}
