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
        if (fadeOut) {
            if (spriteRenderer != null) {
                Color c = spriteRenderer.color;
                c.a = Utilities.easeLinearClamp(time, 1, -1, duration);
                spriteRenderer.color = c;
            }
        }
        if (time >= duration) {
            timeUser.timeDestroy();
        }
    }

    SpriteRenderer spriteRenderer;
    TimeUser timeUser;
    float time = 0;

}
