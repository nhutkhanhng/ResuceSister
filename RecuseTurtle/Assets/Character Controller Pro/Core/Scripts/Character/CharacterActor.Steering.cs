using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lightbug.Utilities;

namespace Lightbug.CharacterControllerPro.Core
{
    public partial class CharacterActor
    {


        public bool IsKinematic
        {
            get
            {
                return RigidbodyComponent.IsKinematic;
            }
            set
            {
                RigidbodyComponent.IsKinematic = value;
            }
        }


        /// <summary>
        /// Gets/Sets the current rigidbody position. This action will produce an "interpolation reset", meaning that (visually) the object will move instantly to the target.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return RigidbodyComponent.Position;
            }
            set
            {
                RigidbodyComponent.Position = value;


                ResetInterpolationPosition();
            }
        }

        /// <summary>
        /// Gets/Sets the current rigidbody rotation. This action will produce an "interpolation reset", meaning that (visually) the object will rotate instantly to the target.
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                return RigidbodyComponent.Rotation;
            }
            set
            {
                RigidbodyComponent.Rotation = value;

                ResetInterpolationRotation();
            }
        }

        /// <summary>
        /// Gets/Sets the current up direction based on the rigidbody rotation (not necessarily transform.up).
        /// </summary>
        public Vector3 Up
        {
            get
            {
                return RigidbodyComponent.Up;
            }
            set
            {
                if (value == Vector3.zero)
                    return;

                value.Normalize();
                Quaternion deltaRotation = Quaternion.FromToRotation(Up, value);

                RotateInternal(deltaRotation);
            }
        }

        Vector3 forward2D = Vector3.right;

        /// <summary>
        /// Gets/Sets the current forward direction based on the rigidbody rotation (not necessarily transform.forward).
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                return Is2D ? forward2D : RigidbodyComponent.Rotation * Vector3.forward;
            }
            set
            {

                if (value == Vector3.zero)
                    return;

                if (Is2D)
                {
                    forward2D = Vector3.Project(value, Right);
                    forward2D.Normalize();
                }
                else
                {
                    // If the up direction is fixed, then make sure the rotation is 100% yaw (up axis).
                    if (constraintRotation)
                    {
                        float signedAngle = Vector3.SignedAngle(Forward, value, Up);
                        Quaternion deltaRotation = Quaternion.AngleAxis(signedAngle, Up);
                        RotateInternal(deltaRotation);
                    }
                    else
                    {
                        Quaternion deltaRotation = Quaternion.FromToRotation(Forward, value);
                        RotateInternal(deltaRotation);
                    }



                }


            }
        }

        /// <summary>
        /// Gets the current up direction based on the rigidbody rotation (not necessarily transform.right).
        /// </summary>
        public Vector3 Right
        {
            get
            {
                return RigidbodyComponent.Rotation * Vector3.right;
            }
        }

    }

}