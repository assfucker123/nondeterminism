using UnityEngine;
using System.Collections;

public class OraclePiece : MonoBehaviour {

    public Vector2 spawnPos = new Vector2(0, 0);
    public float spawnRot = 0;
    public Vector2 explodeVel = new Vector2(10, 10);
    public float explodeAngularVel = 100;
    public float mercyFlashTime = 0;
    public float mercyFlashDuration = .5f;

    public bool flippedHoriz {
        get { return gameObject.transform.localScale.x < 0; }
        set {
            if (value == flippedHoriz)
                return;
            gameObject.transform.localScale = new Vector3(
                -gameObject.transform.localScale.x,
                gameObject.transform.localScale.y,
                gameObject.transform.localScale.z);
        }
    }

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        timeUser = GetComponent<TimeUser>();
    }

    void Start() {
        
    }
    
    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        if (mercyFlashTime < mercyFlashDuration) {
            mercyFlashTime += Time.deltaTime;

            if (mercyFlashTime >= mercyFlashDuration) {
                spriteRenderer.color = Color.white;
            } else {
                float mit = mercyFlashTime;
                float p = ReceivesDamage.MERCY_FLASH_PERIOD;
                float t = (mit - p * Mathf.Floor(mit / p)) / p; //t in [0, 1)
                if (t < .5) {
                    spriteRenderer.color = Color.Lerp(ReceivesDamage.MERCY_FLASH_COLOR, Color.white, t * 2);
                } else {
                    spriteRenderer.color = Color.Lerp(Color.white, ReceivesDamage.MERCY_FLASH_COLOR, (t - .5f) * 2);
                }
            }
        }

    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.floats["mft"] = mercyFlashTime;
        fi.strings["color"] = TimeUser.colorToString(spriteRenderer.color);
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        mercyFlashTime = fi.floats["mft"];
        spriteRenderer.color = TimeUser.stringToColor(fi.strings["color"]);
    }

    Segment segment;

    // components
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;
    TimeUser timeUser;

}
