using System;
using System.Collections.Generic;
using Player;
using TMPro;
using UnityEngine;

namespace CarScripts {
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour {
        private Rigidbody carRb;
        private InputsBrain inputs;
        private Controller player;
        
        private enum WheelDriveMode {
            FWD, //Front
            RWD, //Rear
            AWD //All
        }

        [Header("HUD")]
        public TextMeshProUGUI speedTxt;
        
        [Header("Car Mass")]
        [SerializeField] private float carMass = 1200f;
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
        [SerializeField, Range(0f,1f), Tooltip("The value will influence the grip of the car when steering, a low value mean that it will slip, a high value mean that it will girp")] 
        private float steeringGripLowSpeed;
        [SerializeField, Range(0f,1f)] private float steeringGripHighSpeed;
        [SerializeField, Tooltip("tireMass influence the grip of the tire, base value of 0.5, low value mean a lot slippery, high value mean a lot of grip")] 
        private float tireMassLowSpeed = 60f;
        [SerializeField] private float tireMassHighSpeed = 30f;
        [SerializeField, Tooltip("Enable the rear wheel to turn as well like the front wheel")] 
        private bool rearSteeringAllowed = false;
        
        [Header("Drive Settings")]
        [SerializeField] private WheelDriveMode wheelDriveMode = WheelDriveMode.FWD;
        
        [Header("Engine Settings")]
        [SerializeField, Tooltip("Max engine torque will define in part the max speed of the vehicle")] 
        private float maxEngineTorque = 400f;
        [SerializeField, Tooltip("At which rate the engine torque will go to the max. Low value mean more time to reach it, high value mean less time to reach it")] 
        private float engineResponse = 5f;
        [SerializeField, Tooltip("This value is at which rate the engine will loose power once the throttle is release. So how mush the car will loose speed")] 
        private float engineBrakeTorque = 50f;
        [SerializeField, Tooltip("This value is at which rate the engine will loose power once no one is driving")] 
        private float brakeMultNoDriver = 6f;
        [SerializeField, Tooltip("Define the wheel radius wich participate in the speed calculation, low value mean less force and so less speed, higher value mean more force")] 
        private float wheelRadius = 0.3f;
        [SerializeField, Tooltip("Define the final drive ratio of the engine, lower value mean less force, higher value mean more force")] 
        private float finalDrive = 3.2f;
        [SerializeField, Tooltip("Define the loss of power of the transmission, 1 is no loss, 0 is total loss")] 
        private float transmissionEfficiency = 0.85f;
        [SerializeField, Tooltip("Ratio of the gear, it set the speed and force deliver from the engine to the wheels, a low value mean not much force, and a high value mean a lot of force")] 
         private float gearRatio = 2.66f;
         
        [Header("Brake")]
        [SerializeField] private float brakeForce;

        [Header("Drift Not Working")] 
        [SerializeField] private float minimumDriftSpeed;
        [SerializeField, Range(0f, 90f)] private float minimumDriftAngle = 15f;
        [SerializeField, Range(0f, 90f)] private float maximumDriftAngle = 45f;
        [SerializeField, Range(0f, 1f)] private float minimumDriftGrip = 0.2f;
        [SerializeField, Range(0f, 1f)] private float maximumDriftGrip = 1.0f;
        
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

        [Header("Other")] 
        [SerializeField] private float velThreshold = 20f;
        [SerializeField] private float pushPlayerForce = 20f;
        [SerializeField] private float openDoorWhenBrakeAtSpeed = 20f;
        
        //Pour le moment ces valeurs ne sont pas utillisés
        // float b = 1.25f; //Distance entre le centre de gravité et l'essieu avant
        // float c = 1.25f; //Distance entre le centre de gravité et l'essieu arrière
        // float l = 2.5f; //Empattement
        // float h = 1f; //Hauteur du centre de gravité à partir du sol
        
        //float acceleration; //Va être calculé par la suite
        
        //float  carWeight => carMass * gravity;
        // float frontWeight => c / l * carWeight - h/l * carMass * acceleration; //Appliqué ces forces à un endroit du coup
        // float rearWeight => b / l * carWeight + h/l * carMass * acceleration;

        /*float maxTorque => lookUpTorque.Evaluate(rpm);
        float engineTorque => maxTorque * throttle;
        
        float wheelRotationRate => rigidBody.linearVelocity.magnitude / wheelRadius; //Surement nécessiter des modifications pour régarder la direction de la voiture
        float rpm => wheelRotationRate * gearRatio * differentialRatio * 60 / 2 * 3.14f;*/
        
        //float wheelRPM => carRb.linearVelocity.magnitude / (2 * Mathf.PI * wheelRadius) * 60f;
        
        private Dictionary<Transform, WheelContact> wheelsContact = new();
        private Dictionary<Transform, WheelGrip> wheelGrip = new();
        
        private float steering;
        private float currentSteering;
        private float throttle;
        private float throttleTimer;
        private float brake;

        private float currentEngineTorque;
        private float reverse;
        private float currentSteeringAngle;
        private float maxSteering;

        private float tireMass;
        private float steeringGrip;
        
        private float rollingResistance => 30 * airDrag;
        private float airDrag => 0.5f * frictionCoefficient * carFrontalSurface * airDensity;
        private float speedRatio => carRb.linearVelocity.sqrMagnitude / maxDriveForce;
        private float maxDriveForce => maxEngineTorque * gearRatio * finalDrive * transmissionEfficiency;
        private Vector3 dragForce => -airDrag * carRb.linearVelocity * carRb.linearVelocity.magnitude;
        private Vector3 rollingResistanceForce => rollingResistance * carRb.linearVelocity;

        private bool AiCar = false;

        Vector3 previousVelocity;
        
        public void SetAiCar(bool ai) {
            AiCar = ai;
        }
        
        void Start() {
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
                wheelGrip.Add(suspension, new WheelGrip());
            }
        }
        
        public void BindInput(InputsBrain brain, Controller p = null) {
            inputs = brain;
            player = p;
        }

        void Update() {
            MyInputs();
            WheelsSteering();
            HandleWheelsGrip();
        }

        private void HandleWheelsGrip() {
            steeringGrip = Mathf.Lerp(steeringGripLowSpeed, steeringGripHighSpeed, speedRatio);
            tireMass = Mathf.Lerp(tireMassLowSpeed, tireMassHighSpeed, speedRatio);
        }

        void WheelsSteering() {
            maxSteering = Mathf.Lerp(steeringAngleLowSpeed, steeringAngleHighSpeed, speedRatio);
            currentSteeringAngle = Mathf.Lerp(steeringAngleLowSpeed, steeringAngleHighSpeed, speedRatio);
            
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

        private void OnCollisionEnter(Collision collision) {
            if(collision.transform.TryGetComponent(out EnemyController ctrl))
                ctrl.KnockOut(4f, carRb.linearVelocity.normalized * 10 + Vector3.up * 10);
        }

        void MyInputs() {
            if(AiCar) return;
            
            if (inputs == null) {
                steering = 0;
                throttle = 0;
                reverse = 0;
                return;
            }
            
            steering = inputs.Steering.ReadValue<float>();
            throttle = inputs.Throttle.ReadValue<float>();
            reverse = inputs.Reverse.ReadValue<float>();
            brake = inputs.Brake.ReadValue<float>();

            if (inputs.LeaveCar.WasPressedThisFrame()) {
                player.isDriving = false;
                BindInput(null);
            }
        }

        public void SetAiInputs(float forward, float turn) {
            steering = turn;
            throttle = Mathf.Max(0, forward);
            reverse = Mathf.Min(0, forward);
            reverse = Mathf.Abs(reverse);
        }
        
        void LateUpdate() {
            speedTxt.text = $"{carRb.linearVelocity.magnitude:F0} KM/H\n{currentEngineTorque * 10:F0} RPM";
        }
        
        void FixedUpdate() {
            if (throttle == 0 && reverse == 0) {
                if (carRb.linearVelocity.magnitude < 2) {
                    carRb.linearVelocity = Vector3.Lerp(carRb.linearVelocity, Vector3.zero, Time.deltaTime * 10f);
                }
            }
            
            foreach (var suspension in allSuspensions) {
                var ray = new Ray(suspension.position, -suspension.up);
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
            CheckForVelChange();
        }

        private void CheckForVelChange() {
            if(AiCar) return;
            
            var forwardSpeed = Vector3.Dot(carRb.GetPointVelocity(transform.position), transform.forward);
            
            if (previousVelocity.magnitude - carRb.linearVelocity.magnitude > velThreshold) {
                CarDoors.instance.ForceOpenDoor();
                FindAnyObjectByType<AttachedPlayer>().ApplyForceToController(-previousVelocity.normalized * pushPlayerForce);
            }
            else if (reverse >= .9f && forwardSpeed > openDoorWhenBrakeAtSpeed) {
                CarDoors.instance.ForceOpenDoor();
                FindAnyObjectByType<AttachedPlayer>().ApplyForceToController(-previousVelocity.normalized * pushPlayerForce);
            }
            
            previousVelocity = carRb.linearVelocity;
        }

        void CalculateFrictionForce() {
            carRb.AddForce(dragForce);
            
            if(AllTireToucheGround()) //Force de frottement des roues
                carRb.AddForce(-rollingResistanceForce);
        }
        
        void CalculateSuspension(Transform suspension, Transform tire) {
            if (!wheelsContact[suspension].grounded) {
                tire.position = suspension.position - suspension.up * (suspensionRestDistance - tireMeshRadius);
                return;
            }

            var springDir = Vector3.up;
            if(wheelsContact[suspension].hit.normal != Vector3.up)
                springDir = wheelsContact[suspension].hit.normal.normalized;
            
            if (transform.eulerAngles.z > 45 && transform.eulerAngles.z < 315) {
                if (transform.eulerAngles.z < 60) { //Pousser a droite
                    springDir = suspension.right;
                }
                else if (transform.eulerAngles.z > 300){ //Pousser a gauche
                    springDir = -suspension.right;
                }
                else {
                    return;
                }
            }
            
            var tireWorldVel = carRb.GetPointVelocity(suspension.position);
                
            var offset = suspensionRestDistance - wheelsContact[suspension].hit.distance;
            var vel = Vector3.Dot(springDir, tireWorldVel);
            var force = (offset * suspensionStrength) - (vel * suspensionDamping);
            
            carRb.AddForceAtPosition(springDir * force, suspension.position);
            
            //Visuel des roues
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

            // ==== DRIFT Section ====
            // var forwardSpeed = Vector3.Dot(carRb.GetPointVelocity(suspension.position), suspension.forward);
            // var rawDriftAngle = 0f;
            //
            // if(forwardSpeed > 0f)
            //     rawDriftAngle = Vector3.SignedAngle(suspension.forward, carRb.linearVelocity, suspension.up);
            // else if(forwardSpeed < 0f)
            //     rawDriftAngle = Vector3.SignedAngle(-suspension.forward, carRb.linearVelocity, suspension.up);
            //
            // wheelGrip[suspension].rawAngle = rawDriftAngle;
            //
            // rawDriftAngle = Mathf.Clamp(rawDriftAngle, -90f, 90f);
            // var driftAngle = Mathf.Abs(rawDriftAngle);
            //
            // if (carRb.linearVelocity.magnitude < minimumDriftSpeed)
            //     driftAngle = 0;
            //
            // var currentDriftAngle = Mathf.Clamp(driftAngle, minimumDriftAngle, maximumDriftAngle);
            // var t = Mathf.InverseLerp(minimumDriftAngle, maximumDriftAngle, currentDriftAngle);
            //
            // if (brake > 0f) wheelGrip[suspension].grip = minimumDriftGrip;
            // else wheelGrip[suspension].grip = Mathf.Lerp(maximumDriftGrip, minimumDriftGrip, t);
            
            carRb.AddForceAtPosition(steeringDir * (tireMass * wheelGrip[suspension].grip * desiredAccel), suspension.position);
            Debug.DrawRay(suspension.position, steeringDir * (tireMass * desiredAccel), Color.red);
        }

        void CalculateForwardForce(Transform suspension) {
            if(!wheelsContact[suspension].grounded) return;
            
            var accelDir = suspension.forward;
            var forwardSpeed = Vector3.Dot(carRb.GetPointVelocity(suspension.position), suspension.forward);
            
            var targetTorque = throttle * maxEngineTorque;   
            currentEngineTorque = Mathf.Lerp(currentEngineTorque, targetTorque, engineResponse * Time.fixedDeltaTime);
            var wheelTorque = currentEngineTorque * gearRatio * finalDrive * transmissionEfficiency;
            
            if (reverse > 0 && forwardSpeed < 1f) {
                targetTorque = reverse * maxEngineTorque;
                currentEngineTorque = Mathf.Lerp(currentEngineTorque, targetTorque, engineResponse * Time.fixedDeltaTime);
                wheelTorque = currentEngineTorque * (-gearRatio / 2) * finalDrive * transmissionEfficiency;
            }
            
            var driveForce = wheelTorque / wheelRadius;
            
            if (throttle < 0.01f && forwardSpeed > 0f) driveForce -= engineBrakeTorque * forwardSpeed;

            if (inputs == null && !AiCar) {
                if (forwardSpeed > 1f) driveForce -= engineBrakeTorque * brakeMultNoDriver * forwardSpeed;
                else if (forwardSpeed < -1f) driveForce += engineBrakeTorque * brakeMultNoDriver * forwardSpeed;
            }
            
            var driveWheelCount = wheelDriveMode == WheelDriveMode.AWD ? 4 : 2;
            driveForce /= driveWheelCount;
            
            //Set the force sideway for better drifting
            // if(wheelGrip[suspension].rawAngle > 1f) accelDir = Vector3.Lerp(-suspension.right, suspension.forward, wheelGrip[suspension].grip);
            // else if (wheelGrip[suspension].rawAngle < -1f) accelDir = Vector3.Lerp(suspension.right, suspension.forward, wheelGrip[suspension].grip);
            
            carRb.AddForceAtPosition(accelDir * driveForce, suspension.position); //Added grip to force
            Debug.DrawRay(suspension.position, accelDir * driveForce, Color.blue);
        }

        void CalculateBrakeForce(Transform suspension) {
            if(!wheelsContact[suspension].grounded) return;
            var forwardSpeed = Vector3.Dot(carRb.GetPointVelocity(suspension.position), suspension.forward);
            
            var brakeTorque = reverse * brakeForce;
            if (forwardSpeed < 0f && throttle > 0f) brakeTorque = throttle * brakeForce;
            
            var brakeForceAtWheel = brakeTorque / wheelRadius;
            
            if(forwardSpeed > 0f)
                carRb.AddForceAtPosition(-suspension.forward * brakeForceAtWheel, suspension.position);
            else if (throttle > 0.01f && forwardSpeed < 0f)
                carRb.AddForceAtPosition(suspension.forward * brakeForceAtWheel, suspension.position);
            
        }
        
        bool AllTireToucheGround() {
            foreach (var suspension in allSuspensions) {
                if (!wheelsContact[suspension].grounded)
                    return false;
            }

            return true;
        }

        // private void OnGUI() {
        //     GUI.Box(new Rect(10, 10, 300, 220), "Vehicle Debug");
        //
        //     GUI.Label(new Rect(20,80, 280, 20), $"current Angle: {wheelGrip[FrontLSuspension].rawAngle:F1}°");
        //     GUI.Label(new Rect(20,100, 280, 20), $"Wheel Grip: {wheelGrip[FrontLSuspension].grip:F1}");
        // }
    }

    public struct WheelContact {
        public bool grounded;
        public RaycastHit hit;
    }

    public class WheelGrip {
        public float grip = 1f;
        public float rawAngle = 0f;
    }
}