using UnityEngine;

namespace NonECS.Forcefield {
    public class Forcefield {
        public enum ForceMode {
            PUSH,
            PULL
        }

        public int minRange = 75;
        public int maxRange = 100;

        public const float G = 9.8f;

        float Mass;
        float radius;

        Vector3 position;
        Vector3 velocity;
        Vector3 acceration;

        //mass of attractor
        public Forcefield(float mass) {
            this.Mass = mass;
        }

        public Vector3 CastForce(Vector3 position, Boid b, ForceMode forceMode) {
            this.position = position;

            Vector3 forceDir = position - b.position;
            float d = forceDir.magnitude;
            d = Mathf.Clamp(d, 1, 25);
            forceDir = forceDir.normalized;

            // F = GMm / d^2
            float strength = (G * Mass * b.mass) / (d * d);

            forceDir *= strength;

            return (forceMode == ForceMode.PUSH) ? forceDir * -1 : forceDir;
        }

        public bool InRange(Vector3 target) {
            Vector3 a = target;
            Vector3 b = position;
            float dist = (a - b).magnitude;

            if (dist > minRange && dist < maxRange) {
                return true;
            }

            return false;
        }
    }
}