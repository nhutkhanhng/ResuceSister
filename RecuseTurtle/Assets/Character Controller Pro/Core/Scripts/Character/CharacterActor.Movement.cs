using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lightbug.Utilities;

namespace Lightbug.CharacterControllerPro.Core
{
    public partial class CharacterActor
    {
        public enum CharacterVelocityMode
        {
            UseInputVelocity,
            UsePreSimulationVelocity,
            UsePostSimulationVelocity
        }


        [Tooltip("Should the actor re-assign the rigidbody velocity after the simulation is done?\n\n" +
        "PreSimulationVelocity: the character uses the velocity prior to the simulation (modified by this component).\nPostSimulationVelocity: the character uses the velocity received from the simulation (no re-assignment).\nInputVelocity: the character \"gets back\" its initial velocity (before being modified by this component).")]
        public CharacterVelocityMode stablePostSimulationVelocity = CharacterVelocityMode.UsePostSimulationVelocity;

        [Tooltip("Should the actor re-assign the rigidbody velocity after the simulation is done?\n\n" +
        "PreSimulationVelocity: the character uses the velocity prior to the simulation (modified by this component).\nPostSimulationVelocity: the character uses the velocity received from the simulation (no re-assignment).\nInputVelocity: the character \"gets back\" its initial velocity (before being modified by this component).")]
        public CharacterVelocityMode unstablePostSimulationVelocity = CharacterVelocityMode.UsePostSimulationVelocity;




        [Header("Rotation")]

        [Tooltip("Should this component define the character \"Up\" direction?")]
        public bool constraintRotation = true;

        [Condition("constraintRotation", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
        public Transform upDirectionReference = null;

        [Condition(
            new string[] { "constraintRotation", "upDirectionReference" },
            new ConditionAttribute.ConditionType[] { ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.ConditionType.IsNotNull },
            new float[] { 0f, 0f },
            ConditionAttribute.VisibilityType.Hidden)]
        public VerticalAlignmentSettings.VerticalReferenceMode upDirectionReferenceMode = VerticalAlignmentSettings.VerticalReferenceMode.Away;

        [Condition(
            new string[] { "constraintRotation", "upDirectionReference" },
            new ConditionAttribute.ConditionType[] { ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.ConditionType.IsNull },
            new float[] { 0f, 0f },
            ConditionAttribute.VisibilityType.Hidden)]
        [Tooltip("The desired up direction.")]
        public Vector3 constraintUpDirection = Vector3.up;


        /// <summary>
        /// Gets the character velocity vector (Velocity) assigned prior to the FixedUpdate call. This is also known as the "input" velocity, 
        /// since it is the value the user has specified.
        /// </summary>
        public Vector3 InputVelocity { get; private set; }

        /// <summary>
        /// Gets a velocity vector which is the input velocity modified, based on the character actor internal rules (step up, slope limit, etc). 
        /// This velocity corresponds to the one used by the physics simulation.
        /// </summary>
        public Vector3 PreSimulationVelocity { get; private set; }

        /// <summary>
        /// Gets the character velocity as the result of the Physics simulation.
        /// </summary>
        public Vector3 PostSimulationVelocity { get; private set; }

        /// <summary>
        /// Gets the difference between the post-simulation velocity (after the physics simulation) and the pre-simulation velocity (just before the physics simulation). 
        /// This value is useful to detect any external response due to the physics simulation, such as hits coming from other rigidbodies.
        /// </summary>
        public Vector3 ExternalVelocity { get; private set; }

        // Gets/Sets the rigidbody velocity.
        public Vector3 Velocity
        {
            get
            {
                return RigidbodyComponent.Velocity;
            }
            set
            {
                RigidbodyComponent.Velocity = value;
            }
        }


        /// <summary>
        /// Gets/Sets the rigidbody local velocity.
        /// </summary>
        public Vector3 LocalVelocity
        {
            get
            {
                return transform.InverseTransformVectorUnscaled(Velocity);
            }
            set
            {
                Velocity = transform.TransformVectorUnscaled(value);
            }
        }

        /// <summary>
        /// Gets/Sets the rigidbody velocity projected onto a plane formed by its up direction.
        /// </summary>
        public Vector3 PlanarVelocity
        {
            get
            {
                return Vector3.ProjectOnPlane(Velocity, Up);
            }
            set
            {
                Velocity = Vector3.ProjectOnPlane(value, Up) + VerticalVelocity;
            }
        }

        /// <summary>
        /// Gets/Sets the rigidbody velocity projected onto its up direction.
        /// </summary>
        public Vector3 VerticalVelocity
        {
            get
            {
                return Vector3.Project(Velocity, Up);
            }
            set
            {
                Velocity = PlanarVelocity + Vector3.Project(value, Up);
            }
        }

        /// <summary>
        /// Gets/Sets the rigidbody velocity projected onto a plane formed by its up direction.
        /// </summary>
        public Vector3 StableVelocity
        {
            get
            {
                return CustomUtilities.ProjectOnTangent(Velocity, GroundStableNormal, Up);
            }
            set
            {
                Velocity = CustomUtilities.ProjectOnTangent(value, GroundStableNormal, Up);
            }
        }


        public Vector3 LastGroundedVelocity { get; private set; }

        /// <summary>
        /// Gets/Sets the rigidbody local planar velocity.
        /// </summary>
        public Vector3 LocalPlanarVelocity
        {
            get
            {
                return transform.InverseTransformVectorUnscaled(PlanarVelocity);
            }
            set
            {
                PlanarVelocity = transform.TransformVectorUnscaled(value);
            }
        }



    }


    public partial class CharacterActor
    {

        /// <summary>
        /// Sets the rigidbody velocity based on a target position. The same can be achieved by setting the velocity value manually.
        /// </summary>
        public void Move(Vector3 position)
        {
            RigidbodyComponent.Move(position);
        }


        void HandlePosition(float dt)
        {

            Vector3 position = Position;

            if (alwaysNotGrounded)
                ForceNotGrounded();

            if (IsKinematic)
                return;

            if (IsStable)
            {
                ApplyWeight(GroundContactPoint);

                VerticalVelocity = Vector3.zero;

                Vector3 displacement = CustomUtilities.ProjectOnTangent(
                    CustomUtilities.Multiply(Velocity, dt),
                    GroundStableNormal,
                    Up
                );

                StableCollideAndSlide(ref position, displacement, false);

                SetDynamicGroundData(position);

                if (!IsStable)
                {

#if UNITY_TERRAIN_MODULE
                    CurrentTerrain = null;
#endif
                    groundRigidbodyComponent = null;
                }

            }
            else
            {

                ProcessInheritedVelocity();

                Vector3 displacement = CustomUtilities.Multiply(Velocity, dt);
                UnstableCollideAndSlide(ref position, displacement, dt);

                SetDynamicGroundData(position);

            }

            Move(position);
        }


        void RotateInternal(Quaternion deltaRotation)
        {

            Vector3 preRotationCenter = IsGrounded ? GetBottomCenter(Position) : GetCenter(Position);

            RigidbodyComponent.Rotation = deltaRotation * RigidbodyComponent.Rotation;

            Vector3 postRotationCenter = IsGrounded ? GetBottomCenter(Position) : GetCenter(Position);

            RigidbodyComponent.Position += preRotationCenter - postRotationCenter;
        }

        Vector3 preSimulationPosition = default(Vector3);

        protected override void PreSimulationUpdate(float dt)
        {
            PhysicsComponent.ClearContacts();

            if (!forceNotGroundedFlag)
            {
                WasGrounded = IsGrounded;
                WasStable = IsStable;
            }

            InputVelocity = Velocity;

            HandleSize(dt);
            HandlePosition(dt);

            PreSimulationVelocity = Velocity;
            preSimulationPosition = Position;

            // ------------------------------------------------------------

            if (IsStable)
            {
                StableElapsedTime += dt;
                UnstableElapsedTime = 0f;
            }
            else
            {
                StableElapsedTime = 0f;
                UnstableElapsedTime += dt;
            }

            if (IsGrounded)
            {
                NotGroundedTime = 0f;
                GroundedTime += dt;

                LastGroundedVelocity = Velocity;

                if (!WasGrounded)
                    if (OnGroundedStateEnter != null)
                        OnGroundedStateEnter(LocalVelocity);

            }
            else
            {
                NotGroundedTime += dt;
                GroundedTime = 0f;

                if (WasGrounded)
                    if (OnGroundedStateExit != null)
                        OnGroundedStateExit();

            }

            if (forceNotGroundedFrames != 0)
                forceNotGroundedFrames--;



            // Enable/Disable the ground trigger.
            if (Is2D)
                groundTriggerCollider2D.enabled = useGroundTrigger;
            else
                groundTriggerCollider3D.enabled = useGroundTrigger;

            forceNotGroundedFlag = false;
        }
        protected override void PostSimulationUpdate(float dt)
        {
            HandleRotation(dt);

            GetContactsInformation();

            PostSimulationVelocity = Velocity;
            ExternalVelocity = PostSimulationVelocity - PreSimulationVelocity;


            if (IsStable && !IsKinematic)
            {
                ProcessDynamicGroundMovement(dt);


                PreGroundProbingPosition = Position;

                ProbeGround(dt);

                PostGroundProbingPosition = Position;
                GroundProbingDisplacement = Position - PreGroundProbingPosition;
            }
            else
            {
                GroundProbingDisplacement = Vector3.zero;
            }

            // Velocity assignment ------------------------------------------------------
            SetPostSimulationVelocity();
        }

        void ProcessInheritedVelocity()
        {
            if (!forceNotGroundedFlag)
                return;

            // "local" to the character
            Vector3 localGroundVelocity = transform.InverseTransformVectorUnscaled(GroundVelocity);
            Vector3 planarGroundVelocity = Vector3.ProjectOnPlane(GroundVelocity, Up);
            Vector3 verticalGroundVelocity = Vector3.Project(GroundVelocity, Up);

            Vector3 inheritedGroundVelocity = Vector3.zero;

            if (planarGroundVelocity.magnitude >= inheritedGroundPlanarVelocityThreshold)
                inheritedGroundVelocity += CustomUtilities.Multiply(planarGroundVelocity, inheritedGroundPlanarVelocityMultiplier);

            if (verticalGroundVelocity.magnitude >= inheritedGroundVerticalVelocityThreshold)
            {

                // This prevents an edge case where the character is unable to jump (descending platform)
                if (LocalVelocity.y > -localGroundVelocity.y)
                    inheritedGroundVelocity += CustomUtilities.Multiply(verticalGroundVelocity, inheritedGroundVerticalVelocityMultiplier);
            }


            Velocity += inheritedGroundVelocity;

            GroundVelocity = Vector3.zero;
            PreviousGroundVelocity = Vector3.zero;

        }


        void SetPostSimulationVelocity()
        {
            if (IsStable)
            {

                switch (stablePostSimulationVelocity)
                {
                    case CharacterVelocityMode.UseInputVelocity:

                        Velocity = InputVelocity;

                        break;
                    case CharacterVelocityMode.UsePreSimulationVelocity:

                        Velocity = PreSimulationVelocity;

                        // Take the rigidbody velocity and convert it into planar velocity
                        if (WasStable)
                            PlanarVelocity = CustomUtilities.Multiply(Vector3.Normalize(PlanarVelocity), Velocity.magnitude);

                        break;
                    case CharacterVelocityMode.UsePostSimulationVelocity:

                        // Take the rigidbody velocity and convert it into planar velocity
                        if (WasStable)
                            PlanarVelocity = CustomUtilities.Multiply(Vector3.Normalize(PlanarVelocity), Velocity.magnitude);

                        break;
                }


            }
            else
            {
                switch (unstablePostSimulationVelocity)
                {
                    case CharacterVelocityMode.UseInputVelocity:

                        Velocity = InputVelocity;

                        break;
                    case CharacterVelocityMode.UsePreSimulationVelocity:

                        Velocity = PreSimulationVelocity;

                        break;
                    case CharacterVelocityMode.UsePostSimulationVelocity:

                        break;
                }
            }
        }


        void HandleRotation(float dt)
        {

            if (!constraintRotation)
                return;

            if (upDirectionReference != null)
            {
                Vector3 targetPosition = Position + CustomUtilities.Multiply(Velocity, dt);
                float sign = upDirectionReferenceMode == VerticalAlignmentSettings.VerticalReferenceMode.Towards ? 1f : -1f;

                constraintUpDirection = CustomUtilities.Multiply(Vector3.Normalize(upDirectionReference.position - targetPosition), sign);

            }

            Up = constraintUpDirection;

        }



    }
}
