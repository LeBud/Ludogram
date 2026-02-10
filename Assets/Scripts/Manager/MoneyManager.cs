using System.Collections.Generic;
using UnityEngine;

namespace Manager {
    public class MoneyManager : MonoBehaviour {
        HashSet<MoneyBag> moneyBags = new();

        public void RegisterMoneyBag(MoneyBag bag) {
            moneyBags.Add(bag);
        }

        public void DeregisterMoneyBag(MoneyBag bag) {
            if(moneyBags.Contains(bag))
                moneyBags.Remove(bag);
        }
    }
}