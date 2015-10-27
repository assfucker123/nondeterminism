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
    public float bulletSpread = 3.0f;
    public float bulletMinDuration = 0.3f; //prevents Player from shooting too fast
    public float revertSpeed = 2.0f;
    public float revertEaseDuration = .4f;
    public float minRevertDuration = 1.0f;
    public int maxHealth = 8;
    public float maxPhase = 100;
    public float startPhase = .5f;
    public float phaseDecreaseSpeed = 2.5f;
    public float phaseRevertingDecreaseSpeed = 7.0f;
    public float damageSpeed = 10;
    public float damageFriction = 20;
    public float damageDuration = .5f;
    public float mercyInvincibilityDuration = 1.0f;
    public State state = State.GROUND;
    public AudioClip stepSound;
    public AudioClip jumpSound;
    public AudioClip bulletSound;
    public AudioClip gunDownSound;
    public AudioClip damageSound;
    public AudioClip flashbackBeginSound;
    public AudioClip flashbackEndSound;

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
    public int health { get { return receivesDamage.health; } }
    public float phase { get { return HUD.instance.phaseMeter.phase; } }

    public enum State:int {
        GROUND,
        AIR,
        DAMAGE
    }

    //////////////////////
    // PUBLIC FUNCTIONS //
    //////////////////////


    public void healthPickup(int health) {
        int h = this.health + health;
        if (h > maxHealth)
            h = maxHealth;
        receivesDamage.health = h;
        HUD.instance.setHealth(receivesDamage.health);
        healthFlashTime = 0;
    }

    /* Called by Pickup when picking up a phase pickup */
    public void phasePickup(float phase) {
        HUD.instance.phaseMeter.increasePhase(phase);
        phaseFlashTime = 0;
    }
    /* When reverting to before a phase pickup was collected, need to lose the phase gained.
     * This doesn't have to be done for other pickups because health etc. already reverts correctly.
     * Phase is special because it does not revert back to a previous value. */
    public void revertBeforePhasePickup(float phase) {
        HUD.instance.phaseMeter.setPhase(this.phase - phase);
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
        if (receivesDamage.health > maxHealth) receivesDamage.health = maxHealth;
        HUD.instance.setMaxHealth(maxHealth);
        HUD.instance.setHealth(receivesDamage.health);
        HUD.instance.phaseMeter.setMaxPhase(maxPhase);
        HUD.instance.phaseMeter.setPhase(maxPhase*startPhase);
	}

	void Update() {

        //testing
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            
        }

        // decrease phase over time
        if (!HUD.instance.phaseMeter.increasing) {
            if (TimeUser.reverting) {
                HUD.instance.phaseMeter.setPhase(phase - phaseRevertingDecreaseSpeed * Time.deltaTime);
            } else {
                HUD.instance.phaseMeter.setPhase(phase - phaseDecreaseSpeed * Time.deltaTime);
            }
        }

        // activating vision ability based on amount of phase
        if (VisionUser.abilityActive && phase <= 0) {
            VisionUser.deactivateAbility();
        } else if (!VisionUser.abilityActive && phase > 0) {
            VisionUser.activateAbility();
        }

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
        } else if (healthFlashTime < Pickup.HEALTH_FLASH_DURATION) {
            healthFlashTime += Time.deltaTime;
            float t = Mathf.Min(1, healthFlashTime / Pickup.HEALTH_FLASH_DURATION);
            if (t < .5) {
                spriteRenderer.color = Color.Lerp(Color.white, Pickup.HEALTH_FLASH_COLOR, t * 2);
            } else {
                spriteRenderer.color = Color.Lerp(Pickup.HEALTH_FLASH_COLOR, Color.white, (t - .5f) * 2);
            }
        } else if (phaseFlashTime < Pickup.PHASE_FLASH_DURATION) {
            phaseFlashTime += Time.deltaTime;
            float t = Mathf.Min(1, phaseFlashTime / Pickup.PHASE_FLASH_DURATION);
            if (t < .5) {
                spriteRenderer.color = Color.Lerp(Color.white, Pickup.PHASE_FLASH_COLOR, t * 2);
            } else {
                spriteRenderer.color = Color.Lerp(Pickup.PHASE_FLASH_COLOR, Color.white, (t - .5f) * 2);
            }
        } else {
            if (spriteRenderer.color != Color.white) {
                spriteRenderer.color = Color.white;
            }
        }

        // fire bullets
        bulletTime += Time.deltaTime;
        if (canFireBullet) {
            if (Input.GetButtonDown("Fire1")) {
                bulletPrePress = true;
            }
            if (bulletPrePress && bulletTime >= bulletMinDuration) {
                fireBullet();
                bulletTime = 0;
                bulletPrePress = false;
            }
        }

	}
    
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int) state;
        fi.floats["jumpTime"] = jumpTime;
        fi.floats["idleGunTime"] = idleGunTime;
        fi.floats["damageTime"] = damageTime;
        fi.floats["pFTime"] = phaseFlashTime;
        fi.strings["color"] = TimeUser.colorToString(spriteRenderer.color);
    }
    void OnRevert(FrameInfo fi) {
        state = (State) fi.state;
        jumpTime = fi.floats["jumpTime"];
        idleGunTime = fi.floats["idleGunTime"];
        damageTime = fi.floats["damageTime"];
        phaseFlashTime = fi.floats["pFTime"];
        spriteRenderer.color = TimeUser.stringToColor(fi.strings["color"]);
        HUD.instance.setHealth(receivesDamage.health); //update health on HUD
        bulletTime = 0;
        bulletPrePress = false; //so doesn't bizarrely shoot immediately after revert
    }

    void OnDestroy() {
        _instance = null;
    }

    // control time reverting (called each frame)
    void timeReverting() {
        
        if (TimeUser.reverting) {
            revertTime += Time.deltaTime;
            
            TimeUser.continuousRevertSpeed = Utilities.easeLinearClamp(revertTime, .5f, revertSpeed - .5f, revertEaseDuration);

            CameraControl.instance.enableEffects(
                Utilities.easeOutQuadClamp(revertTime, 0, 1.6f, revertEaseDuration),
                Utilities.easeOutQuadClamp(revertTime, 1, -1, revertEaseDuration));

            //conditions for reverting
            bool stopReverting = !Input.GetButton("Flash");
            if (phase <= 0 || TimeUser.time <= 0.0001f)
                stopReverting = true;
            if (revertTime < minRevertDuration)
                stopReverting = false;

            if (stopReverting) {
                TimeUser.endContinuousRevert();
                SoundManager.instance.stopSFX(flashbackBeginSound);
                SoundManager.instance.playSFX(flashbackEndSound);
                HUD.instance.phaseMeter.endPulse();
                CameraControl.instance.disableEffects();
            }
        } else {
            if (Input.GetButtonDown("Flash") && phase > 0) {
                TimeUser.beginContinuousRevert(.5f);
                SoundManager.instance.playSFX(flashbackBeginSound);
                HUD.instance.phaseMeter.beginPulse();
                revertTime = 0;
                CameraControl.instance.enableEffects(0, 1);
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
                    SoundManager.instance.playSFXRandPitchBend(gunDownSound);
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
            SoundManager.instance.playSFXRandPitchBend(jumpSound);
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

        // step sounds
        if (state == State.GROUND && isAnimatorCurrentState("oracle_run")) {
            stepSoundPlayTime += Time.deltaTime;
            if (stepSoundPlayTime > .25f){
                SoundManager.instance.playSFXRandPitchBend(stepSound);
                stepSoundPlayTime = 0;
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
        SoundManager.instance.playSFXRandPitchBend(stepSound);
        stepSoundPlayTime = 0;
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
        HUD.instance.setHealth(receivesDamage.health);
        damageTime = 0;
        animator.Play("oracle_damage");
        SoundManager.instance.playSFX(damageSound);
        receivesDamage.mercyInvincibility(mercyInvincibilityDuration);
        CameraControl.instance.shake();
        state = State.DAMAGE;

        //end jump if jumping
        jumpTime = jumpMaxDuration + 1;
    }


    // fire a bullet
    void fireBullet() {

        //animation
        idleGunTime = 0;
        if (isAnimatorCurrentState("oracle_gun_to_idle") || isAnimatorCurrentState("oracle_idle")) {
            animator.Play("oracle_gun");
        }
        SoundManager.instance.playSFXRandPitchBend(bulletSound);

        //spawning bullet
        Vector2 relSpawnPosition = bulletSpawn;
        relSpawnPosition.x *= spriteRenderer.transform.localScale.x;
        relSpawnPosition.y *= spriteRenderer.transform.localScale.y;
        float heading = 0;
        if (flippedHoriz) {
            heading = 180;
        }
        heading += (Random.value * 2 - 1) * bulletSpread;
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

    // helper function
    bool isAnimatorCurrentState(string stateString) {
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash(stateString);
    }

    // PRIVATE

    private static Player _instance;

    // vars
    float revertTime = 0;
    float jumpTime = 99999;
    float idleGunTime = 0;
    float bulletTime = 99999; //involved in preventing Player from shooting too fast
    bool bulletPrePress = false;
    float damageTime = 0;
    float phaseFlashTime = 99999;
    float healthFlashTime = 99999;
    float stepSoundPlayTime = 99999;

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
