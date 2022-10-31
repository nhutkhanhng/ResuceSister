using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lightbug.Utilities;

namespace Lightbug.CharacterControllerPro.Core
{

    public enum CharacterActorState
    {
        NotGrounded,
        StableGrounded,
        UnstableGrounded
    }


    /// <summary>
    /// This class represents a character actor. It contains all the character information, collision flags, collision events, and so on. It also responsible for the execution order 
    /// of everything related to the character, such as movement, rotation, teleportation, rigidbodies interactions, body size, etc. Since the character can be 2D or 3D, this abstract class must be implemented in the 
    /// two formats, one for 2D and one for 3D.
    /// </summary>
    [AddComponentMenu("Character Controller Pro/Core/Character Actor")]
    [RequireComponent(typeof(CharacterBody))]
    [DefaultExecutionOrder(ExecutionOrder.CharacterActorOrder)]
    public partial class CharacterActor : PhysicsActor
    {
        [Space(10f)]

        [Tooltip("Objects NOT represented by this layer mask will be considered by the character as \"unstable objects\". " +
        "If you don't want to define unstable layers select \"Everything\" (default value).")]
        public LayerMask stableLayerMask = -1;

        [Tooltip("If the character is stable, the ground slope angle must be less than or equal to this value in order to remain \"stable\". " +
        "The angle is calculated using the \"ground stable normal\".")]
        [Range(1f, 89f)]
        public float slopeLimit = 55f;


        [Tooltip("Situation: The character makes contact with the ground and detect a stable edge." +
        "\n\n True: the character will enter stable state regardless of the collision contact angle.\n" +
        "\n\n False: the character will use the contact angle instead (contact normal) in order to determine stability (<= slopeLimit).")]
        public bool useStableEdgeWhenLanding = true;

        [Tooltip("The offset distance applied to the bottom of the character. A higher offset means more walkable surfaces")]
        [Min(0f)]
        public float stepUpDistance = 0.5f;

        [Tooltip("The distance the character is capable of detecting ground. Use this value to clamp (or not) the character to the ground.")]
        [Min(0f)]
        public float stepDownDistance = 0.5f;

        [Space(10f)]


        [Tooltip("With this enabled the character bottom sphere (capsule) will be simulated as a cylinder. This works only when the character is standing on an edge.")]
        public bool edgeCompensation = false;


        [Tooltip("This value can be used to limit the maximum amount of velocity while climbing an unstable slope (collide and slide algorithm). " +
        "This velocity reduction is applied when the slope angle is between slopeLimit and MaxUnstableUpwardsAngle (CharacterConstants). " +
        "\n\nUsage: When the character acceleration is too high (e.g. tight platformer), the character might climb unstable slopes very easily. " +
        "Use this value to reduce the amount of velocity transferred to the slope.")]
        public float maxUnstableUpwardsVelocity = 7f;

        [Tooltip("This will prevent the character from stepping over an unstable surface (a \"bad\" step). This requires a bit more processing, so if your character does not need this level of precision " +
        "you can disable it.")]
        public bool preventBadSteps = true;


        [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
        [Tooltip("The forward direction of the character will be affected by the rotation of the ground (only yaw motion allowed).")]
        public bool rotateForwardDirection = true;

        [Space(10f)]

        [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
        [Tooltip("This is the maximum ground velocity delta (from the previous frame to the current one) tolerated by the character." +
        "\n\nIf the ground accelerates too much, then the character will stop moving with it." + "\n\nImportant: This does not apply to one way platforms.")]
        public float maxGroundVelocityChange = 30f;

        [Space(8f)]

        [UnityEngine.Serialization.FormerlySerializedAs("maxForceNotGroundedGroundVelocity")]
        [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
        [Tooltip("When the character becomes \"not grounded\" (after a ForceNotGrounded call) part of the ground velocity can be transferred to its own velocity. " +
        "This value represents the minimum planar velocity required.")]
        public float inheritedGroundPlanarVelocityThreshold = 2f;

        [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
        [Tooltip("When the character becomes \"not grounded\" (after a ForceNotGrounded call) part of the ground velocity can be transferred to its own velocity. " +
        "This value represents how much of the planar component is utilized.")]
        public float inheritedGroundPlanarVelocityMultiplier = 1f;

        [Space(8f)]
        [UnityEngine.Serialization.FormerlySerializedAs("maxForceNotGroundedGroundVelocity")]
        [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
        [Tooltip("When the character becomes \"not grounded\" (after a ForceNotGrounded call) part of the ground velocity can be transferred to its own velocity. " +
        "This value represents the minimum vertical velocity required.")]
        public float inheritedGroundVerticalVelocityThreshold = 2f;

        [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
        [Tooltip("When the character becomes \"not grounded\" (after a ForceNotGrounded call) part of the ground velocity can be transferred to its own velocity. " +
        "This value represents how much of the planar component is utilized.")]
        public float inheritedGroundVerticalVelocityMultiplier = 1f;

        [Header("Velocity")]

        [Tooltip("Whether or not to project the initial velocity (stable) onto walls.")]
        [SerializeField]
        public bool slideOnWalls = true;










        [Header("Physics")]

        public bool canPushDynamicRigidbodies = true;

        [Condition("canPushDynamicRigidbodies", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
        public LayerMask pushableRigidbodyLayerMask = -1;

        public bool applyWeightToGround = true;

        [Condition("applyWeightToGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
        public float weightGravity = CharacterConstants.DefaultGravity;




        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────



        public float StepOffset
        {
            get
            {
                return stepUpDistance - BodySize.x / 2f;
            }
        }


        public void OnValidate()
        {
            if (CharacterBody == null)
                CharacterBody = GetComponent<CharacterBody>();

            stepUpDistance = Mathf.Clamp(
                stepUpDistance,
                CharacterConstants.ColliderMinBottomOffset + CharacterBody.BodySize.x / 2f,
                CharacterBody.BodySize.y - CharacterBody.BodySize.x / 2f
            );

            CustomUtilities.SetPositive(ref maxGroundVelocityChange);
            CustomUtilities.SetPositive(ref inheritedGroundPlanarVelocityThreshold);
            CustomUtilities.SetPositive(ref inheritedGroundPlanarVelocityMultiplier);
            CustomUtilities.SetPositive(ref inheritedGroundVerticalVelocityThreshold);
            CustomUtilities.SetPositive(ref inheritedGroundVerticalVelocityMultiplier);
            // CustomUtilities.SetMax( ref unstableToStableSlopeLimit , slopeLimit );

        }

        /// <summary>
        /// Sets up root motion for this actor.
        /// </summary>
        public void SetUpRootMotion(
            bool updateRootPosition = true,
            bool updateRootRotation = true
        )
        {
            UseRootMotion = true;
            UpdateRootPosition = updateRootPosition;
            UpdateRootRotation = updateRootRotation;
        }

        /// <summary>
        /// Sets up root motion for this actor.
        /// </summary>
        public void SetUpRootMotion(
            bool updateRootPosition = true,
            RootMotionVelocityType rootMotionVelocityType = RootMotionVelocityType.SetVelocity,
            bool updateRootRotation = true,
            RootMotionRotationType rootMotionRotationType = RootMotionRotationType.AddRotation
        )
        {
            UseRootMotion = true;
            UpdateRootPosition = updateRootPosition;
            this.rootMotionVelocityType = rootMotionVelocityType;
            UpdateRootRotation = updateRootRotation;
            this.rootMotionRotationType = rootMotionRotationType;

        }


        /// <summary>
        /// Gets the CharacterBody component associated with this character actor.
        /// </summary>
        public bool Is2D => RigidbodyComponent.Is2D;

        /// <summary>
        /// Gets the RigidbodyComponent component associated with the character.
        /// </summary>
        public override RigidbodyComponent RigidbodyComponent => CharacterBody.RigidbodyComponent;

        /// <summary>
        /// Gets the ColliderComponent component associated with the character.
        /// </summary>
        public ColliderComponent ColliderComponent => CharacterBody.ColliderComponent;

        /// <summary>
        /// Gets the physics component from the character.
        /// </summary>
        public PhysicsComponent PhysicsComponent { get; private set; }

        /// <summary>
        /// Gets the CharacterBody component associated with this character actor.
        /// </summary>
        public CharacterBody CharacterBody { get; private set; }

        /// <summary>
        /// Returns the current character actor state. This enum variable contains the information about the grounded and stable state, all in one.
        /// </summary>
        public CharacterActorState CurrentState
        {
            get
            {
                if (IsGrounded)
                    return IsStable ? CharacterActorState.StableGrounded : CharacterActorState.UnstableGrounded;
                else
                    return CharacterActorState.NotGrounded;
            }
        }

        /// <summary>
        /// Returns the character actor state from the previous frame.
        /// </summary>
        public CharacterActorState PreviousState
        {
            get
            {
                if (WasGrounded)
                    return WasStable ? CharacterActorState.StableGrounded : CharacterActorState.UnstableGrounded;
                else
                    return CharacterActorState.NotGrounded;
            }
        }

        #region Collision Properties

        public LayerMask ObstaclesLayerMask => PhysicsComponent.CollisionLayerMask | oneWayPlatformsLayerMask;
        public LayerMask ObstaclesWithoutOWPLayerMask => PhysicsComponent.CollisionLayerMask & ~(oneWayPlatformsLayerMask);

        /// <summary>
        /// Returns true if the character is standing on an edge.
        /// </summary>
        public bool IsOnEdge => characterCollisionInfo.isOnEdge;

        /// <summary>
        /// Returns the angle between the both sides of the edge.
        /// </summary>
        public float EdgeAngle => characterCollisionInfo.edgeAngle;




        // Wall ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────	

        /// <summary>
        /// Gets the wall collision flag, true if the character hit a wall, false otherwise.
        /// </summary>
        public bool WallCollision => characterCollisionInfo.wallCollision;


        /// <summary>
        /// Gets the angle between the contact normal (wall collision) and the Up direction.
        /// </summary>	
        public float WallAngle => characterCollisionInfo.wallAngle;


        /// <summary>
        /// Gets the current contact (wall collision).
        /// </summary>
        public Contact WallContact => characterCollisionInfo.wallContact;


        // Head ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────	

        /// <summary>
        /// Gets the head collision flag, true if the character hits something with its head, false otherwise.
        /// </summary>
        public bool HeadCollision => characterCollisionInfo.headCollision;


        /// <summary>
        /// Gets the angle between the contact normal (head collision) and the Up direction.
        /// </summary>
        public float HeadAngle => characterCollisionInfo.headAngle;


        /// <summary>
        /// Gets the current contact (head collision).
        /// </summary>
        public Contact HeadContact => characterCollisionInfo.headContact;


        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the current stability state of the character. Stability is equal to "grounded + slope angle <= slope limit".
        /// </summary>
        public bool IsStable
        {
            get
            {
                if (!IsGrounded)
                    return false;

                if (!IsStableLayer(characterCollisionInfo.groundLayer))
                    return false;

                if (WasStable)
                {
                    // If the character is stable, then use the groundSlopeAngle (updated in ProbeGround).
                    return characterCollisionInfo.groundSlopeAngle <= slopeLimit;
                }
                else
                {
                    if (useStableEdgeWhenLanding)
                    {
                        return characterCollisionInfo.groundSlopeAngle <= slopeLimit;
                    }
                    else
                    {
                        // If the character was not stable, then define stability by using the contact normal, instead of the "stable" normal.
                        float contactSlopeAngle = Vector3.Angle(Up, characterCollisionInfo.groundContactNormal);
                        return contactSlopeAngle <= slopeLimit;
                    }

                }
            }
        }



        /// <summary>
        /// Returns a concatenated string containing all the current collision information.
        /// </summary>
        public override string ToString()
        {
            const string nullString = " ---- ";


            string triggerString = "";

            for (int i = 0; i < Triggers.Count; i++)
            {
                triggerString += " - " + Triggers[i].gameObject.name + "\n";
            }

            return string.Concat(
                "Ground : \n",
                "──────────────────\n",
                "Is Grounded : ", IsGrounded, '\n',
                "Is Stable : ", IsStable, '\n',
                "Slope Angle : ", characterCollisionInfo.groundSlopeAngle, '\n',
                "Is On Edge : ", characterCollisionInfo.isOnEdge, '\n',
                "Edge Angle : ", characterCollisionInfo.edgeAngle, '\n',
                "Object Name : ", characterCollisionInfo.groundObject != null ? characterCollisionInfo.groundObject.name : nullString, '\n',
                "Layer : ", LayerMask.LayerToName(characterCollisionInfo.groundLayer), '\n',
                "Rigidbody Type: ", GroundRigidbodyComponent != null ? GroundRigidbodyComponent.IsKinematic ? "Kinematic" : "Dynamic" : nullString, '\n',
                "Dynamic Ground : ", GroundRigidbodyComponent != null ? "Yes" : "No", "\n\n",
                "Wall : \n",
                "──────────────────\n",
                "Wall Collision : ", characterCollisionInfo.wallCollision, '\n',
                "Wall Angle : ", characterCollisionInfo.wallAngle, "\n\n",
                "Head : \n",
                "──────────────────\n",
                "Head Collision : ", characterCollisionInfo.headCollision, '\n',
                "Head Angle : ", characterCollisionInfo.headAngle, "\n\n",
                "Triggers : \n",
                "──────────────────\n",
                "Current : ", CurrentTrigger.gameObject != null ? CurrentTrigger.gameObject.name : nullString, '\n',
                triggerString
            );
        }

        #endregion

        protected CharacterCollisionInfo characterCollisionInfo = new CharacterCollisionInfo();

        /// <summary>
        /// Gets a structure with all the information regarding character collisions. Most of the character properties (e.g. IsGrounded, IsStable, GroundObject, and so on)
        /// can be obtained from this structure.
        /// </summary>
        public CharacterCollisionInfo CharacterCollisionInfo => characterCollisionInfo;


        CharacterCollisions characterCollisions = new CharacterCollisions();

        public CharacterCollisions CharacterCollisions => characterCollisions;


        // GameObject groundTriggerObject = null;

        protected override void Awake()
        {

            base.Awake();


            CharacterBody = GetComponent<CharacterBody>();
            targetBodySize = CharacterBody.BodySize;
            BodySize = targetBodySize;

            if (Is2D)
                PhysicsComponent = gameObject.AddComponent<PhysicsComponent2D>();
            else
                PhysicsComponent = gameObject.AddComponent<PhysicsComponent3D>();


            RigidbodyComponent.IsKinematic = false;
            RigidbodyComponent.UseGravity = false;
            RigidbodyComponent.Mass = CharacterBody.Mass;
            RigidbodyComponent.LinearDrag = 0f;
            RigidbodyComponent.AngularDrag = 0f;
            RigidbodyComponent.Constraints = RigidbodyConstraints.FreezeRotation;


            characterCollisions.Initialize(this, PhysicsComponent);

            // Ground trigger
            if (Is2D)
            {
                groundTriggerCollider2D = gameObject.AddComponent<CircleCollider2D>();
                groundTriggerCollider2D.hideFlags = HideFlags.NotEditable;
                groundTriggerCollider2D.isTrigger = true;
                groundTriggerCollider2D.radius = BodySize.x / 2f;
                groundTriggerCollider2D.offset = Vector2.up * (BodySize.x / 2f - CharacterConstants.GroundTriggerOffset);

                Physics2D.IgnoreCollision(GetComponent<CapsuleCollider2D>(), groundTriggerCollider2D, true);

            }
            else
            {
                groundTriggerCollider3D = gameObject.AddComponent<SphereCollider>();
                groundTriggerCollider3D.hideFlags = HideFlags.NotEditable;
                groundTriggerCollider3D.isTrigger = true;
                groundTriggerCollider3D.radius = BodySize.x / 2f;
                groundTriggerCollider3D.center = Vector3.up * (BodySize.x / 2f - CharacterConstants.GroundTriggerOffset);

                Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), groundTriggerCollider3D, true);
            }

        }

        CircleCollider2D groundTriggerCollider2D = null;
        SphereCollider groundTriggerCollider3D = null;

        protected override void Start()
        {
            base.Start();

            HitInfoFilter filter = new HitInfoFilter(
                ObstaclesLayerMask,
                false,
                true,
                oneWayPlatformsLayerMask
            );

            // Initial OWP check
            characterCollisions.CheckOverlapWithLayerMask(
                Position,
                0f,
                in filter
            );

            // Initial "Force Grounded"
            if (forceGroundedAtStart && !alwaysNotGrounded)
                ForceGrounded();

            SetColliderSize(!IsStable);

            forward2D = transform.right;



        }


        protected override void OnEnable()
        {
            base.OnEnable();

            OnTeleport += OnTeleportMethod;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            OnTeleport -= OnTeleportMethod;
        }


        void OnTeleportMethod(Vector3 position, Quaternion rotation)
        {
            Velocity = Vector3.zero;
        }



        // -------------------------------------------------------------------------------------------------------


        void SetColliderSize(bool fullSize)
        {

            float verticalOffset = fullSize ? 0f : Mathf.Max(StepOffset, CharacterConstants.ColliderMinBottomOffset);

            float radius = BodySize.x / 2f;
            float height = BodySize.y - verticalOffset;

            ColliderComponent.Size = new Vector2(2f * radius, height);
            ColliderComponent.Offset = CustomUtilities.Multiply(Vector2.up, verticalOffset + height / 2f);
        }


        /// <summary>
        /// Rotates the character doing yaw rotation (around its vertical axis).
        /// </summary>
        /// <param name="angle">The angle in degrees.</param>
        public void SetYaw(float angle)
        {
            Forward = Quaternion.AngleAxis(angle, Up) * Forward;
        }


        // Contacts ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets a list with all the current contacts.
        /// </summary>
        public List<Contact> Contacts
        {
            get
            {
                if (PhysicsComponent == null)
                    return null;

                return PhysicsComponent.Contacts;
            }
        }



        // Triggers ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the most recent trigger.
        /// </summary>
        public Trigger CurrentTrigger
        {
            get
            {
                if (PhysicsComponent.Triggers.Count == 0)
                    return new Trigger();   // "Null trigger"

                return PhysicsComponent.Triggers[PhysicsComponent.Triggers.Count - 1];
            }
        }

        /// <summary>
        /// Gets a list with all the triggers.
        /// </summary>
        public List<Trigger> Triggers
        {
            get
            {
                return PhysicsComponent.Triggers;
            }
        }




   
        List<Contact> wallContacts = new List<Contact>(10);

        /// <summary>
        /// Returns a lits of all the contacts involved with wall collision events.
        /// </summary>
        public List<Contact> WallContacts => wallContacts;


        List<Contact> headContacts = new List<Contact>(10);

        /// <summary>
        /// Returns a lits of all the contacts involved with head collision events.
        /// </summary>
        public List<Contact> HeadContacts => headContacts;

        List<Contact> groundContacts = new List<Contact>(10);

        /// <summary>
        /// Returns a lits of all the contacts involved with head collision events.
        /// </summary>
        public List<Contact> GroundContacts => groundContacts;

        void GetContactsInformation()
        {
            // bool groundRigidbodyHitFlag = false;
            bool wasCollidingWithWall = characterCollisionInfo.wallCollision;
            bool wasCollidingWithHead = characterCollisionInfo.headCollision;

            groundContacts.Clear();
            wallContacts.Clear();
            headContacts.Clear();

            for (int i = 0; i < Contacts.Count; i++)
            {
                Contact contact = Contacts[i];

                float verticalAngle = Vector3.Angle(Up, contact.normal);

                // Get the wall collision information -------------------------------------------------------------			
                if (CustomUtilities.isCloseTo(verticalAngle, 90f, CharacterConstants.WallContactAngleTolerance))
                    wallContacts.Add(contact);


                // Get the head collision information -----------------------------------------------------------------
                if (verticalAngle >= CharacterConstants.HeadContactMinAngle)
                    headContacts.Add(contact);

                if (verticalAngle <= 89f)
                    groundContacts.Add(contact);


            }


            if (wallContacts.Count == 0)
            {
                characterCollisionInfo.ResetWallInfo();
            }
            else
            {
                Contact wallContact = wallContacts[0];

                characterCollisionInfo.SetWallInfo(in wallContact, this);

                if (!wasCollidingWithWall)
                {
                    if (OnWallHit != null)
                        OnWallHit(wallContact);
                }

            }


            if (headContacts.Count == 0)
            {
                characterCollisionInfo.ResetHeadInfo();
            }
            else
            {
                Contact headContact = headContacts[0];

                characterCollisionInfo.SetHeadInfo(in headContact, this);

                if (!wasCollidingWithHead)
                {
                    if (OnHeadHit != null)
                        OnHeadHit(headContact);

                }
            }


        }


        #region Events



        /// <summary>
        /// This event is called when the character hits its head (not grounded).
        /// 
        /// The related collision information struct is passed as an argument.
        /// </summary>
        public event System.Action<Contact> OnHeadHit;

        /// <summary>
        /// This event is called everytime the character is blocked by an unallowed geometry, this could be
        /// a wall or a steep slope (depending on the "slopeLimit" value).
        /// 
        /// The related collision information struct is passed as an argument.
        /// </summary>
        public event System.Action<Contact> OnWallHit;

        /// <summary>
        /// This event is called everytime the character teleports.
        /// 
        /// The teleported position and rotation are passed as arguments.
        /// </summary>
        public event System.Action<Vector3, Quaternion> OnTeleport;

        /// <summary>
        /// This event is called when the character enters the grounded state.
        /// 
        /// The local linear velocity is passed as an argument.
        /// </summary>
        public event System.Action<Vector3> OnGroundedStateEnter;

        /// <summary>
        /// This event is called when the character exits the grounded state.
        /// </summary>
        public event System.Action OnGroundedStateExit;

        /// <summary>
        /// This event is called when the character make contact with a new ground (object).
        /// </summary>
        public event System.Action OnNewGroundEnter;

        #endregion


        /// <summary>
        /// Sets the teleportation position and rotation using an external Transform reference. 
        /// The character will move/rotate internally using its own internal logic.
        /// </summary>
        public void Teleport(Transform reference)
        {
            Teleport(reference.position, reference.rotation);

        }

        /// <summary>
        /// Sets the teleportation position and rotation. 
        /// The character will move/rotate internally using its own internal logic.
        /// </summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;

            if (OnTeleport != null)
                OnTeleport(Position, Rotation);
        }

        /// <summary>
        /// Sets the teleportation position. 
        /// The character will move/rotate internally using its own internal logic.
        /// </summary>
        public void Teleport(Vector3 position)
        {
            Position = position;

            if (OnTeleport != null)
                OnTeleport(Position, Rotation);

        }



#if UNITY_TERRAIN_MODULE

        /// <summary>
        /// Gets the current terrain the character is standing on.
        /// </summary>
        public Terrain CurrentTerrain { get; private set; }

        /// <summary>
        /// Returns true if the character is standing on a terrain.
        /// </summary>
        public bool IsOnTerrain => CurrentTerrain != null;

#endif


        bool IsAllowedToFollowRigidbodyReference => IsStable && supportDynamicGround && IsGroundARigidbody;

        protected override void UpdateDynamicRootMotionPosition(Vector3 deltaPosition)
        {
            Vector3 rootMotionVelocity = deltaPosition / Time.deltaTime;

            switch (rootMotionVelocityType)
            {
                case RootMotionVelocityType.SetVelocity:
                    Velocity = rootMotionVelocity;
                    break;
                case RootMotionVelocityType.SetPlanarVelocity:
                    PlanarVelocity = rootMotionVelocity;
                    break;
                case RootMotionVelocityType.SetVerticalVelocity:
                    VerticalVelocity = rootMotionVelocity;
                    break;
            }
        }

        /// <summary>
        /// Sweeps the body from its current position (CharacterActor.Position) towards the desired destination using the "collide and slide" algorithm. 
        /// At the end, the character will be moved to a valid position. Triggers and one way platforms will be ignored.
        /// </summary>
        public void SweepAndTeleport(Vector3 destination)
        {
            HitInfoFilter filter = new HitInfoFilter(ObstaclesWithoutOWPLayerMask, false, true);
            SweepAndTeleport(destination, in filter);
        }

        /// <summary>
        /// Sweeps the body from its current position (CharacterActor.Position) towards the desired destination using the "collide and slide" algorithm. 
        /// At the end, the character will be moved to a valid position. 
        /// </summary>
        public void SweepAndTeleport(Vector3 destination, in HitInfoFilter filter)
        {
            Vector3 displacement = destination - Position;
            CollisionInfo collisionInfo = characterCollisions.CastBody(
                Position,
                displacement,
                0f,
                in filter
            );

            Position += collisionInfo.displacement;
        }

        bool IsStableLayer(int layer)
        {
            return CustomUtilities.BelongsToLayerMask(layer, stableLayerMask);
        }

 
        bool IsAnUnstableEdge(in CollisionInfo collisionInfo)
        {
            return collisionInfo.isAnEdge && collisionInfo.edgeUpperSlopeAngle > slopeLimit;
        }

        protected void StableCollideAndSlide(ref Vector3 position, Vector3 displacement, bool useFullBody)
        {

            Vector3 groundPlaneNormal = GroundStableNormal;
            Vector3 slidingPlaneNormal = Vector3.zero;

            HitInfoFilter filter = new HitInfoFilter(
                ObstaclesLayerMask,
                false,
                true,
                oneWayPlatformsLayerMask
            );

            int iteration = 0;


            while (iteration < CharacterConstants.MaxSlideIterations)
            {
                iteration++;

                CollisionInfo collisionInfo = characterCollisions.CastBody(
                    position,
                    displacement,
                    useFullBody ? 0f : StepOffset,
                    in filter
                );


                if (collisionInfo.hitInfo.hit)
                {

                    if (CheckOneWayPlatformLayerMask(collisionInfo))
                    {
                        position += displacement;
                        break;
                    }

                    // Physics interaction ---------------------------------------------------------------------------------------
                    if (canPushDynamicRigidbodies)
                    {

                        if (collisionInfo.hitInfo.IsRigidbody)
                        {
                            if (collisionInfo.hitInfo.IsDynamicRigidbody)
                            {
                                bool belongsToGroundRigidbody = false;

                                if (Is2D)
                                {
                                    if (GroundCollider2D != null)
                                        if (GroundCollider2D.attachedRigidbody != null)
                                            if (GroundCollider2D.attachedRigidbody != collisionInfo.hitInfo.rigidbody2D)
                                                belongsToGroundRigidbody = true;
                                }
                                else
                                {
                                    if (GroundCollider3D != null)
                                        if (GroundCollider3D.attachedRigidbody != null)
                                            if (GroundCollider3D.attachedRigidbody == collisionInfo.hitInfo.rigidbody3D)
                                                belongsToGroundRigidbody = true;
                                }


                                if (!belongsToGroundRigidbody)
                                {


                                    bool canPushThisObject = CustomUtilities.BelongsToLayerMask(collisionInfo.hitInfo.layer, pushableRigidbodyLayerMask);
                                    if (canPushThisObject)
                                    {
                                        // Use the entire displacement and stop the collide and slide.
                                        position += displacement;
                                        break;
                                    }
                                }
                            }

                        }
                    }

                    //-----------------------------------------------------------------------------------------------------------


                    if (slideOnWalls && !Is2D)
                    {
                        position += collisionInfo.displacement;
                        displacement -= collisionInfo.displacement;

                        bool blocked = UpdateCollideAndSlideData(
                            collisionInfo,
                            ref slidingPlaneNormal,
                            ref groundPlaneNormal,
                            ref displacement
                        );
                    }
                    else
                    {
                        if (!WallCollision)
                            position += collisionInfo.displacement;

                        break;
                    }



                }
                else
                {
                    position += displacement;
                    break;
                }

            }

        }


        protected void PostSimulationCollideAndSlide(ref Vector3 position, ref Quaternion rotation, Vector3 displacement, bool useFullBody)
        {

            Vector3 groundPlaneNormal = GroundStableNormal;
            Vector3 slidingPlaneNormal = Vector3.zero;

            HitInfoFilter filter = new HitInfoFilter(
                ObstaclesLayerMask,
                false,
                true,
                oneWayPlatformsLayerMask
            );

            int iteration = 0;


            while (iteration < CharacterConstants.MaxPostSimulationSlideIterations)
            {
                iteration++;

                CollisionInfo collisionInfo = characterCollisions.CastBody(
                    position,
                    displacement,
                    useFullBody ? 0f : StepOffset,
                    in filter
                );


                if (collisionInfo.hitInfo.hit)
                {
                    // If it hits something then reset the rotation.
                    rotation = Rotation;

                    if (CheckOneWayPlatformLayerMask(collisionInfo))
                    {
                        position += displacement;
                        break;
                    }

                    //-----------------------------------------------------------------------------------------------------------

                    if (slideOnWalls && !Is2D)
                    {
                        position += collisionInfo.displacement;
                        displacement -= collisionInfo.displacement;

                        // Get the new slide plane.
                        bool blocked = UpdateCollideAndSlideData(
                            collisionInfo,
                            ref slidingPlaneNormal,
                            ref groundPlaneNormal,
                            ref displacement
                        );
                    }
                    else
                    {
                        if (!WallCollision)
                            position += collisionInfo.displacement;

                        break;
                    }


                }
                else
                {
                    position += displacement;
                    break;
                }

            }

        }

        protected void UnstableCollideAndSlide(ref Vector3 position, Vector3 displacement, float dt)
        {

            HitInfoFilter filter = new HitInfoFilter(
                ObstaclesLayerMask,
                false,
                true,
                oneWayPlatformsLayerMask
            );


            int iteration = 0;

            // Used to determine if the character should collide (or not) with the OWP.
            bool isValidOWP = false;

            bool bottomCollision = false;

            Vector3 slidePlaneANormal = Vector3.zero;
            Vector3 slidePlaneBNormal = Vector3.zero;

            while (iteration < CharacterConstants.MaxSlideIterations || displacement == Vector3.zero)
            {
                iteration++;


                CollisionInfo collisionInfo = characterCollisions.CastBody(
                    position,
                    displacement,
                    0f,
                    in filter
                );

                if (collisionInfo.hitInfo.hit)
                {
                    float slopeAngle = Vector3.Angle(Up, collisionInfo.hitInfo.normal);
                    bottomCollision = slopeAngle < 90f;

                    if (CheckOneWayPlatformLayerMask(collisionInfo))
                    {
                        // Check if the character hits this platform with the bottom part of the capsule
                        Vector3 nextPosition = position + collisionInfo.displacement;

                        isValidOWP = CheckOneWayPlatformCollision(collisionInfo.hitInfo.point, nextPosition);

                        if (!isValidOWP)
                        {
                            position += displacement;
                            break;
                        }
                    }

                    if (canPushDynamicRigidbodies)
                    {
                        if (collisionInfo.hitInfo.IsRigidbody)
                        {
                            if (collisionInfo.hitInfo.IsDynamicRigidbody)
                            {
                                bool canPushThisObject = CustomUtilities.BelongsToLayerMask(collisionInfo.hitInfo.layer, pushableRigidbodyLayerMask);
                                if (canPushThisObject)
                                {
                                    position += displacement;
                                    break;
                                }
                            }

                        }
                    }

                    // Fall back to this
                    position += collisionInfo.displacement;
                    displacement -= collisionInfo.displacement;

                    // Determine the displacement vector and store the slide plane A
                    if (slidePlaneANormal == Vector3.zero)
                    {
                        // New displacement
                        displacement = Vector3.ProjectOnPlane(displacement, collisionInfo.hitInfo.normal);

                        // store the slide plane A
                        slidePlaneANormal = collisionInfo.hitInfo.normal;

                        // ----------------------------------------------------------------------------
                        // Determine if the remaining displacement needs to be limited.
                        bool validAngleRange = slopeAngle > slopeLimit && slopeAngle < CharacterConstants.MaxUnstableUpwardsAngle;

                        if (validAngleRange && !WasGrounded && bottomCollision)
                        {
                            // See if the displacement vertical component is positive...
                            bool isUpwardsDisplacement = transform.InverseTransformVectorUnscaled(Vector3.Project(displacement, Up)).y > 0f;

                            // ...If so, then reduce the displacement magnitude in order to prevent the character from climbing unstable slopes.
                            if (isUpwardsDisplacement)
                            {
                                float maxUnstableUpwardsDisplacementMagnitude = maxUnstableUpwardsVelocity * dt;

                                float remainingVelocity = displacement.magnitude / dt;

                                if (remainingVelocity > maxUnstableUpwardsVelocity)
                                    displacement = Vector3.ClampMagnitude(displacement, maxUnstableUpwardsDisplacementMagnitude);
                            }


                        }
                        // ----------------------------------------------------------------------------	

                    }
                    else if (slidePlaneBNormal == Vector3.zero)
                    {

                        slidePlaneBNormal = collisionInfo.hitInfo.normal;
                        Vector3 displacementDirection = Vector3.Cross(slidePlaneANormal, slidePlaneBNormal);
                        displacementDirection.Normalize();

                        displacement = Vector3.Project(displacement, displacementDirection);

                    }

                }
                else
                {
                    position += displacement;
                    break;
                }

            }

            UnstableProbeGround(position, isValidOWP, dt);

        }


        bool UpdateCollideAndSlideData(CollisionInfo collisionInfo, ref Vector3 slidingPlaneNormal, ref Vector3 groundPlaneNormal, ref Vector3 displacement)
        {

            Vector3 normal = collisionInfo.hitInfo.normal;

            if (collisionInfo.contactSlopeAngle > slopeLimit || !IsStableLayer(collisionInfo.hitInfo.layer))
            {

                if (slidingPlaneNormal != Vector3.zero)
                {
                    bool acuteAngleBetweenWalls = Vector3.Dot(normal, slidingPlaneNormal) > 0f;

                    if (acuteAngleBetweenWalls)
                        displacement = CustomUtilities.DeflectVector(displacement, groundPlaneNormal, normal);
                    else
                        displacement = Vector3.zero;

                }
                else
                {
                    displacement = CustomUtilities.DeflectVector(displacement, groundPlaneNormal, normal);
                }

                slidingPlaneNormal = normal;
            }
            else
            {
                displacement = CustomUtilities.ProjectOnTangent(
                    displacement,
                    normal,
                    Up
                );

                groundPlaneNormal = normal;
                slidingPlaneNormal = Vector3.zero;

            }

            return displacement == Vector3.zero;
        }


        void OnDrawGizmos()
        {
            if (CharacterBody == null)
                CharacterBody = GetComponent<CharacterBody>();

            Gizmos.color = new Color(1f, 1f, 1f, 0.2f);

            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 origin = CustomUtilities.Multiply(Vector3.up, stepUpDistance);
            Gizmos.DrawWireCube(
                origin,
                new Vector3(1.1f * CharacterBody.BodySize.x, 0.02f, 1.1f * CharacterBody.BodySize.x)
            );

            Gizmos.matrix = Matrix4x4.identity;

        }


    }


}