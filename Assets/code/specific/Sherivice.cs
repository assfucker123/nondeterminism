using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sherivice : MonoBehaviour {


    public State state = State.IDLE;

    public int maxHealth = 30;

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
    public float percentHealth3RocksThrown = .7f;
    public float percentHealth4RocksThrown = .4f;
    public float rockVisionDuration = 2.5f;
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
    public float percentHealthToSwitchHalfwayThroughBullet = .5f;
    public float percentHealth3Bullets = .6f;
    public float percentHealth4Bullets = .3f;

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

    public float percentHealthTriggerScript = .3f;
    public float toLowHealthDuration = 1.5f;
    public Vector2 lowHealthPosition = new Vector2();
    public TextAsset lowHealthScript;

    public float coveredSwayRotation = 30;
    public float coveredSwayPeriod = .7f;
    public float coveredBobDist = 4;
    public float coveredBobPeriod = 1.0f;
    public float coveredRockThrowInitialDelay = .5f;
    public float coveredRockThrowPeriod = .7f;

    public float finalHitWait = 5.0f;
    public float finalHitRockAngle = 300;
    public float finalHitRotation = 10;
    public float finalHitShiverDist = .3f;
    public float finalHitShiverPeriod = .07f;
    public Vector2 finalHitTumbleVelocity = new Vector2();
    public Vector2 finalHitTumbleAccel = new Vector2(); // gravity, etc.
    public float finalHitTumbleAngularVelocity = 200;

    public TextAsset finalHitScript;

    public AudioClip wingFlapSound;
    public AudioClip bulletSound;
    public AudioClip rockSound;
    public AudioClip boulderAppearSound;
    public AudioClip boulderThrowSound;
    public AudioClip finalHitSound;
    public AudioClip screamSound;
    

    /* Order: FLY_IN -> INITIAL_TAUNT -> ROCK_THROW -> BULLET -> BOULDER -> (ROCK_THROW or BULLET) -> (the other one) -> (back to boulder, repeat)
     */

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

        TO_LOW_HEALTH,
        LOW_HEALTH,
        COVERED,
        COVERED_THROWING,

        FINAL_HIT_FROZEN,
        FINAL_HIT_TUMBLE,

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
        timeUser = GetComponent<TimeUser>();
        receivesDamage = GetComponent<ReceivesDamage>();
        visionUser = GetComponent<VisionUser>();
        defaultDeath = GetComponent<DefaultDeath>();
        enemyInfo = GetComponent<EnemyInfo>();
        eventHappener = GetComponent<EventHappener>();
        ScriptRunner[] scriptRunners = GetComponents<ScriptRunner>();
        scriptRunner1 = scriptRunners[0];
        scriptRunner2 = scriptRunners[1];
    }

    void Start() {
        // attach to Segment
        /* segment = Segment.findBottom(rb2d.position); */
        receivesDamage.health = maxHealth;
        HUD.instance.bossHealthBar.maxHealth = maxHealth;
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
        wingFlapPlayTime = 0;
        bobOffsetTime = 0;
    }

    void goToBullet() {
        state = State.TO_BULLET;
        time = 0;
        wingFlapPlayTime = 0;
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
        wingFlapPlayTime = 0;
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

    /* goes to TO_LOW_HEALTH state, but only if the timing works */
    void lowHealthTrigger() {
        if (receivesDamage.health * 1.0f / maxHealth > percentHealthTriggerScript)
            return;
        if (state != State.TO_BOULDER && state != State.TO_BULLET && state != State.TO_ROCK_THROW)
            return;

        time = 0;
        wingFlapPlayTime = 0;
        pos0 = rb2d.position;
        pos1 = lowHealthPosition;
        state = State.TO_LOW_HEALTH;

        // clear all boulders on the screen
        IceBoulder[] iceBoulders = GameObject.FindObjectsOfType<IceBoulder>();
        for (int i=0; i< iceBoulders.Length; i++) {
            iceBoulders[i].fadeOut();
        }

        // trigger script
        scriptRunner1.runScript(lowHealthScript);
    }

    /* called by the script */
    void CoverFace() {
        if (state != State.LOW_HEALTH && state != State.TO_LOW_HEALTH)
            return;

        time = 0;
        bobOffsetTime = 0;
        flippedHoriz = false;
        pos0 = rb2d.position;
        animator.Play("forward_covered");
        state = State.COVERED;
    }
    void CoverStartThrow() {
        if (state != State.COVERED) return;

        count = 0;
        time = -coveredRockThrowInitialDelay;
        state = State.COVERED_THROWING;
    }
    void FinalHitTumble() {
        if (state != State.FINAL_HIT_FROZEN)
            return;

        time = 0;
        rb2d.velocity = finalHitTumbleVelocity;
        rb2d.angularVelocity = finalHitTumbleAngularVelocity;
        SoundManager.instance.playSFXIgnoreVolumeScale(screamSound);
        state = State.FINAL_HIT_TUMBLE;

        // destroy rock
        IceRock[] iceRocks = GameObject.FindObjectsOfType<IceRock>();
        for (int i=0; i<iceRocks.Length; i++) {
            iceRocks[i].destroy();
        }
    }

    Vector2 getBoulderPos(float angle) {
        return boulderRevolveCenter + new Vector2(boulderRevolveRadius.x * Mathf.Cos(angle), boulderRevolveRadius.y * Mathf.Sin(angle));
    }

    void Update() {

        if (Keys.instance.flashbackPressed)
            playerPressedFlashback = true;

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

                // vision tutorial screen
                if (!Vars.currentNodeData.eventHappened(AdventureEvent.Physical.VISION_TUTORIAL_SCREEN)) {
                    eventHappener.physicalHappen(AdventureEvent.Physical.VISION_TUTORIAL_SCREEN);
                    if (Player.instance.phase > 0) {
                        ControlsMessageSpawner.instance.spawnHaltScreen(HaltScreen.Screen.VISION);
                    }
                }

            } else {
                // ease camera into the custom mode
                CameraControl.instance.targetPosition = cameraPosition();
            }
            break;
        case State.INITIAL_TAUNT:
            if (time >= initialTauntDuration) {
                // go to rock throw
                rockThrowFirst = true;
                goToRockThrow();
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

            wingFlapPlayTime += Time.deltaTime;
            if (!visionUser.isVision) {
                if (wingFlapPlayTime > .45f && time < 1) {
                    SoundManager.instance.playSFXRandPitchBend(wingFlapSound);
                    wingFlapPlayTime = 0;
                }
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
                    if (rockThrowFirst) {
                        goToBullet();
                    } else {
                        goToBoulder();
                    }
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
                            irvu.becomeVisionNow(rockVisionDuration, null);
                        }

                    }

                    if (!visionUser.isVision) {
                        SoundManager.instance.playSFX(rockSound);
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

            wingFlapPlayTime += Time.deltaTime;
            if (!visionUser.isVision) {
                if (wingFlapPlayTime > .45f && time < 1) {
                    SoundManager.instance.playSFXRandPitchBend(wingFlapSound);
                    wingFlapPlayTime = 0;
                }
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
            if (!visionUser.isVision &&
                count < bulletTimes &&
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
                    // done shooting bullets, go to another state
                    if (rockThrowFirst) {
                        goToBoulder();
                    } else {
                        goToRockThrow();
                    }
                    
                } else if (switchHalfwayThroughBullet && count == bulletTimes / 2) {

                    // instead of shooting bullets, switch positions
                    state = State.BULLET_SWITCH;
                    time = 0;
                    wingFlapPlayTime = 0;
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
                            bvu.becomeVisionNow(VisionUser.VISION_DURATION, visionUser);
                        } else {
                            SoundManager.instance.playSFXRandPitchBend(bulletSound, .01f);
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

            wingFlapPlayTime += Time.deltaTime;
            if (!visionUser.isVision) {
                if (wingFlapPlayTime > .1f && time < .25f) {
                    SoundManager.instance.playSFXRandPitchBend(wingFlapSound);
                    wingFlapPlayTime = 0;
                }
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

            wingFlapPlayTime += Time.deltaTime;
            if (!visionUser.isVision) {
                if (wingFlapPlayTime > .45f && time < 1) {
                    SoundManager.instance.playSFXRandPitchBend(wingFlapSound);
                    wingFlapPlayTime = 0;
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
                if (!visionUser.isVision) {
                    SoundManager.instance.playSFX(boulderAppearSound);
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
                if (!visionUser.isVision) {
                    SoundManager.instance.playSFX(boulderThrowSound);
                }
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
                rockThrowFirst = (timeUser.randomValue() > .5f);
                if (rockThrowFirst) {
                    goToRockThrow();
                } else {
                    goToBullet();
                }
            }
            break;
        case State.TO_LOW_HEALTH:
            pos = quadEaseInOutClamp(toLowHealthDuration);

            if (time >= toLowHealthDuration / 2) {
                if (!isAnimatorCurrentState("side")) {
                    animator.Play("side");
                }
                flippedHoriz = true;
            }

            wingFlapPlayTime += Time.deltaTime;
            if (wingFlapPlayTime > .45f && time < flyInDuration / 2) {
                SoundManager.instance.playSFXRandPitchBend(wingFlapSound);
                wingFlapPlayTime = 0;
            }

            if (time >= toLowHealthDuration) {
                state = State.LOW_HEALTH;
                time = 0;
            }
            break;
        case State.LOW_HEALTH:
            break;
        case State.COVERED:
        case State.COVERED_THROWING:
            bobOffsetTime += Time.deltaTime;

            rb2d.rotation = Mathf.Sin(bobOffsetTime / coveredSwayPeriod * Mathf.PI * 2) * coveredSwayRotation;

            float range = (Mathf.Cos(bobOffsetTime / coveredBobPeriod * Mathf.PI*2 + Mathf.PI) + 1) / 2; // mapped to [0, 1]
            //range = Utilities.easeOutQuad(range, 0, 1, 1); // make range lean more toward 1
            pos.y = pos0.y - range * coveredBobDist;

            if (state == State.COVERED_THROWING) {
                // handle throwing rocks

                // create vision of throwing rocks
                if (visionUser.shouldCreateVisionThisFrame(time - Time.deltaTime, time, coveredRockThrowPeriod, VisionUser.VISION_DURATION + .1f)) {

                    // don't create visions of rocks after the one that deals the final hit
                    //if (time + VisionUser.VISION_DURATION - .5f < finalHitWait || time + VisionUser.VISION_DURATION - .5f - coveredRockThrowPeriod > finalHitWait) {
                    if (!visionUser.isVision) {
                        visionUser.createVision(VisionUser.VISION_DURATION);
                    }
                    //}

                    

                }

                // if should throw rock this frame
                if (visionUser.shouldHaveEventThisFrame(time - Time.deltaTime, time, coveredRockThrowPeriod) && time > coveredRockThrowPeriod) {
                    int index = Mathf.RoundToInt(time / coveredRockThrowPeriod);
                    bool finalHit = (time >= finalHitWait && time - coveredRockThrowPeriod < finalHitWait);

                    float rockAngle = coveredRockThrowAngle(index);
                    if (finalHit)
                        rockAngle = finalHitRockAngle;

                    // throw rock(s)
                    Vector2 rockPos = pos;
                    rockPos += new Vector2(0f, -3f);

                    GameObject iceRockGO = GameObject.Instantiate(rockGameObject, new Vector3(rockPos.x, rockPos.y), Quaternion.identity) as GameObject;
                    IceRock iceRock = iceRockGO.GetComponent<IceRock>();
                    iceRock.heading = rockAngle;
                    if (finalHit) {
                        iceRock.hitsSherivice = true;
                        iceRock.positiveHeading = true;
                    } else {
                        iceRock.positiveHeading = (index % 2 == 0);
                    }
                    
                    iceRock.GetComponent<TimeUser>().setRandSeed(index); // set predictable random value

                    if (visionUser.isVision) { //make bullet a vision if this is also a vision
                        VisionUser irvu = iceRock.GetComponent<VisionUser>();
                        irvu.becomeVisionNow(VisionUser.VISION_DURATION, visionUser);
                    }
                    
                    if (!visionUser.isVision) {
                        SoundManager.instance.playSFX(rockSound);
                    }

                    // do not reset time

                }

            }
            
            break;
        case State.FINAL_HIT_FROZEN:

            float t = Utilities.fmod(time, finalHitShiverPeriod) / finalHitShiverPeriod;
            if (t > .5f)
                t = 1 - t;
            t *= 2;

            pos.x = pos0.x + Utilities.easeLinear(t, 0, finalHitShiverDist, 1);

            break;
        case State.FINAL_HIT_TUMBLE:
            rb2d.velocity = rb2d.velocity + finalHitTumbleAccel * Time.deltaTime;
            rb2d.rotation += finalHitTumbleAngularVelocity * Time.deltaTime;
            break;

        }
        
        if (state != State.FINAL_HIT_TUMBLE) {
            rb2d.MovePosition(pos);
        }
        

        // set camera
        if (!visionUser.isVision &&
            CameraControl.instance.positionMode == CameraControl.PositionMode.CUSTOM) {
            CameraControl.instance.position = cameraPosition();
        }

        // difficulty
        float healthPercent = receivesDamage.health * 1.0f / maxHealth;
        if (state != State.BULLET) {
            if (healthPercent < percentHealthToSwitchHalfwayThroughBullet) {
                switchHalfwayThroughBullet = true;
            }
            if (healthPercent < percentHealth3Bullets) {
                numBullets = 3;
            }
            if (healthPercent < percentHealth4Bullets) {
                numBullets = 4;
            }
        }
        if (state != State.ROCK_THROW) {
            if (healthPercent < percentHealth3RocksThrown) {
                rocksThrownSimultaneously = 3;
            }
            if (healthPercent < percentHealth4RocksThrown) {
                rocksThrownSimultaneously = 4;
            }
        }

        // check needing to run the low health script
        lowHealthTrigger();
        
        // make flashback controls appear if player is doing bad
        if (!playerPressedFlashback && !displayedFlashbackReminder && !Vars.currentNodeData.eventHappened(AdventureEvent.Physical.SHERIVICE_FLASHBACK_CONTROL_REMINDER)) {
            if (Player.instance.state == Player.State.DAMAGE) {
                if (Player.instance.health <= 2 && Player.instance.health >= 1) {
                    eventHappener.physicalHappen(AdventureEvent.Physical.SHERIVICE_FLASHBACK_CONTROL_REMINDER, false);
                    ControlsMessageSpawner.instance.spawnMessage(ControlsMessage.Control.FLASHBACK);
                    displayedFlashbackReminder = true; // this somehow keeps getting set to false through black magic 
                    timeSinceDisplayedFlashbackReminder = 0;
                }
            }
        }
        if (displayedFlashbackReminder) {
            timeSinceDisplayedFlashbackReminder += Time.deltaTime;
            if (timeSinceDisplayedFlashbackReminder > 4.0f) {
                ControlsMessageSpawner.instance.takeDownMessage(ControlsMessage.Control.FLASHBACK);
            }
        }

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

        HUD.instance.bossHealthBar.fadeIn();
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

            for (int i=0; i<numBoulders; i++) {

                GameObject boulderGO = GameObject.Instantiate(boulderGameObject, oldBoulders[i].transform.localPosition, oldBoulders[i].transform.localRotation) as GameObject;
                boulders.Add(boulderGO);
                IceBoulder iceBoulder = boulderGO.GetComponent<IceBoulder>();
                iceBoulder.invincible = true;
                iceBoulder.spawnsPickups = oldBoulders[i].GetComponent<IceBoulder>().spawnsPickups;

                // convert to vision
                boulderGO.GetComponent<VisionUser>().becomeVisionNow(VisionUser.VISION_DURATION, oldBoulders[i].GetComponent<VisionUser>());
            }
            
            
        }
        
    }

    /* called just before taking damage.  Make sure health is at least 1 until the final hit is made */
    void PreDamage(AttackInfo ai) {
        if (state == State.FINAL_HIT_FROZEN) {
            ai.damage = 0;
            return;
        }
        if (ai.message == "final_hit") {
            ai.damage = 999999;
        } else {
            if (receivesDamage.health <= ai.damage) {
                receivesDamage.health = 2;
                ai.damage = 1;
            }
        }
    }

    /* called when this takes damage */
    void OnDamage(AttackInfo ai) {

        HUD.instance.bossHealthBar.setHealth(receivesDamage.health);

        if (receivesDamage.health <= 0) {
            
            // clear boulders (not needed?)
            if (boulders.Count > 0) {
                foreach (GameObject bGO in boulders) {
                    bGO.GetComponent<IceBoulder>().fadeOut();
                }
                boulders.Clear();
            }

            // play finalHitScript
            scriptRunner2.runScript(finalHitScript);
            visionUser.cutVisions();
            rb2d.rotation = finalHitRotation;
            SoundManager.instance.playSFXIgnoreVolumeScale(finalHitSound);
            HUD.instance.speedLines.flashWhite();
            //CameraControl.instance.shake(1f, 1f);
            gameObject.layer = LayerMask.NameToLayer("HitNothing");
            CameraControl.instance.moveToPosition(rb2d.position, .2f);


            state = State.FINAL_HIT_FROZEN;
            pos0 = rb2d.position;
            time = 0;
            animator.Play("damaged");

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
        fi.ints["nb"] = numBullets;
        fi.bools["bh"] = bulletHigh;
        fi.floats["bt"] = bulletTime;
        fi.ints["bc"] = bulletCount;
        fi.bools["bsh"] = bulletSwitchHeight;
        fi.bools["shtb"] = switchHalfwayThroughBullet;
        fi.floats["bro"] = boulderRevolveOffset;
        fi.floats["bhd"] = boulderHoldDuration;
        fi.bools["rtf"] = rockThrowFirst;
        fi.floats["tsdfr"] = timeSinceDisplayedFlashbackReminder;

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
        if (state == State.FINAL_HIT_FROZEN) {
            gameObject.layer = LayerMask.NameToLayer("Enemies"); // this was undone in the FINAL_HIT_TUMBLE state
        }
        time = fi.floats["time"];
        wingFlapPlayTime = fi.floats["wfpt"];
        pos0.Set(fi.floats["p0x"], fi.floats["p0y"]);
        pos1.Set(fi.floats["p1x"], fi.floats["p1y"]);
        toRight = fi.bools["tr"];
        count = fi.ints["count"];
        rockThrowAngle = fi.floats["rta"];
        rocksThrownSimultaneously = fi.ints["rts"];
        bobOffsetTime = fi.floats["bot"];
        numBullets = fi.ints["nb"];
        bulletHigh = fi.bools["bh"];
        bulletTime = fi.floats["bt"];
        bulletCount = fi.ints["bc"];
        bulletSwitchHeight = fi.bools["bsh"];
        switchHalfwayThroughBullet = fi.bools["shtb"];
        boulderRevolveOffset = fi.floats["bro"];
        boulderHoldDuration = fi.floats["bhd"];
        rockThrowFirst = fi.bools["rtf"];
        timeSinceDisplayedFlashbackReminder = fi.floats["tsdfr"];

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
    float coveredRockThrowAngle(int index) {
        switch (index) {
        case 0: return 140;
        case 1: return 80;
        case 2: return 270;
        case 3: return 20;
        case 4: return 280;
        case 5: return 190;
        case 6: return 270;
        case 7: return 80;
        case 8: return 280;
        case 9: return 80;
        case 10: return 200;
        case 11: return 20;
        default: return 260;
        }
    }
    bool timePassedThisFrame(float duration) {
        return time >= duration && time - Time.deltaTime < duration;
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
    bool rockThrowFirst = false;

    // for displaying flashback controls if player is doing bad (don't update in timeUser)
    bool playerPressedFlashback = false;
    bool displayedFlashbackReminder = false;
    float timeSinceDisplayedFlashbackReminder = 0; // okay this will be updated in timeUser
    

    // components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    Animator animator;
    TimeUser timeUser;
    ReceivesDamage receivesDamage;
    VisionUser visionUser;
    DefaultDeath defaultDeath;
    EventHappener eventHappener;
    ScriptRunner scriptRunner1;
    ScriptRunner scriptRunner2;
    
#pragma warning disable 414
    EnemyInfo enemyInfo;
#pragma warning restore 414

}
