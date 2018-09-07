//Author: Fahim Ahmed

using Unity.Mathematics;
using Unity.Entities;

namespace PureECS.Forcefield {

    //Pure ECS always deals with struct, not class.
    //IComponentData is a pure ECS-style component, meaning that it defines no behavior,
    //only data.IComponentDatas are structs rather than classes
    //
    //This is a template struct of a single boid/cube
    public struct Boid : IComponentData {

        public float3 position;
        public float3 velocity { get; set; }
        public float3 acceration;

        public float mass { get; set; }
        public float radius { get; set; }

        //clamp the max velocity magnitude
        public float maxLength { get; set; }
    }
}