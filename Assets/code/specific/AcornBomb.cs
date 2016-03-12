using UnityEngine;
using System.Collections;

public class AcornBomb : MonoBehaviour {

    public float initialYVelocity = 0;
    public float gravity = 10;
    public float finalX = 0;
    public float xDuration = .6f;
    public int explosionDamage = 2;
    public float spinSpeed = 50;
    public bool spinCW = true;
    public GameObject explosionGameObject;
    public AudioClip explodeSound;
    public bool hangingMode = false;
    public float hangingSwayPeriod = 2;
    public float hangingSwayRotation = 20;

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        timeUser = GetComponent<TimeUser>();
        visionUser = GetComponent<VisionUser>();
	}

    void Start() {
        pos0 = rb2d.position;
        yVel = initialYVelocity;
        if (finalX == 0) {
            finalX = rb2d.position.x;
        }
        if (hangingMode) {
            initialYVelocity = 0;
            yVel = 0;
            gameObject.layer = LayerMask.NameToLayer("Enemies");
        }
    }
	
	void FixedUpdate() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        if (hangingMode) {
            rb2d.rotation = Mathf.Sin(time / hangingSwayPeriod * Mathf.PI * 2) * hangingSwayRotation;
            
        } else {
            Vector2 pos = rb2d.position;
            if (Mathf.Abs(xDuration) < .0001f) {
                pos.x = finalX;
            } else {
                pos.x = Utilities.easeOutQuadClamp(time, pos0.x, finalX - pos0.x, xDuration);
            }
            yVel -= gravity * Time.deltaTime;
            pos.y += yVel * Time.deltaTime;
            rb2d.MovePosition(pos);
            if (spinCW) {
                rb2d.rotation -= spinSpeed * Time.deltaTime;
            } else {
                rb2d.rotation += spinSpeed * Time.deltaTime;
            }
        }
        
	}

    void explode() {
        if (exploded) return;

        GameObject explosionGO = GameObject.Instantiate(explosionGameObject, new Vector3(rb2d.position.x, rb2d.position.y), Quaternion.identity) as GameObject;
        LethalExplosion le = explosionGO.GetComponent<LethalExplosion>();
        le.damage = explosionDamage;
        if (visionUser.isVision) {
            explosionGO.GetComponent<VisionUser>().becomeVisionNow(visionUser.timeLeft, visionUser);
        }
        if (!visionUser.isVision) {
            SoundManager.instance.playSFXIfOnScreenRandPitchBend(explodeSound, rb2d.position);
        }

        timeUser.timeDestroy();
        
        exploded = true;
    }

    void OnCollisionEnter2D(Collision2D c2d) {
        if (timeUser.shouldNotUpdate)
            return;
        if (!hangingMode) {
            explode();
        }
    }

    // triggers returning from hanging mode
    void OnDamage(AttackInfo ai) {
        if (!hangingMode) return;

        gameObject.layer = LayerMask.NameToLayer("EnemyAttacks");
        hangingMode = false;
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.bools["e"] = exploded;
        fi.floats["yVel"] = yVel;
        fi.bools["hm"] = hangingMode;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        exploded = fi.bools["e"];
        yVel = fi.floats["yVel"];
        bool prevHangingMode = hangingMode;
        hangingMode = fi.bools["hm"];
        if (!prevHangingMode && hangingMode) {
            gameObject.layer = LayerMask.NameToLayer("Enemies");
        }
    }

    Rigidbody2D rb2d;
    TimeUser timeUser;
    VisionUser visionUser;

    float time = 0;
    Vector2 pos0 = new Vector2();
    float yVel = 0;
    bool exploded = false;

}
