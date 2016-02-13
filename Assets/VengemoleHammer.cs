using UnityEngine;
using System.Collections;

public class VengemoleHammer : MonoBehaviour {

    public float hitBeginFadeDuration = 1.5f;
    public float fadeDuration = .5f;
    public Vector2 initialVelocity = new Vector2();
    public float initialAngularVelocity = 400;
    public bool flingRight = true;
    public AudioClip hitSound;

	void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2d = GetComponent<Rigidbody2D>();
        timeUser = GetComponent<TimeUser>();
        visionUser = GetComponent<VisionUser>();
        attackObject = GetComponent<AttackObject>();
	}

    void Start() {
        if (flingRight) {
            rb2d.angularVelocity = -initialAngularVelocity;
            rb2d.velocity = initialVelocity;
        } else {
            rb2d.angularVelocity = initialAngularVelocity;
            rb2d.velocity = new Vector2(-initialVelocity.x, initialVelocity.y);
        }
    }
	
	void Update() {
        if (timeUser.shouldNotUpdate)
            return;

        if (fading) {
            fadeTime += Time.deltaTime;
            if (fadeTime > hitBeginFadeDuration && attackObject.enabled) {
                attackObject.enabled = false;
            }
        }
        setColor();
        if (fading && fadeTime >= hitBeginFadeDuration+fadeDuration) {
            timeUser.timeDestroy();
        }
	}

    void OnCollisionEnter2D(Collision2D c2d) {
        if (timeUser.shouldNotUpdate)
            return;
        if (c2d.collider.gameObject == Player.instance.gameObject && attackObject.enabled) {
            Player.instance.GetComponent<ReceivesDamage>().dealDamage(attackObject.damage, flingRight);
        }
        hitHappened();
        if (c2d.relativeVelocity.magnitude > 20 && !visionUser.isVision) {
            SoundManager.instance.playSFXRandPitchBend(hitSound, .02f);
        }
    }

    void hitHappened() {
        if (fading) return;
        fading = true;
        fadeTime = 0;
    }

    void setColor() {
        if (!timeUser.exists) return;
        if (visionUser.isVision) return;

        Color c = spriteRenderer.color;
        if (fading && fadeTime > hitBeginFadeDuration) {
            c.a = Utilities.easeLinearClamp(fadeTime-hitBeginFadeDuration, 1, -1, fadeDuration);
        } else {
            c.a = 1;
        }
        spriteRenderer.color = c;
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.bools["f"] = fading;
        fi.floats["ft"] = fadeTime;
        fi.bools["aoe"] = attackObject.enabled;
    }
    void OnRevert(FrameInfo fi) {
        fading = fi.bools["f"];
        fadeTime = fi.floats["ft"];
        attackObject.enabled = fi.bools["aoe"];
        setColor();
    }

    SpriteRenderer spriteRenderer;
    Rigidbody2D rb2d;
    AttackObject attackObject;
    TimeUser timeUser;
    VisionUser visionUser;

    bool fading = false;
    float fadeTime = 0;
}
