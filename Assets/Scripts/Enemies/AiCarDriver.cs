using System;
using CarScripts;
using Manager;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies {
    public class AiCarDriver : MonoBehaviour {
        private CarController carController;
        private Transform carTarget;

        [Header("Settings")]
        [SerializeField] private float distanceToReachTarget;
        [SerializeField] private float repathDistance = 10f;
        [SerializeField] private float cornerReachDistance = 2f;
        [SerializeField] private float cornerClearance = 4f;
        [SerializeField] private float distanceToDespawn = 100f;

        [Header("Frog In Car")] 
        [SerializeField] private EnemyController[] aIs;
        
        private NavMeshPath currentPath;
        private Vector3[] corners;
        private int currentCornerIndex;
        private Vector3 lastTargetPos;

        private Transform currentTarget;
        
        private void Awake() {
            if(TryGetComponent(out carController)) Debug.Log("AiCarDriver Awake");
            else Debug.LogError("No car controller found");
            
            carController.SetAiCar(true);
            carTarget = FindAnyObjectByType<AttachedPlayer>().transform;
        }

        private void Start() {
            RecalculatePath(carTarget.position);
            SetCurrentTarget(carTarget);
        }

        private void Update() {
            if ((carTarget.position - lastTargetPos).magnitude > distanceToDespawn) {
                GameManager.instance.enemyManager.DeregisterCarFromSpawner(this);
                Destroy(gameObject);
            }

            if(AllFrogHaveBag()) return;
            
            if (NoneHaveTarget() && currentTarget != carTarget) SetCurrentTarget(carTarget);
            else if(!NoneHaveTarget() && currentTarget == carTarget) SetCurrentTarget(GetClosestTargetBag());
            
            var forwardAmount = 0f;
            var turnAmount = 0f;
            
            if((carTarget.position - lastTargetPos).sqrMagnitude > repathDistance * repathDistance)
                RecalculatePath(carTarget.position);
            
            if(corners == null || corners.Length == 0) return;
            
            var targetCorner = corners[currentCornerIndex];
            
            if ((targetCorner - transform.position).magnitude < cornerReachDistance) {
                currentCornerIndex++;

                if (currentCornerIndex >= corners.Length) return;
            }
            
            var distance = Vector3.Distance(transform.position, carTarget.position);

            if (distance > distanceToReachTarget) {
                var dirToMovePos = (targetCorner - transform.position).normalized;
                var dot = Vector3.Dot(transform.forward, dirToMovePos);
                if (dot > 0f) forwardAmount = 1f;
                else forwardAmount = -1f;
                
                var angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePos, Vector3.up);
                if(angleToDir > 15f) turnAmount = 1f;
                else if(angleToDir < -15f) turnAmount = -1f;
                else  turnAmount = 0f;
            }
            
            carController.SetAiInputs(forwardAmount, turnAmount);
        }

        private bool AllFrogHaveBag() {
            foreach (var frog in aIs) {
                if(frog.money.HasBag) continue;
                return false;
            }
            
            return true;
        }

        private bool NoneHaveTarget() {
            foreach (var frog in aIs) {
                if(!frog.money.HasTargetBag) continue;
                return false;
            }
            
            return true;
        }

        private Transform GetClosestTargetBag() {
            var dist = float.MaxValue;
            var index = 0;
            for (var i = 0; i < aIs.Length; i++) {
                var distance = Vector3.Distance(transform.position, aIs[i].money.targetedBag.transform.position);
                if (distance < dist) {
                    dist = distance;
                    index = i;
                }
            }
            
            return aIs[index].money.targetedBag.transform;
        }
        
        public void SetCurrentTarget(Transform target) {
            currentTarget = target;
        }
        
        private void RecalculatePath(Vector3 targetPosition) {
            Debug.Log("Recalculating path");
            
            if (!NavMesh.SamplePosition(targetPosition, out var hit, 2f, NavMesh.AllAreas))
                return;

            currentPath = new NavMeshPath();

            if (NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, currentPath)
                && currentPath.status == NavMeshPathStatus.PathComplete) {
                var rawCorners = currentPath.corners;
                corners = new Vector3[rawCorners.Length];

                for (int i = 0; i < rawCorners.Length; i++) {
                    if (i == 0) {
                        corners[i] = rawCorners[i];
                    }
                    else {
                        corners[i] = OffsetCornerFromWall(rawCorners[i], cornerClearance);
                    }
                }

                currentCornerIndex = 0;
                lastTargetPos = targetPosition;
            }
        }
        
        private Vector3 OffsetCornerFromWall(Vector3 corner, float clearance) {
            if (NavMesh.FindClosestEdge(corner, out var edgeHit, NavMesh.AllAreas)) {
                var offset = edgeHit.normal * clearance;

                if (NavMesh.SamplePosition(corner + offset, out var sampleHit, clearance, NavMesh.AllAreas)) {
                    return sampleHit.position;
                }
            }

            return corner;
        }
        
        private void OnDrawGizmos() {
            if (corners == null) return;

            Gizmos.color = Color.cyan;
            for (var i = 0; i < corners.Length - 1; i++) {
                Gizmos.DrawWireSphere(corners[i], 1f);
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }
    }
}