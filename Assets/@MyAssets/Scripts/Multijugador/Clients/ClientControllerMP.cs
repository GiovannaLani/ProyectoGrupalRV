using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

public class ClientControllerMP : PersonControllerMP
{
    public float distanceThreshold = 1f;

    public bool hasBeenAttacked = false;

    public Rigidbody[] deadRigidbodies;
    public Collider[] deadColliders;
    public int countBodyParts = 5;
    public GameObject body;
    public GameObject[] sliceableParts;
    public Vector3 reportDirection;
    public Joint[] joints;
    public bool isFemale;
    public GameObject[] clothesMaterials;
    public GameObject skinMaterial;

    protected override void Start()
    {
        buyProbability = 0.5f;
        foreach (Rigidbody rg in deadRigidbodies)
        {
            rg.isKinematic = true;
        }
        foreach (Collider collider in deadColliders)
        {
            collider.enabled = false;
        }
        base.Start();
    }

    public void ReportDeath()
    {
        if (!isAlive || isReported) return;

        personDirection = -1;
        isFinalMove = true;
        isReported = true;
        StopAllCoroutines();
        reportDirection = agent.destination;
        agent.speed = 4;
        agent.enabled = false;
        StartCoroutine(HandleDeathSequence());
    }

    private IEnumerator HandleDeathSequence()
    {

        if (inBuyPoint)
        {
            buyPointController.FreePoint();
        }
        else if (inQueue)
        {
            buyPointController.RemoveClientFromQueue(this);
        }
        audioSource[4].Stop();
        if (isFemale)
        {
            audioSource[1].Play();
        }
        else
        {
            audioSource[2].Play();
        }
        animator.SetTrigger("scared");

        Vector3 direction = -(reportDirection - transform.position).normalized * personDirection;
        Debug.Log(reportDirection + "mira q: " + direction);
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;

        yield return new WaitForSeconds(2f);

        animator.SetTrigger("run");
        agent.enabled = true;
        yield return StartCoroutine(MoveToFinalPoint());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<WeaponControllerMP>(out WeaponControllerMP weapon) && weapon.isGrabbed.Value)
        {
            Debug.Log("Cliente atacado por un cuchillo.");
            audioSource[4].Stop();
            audioSource[3].Play();
            hasBeenAttacked = true;
            StopAllCoroutines();
            StartCoroutine(Die(other));
        }
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        animator.enabled = false;
        if (gameObject.TryGetComponent<Collider>(out Collider col)) Destroy(col);
        foreach (Collider partcol in deadColliders)
        {
            partcol.enabled = true;
            partcol.gameObject.AddComponent<DetectableTarget>();
            partcol.gameObject.layer = LayerMask.NameToLayer("BodyParts");
        }
        foreach (GameObject part in sliceableParts)
        {
            part.layer = LayerMask.NameToLayer("Sliceable");
        }

    }
    protected virtual IEnumerator Die(Collider other)
    {
        if (isAlive && IsServer)
        {
            if (Clothing.existingClientDescriptions.Contains(appearanceDescription)) Clothing.existingClientDescriptions.Remove(appearanceDescription);
            slider.SetActive(false);
            Destroy(gameObject.GetComponent<Collider>());
            Destroy(gameObject.GetComponent<VisionSensor>());
            GetComponent<Rigidbody>().isKinematic = true;
            isAlive = false;
            if (inBuyPoint)
            {
                Debug.Log("MUERTO SERVICIO");
                buyPointController.FreePoint();
            }
            else if (inQueue)
            {
                Debug.Log("MUERTO EN COLA");
                buyPointController.RemoveClientFromQueue(this);
            }
            if (agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }
            DieClientRpc();
            //animator.enabled = false;
            this.GetComponent<RagdollNetworkManager>().SpawnFollowerBones();
            foreach (Rigidbody rb in deadRigidbodies)
            {
                rb.isKinematic = false;
                


            }

            foreach (Collider col in deadColliders)
            {
                col.enabled = true;
                col.gameObject.AddComponent<DetectableTarget>();
                col.gameObject.layer = LayerMask.NameToLayer("BodyParts");
            }

            Vector3 forceDirection = -(other.transform.position - transform.position).normalized;
            float forceStrength = 4f;
            Vector3 force = forceDirection * forceStrength;

            foreach (Rigidbody rb in deadRigidbodies)
            {
                rb.AddForce(force, ForceMode.Impulse);
            }

            yield return new WaitForSeconds(2f);

            foreach (GameObject part in sliceableParts)
            {
                part.layer = LayerMask.NameToLayer("Sliceable");
            }

        }
    }
    public string AppearanceDescription
    {
        get => appearanceDescription;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("La apariencia asignada es vacía o nula.");
            }
            else
            {
                appearanceDescription = value;
                Debug.Log("Apariencia del mafioso asignada: " + appearanceDescription);
            }
        }
    }
    void OnEnable()
    {
        Clothing.OnClientAppearanceGenerated += HandleAppearanceGenerated;
    }

    void OnDisable()
    {
        Clothing.OnClientAppearanceGenerated -= HandleAppearanceGenerated;
    }

    private void HandleAppearanceGenerated(string description)
    {
        AppearanceDescription = description;
    }
    private void OnDestroy()
    {
        if (Clothing.existingClientDescriptions.Contains(appearanceDescription)) Clothing.existingClientDescriptions.Remove(appearanceDescription);
    }
}

