using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public struct catExect2:IJobParallelFor {

    //not using NativeArray yet
    public bool floating;
    public float gravity;
    public float deltaTime;
    public float catX, catY, catZ;
    public int id;
    float cat2X, cat2Y, cat2Z;

    public void Execute(int index) {
        CatController cat = Catager.cats[index];
        cat2X = cat.position.x;
        cat2Y = cat.position.y;
        cat2Z = cat.position.z;

        if (cat.id == id) {
        } else {
            float distX, distY, distZ;

           /* if (catX > cat2X + .2f ||
                catX < cat2X - .2f ||
                catZ > cat2Z + .2f ||
                catZ < cat2Z - .2f) {
                //return;
            }*/

            if (catY >= cat2Y && catY < cat2Y + .1f && catY + .1f > cat2Y) {
                distX = cat2X - catX;
                distZ = cat2Z - catZ;
                distY = (cat2Y + .05f) - (catY + .05f);
                float distance2 = distX * distX + distZ * distZ + distY * distY;
                for (int a = 0; a < 4; a++) {
                    if (distance2 < .04f) {
                        floating = true;
                        distX = cat2X - catX;
                        distZ = cat2Z - catZ;
                        distY = (cat2Y + .05f) - (catY + .05f);
                        distance2 = distX * distX + distZ * distZ + distY * distY;
                        catY += .1f * deltaTime;
                    } else {
                        break;
                    }
                }
                gravity = 0;
            }
        }

    }

}
public class CatController:MonoBehaviour {
    const float
      WALK_SPEED = 20,
      SLEEP_SPEED = 600,
      DISTRACTION_START = 1,
      DISTRACTION_END = 2,
      SPEED_RANGE_START = 1f,
      SPEED_RANGE_END = 1.1f,
      ANGLE_DISTRACTION = .1f,

      GOAL_SQUARE_RANGE = .3f,
      TERMINAL_VELOCITY = 10;
    public bool alive = false;
    float
        speed = 1,
        gravity = 0,
        timePassed = 0,
        timeUntilNextCatDistraction = 0,
        angleToMove,
        delta,
      GOAL_X = 0,
      GOAL_Z = 0;

    public Vector3
        realPosition,
        position,
        rotation;

    SkinnedMeshRenderer BShapes; //Blend Shapes
    SleepType sleepType = SleepType.ON_BELLY;
    public State state = State.WALK;
    public int id = 0;

    float cat2X, cat2Y, cat2Z;
    float catX, catZ, catY;

    public void Init(int id) {

        alive = true;
        this.id = id;
        float randomAngle = Random.value * Mathf.PI * 2;
        float randomDistance = Random.value * Mathf.PI;
        float rd = Mathf.Sin(randomDistance) * .5f + (Catager.catAmmount / 500) + .5f;

        if (rd < .2)
            rd = .2f;

        if (rd > 1.8f)
            rd = 1.8f;

        rd *= Random.value;

        if (Mathf.Round(Random.value * 8) == 0) {
            rd *= 4;
        }

        GOAL_X = Mathf.Cos(randomAngle) * rd;
        GOAL_Z = Mathf.Sin(randomAngle) * rd;

        position = transform.localPosition;
        catX = position.x;
        catY = position.y;
        catZ = position.z;
        realPosition = transform.localPosition;
        rotation = transform.localEulerAngles;

        float randomVal = Random.value;
        float range = randomVal * (DISTRACTION_END - DISTRACTION_START);
        timeUntilNextCatDistraction = -1;// DISTRACTION_START + range;

        float speedRange = Random.value * (SPEED_RANGE_END - SPEED_RANGE_START);

        speed = SPEED_RANGE_START + speedRange;

        if (Mathf.Round(Random.value * 2) < Mathf.Epsilon) {
            sleepType = SleepType.ON_BELLY;
        } else {
            sleepType = SleepType.SIDEWAYS;
        }


        float xDif = catX - GOAL_X;
        float yDif = catZ - GOAL_Z;
        float angle = Mathf.Atan2(xDif, yDif);
        angleToMove = angle;
        cos = Mathf.Cos(angleToMove - Mathf.PI);
        sin = Mathf.Sin(angleToMove - Mathf.PI);
        rotation.y = -(Mathf.PI * 2 - angleToMove + Mathf.PI * .5f) * Mathf.Rad2Deg;
        transform.localEulerAngles = rotation;
        BShapes = GetComponent<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
    }

    bool negativeAnim = false;
    bool setSt1ToZero = false;
    bool setSt2ToZero = false;
    bool Innerjobs = false;
    public void UpdateCat(float delta, bool jobs) {
        this.Innerjobs = jobs;
        this.delta = delta;
        timePassed += delta;
        switch (state) {
            case State.WALK:
                //absAnim
                float animationPace = timePassed * WALK_SPEED * speed;
                if (animationPace < 0)
                    animationPace = -animationPace;
                float n = animationPace - Mathf.Floor(animationPace *.25f) * 4; //fix:mult  (0->4) then (0->4)
                float reduct = n - 2;//(0-2-4) to (0-0-2)
                if (reduct < 0)
                    reduct = 0;
                float add = (n - 1);
                if (add > 1)
                    add = 1;
                float anim = add - reduct;//-1 to 1 (->)

                //Walking animation
                if (anim >= 0) {
                    setSt1ToZero = false;
                    BShapes.SetBlendShapeWeight(Anims.STEP_1, anim * 100);
                    if (setSt2ToZero == false) {
                        setSt2ToZero = true;
                        BShapes.SetBlendShapeWeight(Anims.STEP_2, 0);
                    }
                } else {
                    anim = -anim;
                    setSt2ToZero = false;
                    if (setSt1ToZero == false) {
                        setSt1ToZero = true;
                        BShapes.SetBlendShapeWeight(Anims.STEP_1, 0);
                    }
                    BShapes.SetBlendShapeWeight(Anims.STEP_2, anim * 100);
                }
                timeUntilNextCatDistraction -= delta;
                catX += sin * speed * delta;
                catZ += cos * speed * delta;

                break;
            case State.SLEEP:
                if (fullyAsleep)
                    break;
                float sleepAnim = timePassed * SLEEP_SPEED;
                if (sleepAnim > 100) {
                    sleepAnim = 100;
                }
                if (sleepType == SleepType.ON_BELLY) {
                    BShapes.SetBlendShapeWeight(Anims.ON_BELLY, sleepAnim);
                } else {
                    BShapes.SetBlendShapeWeight(Anims.SIDEWAYS, sleepAnim);
                }
                if (sleepAnim == 100)
                    fullyAsleep = true;
                break;
        }
    }

    bool fullyAsleep = false;
    float cos, sin;
    bool floating;


    public void CollisionCorrection() {
        // return;
        if (state != State.WALK)
            return;
        if (catX > 5 || catX < -5 || catZ > 5 || catZ < -5)
            return;

        floating = false;


        if (Innerjobs) {

            var job1 = new catExect2 {
                deltaTime = delta,
                floating = floating               
            };
            job1.catX = catX;
            job1.catY = catY;
            job1.catZ = catZ;
            job1.id = id;
            var jobHandle1 = job1.Schedule(Catager.catAmmount, 300);
            jobHandle1.Complete();

            floating = job1.floating;
            gravity = job1.gravity;

        } else {
            for (int i = 0; i < Catager.catAmmount; i++) {
                CatController cat = Catager.cats[i];
                cat2X = cat.position.x;
                cat2Y = cat.position.y;
                cat2Z = cat.position.z;

                if (cat.id == this.id) {
                } else {
                    float distX, distY, distZ;

                    if (catX > cat2X + .2f ||
                        catX < cat2X - .2f ||
                        catZ > cat2Z + .2f ||
                        catZ < cat2Z - .2f) {
                        //return;
                        continue;
                    }

                    if (catY >= cat2Y && catY < cat2Y + .1f && catY + .1f > cat2Y) {
                        distX = cat2X - catX;
                        distZ = cat2Z - catZ;
                        distY = (cat2Y + .05f) - (catY + .05f);
                        float distance2 = distX * distX + distZ * distZ + distY * distY;
                        for (int a = 0; a < 4; a++) {
                            if (distance2 < .04f) {
                                floating = true;
                                distX = cat2X - catX;
                                distZ = cat2Z - catZ;
                                distY = (cat2Y + .05f) - (catY + .05f);
                                distance2 = distX * distX + distZ * distZ + distY * distY;
                                catY += .1f * delta;
                            } else {
                                break;
                            }
                        }
                        gravity = 0;
                    }
                }
            }

        }
    
        if (state != State.WALK)
            return;
        if (!floating) {
            catY -= gravity * delta * 1f;
            if (gravity != TERMINAL_VELOCITY) {
                gravity += delta * 5;
                if (gravity > TERMINAL_VELOCITY) {
                    gravity = TERMINAL_VELOCITY;
                }
            }
        }

        if (catY < 0) {
            catY = 0;
            gravity = 0;
        }
        if (state != State.WALK)
            return;
        if (catX > GOAL_X - GOAL_SQUARE_RANGE &&
           catX < GOAL_X + GOAL_SQUARE_RANGE &&
           catZ > GOAL_Z - GOAL_SQUARE_RANGE &&
           catZ < GOAL_Z + GOAL_SQUARE_RANGE) {
            catY = 0;
            state = State.SLEEP;
            timePassed = 0;
            for (int i = 0; i < Catager.catAmmount; i++) {
                CatController cat = Catager.cats[i];
                Vector3 catPos = cat.position;
                cat2X = catPos.x;
                cat2Y = catPos.y;
                cat2Z = catPos.z;
                if (cat.state != State.SLEEP) {
                   continue;
                }
                if (cat.id == id) {
                } else {
                    float distX = cat2X - catX;
                    float distZ = cat2Z - catZ;
                    float distance = distX * distX + distZ * distZ;
                    if (distance < .13f) {
                            while (catY < cat2Y + .1f &&
                                  catY + .1f > cat2Y) {
                                catY += .01f;
                            }
                    }
                }
            }
        }
    }

    public void CollisionPositionObject() {
        
        position = new Vector3(catX, catY, catZ);
        transform.localPosition = position;       
    }
    

    public enum State {
        WALK,
        SLEEP,
        END
    }

    public enum SleepType {
        ON_BELLY,
        SIDEWAYS
    }

    public class Anims {
        public const int
            STEP_1 = 0,
            STEP_2 = 1,
            ON_BELLY = 2,
            SIDEWAYS = 3;
    }

}