using System;
using System.Collections.Generic;
using Manager;
using Player;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GadgetSeller : MonoBehaviour
{
    public           List<Gadget> gadgetToSell = new();
    public int          total        = 0;
    [SerializeField] TMP_Text     priceText;
    [SerializeField] Transform    spawnPlace;
    public           GameObject   buttonPrefab;
    private          Collider     collider;
    public GadgetShop shop;

    void Start()
    {
        collider       = GetComponent<Collider>();
        total          = 0;
        priceText.text = total.ToString() + "$";
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Gadget gadget))
        {
            if (GameManager.instance.moneyManager.moneySaved >= total + gadget.Price)
            {
                shop.placedGadgets.Remove(gadget.gameObject);
                gadgetToSell.Add(gadget);
                UdpatePrice(gadget.Price);
                other.gameObject.SetActive(false);
                other.attachedRigidbody.linearVelocity = Vector3.zero;
            }
        }
    }

    public void UdpatePrice(int price)
    {
        total += price;
        priceText.text = total.ToString() + "$";
    }

    public void BuyGadget()
    {
        for (int i = gadgetToSell.Count - 1; i >= 0; i--)
        {
            Vector3 position = Random.insideUnitSphere * 2;
            gadgetToSell[i].transform.position = spawnPlace.position + position;
            gadgetToSell[i].gameObject.SetActive(true);
            gadgetToSell.Remove(gadgetToSell[i]);
        }

        GameManager.instance.moneyManager.ActualizeMoney(-total);
        shop.ResetGadget();
        total          = 0;
        priceText.text = total.ToString() + "$";

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(spawnPlace.position, 02f);
    }

    
}
