using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace EnemyStates {
    public class EnemyKnockOutState : EnemyBaseState {
        private float timer = 0f;
        public EnemyKnockOutState(EnemyController ia) : base(ia) {
        }

        public override void OnEnter() {
            ia.DisableNavMesh();
            ia.UnKnockOut();
            ia.isKnockOut = true;

            if (!ia.InCar) {
                ia.rigidbody.isKinematic = false;
                ia.rigidbody.constraints = RigidbodyConstraints.None;
                ia.rigidbody.AddForce(ia.knockOutForce, ForceMode.VelocityChange);
            }
            
            ia.money.DropBag();
            ia.money.ResetClosestExit();
        }

        public override void Update() {
            timer += Time.deltaTime;
        }

        public override void OnExit() {
            ResetState();

            if (!ia.InCar) {
                ia.rigidbody.isKinematic = true;
                ia.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            }
            ia.transform.rotation = Quaternion.Euler(0,ia.transform.eulerAngles.y,0);
            
            ia.EnableNavMesh();
            ia.isKnockOut = false;
        }

        private void ResetState() {
            timer = 0f;
        }
        
        public bool IsTimerFinished() => timer >= ia.knockOutTime;
    }
}