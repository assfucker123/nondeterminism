using UnityEngine;
using System.Collections;

public class GameOverGlyphUI : MonoBehaviour {

    public float fidgetDistMin = 1;
    public float fidgetDistMax = 3;
    public float fidgetTimeMin = .5f;
    public float fidgetTimeMax = 2.0f;
    public float existDuration = 9999f;

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        rectTransform = GetComponent<RectTransform>();
        glyphSprite = transform.Find("GlyphSprite").GetComponent<GlyphSprite>();
	}

    void Start() {
        time = 0;
        duration = timeUser.randomValue() * (fidgetTimeMax - fidgetTimeMin) + fidgetTimeMin;
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        existTime += Time.deltaTime;

        if (time >= duration) {
            // fidget glyph
            float dist = timeUser.randomValue() * (fidgetDistMax - fidgetDistMin) + fidgetDistMin;
            float angle = timeUser.randomValue() * Mathf.PI*2;
            rectTransform.localPosition = rectTransform.localPosition + new Vector3(
                dist * Mathf.Cos(angle),
                dist * Mathf.Sin(angle));
            
            time = 0;
            duration = timeUser.randomValue() * (fidgetTimeMax - fidgetTimeMin) + fidgetTimeMin;
        }

        if (existTime >= existDuration) {
            timeUser.timeDestroy();
        }

	}

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.floats["et"] = existTime;
        fi.floats["rtx"] = rectTransform.localPosition.x;
        fi.floats["rty"] = rectTransform.localPosition.y;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        existTime = fi.floats["et"];
        rectTransform.localPosition = new Vector3(fi.floats["rtx"], fi.floats["rty"]);
    }

    void OnRevertExist() {
        glyphSprite.color = new Color(glyphSprite.color.r, glyphSprite.color.g, glyphSprite.color.b, 1);
    }

    void OnTimeDestroy() {
        glyphSprite.color = new Color(glyphSprite.color.r, glyphSprite.color.g, glyphSprite.color.b, 0);
    }

    TimeUser timeUser;
    public RectTransform rectTransform;
    public GlyphSprite glyphSprite;

    float time = 0;
    float existTime = 0;
    float duration = 0;
}
