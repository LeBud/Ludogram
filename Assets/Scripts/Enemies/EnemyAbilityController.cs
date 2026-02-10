using System;
using UnityEngine;

namespace Enemies {
    public class EnemyAbilityController : MonoBehaviour {
        private EnemyController ia;
        
        [Header("Ability Settings")] 
        [SerializeField] private float abilityCooldown;
        [SerializeField] private float tongueAbilityRange = 15f;
        [SerializeField] private float minRangeToAbility = 2f;
        [SerializeField] private float maxRangeToAbility = 20f;
        
        private float currentCooldown = 0f;
        
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
            currentCooldown = abilityCooldown;
            
            var dir = transform.position - ia.money.targetedBag.transform.position;
            Physics.Raycast(transform.position, dir.normalized, out var hit, tongueAbilityRange);
            
            if(hit.collider == null) return;
            
            if(hit.transform.TryGetComponent(out MoneyBag bag))
                ia.money.GrabBagByAbility(bag);
        }

        private bool CanUseAbility() {
            var getDist = 0f;
            if(ia.money.HasTargetBag) getDist = Vector3.Distance(transform.position, ia.money.targetedBag.transform.position);
            else return false;
            
            return currentCooldown <= 0f && getDist < maxRangeToAbility && getDist > minRangeToAbility;
        }
    }
}