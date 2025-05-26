using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ClientTimerSliderMP : NetworkBehaviour
{
    public Slider timerSlider;
    public Image sliderFill;
    private bool isActive;

    private float maxTime;

    public void SetActive(bool isActive)
    {
        if (timerSlider == null)
        {
            Debug.LogError("timerSlider es null");
            return;
        }

        this.isActive = isActive;
        timerSlider.transform.parent.gameObject.SetActive(isActive);
    }

    [ClientRpc]
    public void SetActiveClientRpc(bool isActive)
    {
        SetActive(isActive);
    }

    public void UpdateSliderFromServer(float currentTime, float maxTime)
    {
        if (IsServer)
        {
            UpdateSliderClientRpc(currentTime, maxTime);
        }
    }

    [ClientRpc]
    private void UpdateSliderClientRpc(float currentTime, float maxTime)
    {
        Debug.Log($"[ClientRpc] Actualizando slider en cliente con tiempo: {currentTime}/{maxTime}");
        SetSliderValue(currentTime, maxTime);
    }

    public void SetSliderValue(float value, float maxValue)
    {
        timerSlider.value = 1 - (value / maxValue);
        UpdateSliderColor(1 - (value / maxValue));
    }

    void LateUpdate()
    {
        if (isActive && Camera.main != null)
        {
            Vector3 directionToCamera = Camera.main.transform.position - transform.position;
            directionToCamera.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
            Vector3 eulerRotation = targetRotation.eulerAngles;
            eulerRotation.x = 0;
            eulerRotation.z = 0;
            timerSlider.transform.parent.rotation = Quaternion.Euler(eulerRotation);
        }
    }

    void UpdateSliderColor(float percentage)
    {
        if (sliderFill != null)
        {
            sliderFill.color = Color.Lerp(Color.red, Color.green, percentage);
        }
    }
}
