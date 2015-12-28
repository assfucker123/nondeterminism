using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TimeUser))]
public class CameraSensor : MonoBehaviour {

    public CameraControl.PositionMode mode = CameraControl.PositionMode.SET_POSITION;
    public float duration = 0;
    public Vector2 position = new Vector2();
    public GameObject positionObject = null; // can set position to the position of this GameObject.  It's okay if this is null
    public bool ignoreIfAlreadyInMode = true;

    void Awake() {
        bc2d = GetComponent<BoxCollider2D>();
        timeUser = GetComponent<TimeUser>();
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        if (Player.instance == null) return;
        Vector3 plrPos = Player.instance.rb2d.position;

        if (bc2d.bounds.Contains(plrPos)) {
            if (!playerContained) {
                applyNewMode();
                playerContained = true;
            }
        } else {
            playerContained = false;
        }

    }

    void LateUpdate() {

        if (TimeUser.reverting ||
            Time.timeScale < .0001f ||
            HUD.instance.gameOverScreen.activated)
            return;
        
    }

    void applyNewMode() {
        if (CameraControl.instance == null) return;
        if (ignoreIfAlreadyInMode &&
            CameraControl.instance.positionMode == mode)
            return;

        switch (mode) {
        case CameraControl.PositionMode.FOLLOW_PLAYER:
            CameraControl.instance.followPlayer(duration);
            break;
        case CameraControl.PositionMode.SET_POSITION:
            Vector2 pos = position;
            if (positionObject != null) {
                pos = new Vector2(positionObject.transform.localPosition.x, positionObject.transform.localPosition.y);
            }
            CameraControl.instance.moveToPosition(pos, duration);
            break;
        case CameraControl.PositionMode.CUSTOM:
            CameraControl.instance.customPositionMode();
            break;
        }

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.bools["pc"] = playerContained;
    }

    void OnRevert(FrameInfo fi) {
        bool prevPlayerContained = playerContained;
        playerContained = fi.bools["pc"];
        if (playerContained && !prevPlayerContained) {
            applyNewMode();
        }
    }

    TimeUser timeUser;
    BoxCollider2D bc2d;
    bool playerContained = false;
}
