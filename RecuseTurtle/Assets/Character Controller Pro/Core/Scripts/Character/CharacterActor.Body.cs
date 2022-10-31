using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lightbug.Utilities;

namespace Lightbug.CharacterControllerPro.Core
{
    public partial class CharacterActor
    {
        [Header("Size")]

        [Tooltip("This field determines a fixed point (top, center or bottom) that will be used as a reference during size changes. " +
   "For instance, by using \"top\" as a reference, the character will shrink/grow my modifying the bottom part of the body (the top part will not move)." +
   "\n\nImportant: For a \"grounded\" character only the \"bottom\" reference is available.")]
        public SizeReferenceType sizeReferenceType = SizeReferenceType.Bottom;
        public enum SizeReferenceType
        {
            Top,
            Center,
            Bottom
        }


        [Min(0f)]
        public float sizeLerpSpeed = 8f;

        Vector2 targetBodySize;

        #region public Body properties
        /// <summary>
        /// Gets the current body size (width and height).
        /// </summary>
        public Vector2 BodySize { get; private set; }

        /// <summary>
        /// Gets the current body size (width and height).
        /// </summary>
        public Vector2 DefaultBodySize => CharacterBody.BodySize;

        /// <summary>
        /// Gets the center of the collision shape.
        /// </summary>
        public Vector3 Center
        {
            get
            {
                return GetCenter(Position);
            }
        }

        /// <summary>
        /// Gets the center of the collision shape.
        /// </summary>
        public Vector3 Top
        {
            get
            {
                return GetTop(Position);
            }
        }

        /// <summary>
        /// Gets the center of the collision shape.
        /// </summary>
        public Vector3 Bottom
        {
            get
            {
                return GetBottom(Position);
            }
        }

        /// <summary>
        /// Gets the center of the collision shape.
        /// </summary>
        public Vector3 TopCenter
        {
            get
            {
                return GetTopCenter(Position);
            }
        }

        /// <summary>
        /// Gets the center of the collision shape.
        /// </summary>
        public Vector3 BottomCenter
        {
            get
            {
                return GetBottomCenter(Position, 0f);
            }
        }

        /// <summary>
        /// Gets the center of the collision shape.
        /// </summary>
        public Vector3 OffsettedBottomCenter
        {
            get
            {
                return GetBottomCenter(Position, StepOffset);
            }
        }

        #endregion

        #region Body functions

        /// <summary>
        /// Gets the center of the collision shape.
        /// </summary>
        public Vector3 GetCenter(Vector3 position)
        {
            return position + CustomUtilities.Multiply(Up, BodySize.y / 2f);
        }

        /// <summary>
        /// Gets the top most point of the collision shape.
        /// </summary>
        public Vector3 GetTop(Vector3 position)
        {
            return position + CustomUtilities.Multiply(Up, BodySize.y - CharacterConstants.SkinWidth);
        }

        /// <summary>
        /// Gets the bottom most point of the collision shape.
        /// </summary>
        public Vector3 GetBottom(Vector3 position)
        {
            return position + CustomUtilities.Multiply(Up, CharacterConstants.SkinWidth);
        }

        /// <summary>
        /// Gets the center of the top sphere of the collision shape.
        /// </summary>
        public Vector3 GetTopCenter(Vector3 position)
        {
            return position + CustomUtilities.Multiply(Up, BodySize.y - BodySize.x / 2f);
        }

        /// <summary>
        /// Gets the center of the top sphere of the collision shape (considering an arbitrary body size).
        /// </summary>
        public Vector3 GetTopCenter(Vector3 position, Vector2 bodySize)
        {
            return position + CustomUtilities.Multiply(Up, bodySize.y - bodySize.x / 2f);
        }

        /// <summary>
        /// Gets the center of the bottom sphere of the collision shape.
        /// </summary>
        public Vector3 GetBottomCenter(Vector3 position, float bottomOffset = 0f)
        {
            return position + CustomUtilities.Multiply(Up, BodySize.x / 2f + bottomOffset);
        }


        /// <summary>
        /// Gets the center of the bottom sphere of the collision shape (considering an arbitrary body size).
        /// </summary>
        public Vector3 GetBottomCenter(Vector3 position, Vector2 bodySize)
        {
            return position + CustomUtilities.Multiply(Up, bodySize.x / 2f);
        }

        /// <summary>
        /// Gets the a vector that goes from the bottom center to the top center (topCenter - bottomCenter).
        /// </summary>
        public Vector3 GetBottomCenterToTopCenter()
        {
            return CustomUtilities.Multiply(Up, BodySize.y - BodySize.x);
        }

        /// <summary>
        /// Gets the a vector that goes from the bottom center to the top center (topCenter - bottomCenter).
        /// </summary>
        public Vector3 GetBottomCenterToTopCenter(Vector2 bodySize)
        {
            return CustomUtilities.Multiply(Up, bodySize.y - bodySize.x);
        }


        #endregion

        void HandleSize(float dt)
        {
            Vector2 previousBodySize = BodySize;
            BodySize = Vector2.Lerp(BodySize, targetBodySize, sizeLerpSpeed * dt);

            SetColliderSize(!IsStable);

            if (!IsGrounded)
            {
                float verticalOffset = 0f;

                switch (sizeReferenceType)
                {
                    case SizeReferenceType.Top:
                        verticalOffset = Mathf.Abs(previousBodySize.y - BodySize.y);
                        break;
                    case SizeReferenceType.Center:
                        verticalOffset = Mathf.Abs(previousBodySize.y - BodySize.y) / 2f;
                        break;
                    case SizeReferenceType.Bottom:
                        verticalOffset = 0f;
                        break;
                }


                if (previousBodySize.y != BodySize.y)
                    RigidbodyComponent.Position += CustomUtilities.Multiply(Vector3.up, verticalOffset);

            }



        }


        /// <summary>
        /// Applies a force at the ground contact point, in the direction of the weight (mass times gravity).
        /// </summary>
        protected virtual void ApplyWeight(Vector3 contactPoint)
        {
            if (!applyWeightToGround)
                return;


            if (Is2D)
            {
                if (GroundCollider2D == null)
                    return;

                if (GroundCollider2D.attachedRigidbody == null)
                    return;

                GroundCollider2D.attachedRigidbody.AddForceAtPosition(CustomUtilities.Multiply(-Up, CharacterBody.Mass, weightGravity), contactPoint);
            }
            else
            {
                if (GroundCollider3D == null)
                    return;

                if (GroundCollider3D.attachedRigidbody == null)
                    return;


                GroundCollider3D.attachedRigidbody.AddForceAtPosition(CustomUtilities.Multiply(-Up, CharacterBody.Mass, weightGravity), contactPoint);
            }


        }



        /// <summary>
        /// Checks if the new character size fits in place. If this check is valid then the real size of the character is changed.
        /// </summary>
        public bool SetBodySize(Vector2 size)
        {

            HitInfoFilter filter = new HitInfoFilter(
                ObstaclesWithoutOWPLayerMask,
                true,
                true
            );

            if (!characterCollisions.CheckBodySize(size, Position, in filter))
                return false;

            targetBodySize = size;

            return true;
        }

    }
}
