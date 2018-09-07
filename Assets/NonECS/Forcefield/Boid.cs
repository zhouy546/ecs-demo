using System.Collections.Generic;
using UnityEngine;

namespace NonECS.Forcefield {

    struct Bound {
        public float x;
        public float y;
        public float w;
        public float h;
    }

    public class Boid {
        public Vector3 position;
        public Vector3 velocity { get; set; }

        public float mass { get; set; }
        public float radius { get; set; }

        public float maxLength { get; set; }

        Vector3 lastPos;
        Vector3 acceration;

        //x, y, w, h
        Bound bound;

        public Boid(Vector3 position, float radius, float mass) {
            this.position = position;
            this.radius = radius;
            this.mass = mass;
        }

        public void Update() {
            //add acceleration
            velocity += acceration;

            //clip max velocity
            if (velocity.magnitude > maxLength) {
                velocity = velocity.normalized;
                velocity *= maxLength;
            }


            //update position based on velocity
            position += velocity;

            //reset acceleration
            acceration *= 0;
        }

        public void ApplyForce(Vector3 force) {            
            //apply push (F = ma)
            acceration = acceration + (force / mass);
        }

        //x, y, w, h
        public void SetBound(Vector4 bound) {
            this.bound.x = bound.x;
            this.bound.y = bound.y;
            this.bound.w = bound.z;
            this.bound.h = bound.w;
        }

        public void Stop() {
            velocity *= 0;
        }

        public void CheckEdge() {
            if (bound.w == 0) {
                Debug.LogError("Bound not set.");
                return;
            }

            if (position.x > bound.w) {
                position.x = 0;
                lastPos.x = 0;
            }
            else if (position.x < bound.x) {
                position.x = bound.w;
                lastPos.x = bound.w;
            }

            if (position.y > bound.h) {
                position.y = 0;
                lastPos.y = 0;
            }
            else if (position.y < bound.y) {
                position.y = bound.h;
                lastPos.y = bound.h;
            }
        }

        public void EdgeBounce() {
            
            if(bound.w == 0) {
                Debug.LogError("Bound not set.");
                return;
            }

            if (position.x > bound.w) {
                var v = velocity;
                v.x *= -1;
                velocity = v;
            }
            else if (position.x < 0) {
                var v = velocity;
                v.x *= -1;
                velocity = v;
            }

            if (position.y > bound.h) {
                var v = velocity;
                v.y *= -1;
                velocity = v;
            }
            else if (position.y < 0) {
                var v = velocity;
                v.y *= -1;
                velocity = v;
            }
        }
    }
}