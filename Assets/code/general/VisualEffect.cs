using UnityEngine;
using System.Collections;

[RequireComponent(typeof (TimeUser))]
public class VisualEffect : MonoBehaviour {

    public float duration = .1f;
    public bool fadeOut = false;
    public Vector2 speed = new Vector2();

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timeUser = GetComponent<TimeUser>();
        visionUser = GetComponent<VisionUser>();
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        transform.localPosition = transform.localPosition + (new Vector3(speed.x, speed.y)) * Time.deltaTime;
        updateFadeColor();
        
        if (time >= duration) {
            timeUser.timeDestroy();
        }
    }

    void updateFadeColor() {
        if (!timeUser.exists)
            return;
        if (visionUser != null && visionUser.isVision && !VisionUser.abilityActive)
            return;
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
    VisionUser visionUser;
    float time = 0;

}
