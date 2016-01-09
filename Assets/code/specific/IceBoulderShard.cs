using UnityEngine;
using System.Collections;

public class IceBoulderShard : MonoBehaviour {

    public Vector2 startPos = new Vector2();
    public float startSpeed = 5;
    public float startHeading = 90;
    public float startAngularVelocity = 100;
    public float fadeStartTime = .6f;
    public float fadeDuration = .3f;

	void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timeUser = GetComponent<TimeUser>();
        rb2d = GetComponent<Rigidbody2D>();
	}
	
	void Update() {
        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        updateColor();

        if (time >= fadeStartTime + fadeDuration) {
            timeUser.timeDestroy();
        }
	}

    void Start() {
        rb2d.velocity = startSpeed * new Vector2(Mathf.Cos(startHeading * Mathf.PI / 180), Mathf.Sin(startHeading * Mathf.PI / 180));
        rb2d.angularVelocity = startAngularVelocity;
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        updateColor();
    }

    void updateColor() {
        if (!timeUser.exists)
            return;
        Color color = spriteRenderer.color;
        if (time < fadeStartTime) {
            color.a = 1;
        } else {
            color.a = Utilities.easeLinearClamp(time - fadeStartTime, 1, -1, fadeDuration);
        }
        spriteRenderer.color = color;
    }

    float time = 0;

    TimeUser timeUser;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb2d;
}
