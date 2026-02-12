using System;
using System.Collections.Generic;
using Manager;
using UnityEngine;

namespace Enemies {
    public class SpawnFrog : MonoBehaviour{
        
        [Header("Spawn Settings")]
        [SerializeField] private Transform[] spawnPoint;
        [SerializeField] private EnemyController FrogPrefab;
        
        private bool areSpawned = false;

        private HashSet<EnemyController> currentlySpawned = new();

        private void Start() {
            GameManager.instance.enemyManager.RegisterSpawner(this);
        }

        void OnTriggerEnter(Collider other) {
            if(areSpawned) return;
            
            if (other.CompareTag("Player")) {
                Spawn();
            }
        }

        void Spawn() {
            areSpawned = true;

            foreach (var spawnPoint in spawnPoint) {
                var frog = Instantiate(FrogPrefab, spawnPoint.position, spawnPoint.rotation);
                currentlySpawned.Add(frog);
            }
        }

        public void RemoveFrogFromSpawned(EnemyController ctrl) {
            if(!areSpawned) return;
            
            if(currentlySpawned.Contains(ctrl))
                currentlySpawned.Remove(ctrl);
            
            if(currentlySpawned.Count == 0)
                areSpawned = false;
        }
    }
}