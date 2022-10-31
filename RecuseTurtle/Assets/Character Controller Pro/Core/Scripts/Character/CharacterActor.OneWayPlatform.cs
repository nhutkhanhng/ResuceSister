using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lightbug.Utilities;

namespace Lightbug.CharacterControllerPro.Core
{
    public partial class CharacterActor
    {
        [Header("On Way Platform")]

        [Tooltip("One way platforms are objects that can be contacted by the character feet (bottom sphere) while descending.")]
        public LayerMask oneWayPlatformsLayerMask = 0;

        [Tooltip("This value defines (in degrees) the total arc used by the one way platform detection algorithm (using the bottom part of the capsule). " +
        "the angle is measured between the up direction and the segment formed by the contact point and the character bottom center (capsule). " +
        "\nArc = 180 degrees ---> contact point = any point on the bottom sphere." +
        "\nArc = 0 degrees ---> contact point = bottom most point")]
        [Range(0f, 179f)]
        public float oneWayPlatformsValidArc = 175f;


        /// <summary>
        /// Returns true if the current ground layer is considered as a one way platform.
        /// </summary>
        public bool IsGroundAOneWayPlatform => CustomUtilities.BelongsToLayerMask(GroundObject.layer, oneWayPlatformsLayerMask);

        public bool CheckOneWayPlatformLayerMask(CollisionInfo collisionInfo)
        {
            int collisionLayer = collisionInfo.hitInfo.layer;
            return CustomUtilities.BelongsToLayerMask(collisionLayer, oneWayPlatformsLayerMask);
        }

        public bool CheckOneWayPlatformCollision(Vector3 contactPoint, Vector3 characterPosition)
        {
            Vector3 contactPointToBottom = GetBottomCenter(characterPosition) - contactPoint;

            float collisionAngle = Is2D ? Vector2.Angle(Up, contactPointToBottom) : Vector3.Angle(Up, contactPointToBottom);
            return collisionAngle <= 0.5f * oneWayPlatformsValidArc;
        }

    }
}