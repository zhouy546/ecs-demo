//Author: Fahim Ahmed

using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace PureECS.Forcefield {

    //Bootstrap or manager class
    public class Boot : MonoBehaviour {

        //shape and material for boids. In this example, I used cube and standard material.
        public Mesh _mesh;
        public Material _mat;
        //End

        [Space]
        public float frictionCoefficient = 0.1f;    //friction intensity
        public int numX = 10;                       //number boids / cubes per row
        public int numY = 8;                        //number boids / cubes per column
        public float boidMass = 1;                  //mass of each boid / cube
        public float scale = 1.0f;                  //size of each boid / cube

        [Space]
        public TMPro.TMP_Text info;

        [Space]        
        public float MAX_LENGTH = 20;               //max speed of each boid / cube

        //for simplicity I used static variables for player inputs
        public static float3 mousePosition;
        public static bool isForceOn;
        public static Forcefield.ForceMode forceMode; //force type - push or pull

        //private stuffs
        int numBoid;
        EntityManager entityManager;

        void Start() {
            //Create entity manager, who will manage all the entity.
            //An Entity is basically a Gameobject.
            entityManager = World.Active.GetOrCreateManager<EntityManager>();

            //Generate entities and position them in a grid
            PopulateField();
        }

        public void PopulateField() {
            numBoid = numX * numY;
            info.text = "Number of Cubes: " + numBoid;

            //gap between boids / cubes. offsets.
            float ox = (Camera.main.aspect * Camera.main.orthographicSize * 2.0f) / numX;
            float oy = Camera.main.orthographicSize * 2.0f / numY;

            //update camera position for center the view in grid
            //
            //in orthographic view, camera view size in pixels are,
            //      width = aspect ratio * ortho. size
            //      height = ortho. size
            //
            //      (ox * 0.5f) is margin
            Camera.main.transform.position = new float3(Camera.main.aspect * Camera.main.orthographicSize - ox * 0.5f,
                                                         Camera.main.orthographicSize - oy * 0.5f, -5);
            //loops for grid
            for (int i = 0; i < numX; i++) {
                for (int j = 0; j < numY; j++) {

                    //create an entity and add the necessary components. This is just the structure of the entity.
                    var cubeEntity = entityManager.CreateEntity(
                                    ComponentType.Create<Position>(),               //equivalent to -> [transform.position]
                                    ComponentType.Create<TransformMatrix>(),        //whole transform matrix -> position, rotation, scale
                                    ComponentType.Create<MeshInstanceRenderer>(),   //equivalent to MeshRenderer component
                                    ComponentType.Create<Boid>(),                   //our Boid struct, don't forget to add IComponentData interface
                                    ComponentType.Create<Forcefield>());            //our Forcefield struct, don't forget to add IComponentData interface

                    //position in grid
                    float3 initialPosition = new float3(i * ox, j * oy, 0);

                    //set the value of each component we attached in our entity

                    //for initial position
                    entityManager.SetComponentData(cubeEntity, new Position { Value =  initialPosition});

                    //boid properties
                    Boid b = new Boid {
                        position = initialPosition,
                        radius = 1,
                        mass = boidMass,
                        maxLength = MAX_LENGTH,
                        velocity = Vector3.zero,
                        acceration = Vector3.zero
                    };

                    //set boid value in entity
                    entityManager.SetComponentData(cubeEntity, b);

                    //bound of the forcefield
                    float4 v = new float4(-ox * 0.5f, -oy * 0.5f, Camera.main.aspect * Camera.main.orthographicSize * 2, Camera.main.orthographicSize * 2);

                    //forcefield properties
                    Forcefield f = new Forcefield { Mass = 15, bound = v, frictionCoe = frictionCoefficient };

                    //set forcefield
                    entityManager.SetComponentData(cubeEntity, f);

                    //MeshInstanceRenderer is a SharedComponentData.
                    //IComponentData is appropriate for data that varies between Entities, such as storing a world position. 
                    //ISharedComponentData is useful when many Entities have something in common,
                    //that means, you can use a single mesh renderer for all the entities.
                    entityManager.SetSharedComponentData(cubeEntity, new MeshInstanceRenderer {
                        mesh = _mesh,
                        material = _mat
                    });
                }
            }
        }

        //Purge!
        private void OnDisable() {
            entityManager.GetAllEntities(Allocator.Temp).Dispose();
            //entityManager.DestroyEntity(entityManager.GetAllEntities(Allocator.Temp));
        }
    }
}