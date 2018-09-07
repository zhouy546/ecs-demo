//Author: Fahim Ahmed

using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;

namespace PureECS.Forcefield {

    //A job system puts jobs into a queue to execute. 
    //Worker threads in a job system take items from the job queue and execute them.
    //This is the main happening center. This where you will write behaviours / logics
    public class BoidSystem : JobComponentSystem {
        bool _isForceOn = false;
        Forcefield.ForceMode _forceMode = Forcefield.ForceMode.PULL;
        float3 mousePos;

        //equivalent to Update method from MonoBehaviour.
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            // ------- input handle ------
            //left - pull
            if (Input.GetMouseButtonDown(0)) {
                _isForceOn = true;
                _forceMode = Forcefield.ForceMode.PULL;
            }

            if (Input.GetMouseButtonUp(0)) {
                _isForceOn = false;
            }

            //right - push
            if (Input.GetMouseButtonDown(1)) {
                _isForceOn = true;
                _forceMode = Forcefield.ForceMode.PUSH;
            }

            if (Input.GetMouseButtonUp(1)) {
                _isForceOn = false;
            }

            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            // ------- end input handle ------

            //start a job for entity. you will access the properties of entity from here.
            //JobProcess is a struct that will implement the IJobProcessComponentData interface
            //job receives parameters and operates on data, similar to how a method call behaves.    
            //you can not  have instance property or initializer inside a struct. You have to pass it from outside
            var job = new JobProcess { isForceOn = _isForceOn, forceMode = _forceMode, mousePosition = mousePos };

            //start the job execution. This allows you to schedule a single job that runs 
            //in parallel to other jobs and the main thread.            
            return job.Schedule(this, 64, inputDeps);
        }

        [BurstCompile]
        struct JobProcess : IJobProcessComponentData<Position, Boid, Forcefield> {
            public bool isForceOn;
            public Forcefield.ForceMode forceMode;
            public float3 mousePosition;

            //Calls for each entity
            public void Execute(ref Position position, ref Boid boid, ref Forcefield forcefield) {

                //Apply force on user input.
                if (isForceOn) {
                    float3 f = forcefield.CastForce(ref mousePosition, ref boid, forceMode);
                    ApplyForce(ref boid, f);
                }

                //if it's very small set the velocity to zero
                //saves unnecessary calculations
                if (math.length(boid.velocity) >= 0.1f)
                    //apply friction, slows down over time
                    ApplyForce(ref boid, CalculateFriction(forcefield.frictionCoe, ref boid));
                else {
                    Stop(ref boid);
                }

                //add acceleration
                boid.velocity += boid.acceration;

                //clip max velocity
                if (math.length(boid.velocity) > boid.maxLength) {
                    boid.velocity = math.normalize(boid.velocity);
                    boid.velocity *= boid.maxLength;
                }

                //check bound
                CheckEdge(ref forcefield, ref boid);

                //update position based on velocity
                boid.position += boid.velocity;

                //[transform.position = boid.position]
                position.Value = boid.position;

                //reset acceleration
                boid.acceration *= 0;
            }

            public void ApplyForce(ref Boid b, float3 force) {
                //F = ma formula
                b.acceration = b.acceration + (force / b.mass);
            }

            public void Stop(ref Boid b) {
                b.velocity *= 0;
            }

            float3 CalculateFriction(float coe, ref Boid b) {
                float3 friction = b.velocity;
                friction *= -1;
                friction = math.normalize(friction);
                friction *= coe;

                return friction;
            }

            //portal effect
            public void CheckEdge(ref Forcefield forcefield, ref Boid b) {
                if (forcefield.bound.z == 0) return;

                if (b.position.x > forcefield.bound.z) {
                    b.position.x = 0;
                }
                else if (b.position.x < forcefield.bound.x) {
                    b.position.x = forcefield.bound.z;
                }

                if (b.position.y > forcefield.bound.w) {
                    b.position.y = 0;
                }
                else if (b.position.y < forcefield.bound.y) {
                    b.position.y = forcefield.bound.w;
                }
            }
        }

        //remap a value in different range
        float Remap(float value, float inputMin, float inputMax, float outputMin, float outputMax, bool clamp) {

            if (math.abs(inputMin - inputMax) < math.epsilon_normal) {
                return outputMin;
            }
            else {
                float outVal = ((value - inputMin) / (inputMax - inputMin) * (outputMax - outputMin) + outputMin);

                if (clamp) {
                    if (outputMax < outputMin) {
                        if (outVal < outputMax) outVal = outputMax;
                        else if (outVal > outputMin) outVal = outputMin;
                    }
                    else {
                        if (outVal > outputMax) outVal = outputMax;
                        else if (outVal < outputMin) outVal = outputMin;
                    }
                }
                return outVal;
            }

        }
    }
}