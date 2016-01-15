using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossHealthBarPiece : MonoBehaviour {

    public Vector2 velocity = new Vector2();
    public float angularVelocity = 0;
    public float fadeDuration = 1.0f;
    public float delay = 0;

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        image = GetComponent<Image>();
    }

    void Start() {
        time = -delay;
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        // don't move when delayed
        if (time < 0)
            return;

        Vector3 pos = GetComponent<RectTransform>().localPosition;
        pos.x += velocity.x * Time.deltaTime;
        pos.y += velocity.y * Time.deltaTime;
        GetComponent<RectTransform>().localPosition = pos;
        float rotation = Utilities.get2DRot(GetComponent<RectTransform>().localRotation);
        rotation += angularVelocity * Time.deltaTime;
        GetComponent<RectTransform>().localRotation = Utilities.setQuat(rotation);

        
        updateAlpha();

        if (time >= fadeDuration) {
            timeUser.timeDestroy();
        }
        
    }

    void updateAlpha() {
        image.color = new Color(1, 1, 1, Utilities.easeLinearClamp(time, 1, -1, fadeDuration));
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;

        RectTransform rectTransform = GetComponent<RectTransform>();
        fi.floats["x"] = rectTransform.localPosition.x;
        fi.floats["y"] = rectTransform.localPosition.y;
        fi.floats["r"] = Utilities.get2DRot(rectTransform.localRotation);

    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        updateAlpha();

        GetComponent<RectTransform>().localPosition = new Vector3(fi.floats["x"], fi.floats["y"]);
        GetComponent<RectTransform>().localRotation = Utilities.setQuat(fi.floats["r"]);

    }

    TimeUser timeUser;
    Image image;

    float time = 0;
}
