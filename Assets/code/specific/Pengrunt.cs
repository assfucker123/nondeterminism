using UnityEngine;
using System.Collections;

public class Pengrunt : MonoBehaviour {

    // IDEA: Move by bouncing from spot to spot in defined arcs

    public enum State {
        IDLE,
        WALK, //walk for set duration (short or long).  Turn around at walls and edges
        SPRAY, //pushed backwards during this
        DEAD //don't do anything; DefaultDeath takes care of this
    }

    public float idleDuration = .5f;
    public float walkShortDuration = 1.1f;
    public float walkLongDuration = 1.9f;
    public float sprayDuration = .7f;
    public float speed = 8;
    public float spraySpeed = 4;
    public GameObject bulletGameObject;
    public Vector2 bulletSpawn = new Vector2();
    public float bulletSpread = 20;
    public float bulletPeriod = .04f;
    public State state = State.IDLE;
    public bool aimRight = false;

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
        spriteObject = this.transform.Find("spriteObject").gameObject;
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
            // next state
            if (time >= idleDuration && colFinder.hitBottom) {
                state = State.WALK;
                animator.Play("walk");
                playerNearbyDuringWalk = false;
                time = 0;
                if (timeUser.randomValue() < .5f) {
                    duration = walkShortDuration;
                } else {
                    duration = walkLongDuration;
                }
            }
            break;

        case State.WALK:

            float s = speed;
            if (flippedHoriz)
                s *= -1;
            float x = segment.travel(rb2d.position.x, s, Time.fixedDeltaTime);
            bool turnaround = segment.travelTurnsAround(rb2d.position.x, s, Time.fixedDeltaTime);
            rb2d.MovePosition(new Vector2(x, rb2d.position.y));
            if (turnaround) {
                flippedHoriz = !flippedHoriz;
            }

            //only consider going to other states (like spray) if player is nearby
            if (!playerNearbyDuringWalk) {
                time -= Time.deltaTime;

                //check if player is nearby
                Vector2 pos = Player.instance.rb2d.position;
                if (rb2d.position.x - 40 < pos.x && pos.x < rb2d.position.x + 40 &&
                    rb2d.position.y - 3 < pos.y && pos.y < rb2d.position.y + 6) {
                    playerNearbyDuringWalk = true;
                }

            }

            // create a vision at the correct time
            if (!visionUser.isVision) {
                if (prevTime + VisionUser.VISION_DURATION < duration &&
                    time + VisionUser.VISION_DURATION >= duration) {
                    GameObject vGO = visionUser.createVision(VisionUser.VISION_DURATION);

                    // would also be a good time to decide which direction to fire in
                    aimRight = (Player.instance.rb2d.position.x > rb2d.position.x);
                    vGO.GetComponent<Pengrunt>().aimRight = aimRight;
                }
            }

            // detect when going to SPRAY state
            if (time >= duration) {
                state = State.SPRAY;
                time -= duration;
                bulletTime = 0;

                flippedHoriz = !aimRight;
                animator.Play("idle");
            }

            break;
        case State.SPRAY:

            //fire bullets
            bulletTime += Time.deltaTime;
            while (bulletTime >= bulletPeriod) {

                //spawning bullet
                Vector2 relSpawnPosition = bulletSpawn;
                relSpawnPosition.x *= spriteRenderer.transform.localScale.x;
                relSpawnPosition.y *= spriteRenderer.transform.localScale.y;
                float heading = 0;
                if (flippedHoriz) {
                    heading = 180;
                }
                heading += (timeUser.randomValue() * 2 - 1) * bulletSpread;
                GameObject bulletGO = GameObject.Instantiate(bulletGameObject,
                    rb2d.position + relSpawnPosition,
                    Utilities.setQuat(heading)) as GameObject;
                Bullet bullet = bulletGO.GetComponent<Bullet>();
                bullet.heading = heading;
                //make bullet a vision if this is also a vision
                if (visionUser.isVision) {
                    VisionUser bvu = bullet.GetComponent<VisionUser>();
                    bvu.becomeVisionNow(visionUser.duration - visionUser.time, visionUser);
                }

                bulletTime -= bulletPeriod;
            }

            //pushed backward
            s = -spraySpeed;
            if (flippedHoriz)
                s *= -1;
            x = segment.travelClamp(rb2d.position.x, s, Time.fixedDeltaTime);
            rb2d.MovePosition(new Vector2(x, rb2d.position.y));

            //detect done with spray
            if (time >= sprayDuration) {
                time -= sprayDuration;
                state = State.IDLE;
            }
            break;
        }

	}

    // called when this becomes a vision
    void TimeSkip(float timeInFuture) {
        Debug.Assert(state == State.WALK); //vision should only be made when Pengrunt is walking

        // Start() hasn't been called yet
        Start();

        // increment time
        time += timeInFuture;

        // change position after duration
        float s = speed;
        if (flippedHoriz)
            s *= -1;
        float x = segment.travel(rb2d.position.x, s, VisionUser.VISION_DURATION);
        bool turnaround = segment.travelTurnsAround(rb2d.position.x, s, VisionUser.VISION_DURATION);
        rb2d.position = new Vector2(x, rb2d.position.y);
        if (turnaround) {
            flippedHoriz = !flippedHoriz;
        }
    }

    void OnDamage(AttackInfo ai) {
        if (receivesDamage.health <= 0) {
            flippedHoriz = ai.impactToRight();
            animator.Play("damage");
        }
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int) state;
        fi.floats["time"] = time;
        fi.floats["duration"] = duration;
        fi.bools["aimRight"] = aimRight;
        fi.bools["pndw"] = playerNearbyDuringWalk;
    }
    void OnRevert(FrameInfo fi) {
        state = (State) fi.state;
        time = fi.floats["time"];
        duration = fi.floats["duration"];
        aimRight = fi.bools["aimRight"];
        playerNearbyDuringWalk = fi.bools["pndw"];
    }

    float time = 0;
    float duration = 0;
    Segment segment = null;
    float bulletTime = 0;
    bool playerNearbyDuringWalk = false;

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
