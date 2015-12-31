using UnityEngine;
using System.Collections;

public class Sibro : MonoBehaviour {

    public Vector2 hitVelocity = new Vector2(10, 10);
    public float hitAngularVelocity = 100;
    public AudioClip boingSound;

    public int timesHit {  get { return _timesHit; } }

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        rb2d = GetComponent<Rigidbody2D>();
	}
	
	void Update() {
		
	}

    void OnDamage(AttackInfo ai) {

        if (timeUser.shouldNotUpdate)
            return;

        if (rb2d.isKinematic) {
            rb2d.isKinematic = false;
        }

        Vector2 vel = hitVelocity;
        float aVel = -hitAngularVelocity;
        if (!ai.impactToRight()) {
            vel.x *= -1;
            aVel *= -1;
        }
        SoundManager.instance.playSFXRandPitchBend(boingSound, .01f);

        rb2d.velocity = vel;
        rb2d.angularVelocity = aVel;

        _timesHit++;

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.bools["ik"] = rb2d.isKinematic;
        fi.ints["th"] = _timesHit;
    }

    void OnRevert(FrameInfo fi) {
        rb2d.isKinematic = fi.bools["ik"];
        _timesHit = fi.ints["th"];
    }

    void OnCollisionEnter2D(Collision2D c2d) {
        if (timeUser.shouldNotUpdate)
            return;
        if (c2d.relativeVelocity.magnitude > 1) {
            SoundManager.instance.playSFXRandPitchBend(boingSound, .01f, Utilities.easeInQuadClamp(c2d.relativeVelocity.magnitude - 1, 0, 1, 3));
        }
    }

    int _timesHit = 0;

    TimeUser timeUser;
    Rigidbody2D rb2d;
}
