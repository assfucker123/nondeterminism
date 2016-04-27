using UnityEngine;
using System.Collections;

/* Three types:
 * SMOSEY: Default Smosey, just idles around and doesn't attack
 * SMOSEY_BULLET: Will begin to fire bullets at player if they see player or take damage
 * SMOSEY_SWARM: Acts like SMOSEY, but when attacked will swarm the player and also call swarm() for nearby SMOSEY_SWARMs and startFiringBullet() for nearby SMOSEY_BULLETs
*/

public class Smosey : MonoBehaviour {
    
    public State state = State.IDLE;
    public float idleXDist = 4;
    public float idleYDist = 2;
    public float idlePeriod = 1;
    public float bulletMinDuration = 1.5f;
    public float bulletMaxDuraiton = 2;
    public Vector2 bulletSpawnPos = new Vector2(0, 0);
    public GameObject bulletGameObject;
    public GameObject bulletMuzzleGameObject;
    public AudioClip bulletSound;
    public float swarmAccel = 30;
    public float swarmMaxSpeed = 25;
    public float swarmOffsetMax = 2;
    public float swarmOffsetChangePeriod = 1.2f;
    public float callSwarmRadius = 8;


    public enum State {
        IDLE,
        SWARM, // going to player
        DEAD //don't do anything; DefaultDeath takes care of this
    }

    public void startFiringBullet() {
        if (firingBullet)
            return;
        if (enemyInfo.id != EnemyInfo.ID.SMOSEY_BULLET)
            return;
        firingBullet = true;
        firingBulletTime = 0;
        bulletDuration = timeUser.randomValue() * (bulletMaxDuraiton - bulletMinDuration) + bulletMinDuration;
        playerAwareness.alwaysAware = true;
    }

    public void swarm() {
        if (state == State.SWARM)
            return;
        if (enemyInfo.id != EnemyInfo.ID.SMOSEY_SWARM)
            return;
        state = State.SWARM;
        time = 0;
        swarmPosOffset.Set((timeUser.randomValue() * 2 - 1) * swarmOffsetMax, (timeUser.randomValue() * 2 - 1) * swarmOffsetMax);
    }

    // calls swarm or startFiringBullet of all Smoseys nearby
    void callSwarm() {
        if (enemyInfo.id != EnemyInfo.ID.SMOSEY_SWARM)
            return;
        Smosey[] smoseys = GameObject.FindObjectsOfType<Smosey>();
        foreach (Smosey smosey in smoseys) {
            if ((smosey.rb2d.position - rb2d.position).sqrMagnitude > callSwarmRadius * callSwarmRadius)
                continue;
            if (smosey.enemyInfo.id == EnemyInfo.ID.SMOSEY_BULLET) {
                smosey.startFiringBullet();
            }
            if (smosey.enemyInfo.id == EnemyInfo.ID.SMOSEY_SWARM) {
                smosey.swarm();
            }
        }
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

    public bool passive {  get { return enemyInfo.id == EnemyInfo.ID.SMOSEY; } }

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
    }

    void Start() {
        if (!visionUser.isVision) {
            idleCenter = rb2d.position;
            time = idlePeriod * timeUser.randomValue(); // offset for idle movement
        }
        
    }

    /* Called when being spawned from a Portal */
    void OnSpawn(SpawnInfo si) {
        flippedHoriz = !si.faceRight;
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;
        if (defaultDeath.activated)
            return;

        time += Time.deltaTime;

        Vector2 v = rb2d.velocity;

        switch (state) {
        case State.IDLE:
            // move in infinity symbol pattern
            Vector2 pos = new Vector2();
            pos.x = Mathf.Cos(time / idlePeriod * Mathf.PI * 2) * idleXDist;
            pos.y = Mathf.Sin(time * 2 / idlePeriod * Mathf.PI * 2) * idleYDist;
            pos += idleCenter;
            rb2d.MovePosition(pos);
            bool faceRight = !flippedHoriz;
            if (playerAwareness.awareOfPlayer) {
                faceRight = Player.instance.rb2d.position.x > rb2d.position.x;
            } else {
                float modT = Utilities.fmod(time, idlePeriod) / idlePeriod;
                faceRight = (modT > .5f);
            }
            flippedHoriz = !faceRight;

            // firing bullet
            if (firingBullet) {
                firingBulletTime += Time.deltaTime;
                if (visionUser.shouldCreateVisionThisFrame(firingBulletTime - Time.deltaTime, firingBulletTime, bulletDuration, VisionUser.VISION_DURATION) && !visionUser.isVision) {
                    // set aim position
                    if (Player.instance != null)
                        bulletAimPos = Player.instance.rb2d.position;
                    timeUser.addCurrentFrameInfo();
                    GameObject vGO = visionUser.createVision(VisionUser.VISION_DURATION);
                    vGO.GetComponent<Smosey>().idleCenter = idleCenter;
                }
                if (firingBulletTime >= bulletDuration) {
                    // spawn bullet
                    Vector2 spawnPos = pos;
                    if (flippedHoriz) {
                        spawnPos.x -= bulletSpawnPos.x;
                    } else {
                        spawnPos.x += bulletSpawnPos.x;
                    }
                    spawnPos.y += bulletSpawnPos.y;
                    float heading = 180/Mathf.PI * Mathf.Atan2(bulletAimPos.y - spawnPos.y, bulletAimPos.x - spawnPos.x);
                    GameObject bulletGO = GameObject.Instantiate(bulletGameObject,
                        spawnPos,
                        Utilities.setQuat(heading)) as GameObject;
                    Bullet bullet = bulletGO.GetComponent<Bullet>();
                    bullet.heading = heading;
                    GameObject bulletMuzzleGO = GameObject.Instantiate(bulletMuzzleGameObject, spawnPos, Utilities.setQuat(heading)) as GameObject;
                    if (visionUser.isVision) { // make bullet a vision if this is also a vision
                        VisionUser bvu = bullet.GetComponent<VisionUser>();
                        bvu.becomeVisionNow(visionUser.timeLeft, visionUser);
                        bulletMuzzleGO.GetComponent<VisionUser>().becomeVisionNow(visionUser.timeLeft, visionUser);
                    } else {
                        SoundManager.instance.playSFXIfOnScreenRandPitchBend(bulletSound, rb2d.position);
                    }
                    
                    firingBulletTime -= bulletDuration;
                    bulletDuration = timeUser.randomValue() * (bulletMaxDuraiton - bulletMinDuration) + bulletMinDuration;
                }
            } else {
                if (enemyInfo.id == EnemyInfo.ID.SMOSEY_BULLET && playerAwareness.awareOfPlayer) {
                    startFiringBullet();
                }
            }
            break;
        case State.SWARM:
            Vector2 swarmPos = Player.instance.rb2d.position;
            swarmPos += swarmPosOffset;
            float angle = Mathf.Atan2(swarmPos.y-rb2d.position.y, swarmPos.x-rb2d.position.x);
            v += swarmAccel * Time.deltaTime * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            if (v.sqrMagnitude > swarmMaxSpeed * swarmMaxSpeed) {
                v.Normalize();
                v *= swarmMaxSpeed;
            }
            flippedHoriz = (rb2d.position.x > swarmPos.x);
            if (time >= swarmOffsetChangePeriod) {
                swarmPosOffset.Set((timeUser.randomValue() * 2 - 1) * swarmOffsetMax, (timeUser.randomValue() * 2 - 1) * swarmOffsetMax);
                time -= swarmOffsetChangePeriod;
            }
            break;
        }

        rb2d.velocity = v;

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

        // Start() hasn't been called yet
        //Start();

        // increment time
        time += timeInFuture;
        firingBulletTime += timeInFuture;
    }

    /* called when this takes damage */
    void OnDamage(AttackInfo ai) {
        // be aware of player
        playerAwareness.alwaysAware = true;
        if (enemyInfo.id == EnemyInfo.ID.SMOSEY_SWARM && state != State.SWARM) {
            swarm();
            callSwarm();
        }
        // die
        if (receivesDamage.health <= 0) {
            flippedHoriz = ai.impactToRight();
            animator.Play("damage");
        }
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["time"] = time;
        fi.bools["fb"] = firingBullet;
        fi.floats["fbt"] = firingBulletTime;
        fi.floats["bapx"] = bulletAimPos.x;
        fi.floats["bapy"] = bulletAimPos.y;
        fi.floats["bd"] = bulletDuration;
        fi.floats["spox"] = swarmPosOffset.x;
        fi.floats["spoy"] = swarmPosOffset.y;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["time"];
        firingBullet = fi.bools["fb"];
        firingBulletTime = fi.floats["fbt"];
        bulletAimPos.Set(fi.floats["bapx"], fi.floats["bapy"]);
        bulletDuration = fi.floats["bd"];
        swarmPosOffset.Set(fi.floats["spox"], fi.floats["spoy"]);
    }

    float time;
    Vector2 idleCenter = new Vector2();
    bool firingBullet = false;
    float firingBulletTime = 0;
    Vector2 bulletAimPos = new Vector2();
    float bulletDuration = 0;
    Vector2 swarmPosOffset = new Vector2();

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

}
