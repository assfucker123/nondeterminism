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
    public Vector2 bulletSpawnUp = new Vector2();
    public Vector2 bulletSpawnDown = new Vector2();
    public float bulletSpread = 3.0f;
    public float bulletMinDuration = 0.3f; //prevents Player from shooting too fast

    public GameObject chargeBulletGameObject;
    public float chargeDuration = .6f;
    public float chargeDownJumpSpeed = 10f;

    public float revertSpeed = 2.0f;
    public float revertEaseDuration = .4f;
    public float minRevertDuration = 1.0f;
    public int maxHealth = 8;
    public float maxPhase = 100;
    public float startPhase = .5f;
    public float phaseDecreaseSpeed = 2.5f;
    public float phaseRevertingDecreaseSpeed = 7.0f;

    public float postRevertDuration = .5f; // still lose a tiny bit of phase after a revert for this duration

    public float damageSpeed = 10;
    public float damageFriction = 20;
    public float damageDuration = .5f;
    public float mercyInvincibilityDuration = 1.0f;
    public float hitPauseDamageMultiplier = .02f;
    public float dieExplodeDelay = 0.3f;
    public float deathHitPause = .4f;
    public GameObject[] pieceGameObjects;
    public GameObject deathLarvaGameObject;
    public GameObject deathExplosionGameObject;
    public State state = State.GROUND;
    public AudioClip stepSound;
    public AudioClip jumpSound;
    public AudioClip chargingStartSound;
    public AudioClip chargingLoopSound;
    public AudioClip bulletSound;
    public AudioClip chargeBulletSound;
    public AudioClip gunDownSound;
    public AudioClip damageSound;
    public AudioClip deathSound;
    public AudioClip larvaScreamSound;
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
    public bool isDead { get { return state == State.DEAD; } }
    public bool exploded { get { return _exploded; } }
    public int health { get { return receivesDamage.health; } }
    public float phase { get { return HUD.instance.phaseMeter.phase; } }
    public AimDirection aimDirection {
        get {
            return _aimDirection;
        }
        set {
            if (aimDirection == value) return;

            _aimDirection = value;

            // change animation
            float normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            bool idleState = isAnimatorCurrentState("gun_to_idle") || isAnimatorCurrentState("idle") ||
                isAnimatorCurrentState("gun") || isAnimatorCurrentState("gun_up") || isAnimatorCurrentState("gun_down");
            bool runState = isAnimatorCurrentState("run") || isAnimatorCurrentState("run_up") || isAnimatorCurrentState("run_down");
            bool idleToRisingState = isAnimatorCurrentState("idle_to_rising") || isAnimatorCurrentState("idle_to_rising_up") || isAnimatorCurrentState("idle_to_rising_down");
            bool risingState = isAnimatorCurrentState("rising") || isAnimatorCurrentState("rising_up") || isAnimatorCurrentState("rising_down");
            bool risingToFallingState = isAnimatorCurrentState("rising_to_falling") || isAnimatorCurrentState("rising_to_falling_up") || isAnimatorCurrentState("rising_to_falling_down");
            bool fallingState = isAnimatorCurrentState("falling") || isAnimatorCurrentState("falling_up") || isAnimatorCurrentState("falling_down");

            switch (aimDirection) {
            case AimDirection.FORWARD:
                if (idleState) {
                    animator.Play("gun");
                    idleGunTime = 0;
                } else if (runState)
                    animator.Play("run", 0, normalizedTime);
                else if (idleToRisingState)
                    animator.Play("idle_to_rising", 0, normalizedTime);
                else if (risingState)
                    animator.Play("rising", 0, normalizedTime);
                else if (risingToFallingState)
                    animator.Play("rising_to_falling", 0, normalizedTime);
                else if (fallingState)
                    animator.Play("falling", 0, normalizedTime);
                break;
            case AimDirection.UP:
                if (idleState)
                    animator.Play("gun_up");
                else if (runState)
                    animator.Play("run_up", 0, normalizedTime);
                else if (idleToRisingState)
                    animator.Play("idle_to_rising_up", 0, normalizedTime);
                else if (risingState)
                    animator.Play("rising_up", 0, normalizedTime);
                else if (risingToFallingState)
                    animator.Play("rising_to_falling_up", 0, normalizedTime);
                else if (fallingState)
                    animator.Play("falling_up", 0, normalizedTime);
                break;
            case AimDirection.DOWN:
                if (idleState)
                    animator.Play("gun_down");
                else if (runState)
                    animator.Play("run_down", 0, normalizedTime);
                else if (idleToRisingState)
                    animator.Play("idle_to_rising_down", 0, normalizedTime);
                else if (risingState)
                    animator.Play("rising_down", 0, normalizedTime);
                else if (risingToFallingState)
                    animator.Play("rising_to_falling_down", 0, normalizedTime);
                else if (fallingState)
                    animator.Play("falling_down", 0, normalizedTime);
                break;
            }


        }
    }

    public enum State:int {
        GROUND,
        AIR,
        DAMAGE,
        DEAD
    }

    public enum AimDirection:int {
        FORWARD,
        UP,
        DOWN
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
        spriteObject = transform.Find("spriteObject").gameObject;
        spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        chargeParticlesObject = transform.Find("chargeParticles").gameObject;
        chargeParticlesObject.transform.SetAsFirstSibling();
        chargeParticles = chargeParticlesObject.GetComponent<ChargeParticles>();
        animator = spriteObject.GetComponent<Animator>();
        colFinder = GetComponent<ColFinder>();
        timeUser = GetComponent<TimeUser>();
        receivesDamage = GetComponent<ReceivesDamage>();

        //need to call this with a title screen
        Vars.startGame();
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
                // just after ending a revert, still lose a bit of phase
                if (postRevertTime < postRevertDuration) {
                    postRevertTime += Time.deltaTime;
                    float postSpeed = Utilities.easeLinearClamp(postRevertTime, phaseRevertingDecreaseSpeed, -phaseRevertingDecreaseSpeed, postRevertDuration);
                    HUD.instance.phaseMeter.setPhase(phase - postSpeed * Time.deltaTime);
                    if (postRevertTime >= postRevertDuration) {
                        HUD.instance.phaseMeter.endPulse();
                    }
                }

                // gradually lose phase over time
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
        leftHeld = Keys.instance.leftHeld;
        rightHeld = Keys.instance.rightHeld;
        if (leftHeld == rightHeld) {
            leftHeld = false;
            rightHeld = false;
        }
        jumpPressed = Keys.instance.jumpPressed;
        jumpHeld = Keys.instance.jumpHeld;

        //control reverting
        timeReverting();

        if (timeUser.shouldNotUpdate) {
            return;
        }
        
        
        //aiming
        switch (state) {
        case State.GROUND:
        case State.AIR:
            if (Keys.instance.upHeld) {
                aimDirection = AimDirection.UP;
            } else if (Keys.instance.downHeld) {
                aimDirection = AimDirection.DOWN;
            } else {
                aimDirection = AimDirection.FORWARD;
            }
            break;
        }

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
        case State.DEAD:
            stateDead();
            break;
        }

        // apply gravity
        Vector2 v = rb2d.velocity;
        v.y -= gravity * Time.deltaTime;
        v.y = Mathf.Max(-terminalVelocity, v.y);
        rb2d.velocity = v;

        // fire bullets
        bulletTime += Time.deltaTime;
        if (canFireBullet) {
            if (Keys.instance.shootPressed) {
                bulletPrePress = true;
            }
            if (bulletPrePress && bulletTime >= bulletMinDuration) {
                fireBullet(false, aimDirection);
                bulletTime = 0;
                bulletPrePress = false;
            }
            if (chargeTime >= chargeDuration && Keys.instance.shootReleased) {
                fireBullet(true, aimDirection);
            }
        }

        // control charging
        controlCharging();

        // color control
        if (exploded) {
            spriteRenderer.color = new Color(1, 1, 1, 0);
        } else if (receivesDamage.isMercyInvincible) {
            float mit = receivesDamage.mercyInvincibilityTime;
            float p = ReceivesDamage.MERCY_FLASH_PERIOD;
            float t = (mit - p * Mathf.Floor(mit / p)) / p; //t in [0, 1)
            if (t < .5) {
                spriteRenderer.color = Color.Lerp(ReceivesDamage.MERCY_FLASH_COLOR, Color.white, t * 2);
            } else {
                spriteRenderer.color = Color.Lerp(Color.white, ReceivesDamage.MERCY_FLASH_COLOR, (t - .5f) * 2);
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
        } else if (chargeTime >= chargeDuration) {
            chargeParticles.tiny = false;
            chargeFlashTime += Time.deltaTime;
            while (chargeFlashTime >= ChargeParticles.CHARGE_FLASH_DURATION) {
                chargeFlashTime -= ChargeParticles.CHARGE_FLASH_DURATION;
            }
            float t = Mathf.Min(1, chargeFlashTime / ChargeParticles.CHARGE_FLASH_DURATION);
            if (t < .5) {
                spriteRenderer.color = Color.Lerp(Color.white, ChargeParticles.CHARGE_FLASH_COLOR, t * 2);
            } else {
                spriteRenderer.color = Color.Lerp(ChargeParticles.CHARGE_FLASH_COLOR, Color.white, (t - .5f) * 2);
            }
        } else {
            if (spriteRenderer.color != Color.white) {
                spriteRenderer.color = Color.white;
            }
        }

        // update camera position
        setCameraPosition();

	}

    void LateUpdate() {

        if (timeUser.shouldNotUpdate)
            return;

    }
    
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int) state;
        fi.floats["jumpTime"] = jumpTime;
        fi.floats["idleGunTime"] = idleGunTime;
        fi.floats["damageTime"] = damageTime;
        fi.floats["pFTime"] = phaseFlashTime;
        fi.floats["deadTime"] = deadTime;
        fi.strings["color"] = TimeUser.colorToString(spriteRenderer.color);
        fi.bools["exploded"] = exploded;
        fi.floats["det"] = deathExplosionTime;
        fi.bools["charging"] = charging;
        fi.floats["chargeTime"] = chargeTime;
        fi.floats["chargeSoundTime"] = chargeSoundTime;
        fi.floats["postRevertTime"] = postRevertTime;
        fi.ints["aimDirection"] = (int)aimDirection;
    }
    void OnRevert(FrameInfo fi) {
        state = (State) fi.state;
        jumpTime = fi.floats["jumpTime"];
        idleGunTime = fi.floats["idleGunTime"];
        damageTime = fi.floats["damageTime"];
        phaseFlashTime = fi.floats["pFTime"];
        deadTime = fi.floats["deadTime"];
        spriteRenderer.color = TimeUser.stringToColor(fi.strings["color"]);
        bool prevExploded = exploded;
        _exploded = fi.bools["exploded"];
        if (prevExploded && !exploded) {
            gameObject.layer = LayerMask.NameToLayer("Players");
        }
        deathExplosionTime = fi.floats["det"];
        charging = fi.bools["charging"];
        chargeTime = fi.floats["chargeTime"];
        chargeSoundTime = fi.floats["chargeSoundTime"];
        postRevertTime = fi.floats["postRevertTime"];
        aimDirection = (AimDirection)fi.ints["aimDirection"];
        HUD.instance.setHealth(receivesDamage.health); //update health on HUD
        bulletTime = 0;
        bulletPrePress = false; //so doesn't bizarrely shoot immediately after revert

        setCameraPosition();
    }

    void OnDestroy() {
        _instance = null;
    }

    // control time reverting (called each frame)
    void timeReverting() {

        // inversion used when The Salesman reverts
        bool useInversion = false;
        
        if (TimeUser.reverting) {
            revertTime += Time.deltaTime;
            
            TimeUser.continuousRevertSpeed = Utilities.easeLinearClamp(revertTime, .5f, revertSpeed - .5f, revertEaseDuration);

            if (useInversion) {
                CameraControl.instance.enableEffects(
                    Utilities.easeOutQuadClamp(revertTime, 0, 1.6f, revertEaseDuration),
                    Utilities.easeOutQuadClamp(revertTime, 1, -1, revertEaseDuration),
                    Utilities.easeOutQuadClamp(revertTime, 0, 1, revertEaseDuration));
            } else {
                CameraControl.instance.enableEffects(
                    Utilities.easeOutQuadClamp(revertTime, 0, 1.6f, revertEaseDuration),
                    Utilities.easeOutQuadClamp(revertTime, 1, -1, revertEaseDuration),
                    0);
            }

            //conditions for reverting
            bool stopReverting = !Keys.instance.flashbackHeld;
            if (phase <= 0 || TimeUser.time <= 0.0001f)
                stopReverting = true;
            if (revertTime < minRevertDuration)
                stopReverting = false;

            if (stopReverting) {
                TimeUser.endContinuousRevert();
                SoundManager.instance.stopSFX(flashbackBeginSound);
                SoundManager.instance.playSFX(flashbackEndSound);
                CameraControl.instance.disableEffects();
                postRevertTime = 0;
                // will end phaseMeter pulse when postRevertTime passes postRevertDuration
            }
        } else if (!PauseScreen.paused && !HUD.instance.gameOverScreen.cannotRevert) {
            if (Keys.instance.flashbackPressed) {
                if (phase > 0) {
                    TimeUser.beginContinuousRevert(.5f);
                    SoundManager.instance.playSFX(flashbackBeginSound);
                    HUD.instance.phaseMeter.beginPulse();
                    revertTime = 0;
                    postRevertTime = 99999;
                    if (useInversion) {
                        CameraControl.instance.enableEffects(0, 1, 1);
                    } else {
                        CameraControl.instance.enableEffects(0, 1, 0);
                    }
                } else {
                    HUD.instance.phaseMeter.playPhaseEmptySound();
                }
            }
        }

    }

    // control charging (called each frame)
    void controlCharging() {

        if (charging) {
            idleGunTime = 0;
            chargeTime += Time.deltaTime;
            if (chargeTime > chargeDuration) {
                // play charged loop sound
                chargeSoundTime += Time.deltaTime;
                if (chargeSoundTime > chargingLoopSound.length) {
                    SoundManager.instance.playSFX(chargingLoopSound);
                    chargeSoundTime = 0;
                }
            }
            if (!Keys.instance.shootHeld || !canFireBullet) {
                chargeParticles.stopSpawning();
                chargeTime = 0;
                charging = false;
                SoundManager.instance.stopSFX(chargingStartSound);
                SoundManager.instance.stopSFX(chargingLoopSound);
            }
            
        } else { // not currently charging
            if (Keys.instance.shootHeld && canFireBullet) {
                chargeParticles.startSpawning();
                chargeParticles.tiny = true;
                chargeTime = 0;
                chargeFlashTime = 0;
                chargeSoundTime = 99999;
                SoundManager.instance.playSFX(chargingStartSound);
                charging = true;
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
                if (!isAnimatorCurrentState("run" + aimSuffix)) {
                    animator.Play("run" + aimSuffix);
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
                if (!isAnimatorCurrentState("run" + aimSuffix)) {
                    animator.Play("run" + aimSuffix);
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
            if (isAnimatorCurrentState("gun")) {
                idleGunTime += Time.deltaTime;
                if (idleGunTime > idleGunDownDuration) {
                    animator.Play("gun_to_idle");
                    SoundManager.instance.playSFXRandPitchBend(gunDownSound);
                }
            } else {
                if (!isAnimatorCurrentState("gun_to_idle" + aimSuffix) &&
                !isAnimatorCurrentState("idle" + aimSuffix)) {
                    animator.Play("gun" + aimSuffix);
                    idleGunTime = 0;
                }
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
        if (state == State.GROUND && isAnimatorCurrentState("run" + aimSuffix)) {
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

        // cancel horiz speed if would be rubbing against a slanted wall
        if (colFinder.hitLeft) {
            v.x = Mathf.Max(v.x, .1f);
        }
        if (colFinder.hitRight) {
            v.x = Mathf.Min(v.x, -.1f);
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
            if ((isAnimatorCurrentState("idle_to_rising" + aimSuffix) || isAnimatorCurrentState("rising" + aimSuffix)) &&
                v.y < 0) {
                animator.Play("rising_to_falling" + aimSuffix);
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

    // dead state (called each frame)
    void stateDead() {
        deadTime += Time.deltaTime;

        rb2d.velocity = Vector2.zero;

        if (exploded) {
            
        } else {

            // apply friction like stateDamage()
            Vector2 v = rb2d.velocity;
            if (v.x < 0) {
                v.x = Mathf.Min(0, v.x + damageFriction * Time.deltaTime);
            } else {
                v.x = Mathf.Max(0, v.x - damageFriction * Time.deltaTime);
            }
            rb2d.velocity = v;

            // explode
            if (deadTime >= dieExplodeDelay) {
                explode();
            }
        }

    }

    // going to ground state
    void toGroundState() {
        state = State.GROUND;
        if (leftHeld || rightHeld) {
            animator.Play("run" + aimSuffix);
        } else {
            animator.Play("gun" + aimSuffix);
            idleGunTime = 0;
        }
        SoundManager.instance.playSFXRandPitchBend(stepSound);
        stepSoundPlayTime = 0;
    }

    // going to air state
    void toAirState(bool rising) {
        state = State.AIR;
        if (rising) {
            animator.Play("idle_to_rising" + aimSuffix);
        } else {
            animator.Play("falling" + aimSuffix);
        }
    }

    // going to dead state
    void die() {
        if (state == State.DEAD)
            return;

        CameraControl.instance.hitPause(deathHitPause);
        HUD.instance.gameOverScreen.activate();
        SoundManager.instance.playSFX(deathSound);
        rb2d.velocity.Set(0, 0); // stop Player from moving
        deadTime = 0;
        deathExplosionTime = 0;
        state = State.DEAD;
    }

    // explode, pieces fly out
    void explode() {
        if (exploded) return;

        Vector3 spawnPos;
        Vector2 explodeVel;

        // spawn explosion
        spawnPos = transform.TransformPoint(Vector3.zero);
        GameObject.Instantiate(deathExplosionGameObject, spawnPos, Quaternion.identity);

        // lauch different pieces of the suit
        foreach (GameObject pieceGameObject in pieceGameObjects) {
            OraclePiece pGOOP = pieceGameObject.GetComponent<OraclePiece>();
            spawnPos = new Vector3(pGOOP.spawnPos.x, pGOOP.spawnPos.y);
            if (flippedHoriz) {
                spawnPos.x *= -1;
            }
            spawnPos = transform.TransformPoint(spawnPos);
            float spawnRot = pGOOP.spawnRot;
            explodeVel = pGOOP.explodeVel;
            float explodeAngularVel = pGOOP.explodeAngularVel;
            if (flippedHoriz) {
                explodeVel.x *= -1;
            }

            GameObject pGO = GameObject.Instantiate(
                pieceGameObject,
                spawnPos,
                Utilities.setQuat(spawnRot)) as GameObject;
            pGO.transform.localScale = transform.localScale;

            Rigidbody2D prb2d = pGO.GetComponent<Rigidbody2D>();
            prb2d.velocity = explodeVel;
            prb2d.angularVelocity = explodeAngularVel;

            OraclePiece op = pGO.GetComponent<OraclePiece>();
            op.mercyFlashTime = receivesDamage.mercyInvincibilityTime;

        }

        // launch DeathLarva
        DeathLarvaOracle dLO = deathLarvaGameObject.GetComponent<DeathLarvaOracle>();
        spawnPos = new Vector3(dLO.spawnPos.x, dLO.spawnPos.y);
        explodeVel = dLO.explodeVel;
        if (flippedHoriz) {
            spawnPos.x *= -1;
            explodeVel.x *= -1;
        }
        spawnPos = transform.TransformPoint(spawnPos);
        GameObject dLGO = GameObject.Instantiate(
            deathLarvaGameObject,
            spawnPos,
            Quaternion.identity) as GameObject;

        Rigidbody2D dlorb2d = dLGO.GetComponent<Rigidbody2D>();
        dlorb2d.velocity = explodeVel;

        dLO = dLGO.GetComponent<DeathLarvaOracle>();
        dLO.mercyFlashTime = receivesDamage.mercyInvincibilityTime;
        dLO.flippedHoriz = flippedHoriz;
        
        // stop Player from moving
        rb2d.velocity.Set(0, 0);

        //change layer to disable collision
        gameObject.layer = LayerMask.NameToLayer("HitNothing");

        SoundManager.instance.playSFX(larvaScreamSound);

        _exploded = true;

    }

    // taking damage
    void PreDamage(AttackInfo ai) { //(before damage is taken from health)
    }
    void OnDamage(AttackInfo ai) { //after damage is taken from health

        if (isDead)
            return;

        bool willDie = (receivesDamage.health <= 0);

        bool knockbackRight = ai.impactToRight();
        CameraControl.instance.hitPause(ai.damage * hitPauseDamageMultiplier);
        if (knockbackRight) {
            flippedHoriz = true;
            rb2d.velocity = new Vector2(damageSpeed, 0);
        } else {
            flippedHoriz = false;
            rb2d.velocity = new Vector2(-damageSpeed, 0);
        }
        animator.Play("damage");
        SoundManager.instance.playSFX(damageSound);
        damageTime = 0;
        state = State.DAMAGE;
        HUD.instance.setHealth(receivesDamage.health);
        receivesDamage.mercyInvincibility(mercyInvincibilityDuration);
        CameraControl.instance.shake();
        //end jump if jumping
        jumpTime = jumpMaxDuration + 1;

        if (willDie) {
            HUD.instance.speedLines.flashHeavyRed();
            die();
        } else {
            HUD.instance.speedLines.flashRed();
        }
        
    }

    // fire a bullet
    void fireBullet(bool charged, AimDirection direction) {

        //animation
        idleGunTime = 0;
        if (isAnimatorCurrentState("gun_to_idle") || isAnimatorCurrentState("idle")) {
            animator.Play("gun");
        }
        if (charged) {
            SoundManager.instance.playSFXRandPitchBend(chargeBulletSound);
        } else {
            SoundManager.instance.playSFXRandPitchBend(bulletSound);
        }
        
        //spawning bullet
        Vector2 relSpawnPosition = bulletSpawn;
        float heading = 0;
        switch (direction) {
        case AimDirection.FORWARD:
            relSpawnPosition = bulletSpawn;
            if (flippedHoriz) {
                heading = 180;
            } else {
                heading = 0;
            }
            break;
        case AimDirection.UP:
            relSpawnPosition = bulletSpawnUp;
            heading = 90;
            break;
        case AimDirection.DOWN:
            relSpawnPosition = bulletSpawnDown;
            heading = -90;
            break;
        }
        relSpawnPosition.x *= spriteRenderer.transform.localScale.x;
        relSpawnPosition.y *= spriteRenderer.transform.localScale.y;

        // oracle stabilizes gun while running, so perfect aim.
        bool runState = isAnimatorCurrentState("run") || isAnimatorCurrentState("run_up") || isAnimatorCurrentState("run_down");
        if (!charged && !runState) {
            heading += (Random.value * 2 - 1) * bulletSpread;
        }
        GameObject theBulletGO = bulletGameObject;
        if (charged) {
            theBulletGO = chargeBulletGameObject;
        }
        GameObject bulletGO = GameObject.Instantiate(theBulletGO,
            rb2d.position + relSpawnPosition,
            Utilities.setQuat(heading)) as GameObject;
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        bullet.heading = heading;

        //spawn muzzle
        GameObject.Instantiate(bulletMuzzleGameObject,
            rb2d.position + relSpawnPosition,
            bullet.transform.localRotation);

        // jump effect when firing a charged shot down and in the air
        if (state == State.AIR && charged && direction == AimDirection.DOWN) {
            Vector2 v = rb2d.velocity;
            // can't go higher than jump speed (prevents abuse)
            v.y = Mathf.Max(chargeDownJumpSpeed, Mathf.Min(jumpSpeed, v.y + chargeDownJumpSpeed));
            rb2d.velocity = v;
            // end jump if jumping (to prevent too much abuse)
            jumpTime = jumpMaxDuration + 1;
        }
    }

    // setting camera position
    void setCameraPosition() {
        CameraControl cameraControl = CameraControl.instance;
        if (cameraControl == null) return;

        Vector2 pos = rb2d.position;
        if (TimeUser.reverting) {
            pos += rb2d.velocity * Time.deltaTime;
        }
        cameraControl.moveToPosition(pos);
    }

    // helper function
    bool isAnimatorCurrentState(string stateString) {
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash(stateString);
    }
    string aimSuffix {
        get {
            switch (aimDirection) {
            case AimDirection.UP:
                return "_up";
            case AimDirection.DOWN:
                return "_down";
            default:
                return "";
            }
        }
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
    float deadTime = 999999;
    float chargeFlashTime = 99999;
    float chargeSoundTime = 99999;
    float deathExplosionTime = 0;
    float postRevertTime = 99999;
    bool _exploded = false;
    bool charging = false;
    float chargeTime = 0;
    AimDirection _aimDirection = AimDirection.FORWARD;

    // components
    Rigidbody2D _rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    GameObject chargeParticlesObject;
    ChargeParticles chargeParticles;
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
    private float SLOPE_STILL_MODIFIER = 1.0f;
    

}
