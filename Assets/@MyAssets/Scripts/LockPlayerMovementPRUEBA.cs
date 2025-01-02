using UnityEngine;

public class LockPlayerMovementPRUEBA : MonoBehaviour
{
    private Vector3 fixedPosition;

    void Start()
    {
        fixedPosition = transform.position; // Guarda la posici�n inicial
        Debug.Log("Fixed position set to: " + fixedPosition);
    }

    void Update()
    {
        Debug.Log("Trying to lock position: " + transform.position);
        // Fijar la posici�n del XR Rig
        transform.position = fixedPosition;
    }
}
