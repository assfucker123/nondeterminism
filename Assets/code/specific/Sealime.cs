using UnityEngine;
using System.Collections;

public class Sealime : MonoBehaviour {


    public float idleDuration = 0.8f;
    public float hopDistance = 2.0f;
    public float hopDuration = 0.8f;
    public float hopHeight = 2.0f;
    public bool doesBigHop = false;
    public int maxNormalHopStreak = 3;
    public float bigHopDuration = 1.5f;
    public float bigHopHeight = 3.0f;
    public float bulletInitialDelay = .2f;
    public float bulletDuration = 1.0f;
    public float bulletPeriod = .1f;
    public float bulletAngle1 = 30f;
    public float bulletAngle2 = -20f;
    public GameObject bulletGameObject = null;
    public Vector2 bulletSpawn = new Vector2(1f, -.8f);
    public AudioClip bulletSound = null;

    public State state = State.IDLE;

    public enum State {
        IDLE,
        HOPPING, // will hop in opposite direction facing
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
    }

    void Start() {
        // attach to Segment
        segment = Segment.findBottom(rb2d.position);
    }

    /* Called when being spawned from a Portal */
    void OnSpawn(SpawnInfo si) {
        flippedHoriz = !si.faceRight;

        // attach to Segment
        segment = Segment.findBottom(rb2d.position);

        if (segment.p0.x + hopDistance > rb2d.position.x) { //too far left, need to hop right
            hoppingRightNext = true;
            hoppingFacingRightNext = true;
        } else if (segment.p1.x - hopDistance < rb2d.position.x) { //too far right, need to hop left
            hoppingRightNext = false;
            hoppingFacingRightNext = false;
        } else {
            hoppingRightNext = flippedHoriz;
            hoppingFacingRightNext = flippedHoriz;
        }
        hoppingRight = hoppingRightNext;
        flippedHoriz = hoppingRight;
        doingBigHopNext = false;
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;
        if (defaultDeath.activated)
            return;

        float prevTime = time;
        time += Time.deltaTime;

        switch (state) {
        case State.IDLE:
            rb2d.velocity = new Vector2(0, rb2d.velocity.y);

            if (!colFinder.hitBottom) { // wait until landed to start
                time -= Time.deltaTime;
            }

            if (time >= idleDuration) {
                // begin hop

                hoppingRight = hoppingRightNext;
                hoppingFacingRight = hoppingFacingRightNext;
                doingBigHop = doingBigHopNext;

                if (doingBigHop) {
                    animator.Play("opening");
                    normalHopsInARow = 0;
                    bulletTime = 0;
                } else {
                    animator.Play("jump");
                    normalHopsInARow++;
                }
                flippedHoriz = !hoppingFacingRight;
                hoppingRightNext = !hoppingRight;
                preHop = rb2d.position;
                state = State.HOPPING;
                time -= idleDuration;

            }
            break;
        case State.HOPPING:
            float gravity = rb2d.gravityScale * Physics2D.gravity.y;
            float dur = hopDuration;
            float height = hopHeight;
            if (doingBigHop) {
                dur = bigHopDuration;
                height = bigHopHeight;
            }

            time = Mathf.Min(bigHopDuration, time);
            Vector2 pos = new Vector2();
            if (hoppingRight) {
                pos.x = preHop.x + hopDistance * (time / dur);
            } else {
                pos.x = preHop.x - hopDistance * (time / dur);
            }
            if (time < dur / 2) {
                pos.y = preHop.y + Utilities.easeOutQuad(time, 0, height, dur / 2);
            } else {
                pos.y = preHop.y + Utilities.easeInQuad(time - dur / 2, height, -height, dur / 2);
            }
            rb2d.MovePosition(pos);

            // rotate and fire bullets
            if (doingBigHop) {
                float rot = 0;
                if (time < bulletInitialDelay) {
                    rot = Utilities.easeInOutQuad(time, 0, bulletAngle1, bulletInitialDelay);
                } else if (time - bulletInitialDelay < bulletDuration) {
                    rot = Utilities.easeInOutQuad(time - bulletInitialDelay, bulletAngle1, bulletAngle2 - bulletAngle1, bulletDuration);
                } else {
                    rot = Utilities.easeInOutQuadClamp(time - bulletInitialDelay - bulletDuration, bulletAngle2, -bulletAngle2, dur - bulletInitialDelay - bulletDuration);
                }
                if (flippedHoriz)
                    rot *= -1;
                spriteObject.transform.localRotation = Utilities.setQuat(rot);

                if (time >= bulletInitialDelay && time - bulletInitialDelay < bulletDuration) {
                    bulletTime += Time.deltaTime;
                    if (bulletTime >= bulletPeriod) {
                        // spawn bullet
                        Vector2 relPos = bulletSpawn;
                        float heading = rot;
                        if (flippedHoriz) {
                            relPos.x *= -1;
                            heading = -(180 - heading);
                        }
                        relPos = Utilities.rotateAroundPoint(relPos, Vector2.zero, rot * Mathf.PI / 180);
                        GameObject bulletGO = GameObject.Instantiate(bulletGameObject,
                            rb2d.position + relPos,
                            Utilities.setQuat(heading)) as GameObject;
                        Bullet bullet = bulletGO.GetComponent<Bullet>();
                        if (flippedHoriz) {
                            bullet.heading = heading;
                        } else {
                            bullet.heading = rot;
                        }
                        if (visionUser.isVision) { //make bullet a vision if this is also a vision
                            VisionUser bvu = bullet.GetComponent<VisionUser>();
                            bvu.becomeVisionNow(visionUser.duration - visionUser.time, visionUser);
                        } else { // not a vision
                            SoundManager.instance.playSFXRandPitchBend(bulletSound);
                        }

                        bulletTime -= bulletPeriod;
                    }
                } else if (time - bulletInitialDelay >= bulletDuration && prevTime - bulletInitialDelay < bulletDuration) {
                    // just finished firing bullets, close mouth
                    animator.Play("closing");
                }
            }

            // decide if next hop should be big or not (and create vision if so)
            if (!visionUser.isVision) {
                if (idleDuration + dur - time <= VisionUser.VISION_DURATION &&
                    idleDuration + dur - prevTime > VisionUser.VISION_DURATION) {
                    hoppingRightNext = !hoppingRight;
                    doingBigHopNext = false;
                    if (doesBigHop) {
                        doingBigHopNext = (timeUser.randomValue() < (1.0f + normalHopsInARow) / (1 + maxNormalHopStreak));
                    }

                    if (doingBigHopNext) {
                        hoppingFacingRightNext = (rb2d.position.x < Player.instance.rb2d.position.x);

                        // create vision
                        timeUser.addCurrentFrameInfo(); //lets vision have info of what to do next
                        GameObject vGO = visionUser.createVision(VisionUser.VISION_DURATION);
                    }

                }
            }

            if (time >= dur) {
                // back to idle
                spriteObject.transform.localRotation = Utilities.setQuat(0);
                if (!doingBigHopNext) {
                    hoppingRightNext = !hoppingRight;
                    hoppingFacingRightNext = !hoppingRight;
                }
                time -= dur;
                animator.Play("idle");
                state = State.IDLE;
            }
            break;
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

    /* called when this becomes a vision */
    void TimeSkip(float timeInFuture) {
        Debug.Assert(state == State.HOPPING);

        // Start() hasn't been called yet
        //Start();

        // increment time
        time += timeInFuture;

        // land
        Vector2 pos = new Vector2();
        if (hoppingRight) {
            pos.x = preHop.x + hopDistance;
        } else {
            pos.x = preHop.x - hopDistance;
        }
        pos.y = preHop.y;
        rb2d.position = pos;

        // do big hop
        state = State.IDLE;
        time = idleDuration;

    }

    /* called when this takes damage */
    void OnDamage(AttackInfo ai) {
        if (receivesDamage.health <= 0) {
            flippedHoriz = ai.impactToRight();
            animator.Play("damage");
        }
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["time"] = time;

        fi.ints["nhiar"] = normalHopsInARow;
        fi.bools["hr"] = hoppingRight;
        fi.bools["hfr"] = hoppingFacingRight;
        fi.bools["dbh"] = doingBigHop;
        fi.bools["hrn"] = hoppingRightNext;
        fi.bools["hfrn"] = hoppingFacingRightNext;
        fi.bools["dbhn"] = doingBigHopNext;
        fi.floats["phx"] = preHop.x;
        fi.floats["phy"] = preHop.y;
        fi.floats["bt"] = bulletTime;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["time"];

        normalHopsInARow = fi.ints["nhiar"];
        hoppingRight = fi.bools["hr"];
        hoppingFacingRight = fi.bools["hfr"];
        doingBigHop = fi.bools["dbh"];
        hoppingRightNext = fi.bools["hrn"];
        hoppingFacingRightNext = fi.bools["hfrn"];
        doingBigHopNext = fi.bools["dbhn"];
        preHop.Set(fi.floats["phx"], fi.floats["phy"]);
        bulletTime = fi.floats["bt"];
    }

    float time;
    Segment segment;
    private Vector2 preHop = new Vector2();
    int normalHopsInARow = 0;
    bool hoppingRight = false;
    bool hoppingFacingRight = false;
    bool doingBigHop = false;
    bool hoppingRightNext = false;
    bool hoppingFacingRightNext = false;
    bool doingBigHopNext = false;
    float bulletTime = 0;

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

}
