using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sherivice : MonoBehaviour {


    public State state = State.IDLE;

    public float flyInDuration = .8f;
    public Vector2 flyInStartPosition = new Vector2(47, 16);
    public Vector2 flyInPosition = new Vector2(33, 14);

    public float toInitialTauntDuration = 2.0f;
    public Vector2 tauntPosition = new Vector2(20.5f, 17);
    public float initialTauntDuration = 1.5f;

    public float toRockThrowDuration = 1.0f;
    public Vector2 rockThrowLeftPosition = new Vector2(8, 15);
    public Vector2 rockThrowRightPosition = new Vector2(35, 14);
    public float rockThrowBobMagnitude = 1.0f;
    public float rockThrowBobPeriod = 1.0f;
    public float rockThrowPeriod = 1.1f;
    public Vector2 rockThrowPoint = new Vector2(1.21f, -1.56f);
    public float rockThrowSpread = 10;
    public int rockThrowTimes = 3;
    public GameObject rockGameObject;
    
    public float toBulletShortDuration = 1.0f;
    public float toBulletFarDuration = 2.0f;
    public float bulletPeriod = 1.1f;
    public int numBullets = 4;
    public float perBulletPeriod = .1f;
    public int bulletTimes = 4;
    public Vector2 bulletPoint = new Vector2(1.8f, -.94f);
    public Vector2 bulletLeftPositionHigh = new Vector2(8, 7);
    public Vector2 bulletLeftPositionLow = new Vector2(8, 6);
    public Vector2 bulletRightPositionHigh = new Vector2(36, 6);
    public Vector2 bulletRightPositionLow = new Vector2(36, 5);
    public float bulletSwitchHeightDuration = .4f;
    public float bulletSwitchDuration = .5f;
    public float bulletBobMagnitude = 1f;
    public float bulletBobPeriod = 1.4f;
    public GameObject bulletGameObject;



    public float toBoulderDuration = 1.5f;
    public float boulderLeftX = 8;
    public float boulderRightX = 35;
    public float boulderY = 20;
    public float boulderXLoopPeriod = 2.0f;
    public float boulderBobMagnitude = 1f;
    public float boulderBobPeriod = 1.4f;

    public int numBoulders = 3;
    public Vector2 boulderRevolveCenter = new Vector2(0, 0);
    public Vector2 boulderRevolveRadius = new Vector2(2.0f, .5f);
    public float boulderRevolvePeriod = 2.0f;
    public float boulderHoldMinDuration = 2.0f;
    public float boulderHoldMaxDuration = 4.0f;
    public float boulderThrowAngleSpread = 15f;
    public float boulderThrowSpeed = 30;
    public GameObject boulderGameObject;

    public float boulderRecoilDist = 3;
    public float boulderRecoilDuration = 3;

    
    public AudioClip wingFlapSound;
    public AudioClip screamSound;

    public enum State {
        IDLE,
        FLY_IN,
        CUTSCENE_IDLE,

        TO_INITIAL_TAUNT,
        INITIAL_TAUNT,

        TO_ROCK_THROW,
        ROCK_THROW,

        TO_BULLET,
        BULLET,
        BULLET_SWITCH,

        TO_BOULDER,
        BOULDER,
        BOULDER_RECOIL,

        DEAD //don't do anything; DefaultDeath takes care of this
    }

    public bool flippedHoriz {
        get { return spriteRenderer.transform.localScale.x < 0; }
        set {
            if (value == flippedHoriz)
                return;
            spriteRenderer.transform.localScale = new Vector3(
                -spriteRenderer.transform.localScale.x,
                spriteRenderer.transform.localScale.y,
                spriteRenderer.transform.localScale.z);
        }
    }

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        spriteObject = transform.Find("spriteObject").gameObject;
        spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        animator = spriteObject.GetComponent<Animator>();
        colFinder = GetComponent<ColFinder>();
        timeUser = GetComponent<TimeUser>();
        receivesDamage = GetComponent<ReceivesDamage>();
        visionUser = GetComponent<VisionUser>();
        defaultDeath = GetComponent<DefaultDeath>();
        enemyInfo = GetComponent<EnemyInfo>();
    }

    void Start() {
        // attach to Segment
        /* segment = Segment.findBottom(rb2d.position); */
    }

    /* Called when being spawned from a Portal */
    void OnSpawn(SpawnInfo si) {
        flippedHoriz = !si.faceRight;
    }

    void goToRockThrow() {
        state = State.TO_ROCK_THROW;
        toRight = timeUser.randomValue() > .5f;
        pos0 = rb2d.position;
        if (toRight) {
            pos1 = rockThrowRightPosition;
        } else {
            pos1 = rockThrowLeftPosition;
        }
        time = 0;
        bobOffsetTime = 0;
    }

    void goToBullet() {
        state = State.TO_BULLET;
        time = 0;
        pos0 = rb2d.position;
        bobOffsetTime = 0;
        toRight = (timeUser.randomValue() > .5f);
        bulletHigh = (timeUser.randomValue() > .5f);
        if (toRight) {
            if (bulletHigh) {
                pos1 = bulletRightPositionHigh;
            } else {
                pos1 = bulletRightPositionLow;
            }
        } else {
            if (bulletHigh) {
                pos1 = bulletLeftPositionHigh;
            } else {
                pos1 = bulletLeftPositionLow;
            }
        }
    }

    void goToBoulder() {
        state = State.TO_BOULDER;
        time = 0;
        bobOffsetTime = 0;
        pos0 = rb2d.position;
        toRight = !toRight;
        if (toRight) {
            pos1.x = boulderRightX;
        } else {
            pos1.x = boulderLeftX;
        }
        pos1.y = boulderY;
    }

    Vector2 getBoulderPos(float angle) {
        return boulderRevolveCenter + new Vector2(boulderRevolveRadius.x * Mathf.Cos(angle), boulderRevolveRadius.y * Mathf.Sin(angle));
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;
        if (defaultDeath.activated)
            return;

        time += Time.deltaTime;

        Vector2 pos = rb2d.position;

        switch (state) {
        case State.IDLE:
            break;
        case State.FLY_IN:
            pos = quadEaseInOutClamp(flyInDuration);

            wingFlapPlayTime += Time.deltaTime;
            if (wingFlapPlayTime > .45f && time < flyInDuration / 2) {
                SoundManager.instance.playSFXRandPitchBend(wingFlapSound);
                wingFlapPlayTime = 0;
            }

            if (time >= flyInDuration) {
                state = State.CUTSCENE_IDLE;
                time = 0;
            }
            break;
        case State.CUTSCENE_IDLE:
            // nothing happens
            break;
        case State.TO_INITIAL_TAUNT:
            pos = quadEaseInOutClamp(toInitialTauntDuration);

            if (time >= toInitialTauntDuration / 2) {
                if (!isAnimatorCurrentState("forward")) {
                    animator.Play("forward");
                }
            }
            if (time >= toInitialTauntDuration) {
                // setting up custom camera
                CameraControl.instance.customPositionMode();
                CameraControl.instance.disableBounds();

                state = State.INITIAL_TAUNT;
                time = 0;
            } else {
                // ease camera into the custom mode
                CameraControl.instance.targetPosition = cameraPosition();
            }
            break;
        case State.INITIAL_TAUNT:
            if (time >= initialTauntDuration) {
                // go to rock throw
                //goToRockThrow();
                goToBoulder();
            }
            break;
        case State.TO_ROCK_THROW:
            pos = quadEaseInOutClamp(toRockThrowDuration);
            // bobbing (ease into rock throw bobbing)
            bobOffsetTime += Time.deltaTime;
            pos.y = pos.y + Mathf.Sin(bobOffsetTime / rockThrowBobPeriod * Mathf.PI * 2) * Utilities.easeLinearClamp(time, 0, rockThrowBobMagnitude, toRockThrowDuration);

            if (time >= toRockThrowDuration / 2) {
                if (!isAnimatorCurrentState("side")) {
                    animator.Play("side");
                }
                flippedHoriz = toRight;
            }

            if (time >= toRockThrowDuration) {
                // rock throw
                state = State.ROCK_THROW;
                time = 0;
                count = 0;
                pos0 = pos1;
            }
            
            break;
        case State.ROCK_THROW:
            // bobbing
            bobOffsetTime += Time.deltaTime;
            pos.y = pos0.y + Mathf.Sin(bobOffsetTime / rockThrowBobPeriod * Mathf.PI * 2) * rockThrowBobMagnitude;
            
            // create vision of throwing rocks
            if (count < rockThrowTimes &&
                time >= rockThrowPeriod - VisionUser.VISION_DURATION &&
                time - Time.deltaTime < rockThrowPeriod - VisionUser.VISION_DURATION) {
                rockThrowAngle = Mathf.Atan2(3 - rb2d.position.y, Player.instance.rb2d.position.x - rb2d.position.x) * 180/Mathf.PI;
                timeUser.addCurrentFrameInfo();
                visionUser.createVision(VisionUser.VISION_DURATION);
            }

            // throwing rocks
            if (time >= rockThrowPeriod) {

                if (count >= rockThrowTimes) {
                    // to other state
                    goToBullet();
                } else {

                    // throw rock(s)
                    bool positiveHeading = (timeUser.randomValue() > .5f);
                    for (int i=0; i<rocksThrownSimultaneously; i++) {

                        float rockAngle = rockThrowAngle + (timeUser.randomValue()*2-1) * rockThrowSpread;

                        Vector2 rockPos = pos;
                        if (flippedHoriz) {
                            rockPos += new Vector2(-rockThrowPoint.x, rockThrowPoint.y);
                            //rockAngle = -180 - rockAngle;
                        } else {
                            rockPos += rockThrowPoint;
                        }
                        GameObject iceRockGO = GameObject.Instantiate(rockGameObject, new Vector3(rockPos.x, rockPos.y), Quaternion.identity) as GameObject;
                        IceRock iceRock = iceRockGO.GetComponent<IceRock>();
                        iceRock.heading = rockAngle;
                        iceRock.positiveHeading = positiveHeading;
                        positiveHeading = !positiveHeading;


                        if (visionUser.isVision) { //make bullet a vision if this is also a vision
                            VisionUser irvu = iceRock.GetComponent<VisionUser>();
                            irvu.becomeVisionNow(visionUser.duration - visionUser.time, visionUser);
                        }

                    }
                    
                    count++;
                    time -= rockThrowPeriod;
                }

            }
            break;
        case State.TO_BULLET:
            float bulletDuration = toBulletShortDuration;
            if (Mathf.Abs(pos1.x - pos0.x) > 20)
                bulletDuration = toBulletFarDuration;
            pos = quadEaseInOutClamp(bulletDuration);
            // bobbing (ease into bullet bobbing)
            bobOffsetTime += Time.deltaTime;
            Vector2 bobOffset = new Vector2(Mathf.Sin(bobOffsetTime / bulletBobPeriod * Mathf.PI * 2) * bulletBobMagnitude, 0) * Utilities.easeLinearClamp(time, 0, 1, bulletDuration);
            pos = pos + bobOffset;

            if (time >= bulletDuration / 2) {
                if (!isAnimatorCurrentState("side")) {
                    animator.Play("side");
                }
                flippedHoriz = toRight;
            }

            if (time >= bulletDuration) {
                // bullet
                state = State.BULLET;
                time = 0;
                count = 0;
                bulletTime = perBulletPeriod; // so first bullet is shot immediately when the time comes
                bulletCount = 0;
                bulletSwitchHeight = timeUser.randomValue() > .5f;
                pos0 = pos1;
                if (bulletSwitchHeight) {
                    bulletHigh = !bulletHigh;
                    if (toRight) {
                        if (bulletHigh) {
                            pos1 = bulletRightPositionHigh;
                        } else {
                            pos1 = bulletRightPositionLow;
                        }
                    } else {
                        if (bulletHigh) {
                            pos1 = bulletLeftPositionHigh;
                        } else {
                            pos1 = bulletLeftPositionLow;
                        }
                    }
                } else {
                    pos1 = pos0;
                }
            }
            break;
        case State.BULLET:

            // create vision of shooting bullet
            if (count < bulletTimes &&
                time >= bulletPeriod - VisionUser.VISION_DURATION - .3f &&
                time - Time.deltaTime < bulletPeriod - VisionUser.VISION_DURATION - .3f) {

                visionUser.createVision(VisionUser.VISION_DURATION);
            }

            // position (if switching height, pos1 is different than pos0.  If not, they are the same)
            if (time < bulletPeriod - bulletSwitchHeightDuration) {
                pos = pos0;
            } else {
                pos.x = Utilities.easeInOutQuadClamp(time - bulletPeriod + bulletSwitchHeightDuration, pos0.x, pos1.x - pos0.x, bulletSwitchHeightDuration);
                pos.y = Utilities.easeInOutQuadClamp(time - bulletPeriod + bulletSwitchHeightDuration, pos0.y, pos1.y - pos0.y, bulletSwitchHeightDuration);
            }
            // bobbing
            bobOffsetTime += Time.deltaTime;
            bobOffset = new Vector2(Mathf.Sin(bobOffsetTime / bulletBobPeriod * Mathf.PI * 2) * bulletBobMagnitude, 0);
            pos = pos + bobOffset;

            if (time >= bulletPeriod) {

                if (count >= bulletTimes) {
                    // done shooting bullets, go to another state (work here)
                    goToBoulder();
                    
                } else if (switchHalfwayThroughBullet && count == bulletTimes / 2) {

                    // instead of shooting bullets, switch positions
                    state = State.BULLET_SWITCH;
                    time = 0;
                    pos0 = rb2d.position;
                    bulletHigh = !bulletHigh;
                    toRight = !toRight;
                    if (toRight) {
                        if (bulletHigh) {
                            pos1 = bulletRightPositionHigh;
                        } else {
                            pos1 = bulletRightPositionLow;
                        }
                    } else {
                        if (bulletHigh) {
                            pos1 = bulletLeftPositionHigh;
                        } else {
                            pos1 = bulletLeftPositionLow;
                        }
                    }
                    count++; // also counts as shooting bullet
                    
                } else {

                    // begin shooting bullets (up to numBullets short duration from each other)
                    bulletTime += Time.deltaTime;
                    if (bulletTime >= perBulletPeriod) {

                        float heading = 0;
                        Vector2 bulletPos = pos;
                        if (flippedHoriz) {
                            bulletPos += new Vector2(-bulletPoint.x, bulletPoint.y);
                            heading = 180;
                        } else {
                            bulletPos += bulletPoint;
                        }
                        GameObject bulletGO = GameObject.Instantiate(bulletGameObject, new Vector3(bulletPos.x, bulletPos.y), Utilities.setQuat(heading)) as GameObject;
                        Bullet bullet = bulletGO.GetComponent<Bullet>();
                        bullet.heading = heading;

                        if (visionUser.isVision) { //make bullet a vision if this is also a vision
                            VisionUser bvu = bullet.GetComponent<VisionUser>();
                            bvu.becomeVisionNow(visionUser.duration - visionUser.time, visionUser);
                        }

                        bulletTime -= perBulletPeriod;
                        bulletCount++;
                        if (bulletCount >= numBullets) {
                            time = 0;
                            bulletCount = 0;
                            bulletTime = perBulletPeriod; // so first bullet is shot immediately when the time comes
                            count++;
                            bulletSwitchHeight = timeUser.randomValue() > .5f;
                            if (count == bulletTimes - 1)
                                bulletSwitchHeight = true; // always switch height on the last bullet
                            if (switchHalfwayThroughBullet && count == bulletTimes / 2) // don't switch height if switching halfway through bullet
                                bulletSwitchHeight = false;
                            if (count >= bulletTimes) // don't switch height if ending, and not going to shoot a bullet
                                bulletSwitchHeight = false;

                            pos0 = rb2d.position - bobOffset;
                            if (bulletSwitchHeight) {
                                bulletHigh = !bulletHigh;
                                if (toRight) {
                                    if (bulletHigh) {
                                        pos1 = bulletRightPositionHigh;
                                    } else {
                                        pos1 = bulletRightPositionLow;
                                    }
                                } else {
                                    if (bulletHigh) {
                                        pos1 = bulletLeftPositionHigh;
                                    } else {
                                        pos1 = bulletLeftPositionLow;
                                    }
                                }
                            } else {
                                pos1 = pos0;
                            }
                            
                        }

                    }

                }

                
            }

            break;
        case State.BULLET_SWITCH:
            pos = quadEaseInOutClamp(bulletSwitchDuration);

            if (time >= bulletSwitchDuration / 2) {
                flippedHoriz = toRight;
            }

            if (time >= bulletSwitchDuration) {
                // go back to bullet, do not set count
                state = State.BULLET;
                time = 0;
                bulletSwitchHeight = timeUser.randomValue() > .5f;
                pos0 = rb2d.position;
            }
            
            break;
        case State.TO_BOULDER:

            pos = quadEaseInOutClamp(toBoulderDuration);
            // bobbing (ease into bullet bobbing)
            bobOffsetTime += Time.deltaTime;
            bobOffset = new Vector2(0, Mathf.Sin(bobOffsetTime / boulderBobPeriod * Mathf.PI * 2) * boulderBobMagnitude) * Utilities.easeLinearClamp(time, 0, 1, toBoulderDuration);
            pos = pos + bobOffset;

            if (time >= toBoulderDuration / 2) {
                if (!isAnimatorCurrentState("forward")) {
                    animator.Play("forward");
                }
            }

            if (time >= toBoulderDuration) {
                state = State.BOULDER;
                time = 0;
                pos0 = new Vector2((boulderRightX + boulderLeftX) / 2, boulderY);
                boulderHoldDuration = boulderHoldMinDuration + timeUser.randomValue() * (boulderHoldMaxDuration - boulderHoldMinDuration);
                if (!toRight) {
                    // on left, so time offset
                    time = boulderXLoopPeriod / 2;
                    boulderHoldDuration += boulderXLoopPeriod / 2;
                }
                
            }

            break;

        case State.BOULDER:

            pos.x = pos0.x + Mathf.Cos(time / boulderXLoopPeriod * Mathf.PI * 2) * (boulderRightX - boulderLeftX) / 2;

            bobOffsetTime += Time.deltaTime;
            bobOffset = new Vector2(0, Mathf.Sin(bobOffsetTime / boulderBobPeriod * Mathf.PI * 2) * boulderBobMagnitude);
            pos.y = pos0.y + bobOffset.y;

            // create vision of throwing boulders
            if (boulders.Count == numBoulders &&
                time >= boulderHoldDuration - VisionUser.VISION_DURATION - .3f &&
                time - Time.deltaTime < boulderHoldDuration - VisionUser.VISION_DURATION - .3f) {

                visionUser.createVision(VisionUser.VISION_DURATION);
            }

            // create boulders if not created yet
            bool createdThisFrame = false;
            if (boulders.Count == 0) {
                boulderRevolveOffset = timeUser.randomValue() * boulderRevolvePeriod;
                int indexToSpawnPickups = Mathf.FloorToInt(timeUser.randomValue() * numBoulders);
                for (int i=0; i<numBoulders; i++) {
                    GameObject boulderGO = GameObject.Instantiate(boulderGameObject);
                    boulders.Add(boulderGO);
                    IceBoulder iceBoulder = boulderGO.GetComponent<IceBoulder>();
                    iceBoulder.fadeIn();
                    iceBoulder.invincible = true;
                    iceBoulder.spawnsPickups = (i == indexToSpawnPickups);
                }
                createdThisFrame = true;
            }

            // move boulders
            float angle0 = time / boulderRevolvePeriod * Mathf.PI*2;
            for (int i=0; i<numBoulders; i++) {
                GameObject boulderGO = boulders[i];
                Rigidbody2D boulderRB2D = boulderGO.GetComponent<Rigidbody2D>();
                float angle = angle0 + (i * 1.0f / numBoulders) * Mathf.PI*2;
                Vector2 boulderPos = getBoulderPos(angle);
                boulderPos += rb2d.position;
                if (createdThisFrame) {
                    boulderRB2D.position = boulderPos;
                    boulderGO.transform.localPosition = new Vector3(boulderPos.x, boulderPos.y);
                } else {
                    boulderRB2D.MovePosition(boulderPos);
                }
                boulderRB2D.rotation = Mathf.Cos(angle) * boulderThrowAngleSpread;
                if (Mathf.Sin(angle) > 0) {
                    // order behind sherivice
                    boulderGO.GetComponent<SpriteRenderer>().sortingOrder = spriteRenderer.sortingOrder - 1;
                } else {
                    // order in front of sherivice
                    boulderGO.GetComponent<SpriteRenderer>().sortingOrder = spriteRenderer.sortingOrder + 1;
                }
            }

            // throw boulders if time
            if (time >= boulderHoldDuration) {
                for (int i = 0; i < numBoulders; i++) {
                    GameObject boulderGO = boulders[i];
                    IceBoulder ib = boulderGO.GetComponent<IceBoulder>();
                    ib.throwBoulder(boulderThrowSpeed, ib.GetComponent<Rigidbody2D>().rotation - 90);
                }
                boulders.Clear();

                animator.Play("throw_boulder");
                state = State.BOULDER_RECOIL;
                time = 0;
                pos0 = rb2d.position;
                pos1 = pos0 + new Vector2(0, boulderRecoilDist);
            }

            break;
        case State.BOULDER_RECOIL:
            pos.x = Utilities.easeOutQuadClamp(time, pos0.x, pos1.x - pos0.x, boulderRecoilDuration);
            pos.y = Utilities.easeOutQuadClamp(time, pos0.y, pos1.y - pos0.y, boulderRecoilDuration);

            if (time >= boulderRecoilDuration) {
                // go to another state
                animator.Play("forward");
                if (timeUser.randomValue() > .5f) {
                    goToRockThrow();
                } else {
                    goToBullet();
                }
            }
            break;

        }
        
        rb2d.MovePosition(pos);

        // set camera
        if (!visionUser.isVision &&
            CameraControl.instance.positionMode == CameraControl.PositionMode.CUSTOM) {
            CameraControl.instance.position = cameraPosition();
        }

        // travel across a segment:
        /*
        x = segment.travelClamp(rb2d.position.x, speed, Time.fixedDeltaTime);
        rb2d.MovePosition(new Vector2(x, rb2d.position.y));
        */

        // create a vision:
        /*
        GameObject vGO = visionUser.createVision(VisionUser.VISION_DURATION);
        */

        // spawn a bullet:
        /*
        GameObject bulletGO = GameObject.Instantiate(bulletGameObject,
            rb2d.position + relSpawnPosition,
            Utilities.setQuat(heading)) as GameObject;
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        bullet.heading = heading;
        if (visionUser.isVision) { //make bullet a vision if this is also a vision
            VisionUser bvu = bullet.GetComponent<VisionUser>();
            bvu.becomeVisionNow(visionUser.duration - visionUser.time, visionUser);
        }
        */

        

    }

    Vector2 cameraPosition() {
        Vector2 center = new Vector2(20.5f, 11.5f);
        Vector2 sherDiff = rb2d.position - center;
        Vector2 plrDiff = Player.instance.rb2d.position - center;
        Vector2 diff = sherDiff * 1.0f + plrDiff * 0f;
        float diffInfluenceX = .25f;
        float diffInfluenceY = 0;// .3f;

        return center + new Vector2(diff.x * diffInfluenceX, diff.y * diffInfluenceY);
    }

    /* called from script */
    void ScriptFlyIn() {
        state = State.FLY_IN;
        time = 0;
        flippedHoriz = true;
        pos0 = flyInStartPosition;
        pos1 = flyInPosition;
        rb2d.position = pos0;
    }
    void ScriptStartFight() {
        state = State.TO_INITIAL_TAUNT;
        time = 0;
        pos0 = rb2d.position;
        pos1 = tauntPosition;

        CameraControl.instance.disableBounds();
        CameraControl.instance.moveToPosition(cameraPosition(), toInitialTauntDuration);
    }

    /* called when this becomes a vision */
    void TimeSkip(float timeInFuture) {

        // Start() hasn't been called yet
        //Start();

        // increment time
        time += timeInFuture;
        bobOffsetTime += timeInFuture;

        if (state == State.BOULDER) {

            // create new boulders
            List<GameObject> oldBoulders = new List<GameObject>();
            oldBoulders.AddRange(boulders);
            boulders.Clear();
            float angle0 = time / boulderRevolvePeriod * Mathf.PI*2;

            for (int i=0; i<numBoulders; i++) {

                GameObject boulderGO = GameObject.Instantiate(boulderGameObject, oldBoulders[i].transform.localPosition, oldBoulders[i].transform.localRotation) as GameObject;
                boulders.Add(boulderGO);
                IceBoulder iceBoulder = boulderGO.GetComponent<IceBoulder>();
                iceBoulder.invincible = true;
                iceBoulder.spawnsPickups = oldBoulders[i].GetComponent<IceBoulder>().spawnsPickups;

                // convert to vision
                boulderGO.GetComponent<VisionUser>().becomeVisionNow(visionUser.duration - visionUser.time, oldBoulders[i].GetComponent<VisionUser>());
            }
            
            
        }
        
    }

    /* called when this takes damage */
    void OnDamage(AttackInfo ai) {
        if (receivesDamage.health <= 0) {
            flippedHoriz = ai.impactToRight();
            //animator.Play("damage");
        }
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["time"] = time;
        fi.floats["wfpt"] = wingFlapPlayTime;
        fi.floats["p0x"] = pos0.x;
        fi.floats["p0y"] = pos0.y;
        fi.floats["p1x"] = pos1.x;
        fi.floats["p1y"] = pos1.y;
        fi.bools["tr"] = toRight;
        fi.ints["count"] = count;
        fi.floats["rta"] = rockThrowAngle;
        fi.ints["rts"] = rocksThrownSimultaneously;
        fi.floats["bot"] = bobOffsetTime;
        fi.bools["bh"] = bulletHigh;
        fi.floats["bt"] = bulletTime;
        fi.ints["bc"] = bulletCount;
        fi.bools["bsh"] = bulletSwitchHeight;
        fi.bools["shtb"] = switchHalfwayThroughBullet;
        fi.floats["bro"] = boulderRevolveOffset;
        fi.floats["bhd"] = boulderHoldDuration;

        for (int i=0; i<numBoulders; i++) {
            if (i < boulders.Count) {
                fi.gameObjects["b" + i] = boulders[i];
            } else {
                fi.gameObjects["b" + i] = null;
            }
        }
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["time"];
        wingFlapPlayTime = fi.floats["wfpt"];
        pos0.Set(fi.floats["p0x"], fi.floats["p0y"]);
        pos1.Set(fi.floats["p1x"], fi.floats["p1y"]);
        toRight = fi.bools["tr"];
        count = fi.ints["count"];
        rockThrowAngle = fi.floats["rta"];
        rocksThrownSimultaneously = fi.ints["rts"];
        bobOffsetTime = fi.floats["bot"];
        bulletHigh = fi.bools["bh"];
        bulletTime = fi.floats["bt"];
        bulletCount = fi.ints["bc"];
        bulletSwitchHeight = fi.bools["bsh"];
        switchHalfwayThroughBullet = fi.bools["shtb"];
        boulderRevolveOffset = fi.floats["bro"];
        boulderHoldDuration = fi.floats["bhd"];

        boulders.Clear();
        for (int i=0; i<numBoulders; i++) {
            if (fi.gameObjects["b" + i] != null) {
                boulders.Add(fi.gameObjects["b" + i]);
            }
        }
    }

    // helpers
    Vector2 quadEaseInOutClamp(float duration) {
        Vector2 ret = new Vector2();
        ret.x = Utilities.easeInOutQuadClamp(time, pos0.x, pos1.x - pos0.x, duration);
        ret.y = Utilities.easeInOutQuadClamp(time, pos0.y, pos1.y - pos0.y, duration);
        return ret;
    }
    bool isAnimatorCurrentState(string stateString) {
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash(stateString);
    }

    float time;
    Segment segment;
    float wingFlapPlayTime = 0;
    Vector2 pos0 = new Vector2();
    Vector2 pos1 = new Vector2();
    bool toRight = false;
    int count = 0;
    float rockThrowAngle = 0;
    int rocksThrownSimultaneously = 2; // 2, then 3, then 4
    float bobOffsetTime = 0;
    bool bulletHigh = false;
    float bulletTime = 0;
    int bulletCount = 0;
    bool bulletSwitchHeight = false;
    bool switchHalfwayThroughBullet = false; // then true
    List<GameObject> boulders = new List<GameObject>();
    float boulderRevolveOffset = 0;
    float boulderHoldDuration = 0;
    

    // components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    Animator animator;
    ColFinder colFinder;
    TimeUser timeUser;
    ReceivesDamage receivesDamage;
    VisionUser visionUser;
    DefaultDeath defaultDeath;
    EnemyInfo enemyInfo;

}
