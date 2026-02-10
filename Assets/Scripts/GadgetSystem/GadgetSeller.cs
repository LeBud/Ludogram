using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GadgetSeller : MonoBehaviour
{
    private          List<Gadget> gadgetToSell = new();
    [SerializeField] int          total = 0;
    [SerializeField] TMP_Text     priceText;
    
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
       Debug.Log("You paid : " + total + "$"); 
    }
    
}
