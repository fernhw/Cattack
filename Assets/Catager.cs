using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catager : MonoBehaviour {
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
            CatDumieData dumieData = new CatDumieData();
        }
    }

    // Update is called once per frame
    void Update() {
        

        bool allAsleep = true;
        for (int i = 0; i < catAmmount; i++) {
            cats[i].UpdateCat();
            cats[i].CollisionCorrection();
            cats[i].CollisionFall();
            cats[i].CollisionLogic();
            cats[i].CollisionPositionObject();

            if (cats[i].state == CatController.State.WALK) {
                allAsleep = false;
            }
        }
        if (allAsleep) {
            int newAmmount = catAmmount * 30;
            print(newAmmount);
            if (newAmmount > 5000)
                newAmmount = catAmmount+2;
            for (int i = 0; i < newAmmount; i++) {
                
                float randomAngle = Random.value * Mathf.PI * 2;
                float randomDistance =  Mathf.PI+5+3*Random.value  + catAmmount * Random.value*.05f;

                float GOAL_X = Mathf.Cos(randomAngle)*randomDistance;
                float GOAL_Z = Mathf.Sin(randomAngle)*randomDistance;

                Vector3 pos = new Vector3(GOAL_X, 0, GOAL_Z);
                Instantiate(prefabCat, pos, Quaternion.identity);

            }
            cats = (CatController[])FindObjectsOfType(typeof(CatController));
            for (int i = 0; i < catAmmount; i++) 
            {
                
            }
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