using System;
using System.Collections.Generic;
using System.Linq;
using Manager;
using UnityEngine;

namespace Enemies {
    public class EnemyMoneyScan : MonoBehaviour {
        
        [Header("Ai Settings")]
        [SerializeField] private float maxDetectableRange = 5f;
        [SerializeField] private float timeBetweenEachScan = 0.5f;
        [SerializeField] private float timeToScanForNewClosestBag = 1f;
        [SerializeField] private float timeWithoutTargetToDespawn = 5f;
        
        public MoneyBag targetedBag { get; private set; }
        public MoneyBag pickupBag { get; private set; }
        public bool HasTargetBag => targetedBag != null;
        public bool HasBag => pickupBag != null;
        
        private float timeSinceLastScan = 0f;
        private float timeSinceNewClosestScan = 0f;

        private float timeToDespawn = 0f;
        
        private void Update() {
            GetBag();

            if (!HasTargetBag) timeToDespawn += Time.deltaTime;
            else timeToDespawn = 0f;

            if (timeToDespawn >= timeWithoutTargetToDespawn) {
                //Despawn
                Debug.Log($"Despawn AI");
            }
        }

        public void DropBag() {
            //Drop bag if get knockOut
        }

        private void GetBag() {
            if(!HasTargetBag && !HasBag && timeSinceLastScan < 0f)
                targetedBag = ScanBags();
            
            if(HasTargetBag && !HasBag && timeSinceNewClosestScan < 0f)
                targetedBag = ScanBags();
            
            timeSinceLastScan -= Time.deltaTime;
            timeSinceNewClosestScan -= Time.deltaTime;
        }
        
        private MoneyBag ScanBags() {
            timeSinceLastScan = timeBetweenEachScan;
            
            var bags = GameManager.instance.moneyManager.GetAllBags();
            List<MoneyBag> outsideBags = new ();

            foreach (var bag in bags) {
                if (GameManager.instance.moneyManager.GetBagsInCar().Contains(bag))
                    continue;
                
                outsideBags.Add(bag);
            }

            if (outsideBags.Count > 0) return GetClosest(outsideBags);

            return GetClosest(bags.ToList());
        }
        
        private MoneyBag GetClosest(List<MoneyBag> bags) {
            var index = 0;
            var dist = maxDetectableRange;
            
            for (int i = 0; i < bags.Count; i++) {
                var distance = Vector3.Distance(bags[i].transform.position, transform.position);

                if (distance < dist) {
                    dist = distance;
                    index = i;
                }
            }

            if (dist == maxDetectableRange) return null;
            
            return bags[index];
        }
    }
    
}