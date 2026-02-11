using System;
using CarScripts;
using Player;
using UnityEngine;

namespace Enemies {
    public class EnemyAbilityController : MonoBehaviour {
        private EnemyController ia;
        
        [Header("Ability Settings")] 
        [SerializeField] private float abilityCooldown = 4f;
        [SerializeField] private float tongueAbilityRange = 15f;
        [SerializeField] private float minRangeToAbility = 2f;
        [SerializeField] private float maxRangeToAbility = 14f;
        [SerializeField] public float abilityStateDuration = 1f;
        
        private float currentCooldown = 0f;
        
        [HideInInspector]
        public bool triggerAbility = false;
        [HideInInspector]
        public bool canUseTongue = true;
        
        //Faire l'action pour tirer la langue sur le sac Target dans MoneyScan
        //Distinguer si le sac est dans le camion

        public void Initialize(EnemyController controller) {
            ia = controller;
        }

        private void Update() {
            currentCooldown -= Time.deltaTime;
            
            if(CanUseAbility())
                UseAbility();
        }

        private void UseAbility() {
            triggerAbility = true;
            currentCooldown = abilityCooldown;
            var dir = ia.money.targetedBag.transform.position - transform.position;

            if (ia.money.BagInCar && !CarDoors.instance.areDoorsOpen) {
                Debug.Log("Target Doors");
                dir = CarDoors.instance.transform.position - transform.position;
            }
            else if(ia.money.targetedBag.isPickedUp)
                dir = ia.money.targetedBag.gadgetController.transform.position - transform.position;
            
            Physics.Raycast(transform.position, dir.normalized, out var hit, tongueAbilityRange);
            
            if(hit.collider == null) return;

            if (hit.transform.TryGetComponent(out SingleDoor door)) {
                Debug.Log("Hit Door");
                door.UseDoor();
                return;
            }
            
            if (hit.transform.TryGetComponent(out Controller c) && ia.money.targetedBag.isPickedUp) {
                c.pickUp.gadgetController.DropGadget();
                ia.money.GrabBagByAbility(ia.money.targetedBag);
                return;
            }

            if (hit.transform.TryGetComponent(out MoneyBag bag)) {
                ia.money.GrabBagByAbility(bag);
                return;
            }
        }

        private void OnDrawGizmos() {
            if(!Application.isPlaying) return;
            
            if (ia.money.HasTargetBag) {
                Gizmos.color = Color.red;
                var dir = ia.money.targetedBag.transform.position - transform.position;
                Gizmos.DrawLine(transform.position, dir.normalized * tongueAbilityRange);
            }
        }

        private bool CanUseAbility() {
            var getDist = 0f;
            if(ia.money.HasTargetBag) getDist = Vector3.Distance(transform.position, ia.money.targetedBag.transform.position);
            else return false;
            
            return currentCooldown <= 0f && getDist < maxRangeToAbility && getDist > minRangeToAbility && canUseTongue;
        }
    }
}