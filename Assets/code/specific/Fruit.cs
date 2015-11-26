using UnityEngine;
using System.Collections;

public class Fruit : MonoBehaviour {

    public bool sway = false;
    public float swayPeriod = 1.0f;
    public float swayAngle = 30;
    public GameObject explosionGameObject;
    public AudioClip explodeSound;

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

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        spriteObject = transform.Find("spriteObject").gameObject;
        spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        colFinder = GetComponent<ColFinder>();
        timeUser = GetComponent<TimeUser>();
        receivesDamage = GetComponent<ReceivesDamage>();
        visionUser = GetComponent<VisionUser>();
    }

    void Start() {
        swayTime = timeUser.randomValue() * swayPeriod;
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        if (sway) {
            swayTime += Time.deltaTime;
            rb2d.rotation = Mathf.Sin(swayTime * Mathf.PI * 2 / swayPeriod) * swayAngle;
        }

    }

    /* called when this takes damage */
    void OnDamage(AttackInfo ai) {
        if (receivesDamage.health <= 0) {
            explode();
        }
    }

    public void explode() {
        // create explosion
        GameObject expGO = GameObject.Instantiate(
            explosionGameObject,
            transform.localPosition,
            Quaternion.identity) as GameObject;
        if (visionUser.isVision) {
            expGO.GetComponent<VisionUser>().becomeVisionNow(visionUser.timeLeft, visionUser);
        } else {
            if (!SoundManager.instance.isSFXPlaying(explodeSound)) {
                SoundManager.instance.playSFX(explodeSound);
            }
        }
        timeUser.timeDestroy();
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.bools["sway"] = sway;
        fi.floats["swayT"] = swayTime;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        sway = fi.bools["sway"];
        swayTime = fi.floats["swayT"];
    }

    float swayTime = 0;

    // components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    ColFinder colFinder;
    TimeUser timeUser;
    ReceivesDamage receivesDamage;
    VisionUser visionUser;

}
