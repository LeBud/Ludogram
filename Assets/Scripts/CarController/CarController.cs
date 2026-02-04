using System.Collections.Generic;
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
        [SerializeField] private float steeringAngleLowSpeed;
        [SerializeField] private float steeringAngleHighSpeed;
        [SerializeField] private float steeringSmoothingLowSpeed;
        [SerializeField] private float steeringSmoothingHighSpeed;
        //public AnimationCurve steeringCurve; //Courbe de % d'angle de rotation des roues en fonction de la vitesse de la voiture
        [SerializeField, Range(0f,1f), Tooltip("The value will influence the grip of the car when steering, a low value mean that it will slip, a high value mean that it will girp")] 
        private float steeringGrip;
        [SerializeField, Tooltip("tireMass influence the grip of the tire, base value of 0.5, low value mean a lot slippery, high value mean a lot of grip")] 
        private float tireMass = 0.5f;
        [SerializeField, Tooltip("Enable the rear wheel to turn as well like the front wheel")] 
        private bool rearSteeringAllowed = false;
        
        [Header("Drive Settings")]
        [SerializeField] private WheelDriveMode wheelDriveMode = WheelDriveMode.FWD;
        // [SerializeField] private float minEngineForce = 100;
        // [SerializeField] private float maxEngineForce = 500;
        // [SerializeField] private float timeToReachMaxForce = 10;
        // [SerializeField, Range(0.001f,1f)] private float decelerationRate = 0.1f;
        
        //public AnimationCurve torqueCurve = AnimationCurve.EaseInOut(0f, 100f, 7000f, 50f);
        
        [Header("Engine Settings")]
        [SerializeField] private float maxEngineTorque = 400f;
        [SerializeField] private float engineResponse = 5f;
        [SerializeField] private float engineBrakeTorque = 50f;
        [SerializeField] private float wheelRadius = 0.3f;
        [SerializeField] private float finalDrive = 3.2f;
        [SerializeField] private float transmissionEfficiency = 0.85f;
         public float[] gears = new float[6];
         public int currentGear = 0;
         
         // public float finalDriveRatio = 3.42f;
         // public float transmissionEfficiency = 0.7f;
         // public float wheelRadius = 0.25f;
         // public float idleRPM = 800f;
         // public float maxRPM = 7000f;
         // public float engineInertia = 0.1f;
         // public float downforceCoefficient = 3.0f;
         
        [Header("Brake")]
        [SerializeField] private float brakeForce;
         
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

        float currentEngineTorque;
        
        private float brake;
        
        // private float accelTime = 0;
        // private float currentEngineForce = 0;
        private float currentSteeringAngle;
        private float maxSteering;
        
        private Dictionary<Transform, WheelContact> wheelsContact = new();
        
        //TODO Revoir le fonctionnement des suspsensions
        
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

            foreach (var suspension in allSuspensions) {
                wheelsContact.Add(suspension, new WheelContact());
            }
        }

        void Update() {
            MyInputs();
            TurnWheels();
        }

        void TurnWheels() {
            maxSteering = Mathf.Lerp(steeringAngleLowSpeed, steeringAngleHighSpeed, throttle * maxEngineTorque / maxEngineTorque);
            currentSteeringAngle = Mathf.Lerp(steeringAngleLowSpeed, steeringAngleHighSpeed,
                throttle * maxEngineTorque / maxEngineTorque);
            
            currentSteering = Mathf.SmoothStep(currentSteering, steering * currentSteeringAngle, maxSteering * Time.deltaTime);
            currentSteering = Mathf.Clamp(currentSteering, -currentSteeringAngle, currentSteeringAngle);
            
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
            brake = Inputs.Brake.ReadValue<float>();
            
            if (Inputs.ShiftGear.WasPressedThisFrame()) {
                if (Inputs.ShiftGear.ReadValue<float>() > 0) currentGear++;
                else currentGear--;
                
                if(currentGear >= gears.Length) currentGear = gears.Length - 1;
                if(currentGear < 0) currentGear = 0;
            }
        }
        
        void LateUpdate() {
            //speedTxt.text = $"Speed: {carRb.linearVelocity.magnitude:F0}\ndriveForce: {currentEngineForce:F0}";
        }
        
        void FixedUpdate() {
            foreach (var suspension in allSuspensions) {
                Ray ray = new Ray(suspension.position, -suspension.up);
                WheelContact contact;
                contact.grounded = Physics.Raycast(ray, out contact.hit, suspensionRestDistance - 0.05f);
                
                wheelsContact[suspension] = contact;
            }
            
            CalculateSuspension(FrontRSuspension, FrontRTire);
            CalculateSuspension(FrontLSuspension, FrontLTire);
            CalculateSuspension(RearRSuspension, RearRTire);
            CalculateSuspension(RearLSuspension, RearLTire);
            
            CalculateSteering(FrontRSuspension);
            CalculateSteering(FrontLSuspension);
            CalculateSteering(RearRSuspension);
            CalculateSteering(RearLSuspension);
            
            CalculateBrakeForce(FrontRSuspension);
            CalculateBrakeForce(FrontLSuspension);
            CalculateBrakeForce(RearRSuspension);
            CalculateBrakeForce(RearLSuspension);
            
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
            if (!wheelsContact[suspension].grounded) return;
            
            var springDir = suspension.up;
            var tireWorldVel = carRb.GetPointVelocity(suspension.position);
                
            var offset = suspensionRestDistance - wheelsContact[suspension].hit.distance;
            var vel = Vector3.Dot(springDir, tireWorldVel);
            var force = (offset * suspensionStrength) - (vel * suspensionDamping);
            
            carRb.AddForceAtPosition(springDir * force, suspension.position);
            
            //Visuels des roues
            tire.position = suspension.position - springDir * (wheelsContact[suspension].hit.distance - tireMeshRadius);
            Debug.DrawRay(suspension.position, springDir * (force / 10), Color.green);
        }

        void CalculateSteering(Transform suspension) {
            if(!wheelsContact[suspension].grounded) return; //Does the tire touch the ground ? if it is not the case, we do not need to calculate the grip of the tire
            
            var steeringDir = suspension.right;
            var tireWorldVel = carRb.GetPointVelocity(suspension.position);
            
            var steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
            var desiredVelChange = -steeringVel * steeringGrip;
            var desiredAccel = desiredVelChange / Time.fixedDeltaTime;
            
            carRb.AddForceAtPosition(steeringDir * (tireMass * desiredAccel), suspension.position);
            Debug.DrawRay(suspension.position, steeringDir * (tireMass * desiredAccel), Color.red);
        }

        void CalculateForwardForce(Transform suspension) {
            if(!wheelsContact[suspension].grounded) return;
            
            var accelDir = suspension.forward;

            float targetTorque = throttle * maxEngineTorque;
            currentEngineTorque = Mathf.Lerp(currentEngineTorque, targetTorque, engineResponse * Time.fixedDeltaTime);
            float wheelTorque = currentEngineTorque * gears[currentGear] * finalDrive * transmissionEfficiency;
            float driveForce = wheelTorque / wheelRadius;

            float forwardSpeed = Vector3.Dot(carRb.GetPointVelocity(suspension.position), suspension.forward);
            
            if (throttle < 0.01f && forwardSpeed > 0f) {
                // float engineBrakeForce = engineBrakeTorque * gears[currentGear] / wheelRadius;
                // driveForce -= engineBrakeForce;
                driveForce -= engineBrakeTorque * forwardSpeed;
            }
            
            // if (throttle > 0) {
            //     //accelTime += Time.fixedDeltaTime * throttle;
            // }
            // else {
            //     //accelTime -= Time.fixedDeltaTime * decelerationRate;
            // }
            
            // accelTime = Mathf.Clamp(accelTime, 0, timeToReachMaxForce);
            // currentEngineForce = Mathf.Lerp(minEngineForce, maxEngineForce, accelTime / timeToReachMaxForce);
            
            //var availableTorque = engineForce * throttle;
            //var availableTorque = currentEngineForce;
                
            // if (wheelDriveMode is WheelDriveMode.AWD) availableTorque /= 4;
            // else availableTorque /= 2;

            var driveWheelCount = wheelDriveMode == WheelDriveMode.AWD ? 4 : 2;
            driveForce /= driveWheelCount;
            
            //if(currentEngineForce > minEngineForce)
                //carRb.AddForceAtPosition(accelDir * availableTorque, suspension.position);
                
            carRb.AddForceAtPosition(accelDir * driveForce, suspension.position);
            
            //Debug.DrawRay(suspension.position, accelDir * availableTorque, Color.blue);
            Debug.DrawRay(suspension.position, accelDir * driveForce, Color.blue);
        }

        void CalculateBrakeForce(Transform suspension) {
            float brakeTorque = brake * brakeForce;
            float brakeForceAtWheel = brakeTorque / wheelRadius;
            
            if(Vector3.Angle(carRb.linearVelocity, transform.forward) < 5 && carRb.linearVelocity.magnitude > 0)
                carRb.AddForceAtPosition(-suspension.forward * brakeForceAtWheel, suspension.position);
            
            //currentEngineForce = carRb.linearVelocity.magnitude;
            //accelTime -= Time.fixedDeltaTime * (brakeForce * brake);
        }
        
        bool AllTireToucheGround() {
            foreach (var suspension in allSuspensions) {
                if (!wheelsContact[suspension].grounded)
                    return false;
            }

            return true;
        }
    }

    public struct WheelContact {
        public bool grounded;
        public RaycastHit hit;
    }
}