using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lightbug.Utilities;

namespace Lightbug.CharacterControllerPro.Core
{
    public partial class CharacterActor
    {
#if UNITY_TERRAIN_MODULE
        Dictionary<Transform, Terrain> terrains = new Dictionary<Transform, Terrain>();
#endif
        Dictionary<Transform, RigidbodyComponent> groundRigidbodyComponents = new Dictionary<Transform, RigidbodyComponent>();

        public RigidbodyComponent GroundRigidbodyComponent
        {
            get
            {
                if (!IsStable)
                    groundRigidbodyComponent = null;

                return groundRigidbodyComponent;
            }
        }

        RigidbodyComponent groundRigidbodyComponent = null;



        [Tooltip("This option will enable a trigger (located at the capsule bottom center) " +
                            "that can be used to generate OnTriggerXXX messages. " +
                        "Normally the character won't generate these messages (OnCollisionXXX/OnTriggerXXX)" +
                            " since the collider is not making direct contact with the ground")]
        public bool useGroundTrigger = false;

        [Header("Grounding")]

        [Tooltip("Prevents the character from enter grounded state (IsGrounded will be false)")]
        public bool alwaysNotGrounded = false;

        [Condition("alwaysNotGrounded", ConditionAttribute.ConditionType.IsFalse)]
        [Tooltip("If enabled the character will do an initial ground check (at \"Start\"). If the test fails the starting state will be \"Not grounded\".")]
        public bool forceGroundedAtStart = true;

        [Header("Unstable ground")]

        [Tooltip("Should the character detect a new (and valid) ground if its vertical velocity is positive?")]
        public bool detectGroundWhileAscending = false;

        /// <summary>
        /// The last vertical displacement calculated by the ground probabing algorithm (PostGroundProbingPosition - PreGroundProbingPosition).
        /// </summary>
        public Vector3 GroundProbingDisplacement { get; private set; }

        /// <summary>
        /// The last rigidbody position prior to the ground probing algorithm.
        /// </summary>
        public Vector3 PreGroundProbingPosition { get; private set; }

        /// <summary>
        /// The last rigidbody position after the ground probing algorithm.
        /// </summary>
        public Vector3 PostGroundProbingPosition { get; private set; }


        /// <summary>
        /// Gets the object below the character (only valid if the character is falling). The maximum prediction distance is defined by the constant "GroundPredictionDistance".
        /// </summary>
        public GameObject PredictedGround { get; private set; }

        /// <summary>
        /// Gets the distance to the "PredictedGround".
        /// </summary>
        public float PredictedGroundDistance { get; private set; }

        // Returns true if the character local vertical velocity is less than zero. 
        public bool IsFalling => LocalVelocity.y < 0f;
        // Returns true if the character local vertical velocity is greater than zero.
        public bool IsAscending => LocalVelocity.y > 0f;

        // Gets the grounded state, true if the ground object is not null, false otherwise.
        public bool IsGrounded => characterCollisionInfo.groundObject != null;

        // Gets the GameObject component of the current ground.
        public GameObject GroundObject => characterCollisionInfo.groundObject;
        // Gets the Transform component of the current ground.
        public Transform GroundTransform => GroundObject != null ? GroundObject.transform : null;

        // Gets the previous grounded state.
        public bool WasGrounded { get; private set; }        

        // Returns true if the character is grounded onto an unstable ground, false otherwise.
        public bool IsOnUnstableGround => IsGrounded && characterCollisionInfo.groundSlopeAngle > slopeLimit;

        // Gets the previous stability state.
        public bool WasStable { get; private set; }

        public float GroundedTime { get; private set; }
        public float NotGroundedTime { get; private set; }

        public float StableElapsedTime { get; private set; }
        public float UnstableElapsedTime { get; private set; }



        // Gets the ground rigidbody position.
        public Vector3 GroundPosition
        {
            get
            {
                return Is2D ?
                new Vector3(
                    GroundCollider2D.attachedRigidbody.position.x,
                    GroundCollider2D.attachedRigidbody.position.y,
                    GroundTransform.position.z
                 ) : GroundCollider3D.attachedRigidbody.position;
            }
        }
        // Gets the ground rigidbody rotation.
        public Quaternion GroundRotation
        {
            get
            {
                return Is2D ? Quaternion.Euler(0f, 0f, GroundCollider2D.attachedRigidbody.rotation) : GroundCollider3D.attachedRigidbody.rotation;
            }
        }


        /// <summary>
        /// Returns true if the current ground is a Rigidbody (2D or 3D), false otherwise.
        /// </summary>
        public bool IsGroundARigidbody
        {
            get
            {
                return Is2D ? characterCollisionInfo.groundRigidbody2D != null : characterCollisionInfo.groundRigidbody3D != null;
            }
        }

        /// <summary>
        /// Returns true if the current ground is a kinematic Rigidbody (2D or 3D), false otherwise.
        /// </summary>
        public bool IsGroundAKinematicRigidbody
        {
            get
            {
                return Is2D ? characterCollisionInfo.groundRigidbody2D.isKinematic : characterCollisionInfo.groundRigidbody3D.isKinematic;
            }
        }

        /// <summary>
        /// Gets the RigidbodyComponent component from the ground.
        /// </summary>

        /// <summary>
        /// Gets the angle between the up vector and the stable normal.
        /// </summary>
        public float GroundSlopeAngle => characterCollisionInfo.groundSlopeAngle;

        /// <summary>
        /// Gets the contact point obtained directly from the ground test (sphere cast).
        /// </summary>
        public Vector3 GroundContactPoint => characterCollisionInfo.groundContactPoint;

        /// <summary>
        /// Gets the normal vector obtained directly from the ground test (sphere cast).
        /// </summary>
        public Vector3 GroundContactNormal => characterCollisionInfo.groundContactNormal;

        /// <summary>
        /// Gets the normal vector used to determine stability. This may or may not be the normal obtained from the ground test.
        /// </summary>
        public Vector3 GroundStableNormal => IsStable ? characterCollisionInfo.groundStableNormal : Up;


        /// <summary>
        /// Gets the Collider2D component of the current ground.
        /// </summary>
        public Collider2D GroundCollider2D => characterCollisionInfo.groundCollider2D;
        /// <summary>
        /// Gets the Collider3D component of the current ground.
        /// </summary>
        public Collider GroundCollider3D => characterCollisionInfo.groundCollider3D;

        /// <summary>
        /// Returns the point velocity (Rigidbody API) of the ground at a given position.
        /// </summary>
        public Vector3 GetGroundPointVelocity(Vector3 position)
        {
            return Is2D ? (Vector3)characterCollisionInfo.groundRigidbody2D.GetPointVelocity(position) : characterCollisionInfo.groundRigidbody3D.GetPointVelocity(position);
        }

        /// <summary>
        /// Gets the velocity of the ground (rigidbody).
        /// </summary>
        public Vector3 GroundVelocity { get; private set; }

        /// <summary>
        /// Gets the previous velocity of the ground (rigidbody).
        /// </summary>
        public Vector3 PreviousGroundVelocity { get; private set; }

        /// <summary>
        /// The ground change in velocity (current velocity - previous velocity).
        /// </summary>
        public Vector3 GroundDeltaVelocity => GroundVelocity - PreviousGroundVelocity;

        /// <summary>
        /// The ground acceleration (GroundDeltaVelocity / dt).
        /// </summary>
        public Vector3 GroundAcceleration => (GroundVelocity - PreviousGroundVelocity) / Time.fixedDeltaTime;


        /// <summary>
        /// Returns true if the ground vertical displacement (moving ground) is positive.
        /// </summary>
        public bool IsGroundAscending => transform.InverseTransformVectorUnscaled(Vector3.Project(CustomUtilities.Multiply(GroundVelocity, Time.deltaTime), Up)).y > 0;
        bool IsAStableEdge(in CollisionInfo collisionInfo)
        {
            return collisionInfo.isAnEdge && collisionInfo.edgeUpperSlopeAngle <= slopeLimit;
        }


        public Vector3 GetGroundSlopeNormal(in CollisionInfo collisionInfo)
        {

#if UNITY_TERRAIN_MODULE
            if (IsOnTerrain)
                return collisionInfo.hitInfo.normal;
#endif

            float contactSlopeAngle = Vector3.Angle(Up, collisionInfo.hitInfo.normal);
            if (collisionInfo.isAnEdge)
            {
                if (contactSlopeAngle < slopeLimit && collisionInfo.edgeUpperSlopeAngle <= slopeLimit && collisionInfo.edgeLowerSlopeAngle <= slopeLimit)
                {
                    return Up;
                }
                else if (collisionInfo.edgeUpperSlopeAngle <= slopeLimit)
                {
                    return collisionInfo.edgeUpperNormal;
                }
                else if (collisionInfo.edgeLowerSlopeAngle <= slopeLimit)
                {
                    return collisionInfo.edgeLowerNormal;
                }
                else
                {
                    return collisionInfo.hitInfo.normal;
                }
            }
            else
            {
                return collisionInfo.hitInfo.normal;
            }



        }

    }


    public partial class CharacterActor
    {

        public bool CanEnterGroundedState => !alwaysNotGrounded && forceNotGroundedFrames == 0;


        bool forceNotGroundedFlag = false;
        int forceNotGroundedFrames = 0;

        /// <summary>
        /// Forces the character to abandon the grounded state (isGrounded = false). 
        /// 
        /// TIP: This is useful when making the character jump.
        /// </summary>
        /// <param name="ignoreGroundContactFrames">The number of FixedUpdate frames to consume in order to prevent the character to 
        /// re-enter grounded state right after a ForceNotGrounded call.</param>
        public void ForceNotGrounded(int ignoreGroundContactFrames = 3)
        {
            forceNotGroundedFrames = ignoreGroundContactFrames;

            WasGrounded = IsGrounded;
            WasStable = IsStable;

            characterCollisionInfo.ResetGroundInfo();

            forceNotGroundedFlag = true;
        }

        /// <summary>
        /// Forces the character to be grounded (isGrounded = true) if possible. The detection distance includes the step down distance.
        /// </summary>
        public void ForceGrounded()
        {
            if (!CanEnterGroundedState)
                return;

            HitInfoFilter filter = new HitInfoFilter(
                ObstaclesLayerMask,
                false,
                true,
                oneWayPlatformsLayerMask
            );

            CollisionInfo collisionInfo = characterCollisions.CheckForGround(
                Position,
                BodySize.y * 0.8f, // 80% of the height
                stepDownDistance,
                filter
            );


            if (collisionInfo.hitInfo.hit)
            {
                ProcessNewGround(collisionInfo.hitInfo.transform, collisionInfo);

                float slopeAngle = Vector3.Angle(Up, GetGroundSlopeNormal(collisionInfo));

                if (slopeAngle <= slopeLimit)
                {
                    // Save the ground collision info
                    characterCollisionInfo.SetGroundInfo(collisionInfo, this, true);
                    Position += collisionInfo.displacement;

                    SetDynamicGroundData(Position);
                }



            }
        }

        void IgnoreGroundCollision()
        {
            for (int i = 0; i < Contacts.Count; i++)
            {
                if (!Contacts[i].isRigidbody)
                    continue;

                if (!Contacts[i].isKinematicRigidbody)
                    continue;

                if (Contacts[i].gameObject.transform == GroundTransform)
                {
                    Velocity = InputVelocity;
                    break;
                }
            }
        }
        void ProbeGround(float dt)
        {
            Vector3 position = Position;

            float groundCheckDistance = edgeCompensation ?
            BodySize.x / 2f + CharacterConstants.GroundCheckDistance :
            CharacterConstants.GroundCheckDistance;

            Vector3 displacement = CustomUtilities.Multiply(-Up, Mathf.Max(groundCheckDistance, stepDownDistance));

            HitInfoFilter filter = new HitInfoFilter(
                ObstaclesLayerMask,
                false,
                true
            );

            CollisionInfo collisionInfo = characterCollisions.CheckForGround(
                position,
                StepOffset,
                stepDownDistance,
                in filter
            );

            if (collisionInfo.hitInfo.hit)
            {
                float slopeAngle = Vector3.Angle(Up, GetGroundSlopeNormal(in collisionInfo));

                if (slopeAngle <= slopeLimit && IsStableLayer(collisionInfo.hitInfo.layer))
                {
                    // Stable hit ---------------------------------------------------				
                    ProcessNewGround(collisionInfo.hitInfo.transform, collisionInfo);

                    // Save the ground collision info
                    characterCollisionInfo.SetGroundInfo(collisionInfo, this, true);

                    position += collisionInfo.displacement;

                    if (edgeCompensation && IsAStableEdge(in collisionInfo))
                    {
                        // calculate the edge compensation and apply that to the final position
                        Vector3 compensation = Vector3.Project((collisionInfo.hitInfo.point - position), Up);
                        position += compensation;
                    }


                }
                else
                {

                    if (preventBadSteps)
                    {

                        if (WasGrounded)
                        {
                            // Restore the initial position and simulate again.
                            Vector3 dynamicGroundDisplacement = CustomUtilities.Multiply(GroundVelocity, dt);
                            Vector3 initialPosition = preSimulationPosition + dynamicGroundDisplacement;
                            position = initialPosition;

                            Vector3 unstableDisplacement = CustomUtilities.ProjectOnTangent(
                                CustomUtilities.Multiply(InputVelocity, dt),
                                GroundStableNormal,
                                Up
                            );
                            // Vector3 unstableDisplacement = Vector3.ProjectOnPlane( CustomUtilities.Multiply( InputVelocity , dt ) , GroundStableNormal );

                            StableCollideAndSlide(ref position, unstableDisplacement, true);

                            // If the body is 2D then redefine velocity.
                            // This eliminates a small "velocity leak" caused by the collide and slide algorithm in 2D.
                            if (Is2D)
                                Velocity = (position - initialPosition) / dt;
                        }
                    }

                    // Re-use the old collisionInfo reference
                    collisionInfo = characterCollisions.CheckForGroundRay(
                        position,
                        filter
                    );

                    ProcessNewGround(collisionInfo.hitInfo.transform, collisionInfo);

                    characterCollisionInfo.SetGroundInfo(collisionInfo, this);

                }



            }
            else
            {
                ForceNotGrounded();

            }

            if (IsStable)
            {
                RigidbodyComponent.Position = position;
            }

        }

    }


    // ============================== DYNAMIC GROUND MOVEMENT ==============================================
    public partial class CharacterActor
    {

        [Space(10f)]
        [Header("Dynamic ground")]

        [Tooltip("Should the character be affected by the movement of the ground?")]
        public bool supportDynamicGround = true;

        Vector3 groundToCharacter;

        Vector3 preSimulationGroundPosition;
        Quaternion preSimulationGroundRotation;




        void ProcessDynamicGroundMovement(float dt)
        {
            if (!supportDynamicGround || !IsGroundARigidbody)
                return;

            // The ground might hit the character really hard (e.g. fast ascending platform), causing this to get some extra velocity (unwanted behaviour). 
            // So, ignore any incoming velocity from the ground by replacing it with the input velocity.
            IgnoreGroundCollision();


            Vector3 targetPosition = Position;
            Quaternion targetRotation = Rotation;

            UpdateDynamicGround(ref targetPosition, ref targetRotation, dt);


            if (!IsGroundAOneWayPlatform && GroundDeltaVelocity.magnitude > maxGroundVelocityChange)
            {
                float upToDynamicGroundVelocityAngle = Vector3.Angle(Vector3.Normalize(GroundVelocity), Up);


                if (upToDynamicGroundVelocityAngle < 45f)
                    ForceNotGrounded();


                Vector3 characterVelocity = PreviousGroundVelocity;

                RigidbodyComponent.Velocity = characterVelocity;
                RigidbodyComponent.Position += CustomUtilities.Multiply(characterVelocity, dt);
                RigidbodyComponent.Rotation = targetRotation;

            }
            else
            {
                Vector3 position = Position;
                PostSimulationCollideAndSlide(ref position, ref targetRotation, targetPosition - position, false);
                RigidbodyComponent.Position = position;
                RigidbodyComponent.Rotation = targetRotation;

            }

        }

        void SetDynamicGroundData(Vector3 position)
        {
            if (IsAllowedToFollowRigidbodyReference)
            {
                preSimulationGroundPosition = GroundPosition;
                preSimulationGroundRotation = GroundRotation;
                groundToCharacter = position - GroundPosition;

                GroundVelocity = PreviousGroundVelocity = GetGroundPointVelocity(position);

            }
            else
            {
                GroundVelocity = Vector3.zero;
                PreviousGroundVelocity = Vector3.zero;
            }

        }

        void UpdateDynamicGround(ref Vector3 position, ref Quaternion rotation, float dt)
        {
            Quaternion deltaRotation = GroundRotation * Quaternion.Inverse(preSimulationGroundRotation);

            Vector3 localGroundToCharacter = GroundTransform.InverseTransformVectorUnscaled(groundToCharacter);
            Vector3 rotatedGroundToCharacter = GroundTransform.rotation * localGroundToCharacter;

            position = GroundPosition + (deltaRotation * groundToCharacter);


            if (!Is2D && rotateForwardDirection)
            {
                // Quaternion deltaRotation = referenceRigidbodyRotation * Quaternion.Inverse( GroundTransform.rotation );
                Vector3 forward = deltaRotation * Forward;
                forward = Vector3.ProjectOnPlane(forward, Up);
                forward.Normalize();

                rotation = Quaternion.LookRotation(forward, Up);
            }


            PreviousGroundVelocity = GroundVelocity;
            GroundVelocity = (position - Position) / dt;

        }

        Vector3 lastPredictedGroundPosition;
        Quaternion lastPredictedGroundRotation;

        void ProcessNewGround(Transform newGroundTransform, CollisionInfo collisionInfo)
        {
            bool isThisANewGround = collisionInfo.hitInfo.transform != GroundTransform;
            if (isThisANewGround)
            {
#if UNITY_TERRAIN_MODULE
                CurrentTerrain = terrains.GetOrRegisterValue<Transform, Terrain>(newGroundTransform);
#endif
                groundRigidbodyComponent = groundRigidbodyComponents.GetOrRegisterValue<Transform, RigidbodyComponent>(newGroundTransform);

                if (OnNewGroundEnter != null)
                    OnNewGroundEnter();

            }
        }

    }


    public partial class CharacterActor
    {

        float unstableGroundContactTime = 0f;

        void UnstableProbeGround(Vector3 position, bool isValidOWP, float dt)
        {
            if (!CanEnterGroundedState)
            {
                unstableGroundContactTime = 0f;
                PredictedGround = null;
                PredictedGroundDistance = 0f;

                characterCollisionInfo.ResetGroundInfo();

                return;
            }

            HitInfoFilter groundCheckFilter = new HitInfoFilter(
                isValidOWP ? ObstaclesLayerMask : ObstaclesWithoutOWPLayerMask,
                false,
                true
            );

            CollisionInfo collisionInfo = characterCollisions.CheckForGround(
                position,
                StepOffset,
                CharacterConstants.GroundPredictionDistance,
                in groundCheckFilter
            );

            if (collisionInfo.hitInfo.hit)
            {
                PredictedGround = collisionInfo.hitInfo.transform.gameObject;
                PredictedGroundDistance = collisionInfo.displacement.magnitude;
                lastPredictedGroundPosition = PredictedGround.transform.position;
                lastPredictedGroundRotation = PredictedGround.transform.rotation;

                bool isPredictedGroundOneWayPlatform = CheckOneWayPlatformLayerMask(collisionInfo);

                if (isPredictedGroundOneWayPlatform)
                    PhysicsComponent.IgnoreCollision(collisionInfo.hitInfo, true);

                bool validForGroundCheck = PredictedGroundDistance <= CharacterConstants.GroundCheckDistance;

                if (validForGroundCheck)
                {
                    unstableGroundContactTime += dt;

                    bool processGround = false;
                    if (detectGroundWhileAscending)
                    {
                        processGround = true;
                    }
                    else
                    {
                        processGround =
                            IsFalling ||
                            unstableGroundContactTime >= CharacterConstants.MaxUnstableGroundContactTime ||
                            collisionInfo.hitInfo.IsRigidbody;
                    }

                    if (processGround)
                    {
                        ProcessNewGround(collisionInfo.hitInfo.transform, collisionInfo);
                        characterCollisionInfo.SetGroundInfo(collisionInfo, this);
                    }


                }
                else
                {
                    unstableGroundContactTime = 0f;
                    characterCollisionInfo.ResetGroundInfo();
                }

            }
            else
            {
                unstableGroundContactTime = 0f;
                PredictedGround = null;
                PredictedGroundDistance = 0f;

                characterCollisionInfo.ResetGroundInfo();

            }
        }

    }


}