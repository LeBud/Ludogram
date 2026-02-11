using System;
using System.Collections.Generic;
using Player;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GadgetSeller : MonoBehaviour
{
    public           int          currentMoney;
    public           List<Gadget> gadgetToSell = new();
    [SerializeField] int          total        = 0;
    [SerializeField] TMP_Text     priceText;
    [SerializeField] Transform    spawnPlace;
    public           GameObject   buttonPrefab;
    private          Collider     collider;

    void Start()
    {
        collider = GetComponent<Collider>();
        total    = 0;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Gadget gadget))
        {
            gadgetToSell.Add(gadget);
            UdpatePrice(gadget.Price);
            other.gameObject.SetActive(false);
            other.attachedRigidbody.linearVelocity = Vector3.zero;
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
            total = 0;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(spawnPlace.position, 02f);
    }

    
}
