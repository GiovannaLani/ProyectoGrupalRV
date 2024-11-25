using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OrderController : MonoBehaviour
{
    private XRSocketInteractor socketBox;
<<<<<<< Updated upstream
=======
    private ClientController client;
    private BoxController boxController;
>>>>>>> Stashed changes
    public string boxTag = "Box";
    public GameObject counterPosition; //posicion en la que esperan los clientes
    private bool isBoxOnSocket;

    // Start is called before the first frame update
    void Start()
    {
        socketBox = GetComponent<XRSocketInteractor>();
        socketBox.selectEntered.AddListener(OnBoxOnSocket);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnBoxOnSocket(SelectEnterEventArgs args)
<<<<<<< Updated upstream
    {//comprobar que la caja esta bien puesta con el producto adecuado
        if (args.interactableObject.transform.CompareTag(boxTag))
        {
            isBoxOnSocket = true;
            Debug.Log("Caja en socket");
=======
    {

        if (args.interactableObject.transform.CompareTag(boxTag))
        {
            boxController = args.interactableObject.transform.GetComponent<BoxController>();
            //isBoxOnSocket = true;
            Debug.Log(boxController.IsReadyForDelivery());
            if (client.isOnBuyPoint() && boxController.IsReadyForDelivery())
            {
                Debug.Log("Objecto se ha destruido");
                var lidSocket = boxController.socketLid.transform.gameObject;
                var contentSocket = boxController.socketContent.transform.gameObject;
                Destroy(args.interactableObject.transform.gameObject);
                Destroy(lidSocket);
                Destroy(contentSocket);
            }
>>>>>>> Stashed changes
        }
    }
    /*
    private void OnTriggerEnter(Collider other)
    {
        if(other)
    }
    /*
    private bool ClientOnCounter()
    {
        if ()
            return;
    }
    */
}
