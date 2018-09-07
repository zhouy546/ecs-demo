using System.Collections.Generic;
using UnityEngine;

namespace NonECS.Forcefield {
    public class FieldManager : MonoBehaviour {

        public GameObject boid;

        [Space]
        public float frictionCoefficient = 0.1f;
        public int numX = 10;
        public int numY = 8;
        public float boidMass = 1;
        public float scale = 1.0f;

        [Space]
        public TMPro.TMP_Text info;

        [Space]
        //max speed
        public float MAX_LENGTH = 20;
        public Texture2D colourPallete;


        //private stuffs
        List<GameObject> boids;
        Forcefield forcefield;
        Vector3 friction;
        Forcefield.ForceMode forceMode;

        int numBoid;
        bool isForceOn;

        void Awake() {
            boids = new List<GameObject>();
            forcefield = new Forcefield(15);
        }

        private void OnEnable() {
            PopulateField();
        }

        void Update() {
            //input control
            //left - pull
            if (Input.GetMouseButtonDown(0)) {
                isForceOn = true;
                forceMode = Forcefield.ForceMode.PULL;
            }

            if (Input.GetMouseButtonUp(0)) {
                isForceOn = false;                
            }

            if (Input.GetKeyUp(KeyCode.R)) {
                PopulateField();
                return;
            }

            //right - push
            if (Input.GetMouseButtonDown(1)) {
                isForceOn = true;
                forceMode = Forcefield.ForceMode.PUSH;
            }

            if (Input.GetMouseButtonUp(1)) {
                isForceOn = false;
            }

            //update boid
            for (int i = 0; i < numBoid; i++) {
                Boid b = boids[i].GetComponent<BoidMono>().boid;
                b.Update();

                if (b.velocity.magnitude >= 0.1f)
                    //slow down over time
                    b.ApplyForce(CalculateFriction(frictionCoefficient, b));
                else {
                    b.Stop();
                }
                
                b.CheckEdge();

                //change colour based on speed
                //Color c = colourPallete.GetPixel((int)Remap(b.velocity.magnitude, 0, MAX_LENGTH-10, 0, colourPallete.width-1, true), 1);                
                //float intensity = Remap(b.velocity.magnitude, 0, MAX_LENGTH - 10, 0.5f, 1.75f, true);

                //boids[i].GetComponent<Renderer>().material.color = c * intensity;
                //boids[i].GetComponent<Renderer>().material.SetColor("_EmissionColor", c);

                //update gameobject position
                boids[i].transform.position = b.position;


                //apply force
                if (isForceOn) {
                    //for (int i = 0; i < numBoid; i++) {
                    // Boid b = boids[i].GetComponent<BoidMono>().boid;

                    Vector3 mp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mp.z = 0;

                    Vector3 f = forcefield.CastForce(mp, b, forceMode);
                    b.ApplyForce(f);
                }
            }
        }

        public void PopulateField() {
            foreach (GameObject g in boids) Destroy(g);
            boids.Clear();

            numBoid = numX * numY;
            info.text = "Number of Cubes: " + numBoid;

            float ox = (Camera.main.aspect * Camera.main.orthographicSize * 2.0f) / numX;
            float oy = Camera.main.orthographicSize * 2.0f / numY;

            //update camera position for center view
            Camera.main.transform.position = new Vector3(Camera.main.aspect * Camera.main.orthographicSize - ox * 0.5f,
                                                         Camera.main.orthographicSize - oy * 0.5f, -5);

            for (int i = 0; i < numX; i++) {
                for (int j = 0; j < numY; j++) {
                    GameObject g = Instantiate(boid, transform);
                    g.transform.position = new Vector3(i * ox, j * oy, 0);
                    g.transform.localScale *= scale;                    
                    boids.Add(g);

                    //set boid properties
                    BoidMono b = g.AddComponent<BoidMono>();
                    b.boid = new Boid(g.transform.position, scale, boidMass);
                    b.boid.maxLength = MAX_LENGTH;
                    b.boid.SetBound(new Vector4(-ox * 0.5f, -oy * 0.5f, Camera.main.aspect * Camera.main.orthographicSize * 2,
                                                Camera.main.orthographicSize * 2));
                }
            }
        }

        Vector3 CalculateFriction(float coe, Boid b) {
            friction = b.velocity;
            friction *= -1;
            friction = friction.normalized;
            friction *= coe;

            return friction;
        }

        float Remap(float value, float inputMin, float inputMax, float outputMin, float outputMax, bool clamp) {

            if (Mathf.Abs(inputMin - inputMax) < Mathf.Epsilon) {
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