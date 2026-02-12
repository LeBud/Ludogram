using System;
using System.Collections;
using Manager;
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

    IEnumerator OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out GadgetController gadgetController))
        {
            gadgetController.isInShop = false;
        }
        
        if (other.TryGetComponent(out Gadget gadget))
        {
            yield return new WaitForSeconds(0.5f);
            if (GameManager.instance.moneyManager.moneySaved >= gadgetSeller.total + gadget.Price)
            {
                gadgetSeller.shop.placedGadgets.Remove(gadget.gameObject);
                gadgetSeller.gadgetToSell.Add(gadget);
                gadgetSeller.UdpatePrice(gadget.Price);
                other.gameObject.SetActive(false);
                gadget.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
            else
            {
                Destroy(gadget.gameObject);
            }
            
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.chocolate;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
