using System;
using System.Collections.Generic;
using Manager;
using UnityEngine;

namespace Enemies {
    public class SpawnFrog : MonoBehaviour{
        
        [Header("Spawn Settings")]
        [SerializeField] private Transform[] spawnPoint;
        [SerializeField] private GameObject FrogPrefab;
        [SerializeField] private float waitForRespawn = 30f;
        
        private bool areSpawned = false;
        private float respawnTimer = 0f;

        private HashSet<EnemyController> currentlyFrogSpawned = new();
        private HashSet<AiCarDriver> currentlySpawnedCar = new();

        private void Start() {
            GameManager.instance.enemyManager.RegisterSpawner(this);
        }

        void OnTriggerEnter(Collider other) {
            if(areSpawned || respawnTimer > 0) return;
            
            if (other.CompareTag("Player") || other.CompareTag("Car")) {
                Spawn();
            }
        }

        private void Update() {
            respawnTimer -= Time.deltaTime;
        }

        void Spawn() {
            areSpawned = true;
            respawnTimer = waitForRespawn;
            
            foreach (var spawnPoint in spawnPoint) {
                var frog = Instantiate(FrogPrefab, spawnPoint.position, spawnPoint.rotation);
                if(frog.TryGetComponent<EnemyController>(out var ctrl))
                    currentlyFrogSpawned.Add(ctrl);
                if(frog.TryGetComponent<AiCarDriver>(out var car))
                    currentlySpawnedCar.Add(car);
            }
        }

        public void RemoveFrogFromSpawned(EnemyController ctrl) {
            if(!areSpawned) return;
            
            if(currentlyFrogSpawned.Contains(ctrl))
                currentlyFrogSpawned.Remove(ctrl);
            
            if(currentlyFrogSpawned.Count == 0)
                areSpawned = false;
        }
        
        public void RemoveCarFromSpawned(AiCarDriver ctrl) {
            if(!areSpawned) return;
            
            if(currentlySpawnedCar.Contains(ctrl))
                currentlySpawnedCar.Remove(ctrl);
            
            if(currentlySpawnedCar.Count == 0)
                areSpawned = false;
        }
    }
}