using UnityEngine;

namespace Manager {
    public class RegisterBagTrigger : MonoBehaviour {
        public void OnTriggerEnter(Collider other) {
            if (other.transform.TryGetComponent(out MoneyBag bag)) {
                GameManager.instance.moneyManager.RegisterMoneyBag(bag);
            }
        }
    }
}