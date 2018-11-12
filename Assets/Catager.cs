using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using System;

public class Catager : MonoBehaviour {

    public bool enableJobSystem;
    public bool Innerjobs;

    public GameObject prefabCat;
    public static CatController[] cats;
    public static List<CatDumieData> catDummies;
    public static int catAmmount;

    // Use this for initialization
    void Start() {
        cats = (CatController[])FindObjectsOfType(typeof(CatController));
        catAmmount = cats.Length;
        catDummies = new List<CatDumieData>();
        for (int i = 0; i < catAmmount; i++) {
            cats[i].Init(i);
        }
    }
    // Update is called once per frame
    void Update() {
        float delta = Time.deltaTime;        

        bool allAsleep = true;


        if (enableJobSystem) {

            for (int i = 0; i < catAmmount; i++) {
                CatController cat = Catager.cats[i];
                cat.UpdateCat(delta, Innerjobs);
            }


            var job1 = new catExect {
                deltaTime = delta
            };
            var jobHandle1 = job1.Schedule(catAmmount, 20);
            jobHandle1.Complete();

            // var jobHandle = job.Schedule(catAmmount, 250);
            // jobHandle.Complete();

            for (int i = 0; i < catAmmount; i++) {
                CatController cat = Catager.cats[i];
                if (cat.state != CatController.State.SLEEP)
                    cat.CollisionPositionObject();
                if (cats[i].state == CatController.State.WALK) {
                    allAsleep = false;
                }
            }
        } else {
            for (int i = 0; i < catAmmount; i++) {
                CatController cat = Catager.cats[i];
                cat.UpdateCat(delta, Innerjobs);
                cat.CollisionCorrection();
                if (cat.state != CatController.State.SLEEP)
                    cats[i].CollisionPositionObject();

                if (cats[i].state == CatController.State.WALK) {
                    allAsleep = false;
                }
            }
        }

        if (allAsleep) {
            int newAmmount = catAmmount * 25;
            print(newAmmount);
            if (newAmmount > 5000)
                newAmmount = catAmmount+2;

            for (int i = 0; i < newAmmount; i++) {
                
                float randomAngle = UnityEngine.Random.value * Mathf.PI * 2;
                float randomDistance =  Mathf.PI+5+3* UnityEngine.Random.value  + catAmmount * UnityEngine.Random.value*.05f;

                float GOAL_X = Mathf.Cos(randomAngle)*randomDistance;
                float GOAL_Z = Mathf.Sin(randomAngle)*randomDistance;

                Vector3 pos = new Vector3(GOAL_X, 0, GOAL_Z);
                Instantiate(prefabCat, pos, Quaternion.identity);

            }
            cats = (CatController[])FindObjectsOfType(typeof(CatController));
            catAmmount = cats.Length;
            for (int i = 0; i < catAmmount; i++) {
                cats[i].id = i;
                if (cats[i].alive == false) {
                    cats[i].transform.localEulerAngles = new Vector3(-90, 0, 0);
                    cats[i].Init(i);
                }
            }
        }
       

    }
}

public struct catExect:IJobParallelFor {

    //not using NativeArray yet
    public float deltaTime;

    public void Execute(int index) {
        CatController cat = Catager.cats[index];
        
        cat.CollisionCorrection();

    }
    
}