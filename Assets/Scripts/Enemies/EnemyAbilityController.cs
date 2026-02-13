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
        [SerializeField] public Transform raycastPoint;

        public float currentCooldown { get; private set; }
        
        //[HideInInspector]
        public bool triggerAbility = false;
        [HideInInspector]
        public bool canUseTongue = true;
        private RaycastHit raycastHit;
        
        public Vector3 lastHitPos { get; private set; }
        
        public void Initialize(EnemyController controller) {
            ia = controller;
        }

        private void Update() {
            currentCooldown -= Time.deltaTime;

            if (!ia.money.HasTargetBag) return;
            
            var dir = ia.money.targetedBag.transform.position - raycastPoint.position;

            if (ia.money.BagInCar && !CarDoors.instance.areDoorsOpen)
                dir = CarDoors.instance.transform.position - raycastPoint.position;
            else if (ia.money.targetedBag.isPickedUp) {
                var pos = ia.money.targetedBag.gadgetController.transform.position + Vector3.up * 0.5f;
                dir = pos - raycastPoint.position;
            }
                
            var distance = Mathf.Min(dir.magnitude, tongueAbilityRange);
            var mask = LayerMask.GetMask("Ghost", "Enemy");
            Physics.Raycast(raycastPoint.position, dir.normalized, out raycastHit, distance, ~mask);
        }

        private void FixedUpdate() {
            if(CanUseAbility())
                UseAbility();
        }

        private void UseAbility() {
            triggerAbility = true;
            currentCooldown = abilityCooldown;
            
            lastHitPos = raycastHit.point;
            
            if (raycastHit.collider.TryGetComponent(out SingleDoor door)) {
                Debug.Log("Hit Door");
                CarDoors.instance.ForceOpenDoor();
                return;
            }
            
            if (raycastHit.collider.TryGetComponent(out Controller c) && ia.money.targetedBag.isPickedUp) {
                Debug.Log("Grab from player");
                ia.money.targetedBag.gadgetController.DropGadget();
                ia.money.GrabBagByAbility(ia.money.targetedBag);
                return;
            }

            if (raycastHit.collider.TryGetComponent(out MoneyBag bag) && !ia.money.targetedBag.isPickedUp) {
                Debug.Log("Grab with tongue");
                ia.money.GrabBagByAbility(bag);
                return;
            }
        }

        private void OnDrawGizmos() {
            if(!Application.isPlaying) return;

            if (!ia.money.HasTargetBag) return;
            
            Gizmos.color = Color.red;
            var dir = ia.money.targetedBag.transform.position - raycastPoint.position;
            var distance = Mathf.Min(dir.magnitude, tongueAbilityRange);
            Gizmos.DrawLine(raycastPoint.position, raycastPoint.position + dir.normalized * distance);
                
            Gizmos.color = Color.green;
            dir = CarDoors.instance.transform.position - raycastPoint.position;
            distance = Mathf.Min(dir.magnitude, tongueAbilityRange);
            Gizmos.DrawLine(raycastPoint.position, raycastPoint.position + dir.normalized * distance);
                
            Gizmos.color = Color.cornflowerBlue;
            var pos = ia.money.targetedBag.gadgetController.transform.position + Vector3.up * 0.5f;
            dir = pos - raycastPoint.position;
            distance = Mathf.Min(dir.magnitude, tongueAbilityRange);
            Gizmos.DrawLine(raycastPoint.position, raycastPoint.position + dir.normalized * distance);
        }

        private bool CanUseAbility() {
            var getDist = 0f;
            if(ia.money.HasTargetBag) getDist = Vector3.Distance(transform.position, ia.money.targetedBag.transform.position);
            else return false;
            
            return currentCooldown <= 0f && getDist < maxRangeToAbility && getDist > minRangeToAbility && canUseTongue && raycastHit.collider;
        }
    }
}