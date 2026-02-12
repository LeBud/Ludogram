using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager {
    public class MoneyManager : MonoBehaviour
    {
        [SerializeField]  int               quotas;
        public int               moneySaved;
        int               quotasDifference;
        int               moneyMissed;
        HashSet<MoneyBag> detectableBags = new();
        HashSet<MoneyBag> bagsInCar      = new();
        
        [Header("UI Elements")]
        [SerializeField] TMP_Text moneySavedText;
        [SerializeField] TMP_Text quotasDifText;
        [SerializeField] TMP_Text moneyMissedText;

        void Start()
        {
            moneySaved          = 0;
            moneySavedText.text = moneySaved + "$";

            ActualizeQuotasDifference();
            ActualizeMoneyOnMap();
        }
        
        public void RegisterMoneyBag(MoneyBag bag) {
            detectableBags.Add(bag);
        }

        public void DeregisterMoneyBag(MoneyBag bag) {
            if (detectableBags.Contains(bag))
            {
                detectableBags.Remove(bag);
                ActualizeMoneyOnMap();
            }
            
        }
        
        public void RegisterBagInCar(MoneyBag bag) {
            bagsInCar.Add(bag);
            DeregisterMoneyBag(bag);
            ActualizeMoneyOnMap();
        }

        public void DeregisterBagInCar(MoneyBag bag) {
            if (bagsInCar.Contains(bag))
            {
                bagsInCar.Remove(bag);
                RegisterMoneyBag(bag);
                ActualizeMoneyOnMap();
            }
        }
        
        public HashSet<MoneyBag> GetAllBags() {
            return detectableBags;
        }
        
        public HashSet<MoneyBag> GetBagsInCar() {
            return bagsInCar;
        }

        public void ActualizeMoney(int moneyValue)
        {
            moneySaved          += moneyValue;
            moneySavedText.text =  moneySaved + "$";
            ActualizeQuotasDifference();
        }
        
        public void ActualizeQuotasDifference() {
            quotasDifText.text = moneySaved + "$/ " + quotas + "$";
        }
        
        public void ActualizeMoneyOnMap() {
            moneyMissed = GetMoneyOnMap();
            moneyMissedText.text = moneyMissed + "$";
        }
        
        
        public int GetMoneyOnMap()
        {
            int money = 0;
            foreach (MoneyBag moneyBag in detectableBags)
            {
                money += moneyBag.moneyValue;
            }
            return money;
        }
        
    }
}