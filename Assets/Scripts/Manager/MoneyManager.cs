using System.Collections.Generic;
using UnityEngine;

namespace Manager {
    public class MoneyManager : MonoBehaviour {
        HashSet<MoneyBag> detectableBags = new();
        HashSet<MoneyBag> bagsInCar = new();
        
        public void RegisterMoneyBag(MoneyBag bag) {
            detectableBags.Add(bag);
        }

        public void DeregisterMoneyBag(MoneyBag bag) {
            if(detectableBags.Contains(bag))
                detectableBags.Remove(bag);
        }
        
        public void RegisterBagInCar(MoneyBag bag) {
            bagsInCar.Add(bag);
        }

        public void DeregisterBagInCar(MoneyBag bag) {
            if(bagsInCar.Contains(bag))
                bagsInCar.Remove(bag);
        }
    }
}