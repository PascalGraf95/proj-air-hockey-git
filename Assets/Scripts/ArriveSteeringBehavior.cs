using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class ArriveSteeringBehavior
    {
        /// <summary>
        /// Returns the steering for a Character so it arrives at the target
        /// </summary>
        public Vector3 Arrive(Vector3 targetPosition, Character Character, float targetRadius, float slowDownRadius, float maxSpeed, float maxAcceleration, float timeToTarget)
        {
            Debug.DrawLine(Character.Position, targetPosition, Color.red, 0f, false);

            // get the direction to the target
            Vector3 targetVelocity = targetPosition - Character.Position;

            // Get the distance to the target
            float distance = targetVelocity.magnitude;

            // check if we have arrived
            if (distance < targetRadius)
            {
                Character.Velocity = Vector3.zero;
                return Vector3.zero;
            }

            // if we are outside the slowRadius, then go max speed
            float targetSpeed;
            if (distance > slowDownRadius)
            {
                targetSpeed = maxSpeed;
            }
            else
            {
                targetSpeed = maxSpeed * (distance / slowDownRadius);
            }

            // Give targetVelocity the correct speed
            targetVelocity.Normalize();
            targetVelocity *= targetSpeed;

            // Calculate the linear acceleration we want
            Vector3 acceleration = targetVelocity - Character.Velocity;
            /* Rather than accelerate the Character to the correct speed in 1 second, 
             * accelerate so we reach the desired speed in timeToTarget seconds 
             * (if we were to actually accelerate for the full timeToTarget seconds). */
            acceleration *= 1 / timeToTarget;

            // Make sure we are accelerating at max acceleration
            if (acceleration.magnitude > maxAcceleration)
            {
                acceleration.Normalize();
                acceleration *= maxAcceleration;
            }

            // add pusher y offset
            acceleration.y = 0.01f;

            return acceleration;
        }
    }
}
