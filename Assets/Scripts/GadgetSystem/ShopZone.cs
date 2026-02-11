using UnityEngine;

public class ShopZone : MonoBehaviour
{
    public GadgetSeller gadgetSeller;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out GadgetController gadgetController))
        {
            Debug.Log(gadgetController.name);
            gadgetController.isInShop = true;
            gadgetController.buttonToBuy = gadgetSeller.buttonPrefab;
            gadgetController.gadgetSeller = gadgetSeller;
        }

       
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out GadgetController gadgetController))
        {
            gadgetController.isInShop = false;
        }
        
        if (other.TryGetComponent(out Gadget gadget))
        {
            gadgetSeller.gadgetToSell.Add(gadget);
            gadgetSeller.UdpatePrice(gadget.Price);
            other.gameObject.SetActive(false);
            other.attachedRigidbody.linearVelocity = Vector3.zero;
        }
    }

    
}
