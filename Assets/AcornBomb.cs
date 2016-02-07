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

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        timeUser = GetComponent<TimeUser>();
        visionUser = GetComponent<VisionUser>();
	}

    void Start() {
        pos0 = rb2d.position;
        yVel = initialYVelocity;
        if (finalX == 0) { // just for testing
            finalX = rb2d.position.x;
        }
    }
	
	void FixedUpdate() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

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

    void explode() {
        if (exploded) return;

        GameObject explosionGO = GameObject.Instantiate(explosionGameObject, new Vector3(rb2d.position.x, rb2d.position.y), Quaternion.identity) as GameObject;
        LethalExplosion le = explosionGO.GetComponent<LethalExplosion>();
        le.damage = explosionDamage;
        if (visionUser.isVision) {
            explosionGO.GetComponent<VisionUser>().becomeVisionNow(visionUser.timeLeft, visionUser);
        }
        if (!visionUser.isVision) {
            SoundManager.instance.playSFXRandPitchBend(explodeSound);
        }

        timeUser.timeDestroy();
        
        exploded = true;
    }

    void OnCollisionEnter2D(Collision2D c2d) {
        explode();
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.bools["e"] = exploded;
        fi.floats["yVel"] = yVel;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        exploded = fi.bools["e"];
        yVel = fi.floats["yVel"];
    }

    Rigidbody2D rb2d;
    TimeUser timeUser;
    VisionUser visionUser;

    float time = 0;
    Vector2 pos0 = new Vector2();
    float yVel = 0;
    bool exploded = false;

}
