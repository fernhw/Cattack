using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CatController : MonoBehaviour {
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
        SortieAngle();
        BShapes = GetComponent<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
    }

    public void UpdateCat() {
        delta = Time.deltaTime * speed;
        timePassed += delta;
        switch (state) {
        case State.WALK:
        float anim = Mathf.Cos(timePassed * WALK_SPEED * speed);
        //Walking animation
        if (anim >= 0) {
            BShapes.SetBlendShapeWeight(Anims.STEP_1, anim * 100);
            BShapes.SetBlendShapeWeight(Anims.STEP_2, 0);
        } else {
            anim = -anim;
            BShapes.SetBlendShapeWeight(Anims.STEP_1, 0);
            BShapes.SetBlendShapeWeight(Anims.STEP_2, anim * 100);
        }
        timeUntilNextCatDistraction -= delta;
        if (timeUntilNextCatDistraction < 0) {
            SortieAngle();
        }


        position.x += sin * speed * delta;
        position.z += cos * speed * delta;

        break;
        case State.SLEEP:

        float sleepAnim = timePassed * SLEEP_SPEED;
        if (sleepAnim > 100) {
            sleepAnim = 100;
        }
        if (sleepType == SleepType.ON_BELLY) {
            BShapes.SetBlendShapeWeight(Anims.ON_BELLY, sleepAnim);
        } else {
            BShapes.SetBlendShapeWeight(Anims.SIDEWAYS, sleepAnim);
        }

        break;
        }
    }
    float cos, sin;
    bool floating;

    public void UpdateCatCollision() {
            CollisionCorrection();
            CollisionFall();
            CollisionLogic();
            CollisionPositionObject();
    }

    public void CollisionCorrection (){
        if (state != State.WALK)
            return;
        rotation.y = -(Mathf.PI * 2 - angleToMove + Mathf.PI * .5f) * Mathf.Rad2Deg;
        floating = false;

        for (int i = 0; i < Catager.catAmmount; i++) {
            CatController cat = Catager.cats[i];
            if (cat.id == this.id) {
            } else {
                float distX, distY, distZ;

                if (position.x > cat.position.x + .2f ||
                    position.x < cat.position.x - .2f ||
                    position.z > cat.position.z + .2f ||
                    position.z < cat.position.z - .2f) {
                    continue;
                }

                if (position.y >= cat.position.y &&
                    position.y < cat.position.y + .1f &&
                    position.y + .1f > cat.position.y) {
                    distX = cat.position.x - position.x;
                    distZ = cat.position.z - position.z;
                    distY = (cat.position.y + .05f) - (position.y + .05f);
                    float distance2 = distX * distX + distZ * distZ + distY * distY;
                    for (int a = 0; a < 4; a++) {
                        if (distance2 < .04f) {
                            floating = true;
                            distX = cat.position.x - position.x;
                            distZ = cat.position.z - position.z;
                            distY = (cat.position.y + .05f) - (position.y + .05f);
                            distance2 = distX * distX + distZ * distZ + distY * distY;
                            position.y += .2f * delta;

                        } else {
                            break;
                        }
                    }
                    gravity = 0;
                }
            }
        }
    }

    public void CollisionFall (){
        if (state != State.WALK)
            return;
            if (!floating) {
            position.y -= gravity * delta * 1f;
            if (gravity != TERMINAL_VELOCITY) {
                gravity += delta * 5;
                if (gravity > TERMINAL_VELOCITY) {
                    gravity = TERMINAL_VELOCITY;
                }
            }
        }

        if (position.y < 0) {
            position.y = 0;
            gravity = 0;
        }
    }

    public void CollisionLogic(){
        if (state != State.WALK)
            return;
            if (position.x > GOAL_X - GOAL_SQUARE_RANGE &&
               position.x < GOAL_X + GOAL_SQUARE_RANGE &&
               position.z > GOAL_Z - GOAL_SQUARE_RANGE &&
               position.z < GOAL_Z + GOAL_SQUARE_RANGE) {
            position.y = 0;
            state = State.SLEEP;
            timePassed = 0;
            for (int i = 0; i < Catager.catAmmount; i++) {
                CatController cat = Catager.cats[i];
                if (cat.state != State.SLEEP) {
                    continue;
                }
                if (cat.id == id) {
                } else {
                    float distX = cat.realPosition.x - position.x;
                    float distZ = cat.realPosition.z - position.z;
                    float distance = Mathf.Sqrt(distX * distX + distZ * distZ);
                    if (distance < .2f) {
                        if (position.y >= cat.position.y) {
                            while (position.y < Catager.cats[i].position.y + .11f &&
                                  position.y + .11f > Catager.cats[i].position.y) {
                                position.y += .01f;
                            }
                        }
                    }
                }
            }
        }
    }

    public void CollisionPositionObject(){
        transform.localPosition = position;
        transform.localEulerAngles = rotation;
    }


    void SortieAngle() {
        float randomVal = Random.value;
        float range = randomVal * (DISTRACTION_END - DISTRACTION_START);
        timeUntilNextCatDistraction = DISTRACTION_START + range;
        float xDif = position.x - GOAL_X;
        float yDif = position.z - GOAL_Z;
        float angle = Mathf.Atan2(xDif, yDif);
        float rng = Random.value * ANGLE_DISTRACTION;
        angleToMove = angle;
        cos = Mathf.Cos(angleToMove - Mathf.PI);
        sin = Mathf.Sin(angleToMove - Mathf.PI);
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