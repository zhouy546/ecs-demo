//Author: Fahim Ahmed

using Unity.Mathematics;
using Unity.Entities;

namespace PureECS.Forcefield {

    //Pure ECS always deals with struct, not class.
    //IComponentData is a pure ECS-style component, meaning that it defines no behavior,
    //only data.IComponentDatas are structs rather than classes
    public struct Forcefield : IComponentData {
        public enum ForceMode {
            PUSH,
            PULL
        }

        //this is basically the mass of your mouse pointer / forcefield
        public const float G = 9.8f;

        //friction level. how fast the velocity will fall.
        public float frictionCoe;

        //mass of the forcefield / pointer
        public float Mass;

        //x, y, w, h
        public float4 bound;

        //apply force on boids
        public float3 CastForce(ref float3 position, ref Boid b, ForceMode forceMode) {            
            float3 forceDir = (position - b.position);
            float d = math.length(forceDir);
            d = math.clamp(d, 1, 25);
            forceDir = math.normalize(forceDir);

            // F = GMm / d^2
            float strength = (G * Mass * b.mass) / (d * d);

            forceDir *= strength;

            return (forceMode == ForceMode.PUSH) ? forceDir * -1 : forceDir;
        }
    }
}