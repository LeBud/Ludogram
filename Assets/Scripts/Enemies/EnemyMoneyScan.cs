using System;
using System.Collections.Generic;
using System.Linq;
using Manager;
using UnityEngine;

namespace Enemies {
    public class EnemyMoneyScan : MonoBehaviour {
        private EnemyController ia;
        
        [Header("Ai Settings")]
        [SerializeField] private float maxDetectableRange = 5f;
        [SerializeField] private float timeBetweenEachScan = 0.5f;
        [SerializeField] private float timeToScanForNewClosestBag = 1f;
        [SerializeField] private float timeWithoutTargetToDespawn = 5f;
        [SerializeField] private float rangeToGrabBag = 1.5f;
        [SerializeField] private Transform bagPos;
        
        public MoneyBag targetedBag { get; private set; }
        public MoneyBag pickupBag { get; private set; }
        public bool HasTargetBag => targetedBag != null;
        public bool HasBag => pickupBag != null;
        
        private float timeSinceLastScan = 0f;
        private float timeSinceNewClosestScan = 0f;

        private float timeToDespawn = 0f;

        public void Initialize(EnemyController controller) {
            ia = controller;
        }
        
        private void Update() {
            if(ia.isKnockOut) return;
            GetBag();

            if (!HasTargetBag) timeToDespawn += Time.deltaTime;
            else timeToDespawn = 0f;

            if (timeToDespawn >= timeWithoutTargetToDespawn) {
                //Despawn
            }
        }

        public void DropBag() {
            //Drop bag if get knockOut
            if(!HasBag) return;
            
            pickupBag.transform.parent = null;
            pickupBag.rb.isKinematic = false;
            pickupBag = null;
        }

        private void GetBag() {
            if(HasBag) return;
            
            if (HasTargetBag) GrabBag();
            
            if(!HasTargetBag && timeSinceLastScan < 0f) targetedBag = ScanBags();
            if(HasTargetBag && timeSinceNewClosestScan < 0f) targetedBag = ScanBags();
            
            timeSinceLastScan -= Time.deltaTime;
            timeSinceNewClosestScan -= Time.deltaTime;
        }

        private void GrabBag() {
            if (Vector3.Distance(targetedBag.transform.position, transform.position) > rangeToGrabBag) return;
            
            pickupBag = targetedBag;
            pickupBag.rb.isKinematic = true;
            pickupBag.transform.parent = transform;
            pickupBag.transform.position = bagPos.position;
            targetedBag = null;
        }
        
        private MoneyBag ScanBags() {
            timeSinceLastScan = timeBetweenEachScan;
            timeSinceNewClosestScan = timeToScanForNewClosestBag;
            
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