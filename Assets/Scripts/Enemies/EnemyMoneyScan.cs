using System;
using System.Collections.Generic;
using System.Linq;
using Manager;
using UnityEngine;

namespace Enemies {
    public class EnemyMoneyScan : MonoBehaviour {
        private EnemyController ia;
        
        [Header("Money Settings")]
        [SerializeField] private float maxDetectableRange = 5f;
        [SerializeField] private float timeBetweenEachScan = 0.5f;
        [SerializeField] private float timeToScanForNewClosestBag = 1f;
        [SerializeField] private float timeWithoutTargetToDespawn = 5f;
        [SerializeField] private float rangeToGrabBag = 1.5f;
        [SerializeField] private Transform bagPos;
        
        [Header("Money Settings")] 
        [SerializeField] private float timeToExitLevel = 2f;
        [SerializeField] private float rangeToExit = 1.5f;
        
        //====Money====
        public MoneyBag targetedBag { get; private set; }
        private MoneyBag pickupBag { get; set; }
        public bool HasTargetBag => targetedBag != null;
        public bool HasBag => pickupBag != null;
        public bool BagInCar { get; private set; }
        
        private float timeSinceLastScan = 0f;
        private float timeSinceNewClosestScan = 0f;

        private float timeToDespawn = 0f;

        private MoneyBag previousTarget;
        
        //====Manhole====
        public Transform closestManhole { get; private set; }
        private float timeToExit = 0f;
        
        public void Initialize(EnemyController controller) {
            ia = controller;
        }
        
        private void Update() {
            if(ia.isKnockOut) return;
            GetBag();

            if (HasBag && closestManhole == null)
                closestManhole = GetClosestExit();

            if (CanExit()) {
                timeToExit += Time.deltaTime;
                if(timeToExit >= timeToExitLevel) FrogExit();
            }
            else
                timeToExit = 0f;

            if (!HasTargetBag) timeToDespawn += Time.deltaTime;
            else timeToDespawn = 0f;

            if (timeToDespawn >= timeWithoutTargetToDespawn) {
                //Despawn
            }
        }

        public void DropBag() {
            //Drop bag if get knockOut
            if(!HasBag) return;
            
            GameManager.instance.enemyManager.DeregisterTarget(pickupBag);
            pickupBag.transform.parent = null;
            pickupBag.rb.isKinematic = false;
            pickupBag.EnableCollider();
            pickupBag = null;
        }

        private void GetBag() {
            if(HasBag) return;

            if (HasTargetBag) {
                BagInCar = GameManager.instance.moneyManager.GetBagsInCar().Contains(targetedBag);
                GrabBag();
            }

            if (!HasTargetBag && timeSinceLastScan < 0f) {
                previousTarget = targetedBag;
                targetedBag = ScanBags();
                if(targetedBag != previousTarget){
                    GameManager.instance.enemyManager.DeregisterTarget(previousTarget);
                    GameManager.instance.enemyManager.RegisterTarget(targetedBag);
                }
            }

            if (HasTargetBag && timeSinceNewClosestScan < 0f) {
                previousTarget = targetedBag;
                targetedBag = ScanBags();
                if(targetedBag != previousTarget){
                    GameManager.instance.enemyManager.DeregisterTarget(previousTarget);
                    GameManager.instance.enemyManager.RegisterTarget(targetedBag);
                }
            }
            
            timeSinceLastScan -= Time.deltaTime;
            timeSinceNewClosestScan -= Time.deltaTime;
        }

        private void FrogExit() {
            GameManager.instance.moneyManager.DeregisterMoneyBag(pickupBag);
            GameManager.instance.enemyManager.DeregisterTarget(pickupBag);
            
            Destroy(gameObject);
        }
        
        private void GrabBag() {
            if (Vector3.Distance(targetedBag.transform.position, transform.position) > rangeToGrabBag) return;
            
            Debug.Log("Grab from ground");
            if(targetedBag.isPickedUp) targetedBag.gadgetController.DropGadget();
            
            pickupBag = targetedBag;
            SetPickUpBag();
        }

        public void SetPickUpBag() {
            pickupBag.rb.isKinematic = true;
            pickupBag.transform.parent = transform;
            pickupBag.transform.position = bagPos.position;
            pickupBag.DisableCollider();
            targetedBag = null;
        }
        
        public void GrabBagByAbility(MoneyBag bag) {
            pickupBag = bag;
            //SetPickUpBag();
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

        private Transform GetClosestExit() {
            Transform tempClosest = null;
            var dist = float.MaxValue;
            
            foreach (var hole in GameManager.instance.enemyManager.GetManholeCover()) {
                var distance = Vector3.Distance(hole.transform.position, transform.position);

                if (distance < dist) {
                    dist = distance;
                    tempClosest = hole.transform;
                }
            }

            return tempClosest;
        }

        private bool CanExit() {
            if(closestManhole == null) return false;
            
            return Vector3.Distance(transform.position, closestManhole.position) <= rangeToExit;
        }

        public void ResetClosestExit() {
            closestManhole = null;
        }
        
        private MoneyBag GetClosest(List<MoneyBag> bags) {
            var index = 0;
            var dist = maxDetectableRange;
            
            for (int i = 0; i < bags.Count; i++) {
                if(GameManager.instance.enemyManager.targetedBag.Contains(bags[i]) && bags[i] != targetedBag)
                    continue;
                
                var distance = Vector3.Distance(bags[i].transform.position, transform.position);

                if (distance < dist) {
                    dist = distance;
                    index = i;
                }
            }

            if (dist >= maxDetectableRange) return null;
            
            return bags[index];
        }
    }
    
}