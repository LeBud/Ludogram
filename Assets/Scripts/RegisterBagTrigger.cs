using UnityEngine;

namespace Manager {
    public class RegisterBagTrigger : MonoBehaviour {
        public void OnTriggerEnter(Collider other) {
            if (other.TryGetComponent(out MoneyBag bag)) {
                GameManager.instance.moneyManager.DeregisterMoneyBag(bag);
            }
            
            if (other.TryGetComponent(out GadgetController ctrl) && ctrl.selectedGadget as MoneyBag) {
                GameManager.instance.moneyManager.DeregisterMoneyBag(ctrl.selectedGadget as MoneyBag);
            }
        }
        
        public void OnTriggerExit(Collider other) {
            if (other.TryGetComponent(out MoneyBag bag)) {
                Debug.Log("Register money bag");
                GameManager.instance.moneyManager.RegisterMoneyBag(bag);
            }
            
            if (other.TryGetComponent(out GadgetController ctrl) && ctrl.selectedGadget as MoneyBag) {
                GameManager.instance.moneyManager.RegisterMoneyBag(ctrl.selectedGadget as MoneyBag);
            }
        }
    }
}