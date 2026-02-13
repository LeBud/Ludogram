using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager {
    public class MoneyManager : MonoBehaviour
    {
        [SerializeField] int quotas;
        public           int moneySaved;
        public           int moneyInCar;
        int                  quotasDifference;
        int                  moneyMissed;
        
        HashSet<MoneyBag>    detectableBags = new();
        HashSet<MoneyBag>    bagsInCar      = new();
        HashSet<MoneyBag>    allBags      = new();
        
        [Header("UI Elements")]
        [SerializeField] TMP_Text moneySavedText;
        [SerializeField] TMP_Text quotasDifText;
        [SerializeField] TMP_Text moneyMissedText;
        [SerializeField] TMP_Text moneyInCarText;

        void Start()
        {
            moneySaved          = 0;
            moneySavedText.text = moneySaved + "$";

            ActualizeQuotasDifference();
            ActualizeMoneyOnMap();
        }

        public void RegisterAllBags(MoneyBag bag) {
            allBags.Add(bag);
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
            ActualizeMoneyInCar(GetMoneyOnCar());
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
        
        private void ActualizeMoneyInCar(int moneyValue)
        {
            moneyInCar          += moneyValue;
            moneyInCarText.text =  moneyInCar + "$";
        }
        
        private void ActualizeQuotasDifference() {
            quotasDifText.text = moneySaved + "$/ " + quotas + "$";
        }
        
        private void ActualizeMoneyOnMap() {
            moneyMissed = GetMoneyOnMap();
            moneyMissedText.text = moneyMissed + "$";
        }
        
        
        private int GetMoneyOnMap()
        {
            int money = 0;
            foreach (MoneyBag moneyBag in allBags)
            {
                money += moneyBag.moneyValue;
            }
            return money;
        }
        
        private int GetMoneyOnCar()
        {
            int money = 0;
            foreach (MoneyBag moneyBag in bagsInCar)
            {
                money += moneyBag.moneyValue;
            }
            return money;
        }
        
    }
}