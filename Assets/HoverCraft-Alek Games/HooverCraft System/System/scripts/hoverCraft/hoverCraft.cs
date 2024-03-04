using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlekGames.HoverCraftSystem.Systems.Main
{
    [RequireComponent(typeof(Rigidbody))]
    public class hoverCraft : MonoBehaviour
    {

        #region values
        private enum InputT {external, axes };

        private enum liftType { world, local};

        private enum groundCheckT {both, detectorDown, worldDown}

        private enum turnT { independent, dependent, instantDependent};

        private enum invertType { input, velocity, none};


        [SerializeField, Tooltip("what input should the script use? axes - use axes other - asighn input from other script by function parseInput")]
        private InputT inputType;

        [SerializeField, Tooltip("if should, just like a car have inverted input when going backwards. if set to input will invert input if vehicle wants to go back (back input is true), if velocity, will check if going back at speed over than 0.1, if non, will never invert input")]
        private invertType inputInvertType;

        [SerializeField, Tooltip("world - Vector3.up ; local - transform.up")]
        private liftType liftTypeSpace;

        [SerializeField, Tooltip("speed of making the x trust mach the input")]
        private float xThrustCorrectionSpeed = 15;
        [SerializeField, Tooltip("speed of making the z trust mach the input")]
        private float zThrustCorrectionSpeed = 15;

        [SerializeField, Tooltip("center of the vehicle, should be between all of hoverPoints")]
        private Transform center;

        [Header("forward forces")]

        [SerializeField, Tooltip("forward of the vehicle")]
        private Transform forwardVector;

        [SerializeField, Tooltip("speed of vehicle that is applyied forward when going forward")]
        private float forwardSpeed = 5000;

        [SerializeField, Tooltip("speed of vehicle that is applyied backwards when going backwards")]
        private float backwardsSpeed = 2000;

        [SerializeField, Tooltip("the maxinum velocity.magditud for the hoovercraft to apply forces forward/backwards")]
        private float maxSpeed = 20;

        [SerializeField, Tooltip("forceMode of forces forward/backwards")]
        private ForceMode forwardForceMode = ForceMode.Acceleration;


        [Header("angular forces (turning)")]

        [SerializeField, Tooltip("independent - will mach input. dependent - will mach input if moving, will take velocity in consideration. instantDependent - will apply input only if moving (only if input to move forward backwards)")]
        private turnT turnType;

        [SerializeField]
        private bool canAirTurn = true;

        [SerializeField, Tooltip("torque applyied to rotate the veicle")]
        private float angularSpeed = 750;

        [SerializeField, Tooltip("the maxinum angularVelocity.magditud for the hoovercraft to apply Torque left/right")]
        private float maxAngularSpeed = 12;

        [SerializeField, Tooltip("forceMode of Torques left/right")]
        private ForceMode angularForceMode = ForceMode.Acceleration;

        [Header("counter forces")]

        [SerializeField, Min(0), Tooltip("force of counter movement while input is not 0. applies only to XZ axes")]
        private float moveCounterForce = 300;
        [SerializeField, Min(0), Tooltip("force of counter movement while input is 0. applies only to XZ axes")]
        private float floatCounterForce = 1000;
        [SerializeField, Tooltip("if script should clamp the velocity on xz axis to make counter forces be more accurate, but may make speedy movement harder")]
        private bool ClampXZ = true;
        [SerializeField, Min(0), Tooltip("force of counter movement on Y axis when hoverCraft is ment to go up")]
        private float YUpCounterForce = 300;
        [SerializeField, Min(0), Tooltip("force of counter movement on Y axis")]
        private float YDownCounterForce = -50;
        [SerializeField, Tooltip("if script should clamp the velocity on y axis to make counter forces be more accurate, but may make speedy movement harder")]
        private bool ClampY = false;
        [SerializeField, Tooltip("forceMode of counter movement")]
        private ForceMode counterForceMode = ForceMode.Acceleration;

        [Header("main ground detection & actions")]


        [SerializeField, Tooltip("spece for checking if hovercraft is grounded")]
        private groundCheckT groundCheckType;

        [SerializeField, Tooltip("distance that the veicle will detect the ground from groundDetector")]
        private float groundDetectionDistance = 2.4f;

        [SerializeField, Tooltip("a point from which the ground will be detected. used only if groundDetectionType is set to both or detector")]
        private Transform groundDetector;

        [SerializeField, Tooltip("the layer that the ground  is on")]
        private LayerMask groundLayer;

        [SerializeField, Tooltip("desired height for the hoovercraft to be on (will be a little lover)")]
        private float hoverHeight = 1.8f;

        [Header("max slopes")]

        [SerializeField, Tooltip("the maximum normal diffrence accepable to float with out moving sideways")]
        [Range(0.1f, 180)] private float maxNormalAngle = 90;

        [SerializeField, Tooltip("force used to push veicle of hill with slope over maxNormal angle. Keep in mind that tis force is checked and possibly added foreach hoverPoint")]
        private float normalFixForce = 5000;

        [SerializeField, Tooltip("ForceMode used for normalFixForce")]
        private ForceMode normalFixForceMode = ForceMode.Force;

        [Header("additional forces")]

        [SerializeField, Tooltip("the force that is applyied for the vehicle when it is grounded up.")]
        private float additionalGroundedForceUp = 3000;

        [SerializeField, Tooltip("the force that is applyied for the vehicle when it is not grounded up")]
        private float additionalUnGroundedForceUp = -1000;

        [Header("Jumping")]

        [SerializeField, Tooltip("the force of jump")]
        private float jumpForce = 10000;

        [SerializeField, Tooltip("forceMode of the jump")]
        private ForceMode jumpForceMode = ForceMode.Acceleration;

        [Space]
        [SerializeField] 
        private hoverPointSettings[] hoverPoints;

        private bool grounded;

        private Vector2 inputMovement;
        private bool inputJump;
        private Vector2 correctedInput;
        private Vector3 thrust;


        private Vector3 groundNormal;

        private Rigidbody rb;

        #endregion

        #region updates

        void Start()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>(); //can be setup to not be null using rigidbodySetup function
                rb.centerOfMass = center.localPosition;
            }

        }

        private void Update()
        {
            gatherInput();
            reCalculateInputs(); // applyies corrects input to be correctly mached with trusts
            // since i gather input in update no thrust will change till next update so i calculate thrusts here
            calculateThrusts(); 
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            checkIfGrounded();
            addHoverForce();
            addMoveForces();
            addTorques();
            addCounterForce();
        }

        #endregion

        #region values update

        private void gatherInput()
        {
            if (inputType == InputT.axes)
            {
                inputMovement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                inputJump = Input.GetKey(KeyCode.Space);
            }
        }

        private void reCalculateInputs()
        {
            correctedInput.y = inputMovement.y;

            if (canAirTurn || grounded)
            {
                switch (turnType)
                {
                    case turnT.independent:
                        correctedInput.x = inputMovement.x;

                        if (inputInvertType != invertType.none)
                        {
                            if (inputInvertType == invertType.input && inputMovement.y == -1) correctedInput.x *= -1;
                            else if (inputInvertType == invertType.velocity && transform.InverseTransformDirection(rb.velocity).z < 0.1f) correctedInput.x *= -1;
                        }

                        break;
                    case turnT.dependent:
                        Vector2 vel = Vector2.ClampMagnitude(new Vector2(rb.velocity.x, rb.velocity.z) / maxSpeed, 1);

                        float mult = vel.magnitude;

                        if (inputInvertType != invertType.none)
                        {
                            if (inputInvertType == invertType.input && inputMovement.y == -1) mult *= -1;
                            else if(inputInvertType == invertType.velocity && transform.InverseTransformDirection(rb.velocity).z < 0.1f) mult *= -1;
                        }

                        correctedInput.x = inputMovement.x * mult;
                        break;
                    case turnT.instantDependent:
                        correctedInput.x = inputMovement.x * inputMovement.y;

                        break;
                }
            }
            else correctedInput.x = 0;
        }



        private void calculateThrusts()
        {
           Vector2 nt = Vector2.zero;
            
            if (correctedInput.y > 0)
                nt.y = correctedInput.y * forwardSpeed;
            else if (correctedInput.y < 0)
                nt.y = correctedInput.y * backwardsSpeed;

            nt.x = correctedInput.x * angularSpeed;

            thrust.y = (inputJump && grounded) ? jumpForce : 0;

            thrust.x = Mathf.Lerp(thrust.x, nt.x, xThrustCorrectionSpeed * Time.deltaTime);
            thrust.z = Mathf.Lerp(thrust.z, nt.y, zThrustCorrectionSpeed * Time.deltaTime);
        }
        #endregion

        #region physics

        private void checkIfGrounded()
        {
            RaycastHit info;
            if (groundCheckType != groundCheckT.worldDown && Physics.Raycast(groundDetector.position, -groundDetector.up, out info, groundDetectionDistance, groundLayer))
            {
                grounded = true;
                groundNormal = info.normal;
            }
            else if (groundCheckType != groundCheckT.detectorDown && Physics.Raycast(groundDetector.position, Vector3.down, out info, groundDetectionDistance, groundLayer))
            {
                grounded = true;
                groundNormal = info.normal;
            }
            else
            {
                grounded = false;
                groundNormal = transform.up;
            }
        }

        private void addMoveForces()
        {
            if (rb.velocity.magnitude <= maxSpeed && thrust.z != 0)
            {
                rb.AddForce(forwardVector.forward * thrust.z * Time.deltaTime, forwardForceMode);
            }

            if (jumpForce != 0)
            {
                //Debug.Log("jumping");
                rb.AddForce(center.up * thrust.y * Time.deltaTime, jumpForceMode);
            }

            if (grounded && additionalGroundedForceUp != 0) rb.AddForce(Vector3.up * additionalGroundedForceUp * Time.deltaTime);
            else if (!grounded && additionalUnGroundedForceUp != 0) rb.AddForce(Vector3.up * additionalUnGroundedForceUp * Time.deltaTime);
        }

        private void addTorques()
        {
            if (rb.angularVelocity.magnitude <= maxAngularSpeed && thrust.x != 0)
                rb.AddRelativeTorque(Vector3.up * thrust.x * Time.deltaTime, angularForceMode);
            
        }


        private void addHoverForce()
        {
            for (int i = 0; i < hoverPoints.Length; i++)
            {
                Vector3 levelingPos = hoverPoints[i].point.position + hoverPoints[i].point.up * hoverPoints[i].levelingOffset;

                if (Physics.Raycast(hoverPoints[i].point.position, -hoverPoints[i].point.up, out RaycastHit hitInfo, hoverHeight, groundLayer))
                {
                    Vector3 updir = (liftTypeSpace == liftType.world) ? Vector3.up : transform.up;

                    rb.AddForceAtPosition(updir * hoverPoints[i].liftForce * (1 - (hitInfo.distance / hoverHeight)) * Time.deltaTime, hoverPoints[i].point.position, hoverPoints[i].liftForceMode);


                    if(Vector3.Angle(hitInfo.normal, Vector3.up) > maxNormalAngle)
                    {
                        Vector3 dir = new Vector3(hitInfo.normal.x, 0, hitInfo.normal.z).normalized; //just get the XZ dir, when there is Y the hoverCraft chitters to much, due to it moving up and down
                        rb.AddForce(dir * normalFixForce * Time.deltaTime,  normalFixForceMode); // just addForce, not at position. if at position hoverCraft will spin too much
                    }
                }
                else if ((hoverPoints[i].WhenToLevel == hoverPointSettings.lw.WhenVeicleGroundedAndThisNot && grounded) || (hoverPoints[i].WhenToLevel == hoverPointSettings.lw.whenThisNotGrounded && !grounded))
                {
                    levelVeicle(i, levelingPos);
                }
                else if (hoverPoints[i].WhenToLevel == hoverPointSettings.lw.always)
                {
                    levelVeicle(i, levelingPos);
                }

            }
        }

        private void levelVeicle(int index, Vector3 levelingPos)
        {
            float multiplyier = 1;
            if (Vector3.Distance(Vector3.down, forwardVector.up) < Vector3.Distance(Vector3.up, forwardVector.up)) multiplyier = -1;

            if (hoverPoints[index].point.position.y - hoverPoints[index].levelingHeightDiffrenceTolerance > center.position.y)
            {
                rb.AddForceAtPosition(Vector3.down * hoverPoints[index].levelingForce * multiplyier * Time.deltaTime, levelingPos, hoverPoints[index].levelForceMode);
            }
            else if (hoverPoints[index].point.position.y + hoverPoints[index].levelingHeightDiffrenceTolerance < center.position.y)
            {
                rb.AddForceAtPosition(Vector3.up * hoverPoints[index].levelingForce * multiplyier * Time.deltaTime, levelingPos, hoverPoints[index].levelForceMode);
            }
        }

        private void addCounterForce()
        {
            float force = inputMovement != Vector2.zero ? moveCounterForce : floatCounterForce;

            Vector3 countedVel = rb.velocity;
            countedVel.y = 0;

            if (ClampXZ) countedVel = Vector3.ClampMagnitude(countedVel, 1);
            rb.AddForce(-countedVel * force * Time.deltaTime, counterForceMode);

            float yVel = rb.velocity.y;

            if (ClampY) yVel = Mathf.Clamp(yVel, -1, 1);

            float usedForce = yVel >= 0 ? YUpCounterForce : YDownCounterForce;

            rb.AddForce(Vector3.down * yVel * usedForce * Time.deltaTime, counterForceMode);
        }

        #endregion

        #region external

        /// <summary>
        /// changes input on y axis
        /// </summary>
        /// <param name="to"></param>
        public void changeYMoveInput(float to)
        {
            to = Mathf.Clamp(to, -1, 1);

            inputMovement.y = to;
        }

        /// <summary>
        /// changes input on x axis
        /// </summary>
        /// <param name="to"></param>
        public void changeXMoveInput(float to)
        {
            to = Mathf.Clamp(to, -1, 1);

            inputMovement.x = to;
        }

        /// <summary>
        /// changes jump input
        /// </summary>
        /// <param name="to"></param>
        public void changeJumpInput(bool to) => inputJump = to;

        /// <summary>
        /// copies values given, and applies them to input
        /// </summary>
        /// <param name="move"></param>
        /// <param name="jump"></param>
        public void parseInput(Vector2 move, bool jump)
        {
            inputMovement = move;
            inputJump = jump;
        }

        /// <summary>
        /// gets current ground normal detected by graund detector
        /// </summary>
        /// <returns></returns>
        public Vector3 getGroundNormal()
        {
          
            return groundNormal;
        }

        /// <summary>
        /// gets input used by hovercraft
        /// </summary>
        /// <returns></returns>
        public Vector2 getInput()
        {
            return inputMovement;
        }

        /// <summary>
        /// gets input that has been corrected by some calculations used by hovercraft
        /// </summary>
        /// <returns></returns>
        public Vector2 getCorrectedInput()
        {
            return correctedInput;
        }

        /// <summary>
        /// gets dir of trust of vehicle
        /// </summary>
        /// <returns></returns>
        public Vector2 getThrustDir()
        {
            float x = thrust.x / angularSpeed;
            float z = thrust.z > 0? thrust.z / forwardSpeed: thrust.z / backwardsSpeed;
            return new Vector2(x, z);
        }

        #endregion

        #region easySetup

        [ContextMenu("set up hover point from 0")]
        public void setUpHoverPoint()
        {
            for (int i = 1; i < hoverPoints.Length; i++) //copy from 0 to others
            {
                Transform p = (hoverPoints[i].point != null)? hoverPoints[i].point: null;
                hoverPoints[i] = hoverPoints[0];
                if(p != null) hoverPoints[i].point = p;
            }
        }

        [ContextMenu("find hover points from hoverPoint 0")]
        public void findHoverPointFromPoint0()
        {
            if (hoverPoints == null) hoverPoints = new hoverPointSettings[0];

            if (hoverPoints[0].point == null)
            {
                Debug.LogError("no hoover point selected in slot 1 of hoverPoints in obj: " + transform.name);
                return;
            }

            Transform parent = hoverPoints[0].point.parent;

            hoverPointSettings hoverCopy = hoverPoints[0];

            hoverPoints = new hoverPointSettings[hoverPoints[0].point.parent.childCount];

            for (int i = 0; i < hoverPoints.Length; i++) //paste settings from 1 to others, andd after assighn apropporiate child
            {
                hoverPoints[i] = hoverCopy;
                hoverPoints[i].point = parent.GetChild(i);
            }

            return;
        }

        [ContextMenu("rigidbodySetup")]
        public void rigidbodySetup()
        {
            rb = GetComponent<Rigidbody>();

            rb.drag = 0f;
            rb.angularDrag = 3.6f;
            rb.mass = 25;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.centerOfMass = center.localPosition;
        }

        public void setup(Transform center, Transform forward, Transform groundDetector, Transform hoverPoint0, LayerMask ground, bool useExternalInput)
        {
            this.center = center;
            this.forwardVector = forward;
            this.groundDetector = groundDetector;

            groundLayer = ground;

            rigidbodySetup();

            if (hoverPoint0 != null)
            {
                hoverPoints = new hoverPointSettings[1];
                hoverPoints[0] = new hoverPointSettings();
                hoverPoints[0].point = hoverPoint0;
                hoverPoints[0].liftForce = 25000;
                hoverPoints[0].levelingForce = 1000;
                hoverPoints[0].levelingOffset = 0.3f;
                findHoverPointFromPoint0();
                setUpHoverPoint();
            }

            if (useExternalInput) inputType = InputT.external;
            else inputType = InputT.axes;
        }

        public void set0HoverPoint(Transform point)
        {
            if (hoverPoints == null) hoverPoints = new hoverPointSettings[1];
            if (hoverPoints.Length == 0) hoverPoints = new hoverPointSettings[1];

            hoverPoints[0].point = point;

            findHoverPointFromPoint0();
        }

        #endregion

        #region classes

        [System.Serializable]
        public struct hoverPointSettings
        {

            [Tooltip("position of the hoverpoint")]
            public Transform point;

            [Tooltip("force used to lift the veicle on this point")]
            public float liftForce;

            [Tooltip("the forceMode of force used to lift the veicle")]
            public ForceMode liftForceMode;

            [Tooltip("the conditions for the vehicle to start leveling (proces of making v3 up to vehicle up)")]
            public lw WhenToLevel;

            [Tooltip("the force of leveling")]
            public float levelingForce;

            [Tooltip("the ofset from the point up, for better leveling")]
            public float levelingOffset;

            [Tooltip("the tolerance of diffrent height when leveling")]
            public float levelingHeightDiffrenceTolerance;

            [Tooltip("forceMode used for leveling")]
            public ForceMode levelForceMode;

            public enum lw { whenThisNotGrounded, WhenVeicleGroundedAndThisNot, always };
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            RaycastHit info;


            for (int i = 0; i < hoverPoints.Length; i++)
            {
                Gizmos.color = Color.cyan;
                Vector3 pos = hoverPoints[i].point.position;
                Gizmos.DrawSphere(pos, 0.1f);

                Vector3 down = liftTypeSpace == liftType.world ? Vector3.down : -transform.up;


                if (Physics.Raycast(pos, down, out info, hoverHeight, groundLayer, QueryTriggerInteraction.Ignore))
                {
                    Gradient gradient = new Gradient();

                    GradientColorKey[] k = { new GradientColorKey(Color.red, 0), new GradientColorKey(new Color32(255, 215, 0, 1), 0.8f), new GradientColorKey(Color.cyan, 1f) };
                    gradient.colorKeys = k;

                    Gizmos.color = gradient.Evaluate(Vector3.Distance(info.point, pos) / hoverHeight);

                    Gizmos.DrawLine(hoverPoints[i].point.position, info.point);
                    Gizmos.DrawSphere(info.point, 0.1f);
                }
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(groundDetector.position, 0.3f);

            if (groundCheckType != groundCheckT.worldDown && Physics.Raycast(groundDetector.position, -groundDetector.up, out info, groundDetectionDistance, groundLayer))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(groundDetector.position, info.point);
                Gizmos.DrawSphere(info.point, 0.25f);
                Vector3 withNormal = info.point + info.normal;
                Gizmos.DrawLine(info.point, withNormal);
                Gizmos.DrawSphere(withNormal, 0.15f);
            }
            else if (groundCheckType != groundCheckT.detectorDown && Physics.Raycast(groundDetector.position, Vector3.down, out info, groundDetectionDistance, groundLayer))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(groundDetector.position, info.point);
                Gizmos.DrawSphere(info.point, 0.25f);
                Vector3 withNormal = info.point + info.normal;
                Gizmos.DrawLine(info.point, withNormal);
                Gizmos.DrawSphere(withNormal, 0.15f);
            }
        }

#endif
    }
}
