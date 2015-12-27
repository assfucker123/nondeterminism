using UnityEngine;
using System.Collections;

[RequireComponent(typeof (TimeUser))]
public class VisualEffect : MonoBehaviour {

    public float duration = .1f;
    public bool fadeOut = false;

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timeUser = GetComponent<TimeUser>();
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        updateFadeColor();
        
        if (time >= duration) {
            timeUser.timeDestroy();
        }
    }

    void updateFadeColor() {
        if (fadeOut) {
            if (spriteRenderer != null) {
                Color c = spriteRenderer.color;
                c.a = Utilities.easeLinearClamp(time, 1, -1, duration);
                spriteRenderer.color = c;
            }
        }
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
    }
    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        updateFadeColor();
    }

    SpriteRenderer spriteRenderer;
    TimeUser timeUser;
    float time = 0;

}
