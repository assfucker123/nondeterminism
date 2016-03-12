using UnityEngine;
using System.Collections;

/* Call defaultDeath.activate() to activate the death behavior.
 * Make sure to not edit anything else while this behavior is activated.
 * */

public class DefaultDeath : MonoBehaviour {

    public float duration = .6f; //time until death
    public float explosionSpawnWidth = 1.0f;
    public float explosionSpawnHeight = 2.0f;
    public float explosionDensity = 20f;
    public Vector2 velocity = new Vector2(3.0f, 3.5f);
    public float gravity = 10f;
    public bool setColor = true;
    public GameObject explosionGameObject;
    public PickupSpawner.BurstSize pickupSize = PickupSpawner.BurstSize.NONE;
    public AudioClip deathSound1 = null; // optional, recommended to be the explosion sound effect
    public AudioClip deathSound2 = null; // optional, recommended to be a customized sound effect (will be pitch bent)

    float time = 0;
    bool toRight = false;
    float explosionTime = 0;
    
    /* Call this to activate the death behavior */
    public void activate(bool toRight = true) {
        if (activated) return;

        if (timeUser.shouldNotUpdate)
            return;

        //initial velocity
        this.toRight = toRight;
        float xVel = velocity.x;
        if (!toRight)
            xVel *= -1;
        rb2d.velocity = new Vector2(xVel, velocity.y);

        //change layer to disable collision
        preActiveLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("HitNothing");

        //cut visions
        if (visionUser != null) {
            visionUser.cutVisions();
        }

        //spawn pickups
        if (pickupSpawner == null) {
            if (pickupSize != PickupSpawner.BurstSize.NONE) {
                Debug.Log("Not spawning pickups because object doesn't have PickupSpawner component");
            }
        } else {
            pickupSpawner.burstSpawn(rb2d.position, pickupSize);
        }

        //death sound
        if (deathSound1 != null) {
            SoundManager.instance.playSFXIfOnScreen(deathSound1, rb2d.position);
        }
        if (deathSound2 != null) {
            SoundManager.instance.playSFXIfOnScreenRandPitchBend(deathSound2, rb2d.position);
        }

        // update score (arcade mode only)
        if (Vars.arcadeMode) {
            int score = 0;
            EnemyInfo ei = GetComponent<EnemyInfo>();
            if (ei != null) {
                score = ei.score;
            }
            HUD.instance.countdownTimer.score += score;
        }

        _activated = true;
    }

    public bool activated { get { return _activated; } }
    bool _activated = false;

	void Awake() {
		rb2d = GetComponent<Rigidbody2D>();
        Transform sot = this.transform.Find("spriteObject");
        GameObject spriteObject;
        if (sot == null) {
            spriteObject = gameObject;
            spriteRenderer = this.GetComponent<SpriteRenderer>();
        } else {
            spriteObject = sot.gameObject;
            spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        }
        timeUser = GetComponent<TimeUser>();
        Debug.Assert(rb2d != null && timeUser != null);
        visionUser = GetComponent<VisionUser>();
        pickupSpawner = GetComponent<PickupSpawner>();
	}
	
	void Update() {
        if (!activated)
            return;
        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        float xVel = velocity.x;
        if (!toRight)
            xVel *= -1;
        rb2d.velocity = new Vector2(xVel, velocity.y - gravity * time);

        //spawn explosions
        if (time < duration * 3 / 4) { //don't spawn explosions near end of death
            explosionTime += Time.deltaTime;
        }
        float area = explosionSpawnWidth * explosionSpawnHeight;
        while (explosionTime > area / explosionDensity) {

            //create explosion
            Vector2 relPos = new Vector2(
                (timeUser.randomValue() * 2 - 1) * explosionSpawnWidth / 2,
                (timeUser.randomValue() * 2 - 1) * explosionSpawnHeight / 2);
            Vector2 pos = relPos + rb2d.position;
            GameObject.Instantiate(explosionGameObject, new Vector3(pos.x, pos.y, 0), Quaternion.identity);

            explosionTime -= area / explosionDensity;
        }

        //color
        if (setColor)
            setColorF();

        if (time >= duration) {
            timeUser.timeDestroy();
        }

	}

    void OnSpawn(SpawnInfo si) {
        if (!si.spawnPickups) {
            pickupSize = PickupSpawner.BurstSize.NONE;
        }
    }

    void setColorF() {
        if (spriteRenderer == null) return;
        if (activated) {
            float t = time / duration;
            t = Utilities.easeInQuadClamp(t, 0, 1, 1);
            Color c0 = ReceivesDamage.HIT_FLASH_COLOR;
            Color c1 = new Color(0, 0, 0, 0);

            spriteRenderer.color = Color.Lerp(c0, c1, t);
        } else {
            spriteRenderer.color = Color.white;
        }
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["dDeathTime"] = time;
        fi.floats["dDeathETime"] = explosionTime;
        fi.bools["dDeathActivated"] = activated;
    }
    void OnRevert(FrameInfo fi) {
        bool prevActivated = activated;
        time = fi.floats["dDeathTime"];
        explosionTime = fi.floats["dDeathETime"];
        _activated = fi.bools["dDeathActivated"];

        // if just reverted to before activated
        if (prevActivated && !activated) {
            if (preActiveLayer != -1) {
                gameObject.layer = preActiveLayer;
            }
        }

        if (setColor) {
            setColorF();
        }
    }

    int preActiveLayer = -1;
	
	// components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    TimeUser timeUser;
    VisionUser visionUser;
    PickupSpawner pickupSpawner;
}
