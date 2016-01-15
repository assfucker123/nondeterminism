using UnityEngine;
using System.Collections;

/* Hides when player is inside it, shows when player is outside */

public class FadingForeground : MonoBehaviour {

    public float fadeDuration = .4f;

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	void Update() {

        if (timeUser != null && timeUser.shouldNotUpdate)
            return;

        // fading
        float alpha = spriteRenderer.color.a;

        if (playerInside) {
            alpha = Mathf.Max(0, alpha - Time.deltaTime / fadeDuration);
            if (justStarted)
                alpha = 0;
        } else {
            alpha = Mathf.Min(1, alpha + Time.deltaTime / fadeDuration);
            if (justStarted)
                alpha = 1;
        }

        spriteRenderer.color = new Color(1, 1, 1, alpha);

        justStartedTime += Time.deltaTime;

	}
    
    void OnTriggerEnter2D(Collider2D c2d) {

        if (Player.instance == null) return;
        if (c2d.gameObject != Player.instance.gameObject) return;

        playerInside = true;
        if (justStarted) {
            spriteRenderer.color = new Color(1, 1, 1, 0);
        }

    }

    void OnTriggerExit2D(Collider2D c2d) {

        if (Player.instance == null) return;
        if (c2d.gameObject != Player.instance.gameObject) return;

        playerInside = false;
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["jst"] = justStartedTime;
        fi.bools["pi"] = playerInside;
        fi.floats["a"] = spriteRenderer.color.a;
    }

    void OnRevert(FrameInfo fi) {
        justStartedTime = fi.floats["jst"];
        playerInside = fi.bools["pi"];
        spriteRenderer.color = new Color(1, 1, 1, fi.floats["a"]);
    }

    bool justStarted {  get { return justStartedTime < .06f; } }

    TimeUser timeUser;
    SpriteRenderer spriteRenderer;

    float justStartedTime = 0;
    [HideInInspector]
    public bool playerInside = false; // set public so it's possible to manually set this without needing the player.  not recommended

}
