using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager {
    public class MoneyManager : MonoBehaviour
    {

        public int               quotas;
        public int               moneySaved;
        public int               moneyMissed;
        public HashSet<MoneyBag> detectableBags = new();
        public HashSet<MoneyBag> bagsInCar      = new();
        
        
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
        
        public HashSet<MoneyBag> GetAllBags() {
            return detectableBags;
        }
        
        public HashSet<MoneyBag> GetBagsInCar() {
            return bagsInCar;
        }

        public int GetTotalOnMap()
        {
            int money = 0;
            foreach (MoneyBag moneyBag in detectableBags)
            {
                money += moneyBag.moneyValue;
            }
            return money;
        }

        public int GetQuotasDifference()
        {
            return quotas - moneySaved;
        }
    }
}