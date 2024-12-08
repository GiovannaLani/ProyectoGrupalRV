using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class InfoUI : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel; 
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI orderText;
    [SerializeField] private TextMeshProUGUI textCash;
    [SerializeField] private TextMeshProUGUI textWeapon;
    [SerializeField] private Button repairButton;
    [SerializeField] private TextMeshProUGUI repairMessage;


    private OrderController orderController;
    private MafiaController currentMafia;
    private WeaponController weaponController;
    private Coroutine messageCoroutine;

    private void Start()
    {
        orderController = FindObjectOfType<OrderController>();
        weaponController = FindObjectOfType<WeaponController>();
    }

    void Update()
    {
        if (infoPanel != null && infoPanel.activeSelf)
        {
            UpdateMafiaInfo();
            UpdateCashInfo();
            UpdateWeaponInfo();
        }
    }

    private void UpdateMafiaInfo()
    {
        currentMafia = FindObjectOfType<MafiaController>();

        if (currentMafia != null)
        {
            descriptionText.text = currentMafia.AppearanceDescription;
            orderText.text = currentMafia.orderDescription;
        }
        else
        {
            descriptionText.text = "No hay mafioso presente";
            orderText.text = "Sin pedido";
        }
    }

    public void UpdateCashInfo()
    {
        textCash.text =  orderController.cash.ToString();
    }

    public void UpdateWeaponInfo()
    {
        textWeapon.text = $"Durabilidad del cuchillo (max 10): " + weaponController.currentDurability;
        UpdateRepairButton();
    }

    private void UpdateRepairButton()
    {
        if (repairButton != null)
        {
            bool canRepair = weaponController.currentDurability <= 0 && orderController.cash >= weaponController.repairCost;
            repairButton.interactable = canRepair;
            var colors = repairButton.colors;
            colors.normalColor = canRepair ? Color.white : new Color(1f, 1f, 1f, 0.5f); // mas claro
            repairButton.colors = colors;

            if (!canRepair)
            {
                ShowRepairMessage("No tienes suficiente dinero para reparar el cuchillo.");
            }
            else
            {
                HideRepairMessage();
            }

            if (weaponController.currentDurability > 0)
            {
                ShowRepairMessage("Tu arma todav�a no necesita reparaci�n.");
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
