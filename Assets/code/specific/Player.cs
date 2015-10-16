using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public static Player instance { get { return _instance; } }

    ///////////////////////
    // PUBLIC PROPERTIES //
    ///////////////////////

    public float groundAccel = 100;
    public float groundMaxSpeed = 10;
    public float groundFriction = 50;
    public float airAccel = 100;
    public float airMaxSpeed = 10;
    public float airFriction = 50;
    public float gravity = 100;
    public float terminalVelocity = 30;
    public float jumpSpeed = 20;
    public float jumpMinDuration = .8f;
    public float jumpMaxDuration = 1.5f;
    public float idleGunDownDuration = .8f;
    public GameObject bulletGameObject;
    public GameObject bulletMuzzleGameObject;
    public Vector2 bulletSpawn = new Vector2(1f, .4f);
    public float revertSpeed = 2.0f;
    public float revertEaseDuration = .4f;
    public float minRevertDuration = 1.0f;
    public float damageSpeed = 10;
    public float damageFriction = 20;
    public float damageDuration = .5f;
    public float mercyInvincibilityDuration = 1.0f;
    public State state = State.GROUND;

    public Rigidbody2D rb2d { get { return _rb2d; } }
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
    public bool jumping {
        get { return jumpTime < jumpMaxDuration;  }
    }
    public bool canFireBullet {
        get {
            switch (state) {
            case State.GROUND:
            case State.AIR:
                return true;
            default:
                return false;
            }
        }
    }

    public enum State:int {
        GROUND,
        AIR,
        DAMAGE
    }

    /////////////////////
    // EVENT FUNCTIONS //
    /////////////////////

    void Awake() {
        _instance = this;
        _rb2d = GetComponent<Rigidbody2D>();
        spriteObject = this.transform.Find("spriteObject").gameObject;
        spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        animator = spriteObject.GetComponent<Animator>();
        colFinder = GetComponent<ColFinder>();
        timeUser = GetComponent<TimeUser>();
        receivesDamage = GetComponent<ReceivesDamage>();
    }
    void Start() {
        
	}

	void Update() {

        //set button vars
        horiz = Input.GetAxis("Horizontal");
        leftHeld = (horiz < 0);
        rightHeld = (horiz > 0);
        jumpPressed = Input.GetButtonDown("Jump");
        jumpHeld = Input.GetButton("Jump");

        //control reverting
        timeReverting();

        if (timeUser.shouldNotUpdate)
            return;

        switch (state) {
        case State.GROUND:
            stateGround();
            break;
        case State.AIR:
            stateAir();
            break;
        case State.DAMAGE:
            stateDamage();
            break;
        }

        // apply gravity
        Vector2 v = rb2d.velocity;
        v.y -= gravity * Time.deltaTime;
        v.y = Mathf.Max(-terminalVelocity, v.y);
        rb2d.velocity = v;

        // color control
        if (receivesDamage.isMercyInvincible) {
            float mit = receivesDamage.mercyInvincibilityTime;
            float p = ReceivesDamage.MERCY_FLASH_PERIOD;
            float t = (mit - p * Mathf.Floor(mit / p)) / p; //t in [0, 1)
            if (t < .5){
                spriteRenderer.color = Color.Lerp(ReceivesDamage.MERCY_FLASH_COLOR, Color.white, t * 2);
            } else {
                spriteRenderer.color = Color.Lerp(Color.white, ReceivesDamage.MERCY_FLASH_COLOR, (t-.5f) * 2);
            }
        } else {
            if (spriteRenderer.color != Color.white) {
                spriteRenderer.color = Color.white;
            }
        }

        // fire bullets
        if (canFireBullet) {
            if (Input.GetButtonDown("Fire1")) {
                fireBullet();
            }
        }

	}

    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int) state;
        fi.floats["jumpTime"] = jumpTime;
        fi.floats["idleGunTime"] = idleGunTime;
        fi.floats["damageTime"] = damageTime;
        fi.strings["color"] = "" + spriteRenderer.color.r + "," + spriteRenderer.color.g + "," + spriteRenderer.color.b + "," + spriteRenderer.color.a;
    }
    void OnRevert(FrameInfo fi) {
        state = (State) fi.state;
        jumpTime = fi.floats["jumpTime"];
        idleGunTime = fi.floats["idleGunTime"];
        damageTime = fi.floats["damageTime"];
        char[] chars = {','};
        string[] colorParts = fi.strings["color"].Split(chars);
        spriteRenderer.color = new Color(float.Parse(colorParts[0]), float.Parse(colorParts[1]), float.Parse(colorParts[2]), float.Parse(colorParts[3]));
    }

    void OnDestroy() {
        _instance = null;
    }

    // control time reverting (called each frame)
    void timeReverting() {
        Camera cam = Camera.main;
        UnityStandardAssets.ImageEffects.BloomOptimized bo = cam.GetComponent<UnityStandardAssets.ImageEffects.BloomOptimized>();
        UnityStandardAssets.ImageEffects.ColorCorrectionCurves ccc = cam.GetComponent<UnityStandardAssets.ImageEffects.ColorCorrectionCurves>();

        if (TimeUser.reverting) {
            revertTime += Time.deltaTime;

            TimeUser.continuousRevertSpeed = Utilities.easeLinearClamp(revertTime, .5f, revertSpeed - .5f, revertEaseDuration);

            if (bo != null) {
                bo.intensity = Utilities.easeOutQuadClamp(revertTime, 0, 1.6f, revertEaseDuration);
            }
            if (ccc != null) {
                ccc.saturation = Utilities.easeOutQuadClamp(revertTime, 1, -1, revertEaseDuration);
            }

            bool stopReverting = !Input.GetButton("Flash");
            if (revertTime < minRevertDuration)
                stopReverting = false;
            if (stopReverting) {
                TimeUser.endContinuousRevert();

                if (bo != null) {
                    bo.enabled = false;
                }
                if (ccc != null) {
                    ccc.enabled = false;
                }
            }
        } else {
            if (Input.GetButtonDown("Flash")) {
                TimeUser.beginContinuousRevert(.5f);
                revertTime = 0;

                if (bo != null) {
                    bo.enabled = true;
                    bo.intensity = 0;
                }
                if (ccc != null) {
                    ccc.enabled = true;
                    ccc.saturation = 1;
                }
            }
        }

    }

    // ground state (called each frame)
    void stateGround() {
        Vector2 v = rb2d.velocity;
        bool stillAnim = false;

        if (leftHeld) {

            //movement
            if (v.x > 0) { //currently going right
                v.x = Mathf.Max(0, v.x - groundFriction * Time.deltaTime); //apply friction
            }
            v.x -= groundAccel * Time.deltaTime;
            v.x = Mathf.Max(-groundMaxSpeed, v.x); //apply max ground speed

            //animation
            flippedHoriz = true;
            if (v.x < 0) { //actually moving left
                if (!isAnimatorCurrentState("oracle_run")) {
                    animator.Play("oracle_run");
                }
            } else { //still moving right (sliding)
                stillAnim = true;
            }

        } else if (rightHeld) {

            //movement
            if (v.x < 0) { //currently going left
                v.x = Mathf.Min(0, v.x + groundFriction * Time.deltaTime); //apply friction
            }
            v.x += groundAccel * Time.deltaTime;
            v.x = Mathf.Min(groundMaxSpeed, v.x); //apply max ground speed

            //animation
            flippedHoriz = false;
            if (v.x > 0) { //actually moving right
                if (!isAnimatorCurrentState("oracle_run")) {
                    animator.Play("oracle_run");
                }
            } else { //still moving left (sliding)
                stillAnim = true;
            }

        } else { //no horiz input

            //just apply friction
            if (v.x < 0) {
                v.x = Mathf.Min(0, v.x + groundFriction * Time.deltaTime);
            } else {
                v.x = Mathf.Max(0, v.x - groundFriction * Time.deltaTime);
            }
            stillAnim = true;
        }

        if (stillAnim) {
            if (isAnimatorCurrentState("oracle_gun")) {
                idleGunTime += Time.deltaTime;
                if (idleGunTime > idleGunDownDuration) {
                    animator.Play("oracle_gun_to_idle");
                }
            } else if (!isAnimatorCurrentState("oracle_gun_to_idle") &&
                !isAnimatorCurrentState("oracle_idle")) {
                animator.Play("oracle_gun");
                idleGunTime = 0;
            }
        }

        if (jumpPressed) {
            jumpTime = 0;
            v.y = jumpSpeed;
            toAirState(true);
        }

        if (colFinder.hitBottom) {
            //offset v.y to account for slope
            float a = colFinder.normalBottom - Mathf.PI / 2;
            v.y = v.x * Mathf.Atan(a) * SLOPE_RUN_MODIFIER;
            float g = gravity * Time.fixedDeltaTime;
            v.y -= g;

            //offset v.x to match gravity so object doesn't slide down when still
            v.x += g * SLOPE_STILL_MODIFIER * Mathf.Atan(a);

        } else { //not touching ground
            //attempt to snap back to the ground
            if (!colFinder.raycastDownCorrection()) {
                //attempt failed, in air
                toAirState(false);
            }

        }

        rb2d.velocity = v;
    }

    // air state (called each frame)
    void stateAir() {
        Vector2 v = rb2d.velocity;

        if (leftHeld) {

            //movement
            if (v.x > 0) { //currently going right
                v.x = Mathf.Max(0, v.x - airFriction * Time.deltaTime); //apply friction
            }
            v.x -= airAccel * Time.deltaTime;
            v.x = Mathf.Max(-airMaxSpeed, v.x); //apply max air speed

            //animation
            flippedHoriz = true;

        } else if (rightHeld) {

            //movement
            if (v.x < 0) { //currently going left
                v.x = Mathf.Min(0, v.x + airFriction * Time.deltaTime); //apply friction
            }
            v.x += airAccel * Time.deltaTime;
            v.x = Mathf.Min(airMaxSpeed, v.x); //apply max air speed

            //animation
            flippedHoriz = false;

        } else { //no horiz input

            //just apply friction
            if (v.x < 0) {
                v.x = Mathf.Min(0, v.x + airFriction * Time.deltaTime);
            } else {
                v.x = Mathf.Max(0, v.x - airFriction * Time.deltaTime);
            }
        }

        if (jumping) {
            jumpTime += Time.deltaTime;
            v.y = jumpSpeed;
            if (jumpTime > jumpMinDuration) {
                if (jumpTime >= jumpMaxDuration || !jumpHeld ||
                    colFinder.hitTop) {
                    //end jump
                    jumpTime = jumpMaxDuration + 1;
                }
            }

        } else { // not jumping

            if (colFinder.hitBottom) {
                toGroundState();
            }
        }

        if (state == State.AIR) {
            //animation
            
            if ((isAnimatorCurrentState("oracle_idle_to_rising") || isAnimatorCurrentState("oracle_rising")) &&
                v.y < 0) {
                animator.Play("oracle_rising_to_falling");
            }
        }

        rb2d.velocity = v;
    }

    // damage state (called each frame)
    void stateDamage() {
        Vector2 v = rb2d.velocity;

        //just apply friction
        if (v.x < 0) {
            v.x = Mathf.Min(0, v.x + damageFriction * Time.deltaTime);
        } else {
            v.x = Mathf.Max(0, v.x - damageFriction * Time.deltaTime);
        }

        damageTime += Time.deltaTime;
        if (damageTime >= damageDuration) {
            if (colFinder.hitBottom) {
                toGroundState();
            } else {
                toAirState(false);
            }
        }

        rb2d.velocity = v;
    }

    // going to ground state
    void toGroundState() {
        state = State.GROUND;
        if (leftHeld || rightHeld) {
            animator.Play("oracle_run");
        } else {
            animator.Play("oracle_gun");
            idleGunTime = 0;
        }
    }

    // going to air state
    void toAirState(bool rising) {
        state = State.AIR;
        if (rising) {
            animator.Play("oracle_idle_to_rising");
        } else {
            animator.Play("oracle_falling");
        }
    }

    // taking damage
    void PreDamage(AttackInfo ai) { //(before damage is taken from health)
    }
    void OnDamage(AttackInfo ai) { //after damage is taken from health
        
        bool knockbackRight = ai.impactToRight();
        if (receivesDamage.health <= 0) {
            Debug.Log("death");
        }
        if (knockbackRight) {
            flippedHoriz = true;
            rb2d.velocity = new Vector2(damageSpeed, 0);
        } else {
            flippedHoriz = false;
            rb2d.velocity = new Vector2(-damageSpeed, 0);
        }
        damageTime = 0;
        animator.Play("oracle_damage");
        receivesDamage.mercyInvincibility(mercyInvincibilityDuration);
        state = State.DAMAGE;
    }


    // fire a bullet
    void fireBullet() {

        //animation
        idleGunTime = 0;
        if (isAnimatorCurrentState("oracle_gun_to_idle") || isAnimatorCurrentState("oracle_idle")) {
            animator.Play("oracle_gun");
        }

        //spawning bullet
        Vector2 relSpawnPosition = bulletSpawn;
        relSpawnPosition.x *= spriteRenderer.transform.localScale.x;
        relSpawnPosition.y *= spriteRenderer.transform.localScale.y;
        float heading = 0;
        if (flippedHoriz) {
            heading = 180;
        }
        GameObject bulletGO = GameObject.Instantiate(bulletGameObject,
            rb2d.position + relSpawnPosition,
            Utilities.setQuat(heading)) as GameObject;
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        bullet.heading = heading;

        //spawn muzzle
        GameObject.Instantiate(bulletMuzzleGameObject,
            rb2d.position + relSpawnPosition,
            bullet.transform.localRotation);
    }



    bool isAnimatorCurrentState(string stateString) {
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash(stateString);
    }

    // PRIVATE

    private static Player _instance;

    // vars
    float revertTime = 0;
    float jumpTime = 99999;
    float idleGunTime = 0;
    float damageTime = 0;

    // components
    Rigidbody2D _rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    Animator animator;
    ColFinder colFinder;
    TimeUser timeUser;
    ReceivesDamage receivesDamage;

    // button vars
    float horiz;
    bool leftHeld;
    bool rightHeld;
    bool jumpPressed;
    bool jumpHeld;

    //collision stuff
    private float SLOPE_RUN_MODIFIER = 1f;
    private float SLOPE_STILL_MODIFIER = 1.9f;
    

}
