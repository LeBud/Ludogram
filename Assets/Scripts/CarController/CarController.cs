using DefaultNamespace;
using TMPro;
using UnityEngine;

namespace CarController {
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour {
        private Rigidbody carRb;
        private InputsBrain Inputs;

        public enum WheelDriveMode {
            FWD, //Front
            RWD, //Rear
            AWD //All
        }

        [Header("HUD")]
        public TextMeshProUGUI speedTxt;
        // public TextMeshProUGUI suspensionTxt;
        
        [Header("Settings")] 
        [SerializeField] private Vector3 centerOfMass;
        
        [Header("Suspension Transform")]
        [SerializeField] private Transform FrontRSuspension;
        [SerializeField] private Transform FrontLSuspension;
        [SerializeField] private Transform RearRSuspension;
        [SerializeField] private Transform RearLSuspension;

        Transform[] allSuspensions = new Transform[4];
        
        [Header("Suspension Settings")] 
        [SerializeField] private float suspensionRestDistance;
        [SerializeField] private float suspensionStrength;
        [SerializeField] private float suspensionDamping;

        [Header("Steering Settings")] 
        [SerializeField] private float steeringMaxAngle;
        [SerializeField] private float steeringSmoothing;
        //public AnimationCurve steeringCurve; //Courbe de % d'angle de rotation des roues en fonction de la vitesse de la voiture
        [Range(0, 1)]
        [SerializeField] private float steeringGrip; //Doit être compris entre 0 et 1 -- 0 étant pas de grip - 1 étant maximum grip
        [SerializeField] private float tireMass; //Des choses à y faire
        [SerializeField] private bool rearSteeringAllowed = false;
        
        [Header("Drive Settings")]
        [SerializeField] private WheelDriveMode wheelDriveMode = WheelDriveMode.FWD;
        
        //public AnimationCurve torqueCurve = AnimationCurve.EaseInOut(0f, 100f, 7000f, 50f);
        
         public float[] gears = new float[6];
         public int currentGear = 0;
         // public float finalDriveRatio = 3.42f;
         // public float transmissionEfficiency = 0.7f;
         // public float wheelRadius = 0.25f;
         // public float idleRPM = 800f;
         // public float maxRPM = 7000f;
         // public float engineInertia = 0.1f;
         // public float downforceCoefficient = 3.0f;
        
        [Header("Tire Visual")]
        [SerializeField] private Transform FrontRTire;
        [SerializeField] private Transform FrontLTire;
        [SerializeField] private Transform RearRTire;
        [SerializeField] private Transform RearLTire;
        [SerializeField] private float tireMeshRadius = 0.25f;

        [Header("Friction Settings")] 
        [SerializeField] private float frictionCoefficient = 0.3f;
        [SerializeField] private float carFrontalSurface = 2.2f;
        [SerializeField] private float airDensity = 1.29f;

        [Header("Car Mass")] 
        [SerializeField] private float carMass = 10f;
        //[SerializeField] private float gravity = 9.81f;
        
        //Pour le moment ces valeurs ne sont pas utillisés
        float b = 1.25f; //Distance entre le centre de gravité et l'essieu avant
        float c = 1.25f; //Distance entre le centre de gravité et l'essieu arrière
        float l = 2.5f; //Empattement
        float h = 1f; //Hauteur du centre de gravité à partir du sol
        
        float acceleration; //Va être calculé par la suite
        
        //float  carWeight => carMass * gravity;
        // float frontWeight => c / l * carWeight - h/l * carMass * acceleration; //Appliqué ces forces à un endroit du coup
        // float rearWeight => b / l * carWeight + h/l * carMass * acceleration;

        /*float maxTorque => lookUpTorque.Evaluate(rpm);
        float engineTorque => maxTorque * throttle;
        
        float wheelRotationRate => rigidBody.linearVelocity.magnitude / wheelRadius; //Surement nécessiter des modifications pour régarder la direction de la voiture
        float rpm => wheelRotationRate * gearRatio * differentialRatio * 60 / 2 * 3.14f;*/
        
        //float wheelRPM => carRb.linearVelocity.magnitude / (2 * Mathf.PI * wheelRadius) * 60f;
        float engineRPM;
        public float engineForce;
        
        float rollingResistance => 30 * airDrag;
        
        float steering;
        float currentSteering;
        
        float throttle;
        float airDrag => 0.5f * frictionCoefficient * carFrontalSurface * airDensity;
        private float throttleTimer;
        Vector3 dragForce => -airDrag * carRb.linearVelocity * carRb.linearVelocity.magnitude;
        Vector3 rollingResistanceForce => rollingResistance * carRb.linearVelocity;
        //float driveForce => engineForce * gears[currentGear] * finalDriveRatio * transmissionEfficiency / wheelRadius;
        
        void Start() {
            if (TryGetComponent(out Inputs)) Debug.Log($"Inputs Assigned");
            else Debug.LogWarning($"Inputs Not Found");
            
            if (TryGetComponent(out carRb)) Debug.Log($"RigidBody Assigned");
            else Debug.LogWarning($"RigidBody Not Found");
            
            SetupCar();
        }

        void SetupCar() {
            carRb.mass = carMass;
            carRb.centerOfMass = centerOfMass;
            
            allSuspensions[0] = FrontRSuspension;
            allSuspensions[1] = FrontLSuspension;
            allSuspensions[2] = RearRSuspension;
            allSuspensions[3] = RearLSuspension;
        }

        void Update() {
            MyInputs();
            TurnWheels();
        }

        void TurnWheels() {
            currentSteering = Mathf.SmoothStep(currentSteering, steering * steeringMaxAngle, steeringSmoothing * Time.deltaTime);
            currentSteering = Mathf.Clamp(currentSteering, -steeringMaxAngle, steeringMaxAngle);
            
            FrontLSuspension.localRotation = Quaternion.Euler(0,currentSteering,0);
            FrontRSuspension.localRotation = Quaternion.Euler(0,currentSteering,0);
            
            //Visuel
            FrontRTire.localRotation = Quaternion.Euler(0,currentSteering,0);
            FrontLTire.localRotation = Quaternion.Euler(0,currentSteering,0);

            if (rearSteeringAllowed) {
                RearLSuspension.localRotation = Quaternion.Euler(0,-currentSteering,0);
                RearRSuspension.localRotation = Quaternion.Euler(0,-currentSteering,0);
                
                //Visuel
                RearRTire.localRotation = Quaternion.Euler(0,-currentSteering,0);
                RearLTire.localRotation = Quaternion.Euler(0,-currentSteering,0);
            }
        }

        void MyInputs() {
            steering = Inputs.Steering.ReadValue<float>();
            throttle = Inputs.Throttle.ReadValue<float>();
            
            if (Inputs.ShiftGear.WasPressedThisFrame()) {
                if (Inputs.ShiftGear.ReadValue<float>() > 0) currentGear++;
                else currentGear--;
                
                if(currentGear >= gears.Length) currentGear = gears.Length - 1;
                if(currentGear < 0) currentGear = 0;
            }
        }
        
        void LateUpdate() {
            speedTxt.text = $"Speed: {carRb.linearVelocity.magnitude:F0}\ndriveForce: {engineForce}";
        }
        
        void FixedUpdate() {
            CalculateSuspension(FrontRSuspension, FrontRTire);
            CalculateSuspension(FrontLSuspension, FrontLTire);
            CalculateSuspension(RearRSuspension, RearRTire);
            CalculateSuspension(RearLSuspension, RearLTire);
            
            CalculateSteering(FrontRSuspension);
            CalculateSteering(FrontLSuspension);
            CalculateSteering(RearRSuspension);
            CalculateSteering(RearLSuspension);
            
            switch (wheelDriveMode) {
                case WheelDriveMode.FWD:
                    CalculateForwardForce(FrontRSuspension);
                    CalculateForwardForce(FrontLSuspension);
                    break;
                case WheelDriveMode.RWD:
                    CalculateForwardForce(RearRSuspension);
                    CalculateForwardForce(RearLSuspension);
                    break;
                case WheelDriveMode.AWD:
                    CalculateForwardForce(FrontRSuspension);
                    CalculateForwardForce(FrontLSuspension);
                    CalculateForwardForce(RearRSuspension);
                    CalculateForwardForce(RearLSuspension);
                    break;
                default:
                    Debug.LogWarning($"Invalid wheel drive mode {wheelDriveMode}");
                    break;
            }
            
            CalculateFrictionForce();
        }

        void CalculateFrictionForce() {
            carRb.AddForce(dragForce);
            
            if(AllTireToucheGround()) //Force de frottement des roues
                carRb.AddForce(-rollingResistanceForce);
        }
        
        void CalculateSuspension(Transform suspension, Transform tire) {
            var ray = new Ray(suspension.position, -suspension.up);

            if (!Physics.Raycast(ray, out var hit, suspensionRestDistance)) return;
            
            var springDir = suspension.up;
            var tireWorldVel = carRb.GetPointVelocity(suspension.position);
                
            var offset = suspensionRestDistance - hit.distance;
            var vel = Vector3.Dot(springDir, tireWorldVel);
            var force = (offset * suspensionStrength) - (vel * suspensionDamping);
            
            carRb.AddForceAtPosition(springDir * force, suspension.position);
            
            //Visuels des roues
            tire.position = suspension.position - springDir * (hit.distance - tireMeshRadius);
            Debug.DrawRay(suspension.position, springDir * (force / 10), Color.green);
        }

        void CalculateSteering(Transform suspension) {
            if(!TireToucheGround(suspension)) return; //Does the tire touch the ground ? if it is not the case, we do not need to calculate the grip of the tire
            
            var steeringDir = suspension.right;
            var tireWorldVel = carRb.GetPointVelocity(suspension.position);
            
            var steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
            var desiredVelChange = -steeringVel * steeringGrip;
            var desiredAccel = desiredVelChange / Time.fixedDeltaTime;
            
            carRb.AddForceAtPosition(steeringDir * (tireMass * desiredAccel), suspension.position);
            Debug.DrawRay(suspension.position, steeringDir * (tireMass * desiredAccel), Color.red);
        }

        void CalculateForwardForce(Transform suspension) {
            if(!TireToucheGround(suspension)) return;
            
            var accelDir = suspension.forward;

            if (throttle != 0) {
                var availableTorque = engineForce * throttle;
                
                if (wheelDriveMode is WheelDriveMode.AWD) availableTorque /= 4;
                else availableTorque /= 2;
                
                carRb.AddForceAtPosition(accelDir * availableTorque, suspension.position);
                Debug.DrawRay(suspension.position, accelDir * availableTorque, Color.blue);
            }
        }
        
        bool TireToucheGround(Transform tire) {
            var ray = new Ray(tire.position, -tire.up);

            return Physics.Raycast(ray, out var hit, suspensionRestDistance);
        }

        bool AllTireToucheGround() {
            foreach (var suspension in allSuspensions) {
                if (!TireToucheGround(suspension))
                    return false;
            }

            return true;
        }
    }
}