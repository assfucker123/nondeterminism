using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Vengemole : MonoBehaviour {

    /*
Vengemole plan:

- idle, waits

- Goes underground
	- At this instant, create vision of Vengemole popping out of the ground somewhere
- waits underground
- Pops out of the ground
	- back to idle OR
	- swings hammer OR
	- throws hammer
- back to waits
    */

    public State state = State.IDLE;
    public float idleMinDuration = .5f;
    public float idleMaxDuration = 4;
    public float undergroundMaxDuration = 3f; // min duration is VISION_DURATION
    public float undergroundGetHammerMinDuration = 2f;
    public float undergroundGetHammerMaxDuration = 4f;
    public float fallChangeLayerDuration = .2f;
    public float riseChangeLayerDuration = .1f;
    public float throwHammerChance = .7f;
    public float riseSwingDelay = .2f;
    public float hammerHitboxDelay = .25f;
    public float hammerHitboxDuration = .1f;
    public float throwDelay = .08f;
    public float postSwingDuration = .6f;
    public float hammerSwingDuration = .1f;
    public Vector2 hammerParticleCenter = new Vector2();
    public int hammerNumParticles = 15;
    public float hammerParticleXSpread = 4;
    public float hammerParticleAngleSpread = 30;
    public float hammerParticleVelocityMin = 10;
    public float hammerParticleVelocityMax = 20;
    public float hammerParticleFadeDurationRandIncrease = .2f;
    public float hammerCamShakeMagnitude = .5f;
    public float hammerCamShakeDuration = .5f;
    public GameObject hammerParticleGameObject;
    public AudioClip hammerSwingSound;
    public AudioClip hammerSlamSound;
    public AudioClip burrowSound;
    public AudioClip burrowUpSound;

    public GameObject hammerGameObject;
    public Vector2 hammerSpawnPos = new Vector2();
    public float hammerSpawnRotation = -20f;

    public enum State {
        IDLE,
        UNDERGROUND,
        RISE,
        RISE_SWING,
        RISE_THROW,
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
        playerAwareness = GetComponent<PlayerAwareness>();
        attackObject = transform.Find("attackObject").GetComponent<AttackObject>();
        leftAttack = transform.Find("hammerLeftAttack").GetComponent<AttackObject>();
        rightAttack = transform.Find("hammerRightAttack").GetComponent<AttackObject>();
        
    }

    void Start() {

        if (visionUser.isVision)
            return;

        // attach to Segment
        segment = Segment.findBottom(rb2d.position);

        idleState();
        animator.Play("idle");
    }

    void idleState() {
        state = State.IDLE;
        time = 0;
        duration = timeUser.randomValue() * (idleMaxDuration - idleMinDuration) + idleMinDuration;
    }

    void undergroundState() {
        if (state == State.UNDERGROUND) return;
        // decide where to rise next
        bool attackingPlayer = playerAwareness.awareOfPlayer;
        Vector2 plrPos = Player.instance.rb2d.position;
        Segment nextSegment;
        if (attackingPlayer) {
            nextSegment = Segment.segmentClosestToPoint(Segment.bottomSegments, plrPos);
            posAfterUnderground = nextSegment.interpolate(timeUser.randomValue());
            flippedHorizAfterUnderground = (posAfterUnderground.x > plrPos.x);
        } else {
            List<Segment> segs = Segment.bottomSegments;
            if (segs.Count > 1 && segment != null) {
                segs.Remove(segment);
            }
            nextSegment = Segment.weightedRandom(segs, timeUser.randomValue());
            posAfterUnderground = nextSegment.interpolate(timeUser.randomValue());
            flippedHorizAfterUnderground = (timeUser.randomValue() < .5f);
        }
        posAfterUnderground.y += enemyInfo.spawnDist; // adjusting for spawn distance

        // action to take after rising
        if (attackingPlayer) {
            if (enemyInfo.id == EnemyInfo.ID.VENGEMOLE_CAN_THROW) {
                if (timeUser.randomValue() < throwHammerChance) {
                    stateAfterUnderground = State.RISE_THROW;
                } else {
                    stateAfterUnderground = State.RISE_SWING;
                }
            } else {
                stateAfterUnderground = State.RISE_SWING;
            }
        } else {
            stateAfterUnderground = State.RISE;
        }

        // state transition
        time = 0;
        if (hasHammer) {
            animator.Play("fall");
        } else {
            animator.Play("fall_nohammer");
        }
        state = State.UNDERGROUND;
        attackObject.enabled = false;
        if (!visionUser.isVision)
            SoundManager.instance.playSFXIfOnScreenRandPitchBend(burrowSound, rb2d.position);

        // how long underground?
        if (hasHammer) {
            duration = timeUser.randomValue() * (undergroundMaxDuration - VisionUser.VISION_DURATION + .1f) + VisionUser.VISION_DURATION + .1f;
        } else {
            duration = timeUser.randomValue() * (undergroundGetHammerMaxDuration - undergroundGetHammerMinDuration) + undergroundGetHammerMinDuration;
        }

    }

    void riseState() {
        state = stateAfterUnderground;
        time = 0;
        rb2d.position = posAfterUnderground;
        flippedHoriz = flippedHorizAfterUnderground;
        hasHammer = true;
        state = stateAfterUnderground;
        attackObject.enabled = true;
        if (!visionUser.isVision) {
            SoundManager.instance.playSFXIfOnScreenRandPitchBend(burrowUpSound, rb2d.position);
        }
        switch (state) {
        case State.RISE:
            idleState();
            animator.Play("rise");
            break;
        case State.RISE_SWING:
        case State.RISE_THROW:
            animator.Play("rise_to_swing");
            duration = postSwingDuration;
            break;
        }
    }

    void hammerThrow() {
        // create hammer
        Vector3 hamPos = transform.localPosition;
        hamPos.y += hammerSpawnPos.y;
        float hamRotation = hammerSpawnRotation;
        if (flippedHoriz) {
            hamRotation *= -1;
            hamPos.x -= hammerSpawnPos.x;
        } else {
            hamPos.x += hammerSpawnPos.x;
        }
        GameObject hammerGO = GameObject.Instantiate(hammerGameObject, hamPos, Utilities.setQuat(hamRotation)) as GameObject;
        VengemoleHammer vm = hammerGO.GetComponent<VengemoleHammer>();
        vm.flingRight = !flippedHoriz;
        if (visionUser.isVision) {
            hammerGO.GetComponent<VisionUser>().becomeVisionNow(visionUser.timeLeft, visionUser);
        }

        hasHammer = false;
    }

    /* Called when being spawned from a Portal */
    void OnSpawn(SpawnInfo si) {
        flippedHoriz = !si.faceRight;
    }

    void FixedUpdate() {

        if (timeUser.shouldNotUpdate)
            return;
        if (defaultDeath.activated)
            return;

        time += Time.deltaTime;

        Vector2 v = rb2d.velocity;

        switch (state) {
        case State.IDLE:
            if (!visionUser.isVision && time >= riseChangeLayerDuration) {
                gameObject.layer = LayerMask.NameToLayer("Enemies");
            }
            if (time >= duration) {
                // go underground
                undergroundState();
            }
            break;
        case State.UNDERGROUND:

            // changing layer to remove hitboxes
            if (time >= fallChangeLayerDuration) {
                gameObject.layer = LayerMask.NameToLayer("Visions");
            }
            // creating vision of rising
            if (visionUser.shouldCreateVisionThisFrame(time-Time.deltaTime, time, duration, VisionUser.VISION_DURATION)) {
                visionUser.createVision(VisionUser.VISION_DURATION);
            }

            if (time >= duration) {
                // rise from the ground
                riseState(); // uses properties already set when undergroundState() was called
            }
            break;
        case State.RISE:
        case State.RISE_SWING:
        case State.RISE_THROW:
            // changing layer to add hitboxes
            if (!visionUser.isVision && time >= riseChangeLayerDuration) {
                gameObject.layer = LayerMask.NameToLayer("Enemies");
            }
            // swinging hammer
            if ((state == State.RISE_SWING || state == State.RISE_THROW) &&
                time-Time.deltaTime < riseSwingDelay && time >= riseSwingDelay) {
                if (state == State.RISE_SWING) {
                    animator.Play("swing");
                } else {
                    animator.Play("throw");
                }
                if (!visionUser.isVision) {
                    SoundManager.instance.playSFXIfOnScreenRandPitchBend(hammerSwingSound, rb2d.position);
                }
            }
            // swinging hammer hitboxes
            if (state == State.RISE_SWING && !visionUser.isVision) {
                if (hammerHitboxDelay <= time && time <= hammerHitboxDelay + hammerHitboxDuration) {
                    if (flippedHoriz)
                        leftAttack.enabled = true;
                    else
                        rightAttack.enabled = true;
                } else {
                    if (flippedHoriz)
                        leftAttack.enabled = false;
                    else
                        rightAttack.enabled = false;
                }
            }
            // throwing hammer
            if (state == State.RISE_THROW &&
                time-Time.deltaTime < riseSwingDelay+throwDelay && time >= riseSwingDelay + throwDelay) {
                hammerThrow();
            }
            // hammer hitting the ground after swing
            if (state == State.RISE_SWING && time-Time.deltaTime < hammerSwingDuration && hammerSwingDuration <= time) {
                float centerY = rb2d.position.y + hammerParticleCenter.y;
                float centerX = rb2d.position.x;
                Vector2 raycastPt = new Vector2(0, centerY);
                if (flippedHoriz) {
                    centerX -= hammerParticleCenter.x;
                    raycastPt.x = centerX + hammerParticleXSpread;
                } else {
                    centerX += hammerParticleCenter.x;
                    raycastPt.x = centerX - hammerParticleXSpread;
                }
                // do raycas=t to see if hammer hit the ground
                raycastPt.y += 1f;
                RaycastHit2D rh2d = Physics2D.Raycast(raycastPt, new Vector2(0,-1), 1.1f, 1 << LayerMask.NameToLayer("Default"));
                if (rh2d.collider != null && !visionUser.isVision) {
                    // spawn particles
                    for (int i = 0; i < hammerNumParticles; i++) {
                        float inter = i / (hammerNumParticles - 1.0f)*2 - 1; // in [-1, 1]
                        float magnitude = timeUser.randomValue() * (hammerParticleVelocityMax - hammerParticleVelocityMin) + hammerParticleVelocityMin;
                        Vector2 pos = new Vector2(centerX + inter*hammerParticleXSpread, centerY);
                        float angle = Mathf.PI/2 - inter * hammerParticleAngleSpread *Mathf.PI/180;
                        ImpactShard impactShard = (GameObject.Instantiate(hammerParticleGameObject,pos,Quaternion.identity) as GameObject).GetComponent<ImpactShard>();
                        impactShard.initialVelocity.Set(magnitude * Mathf.Cos(angle), magnitude * Mathf.Sin(angle));
                        impactShard.GetComponent<VisualEffect>().duration += timeUser.randomValue() * hammerParticleFadeDurationRandIncrease;
                    }
                    // shake ground
                    CameraControl.instance.shake(hammerCamShakeMagnitude, hammerCamShakeDuration);
                    //
                    SoundManager.instance.playSFXIfOnScreenRandPitchBend(hammerSlamSound, rb2d.position);
                }
            }

            // going back underground
            if (time >= postSwingDuration) {
                undergroundState();
            }
            break;


        }

        rb2d.velocity = v;

    }

    /* called when this becomes a vision */
    void TimeSkip(float timeInFuture) {

        // Start() hasn't been called yet

        time += timeInFuture;

        // rise from the ground
        riseState(); // uses properties already set in idle

    }

    /* called when this takes damage */
    void OnDamage(AttackInfo ai) {
        playerAwareness.alwaysAware = true;
        if (receivesDamage.health <= 0) {
            flippedHoriz = ai.impactToRight();
            animator.Play("damage");
        } else if (state == State.IDLE) {
            undergroundState();
        }
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["time"] = time;
        fi.floats["d"] = duration;
        fi.floats["paux"] = posAfterUnderground.x;
        fi.floats["pauy"] = posAfterUnderground.y;
        fi.ints["sau"] = (int)stateAfterUnderground;
        fi.bools["fhau"] = flippedHorizAfterUnderground;
        fi.bools["hh"] = hasHammer;
        fi.bools["aoe"] = attackObject.enabled;
        fi.bools["rae"] = rightAttack.enabled;
        fi.bools["lae"] = leftAttack.enabled;
        fi.ints["gol"] = gameObject.layer;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["time"];
        duration = fi.floats["d"];
        posAfterUnderground.Set(fi.floats["paux"], fi.floats["pauy"]);
        stateAfterUnderground = (State)fi.ints["sau"];
        flippedHorizAfterUnderground = fi.bools["fhau"];
        hasHammer = fi.bools["hh"];
        attackObject.enabled = fi.bools["aoe"];
        rightAttack.enabled = fi.bools["rae"];
        leftAttack.enabled = fi.bools["lae"];
        gameObject.layer = fi.ints["gol"];
    }

    float time = 0;
    float duration = 0;
    Vector2 posAfterUnderground = new Vector2();
    State stateAfterUnderground = State.RISE;
    bool flippedHorizAfterUnderground = false;
    bool hasHammer = true;
    Segment segment;

    // components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    Animator animator;

#pragma warning disable 414
    ColFinder colFinder;
#pragma warning restore 414

    TimeUser timeUser;
    ReceivesDamage receivesDamage;
    VisionUser visionUser;
    DefaultDeath defaultDeath;
    EnemyInfo enemyInfo;
    PlayerAwareness playerAwareness;
    AttackObject attackObject;
    AttackObject leftAttack;
    AttackObject rightAttack;

}
