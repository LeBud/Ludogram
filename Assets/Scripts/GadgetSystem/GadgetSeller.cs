using System;
using System.Collections.Generic;
using Player;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GadgetSeller : MonoBehaviour
{
    private          List<Gadget> gadgetToSell = new();
    [SerializeField] int          total = 0;
    [SerializeField] TMP_Text     priceText;
    [SerializeField] Transform   spawnPlace;
    public GameObject buttonPrefab;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Gadget gadget))
        {
            gadgetToSell.Add(gadget);
            UdpatePrice(gadget.Price);
            other.gameObject.SetActive(false);
        }
    }

    void UdpatePrice(int price)
    {
        total += price;
        priceText.text = total.ToString();
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
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(spawnPlace.position, 02f);
    }
}
