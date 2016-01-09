using UnityEngine;
using System.Collections;

public class IceBoulder : MonoBehaviour {

    public GameObject[] shardGameObjects;
    public bool spawnsPickups = true;
    public PickupSpawner.BurstSize pickupsBurstSize = PickupSpawner.BurstSize.MEDIUM;
    public Vector2 pickupsSpawnOffset = new Vector2(0,0);
    public AudioClip iceShatterSound;
    public float fadeInDuration = .8f;
    public float throwRaycastOffset = 2.0f;
    public float throwRaycastEndOffset = 1.0f;
    public int damage = 1;

    public bool invincible = false;

    public void fadeIn() {
        if (fadingIn) return;
        time = 0;
        fadingIn = true;
        setColor();
    }

    public void throwBoulder(float speed, float heading) {
        float headingR = heading * Mathf.PI/180;
        throwDirection = new Vector2(Mathf.Cos(headingR), Mathf.Sin(headingR));
        throwSpeed = speed;
        rb2d.velocity = throwSpeed * throwDirection;
        throwing = true;
        rb2d.isKinematic = false;
        time = 0;
    }

    void OnCollisionEnter2D(Collision2D c2d) {
        if (!throwing) return;
        if (timeUser.shouldNotUpdate)
            return;

        if (c2d.collider.gameObject == Player.instance.gameObject) {
            // hit player, damage player and destroy
            Player.instance.GetComponent<ReceivesDamage>().dealDamage(damage, rb2d.position.x < Player.instance.rb2d.position.x);
            destroy();
        } else {
            // stick in ground
            rb2d.isKinematic = true;
            rb2d.angularVelocity = 0;
            invincible = false;
            throwing = false;
        }
    }



    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        receivesDamage = GetComponent<ReceivesDamage>();
        rb2d = GetComponent<Rigidbody2D>();
        timeUser = GetComponent<TimeUser>();
        visionUser = GetComponent<VisionUser>();
        pickupSpawner = GetComponent<PickupSpawner>();
	}
	
    void Start() {

    }

	void Update() {

        if (timeUser.shouldNotUpdate)
            return;
		
        if (throwing) {
            rb2d.velocity = throwDirection * throwSpeed;
        } else if (fadingIn) {
            time += Time.deltaTime;
            if (time >= fadeInDuration) {
                fadingIn = false;
            }
            setColor();
        }

        if (!CameraControl.pointContainedInMapBounds(rb2d.position, 10)) {
            timeUser.timeDestroy();
        }
        
        // destroy if getting too old
        if (timeUser.age > 40f) {
            destroy(false);
        }
	}

    void setColor() {
        if (!timeUser.exists)
            return;

        Color color = spriteRenderer.color;
        if (fadingIn) {
            color.a = Utilities.easeLinearClamp(time, 0, 1, fadeInDuration);
        } else {
            color.a = 1;
        }
        spriteRenderer.color = color;
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.bools["f"] = fadingIn;
        fi.bools["i"] = invincible;
        fi.ints["so"] = spriteRenderer.sortingOrder;
        fi.bools["throwing"] = throwing;
        fi.bools["ik"] = rb2d.isKinematic;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        fadingIn = fi.bools["f"];
        invincible = fi.bools["i"];
        spriteRenderer.sortingOrder = fi.ints["so"];
        throwing = fi.bools["throwing"];
        rb2d.isKinematic = fi.bools["ik"];
        setColor();
    }

    /* Immune to damage until invincible is false */
    void PreDamage(AttackInfo ai) {
        if (invincible) {
            ai.damage = 0;
        }
    }

    /* Detect being destroyed, spawning shards */
    void OnDamage(AttackInfo ai) {
        if (receivesDamage.health <= 0) {
            // be destroyed
            destroy();
        }
    }

    void destroy(bool playSound = true) {
        if (playSound) {
            SoundManager.instance.playSFXRandPitchBend(iceShatterSound);
        }
        if (spawnsPickups) {
            pickupSpawner.burstSpawn(rb2d.position + pickupsSpawnOffset, pickupsBurstSize);
        }
        spawnShards();
        timeUser.timeDestroy();
    }

    void spawnShards() {
        
        foreach (GameObject shardGameObject in shardGameObjects) {
            IceBoulderShard ibs = shardGameObject.GetComponent<IceBoulderShard>();
            Vector2 relPos = Utilities.rotateAroundPoint(ibs.startPos, Vector2.zero, rb2d.rotation * Mathf.PI / 180);

            GameObject.Instantiate(shardGameObject, transform.localPosition + new Vector3(relPos.x, relPos.y), Utilities.setQuat(rb2d.rotation));
        }
    }

    SpriteRenderer spriteRenderer;
    ReceivesDamage receivesDamage;
    Rigidbody2D rb2d;
    TimeUser timeUser;
    VisionUser visionUser;
    PickupSpawner pickupSpawner;

    Vector2 throwDirection = new Vector2();
    float throwSpeed = 0;
    
    bool fadingIn = false;
    float time = 0;
    bool throwing = false;
}
