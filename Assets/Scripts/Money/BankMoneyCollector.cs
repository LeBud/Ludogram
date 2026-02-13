using Manager;
using UnityEngine;

public class BankMoneyCollector : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out MoneyBag moneyBag))
        {
            GameManager.instance.moneyManager.ActualizeMoney(moneyBag.moneyValue);
            GameManager.instance.moneyManager.DeregisterMoneyBag(moneyBag);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
