using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ControlsMessage : MonoBehaviour {

    public enum Control {
        ADVANCE_MESSAGES = 0,
        RUN_AIM = 1,
        JUMP = 2,
        SHOOT = 3,
        FLASHBACK = 4,
        PAUSE_GAME = 5,
        DODGE = 6
    }

    public Control control = Control.ADVANCE_MESSAGES;
    public Vector2 initalPositionIndex0 = new Vector2();
    public float ySpacing = 0;
    public float fadeXDist = 30;
    public float fadeDuration = .5f;

    public Sprite advanceMessagesSprite;
    public Sprite runAimSprite;
    public Sprite jumpSprite;
    public Sprite shootSprite;
    public Sprite flashbackSprite;
    public Sprite pauseGameSprite;
    public Sprite dodgeSprite;
    
    /* Fades out message and automatically destroys it */
    public void fadeOut() {
        if (fadingOut) return;
        fadingOut = true;
        time = 0;
    }

    public static List<ControlsMessage> allMessages = new List<ControlsMessage>();

    Vector2 mainPosition = new Vector2();

	void Awake() {
        
        int yIndex = 0;
        foreach (ControlsMessage cm in allMessages) {
            if (cm.transform.parent == null) continue;
            if (cm.GetComponent<TimeUser>().exists)
                yIndex++;
        }

        allMessages.Add(this);

        mainPosition = initalPositionIndex0 - new Vector2(0, ySpacing * yIndex);

        image = GetComponent<Image>();
        timeUser = GetComponent<TimeUser>();

	}

    void Start() {
        switch (control) {
        case Control.ADVANCE_MESSAGES:
            image.sprite = advanceMessagesSprite;
            break;
        case Control.RUN_AIM:
            image.sprite = runAimSprite;
            break;
        case Control.JUMP:
            image.sprite = jumpSprite;
            break;
        case Control.SHOOT:
            image.sprite = shootSprite;
            break;
        case Control.FLASHBACK:
            image.sprite = flashbackSprite;
            break;
        case Control.PAUSE_GAME:
            image.sprite = pauseGameSprite;
            break;
        case Control.DODGE:
            image.sprite = dodgeSprite;
            break;
        }
        image.color = Color.clear;
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        updatePosition();

	}

    void OnDestroy() {
        allMessages.Remove(this);
    }

    void updatePosition() {
        if (!timeUser.exists)
            return;
        Color color = Color.white;
        float m = 0;
        if (fadingOut) {
            if (time < fadeDuration) {
                m = Utilities.easeLinearClamp(time, 0, 1, fadeDuration);
            } else {
                image.color = Color.clear;
                timeUser.timeDestroy();
                return;
            }
        } else {
            if (time < fadeDuration) {
                m = Utilities.easeLinearClamp(time, 1, -1, fadeDuration);
            } else {
                m = 0;
            }
        }

        color.a = 1 - m;
        image.color = color;
        if (!fadingOut) {
            GetComponent<RectTransform>().localPosition = new Vector3(mainPosition.x + fadeXDist * Utilities.easeInQuad(m, 0, 1, 1), mainPosition.y, 0);
        }
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.bools["fo"] = fadingOut;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        fadingOut = fi.bools["fo"];
        updatePosition();
    }

    Image image;
    TimeUser timeUser;

    float time = 0;
    bool fadingOut = false;

}
