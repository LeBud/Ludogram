using System;
using CarScripts;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies {
    public class AiCarDriver : MonoBehaviour {
        private CarController carController;
        private Transform target;

        [SerializeField] private float distanceToReachTarget;
        
        NavMeshPath currentPath;
        Vector3[] corners;
        int currentCornerIndex;

        Vector3 lastTargetPos;
        [SerializeField] float repathDistance = 10f;
        [SerializeField] float cornerReachDistance = 2f;
        [SerializeField] float cornerClearance = 4f;
        
        private void Awake() {
            if(TryGetComponent(out carController)) Debug.Log("AiCarDriver Awake");
            else Debug.LogError("No car controller found");
            
            carController.SetAiCar(true);
        }

        private void Start() {
            target = FindAnyObjectByType<AttachedPlayer>().transform;
            
            RecalculatePath(target.position);
        }

        private void Update() {
            float forwardAmount = 0f;
            float turnAmount = 0f;
            
            if((target.position - lastTargetPos).sqrMagnitude > repathDistance * repathDistance)
                RecalculatePath(target.position);
            
            if(corners == null || corners.Length == 0) return;
            
            var targetCorner = corners[currentCornerIndex];
            var distToCorner = targetCorner - transform.position;
            if (distToCorner.magnitude < cornerReachDistance) {
                currentCornerIndex++;

                if (currentCornerIndex >= corners.Length) return;
            }
            
            var distance = Vector3.Distance(transform.position, target.position);

            if (distance > distanceToReachTarget) {
                var dirToMovePos = (targetCorner - transform.position).normalized;
                var dot = Vector3.Dot(transform.forward, dirToMovePos);
                if (dot > 0f) forwardAmount = 1f;
                else forwardAmount = -1f;
                
                var angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePos, Vector3.up);
                if(angleToDir > 10f) turnAmount = 1f;
                else if(angleToDir < -10f) turnAmount = -1f;
                else  turnAmount = 0f;
            }
            
            carController.SetAiInputs(forwardAmount, turnAmount);
        }
        
        void RecalculatePath(Vector3 targetPosition) {
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
        
        Vector3 OffsetCornerFromWall(Vector3 corner, float clearance) {
            if (NavMesh.FindClosestEdge(corner, out var edgeHit, NavMesh.AllAreas)) {
                var offset = edgeHit.normal * clearance;

                if (NavMesh.SamplePosition(corner + offset, out var sampleHit, clearance, NavMesh.AllAreas)) {
                    return sampleHit.position;
                }
            }

            return corner;
        }
        
        void OnDrawGizmos() {
            if (corners == null) return;

            Gizmos.color = Color.cyan;
            for (var i = 0; i < corners.Length - 1; i++) {
                Gizmos.DrawWireSphere(corners[i], 1f);
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }
    }
}