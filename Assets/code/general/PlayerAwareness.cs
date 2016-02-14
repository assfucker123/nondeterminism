using UnityEngine;
using System.Collections;

/* Used to see if enemy should be "aware" of the player.  If player is close, or if they can see player, etc. */
public class PlayerAwareness : MonoBehaviour {

    public Vector2 sightEyesPosition = new Vector2(0, 0); // position of eyes relative to the transform
    public float sightEyesAngle = 0; // angle of eyes relative to the transform
    public float sightRange = 30;
    public float sightAngleSpread = 70;
    public float awareBoxWidth = 8; // if player is in this box, then enemy is automatically ware of player, even if behind a wall
    public float awareBoxHeight = 6;
    public float hiddenPlayerAwareDuration = 1; // how long to remain aware of player after losing him
    public bool recordAboveInspectorValuesInTimeUser = false;
    public string[] sightLayers = {"Default"};
    
    public bool awareOfPlayer {  get { return _awareOfPlayer || alwaysAware; } }
    public bool alwaysAware = false; // can be set to true in-game to be always aware of the player

    /* Returns if a point can be seen from the center, using raycasts to make sure point isn't behind a wall */
    public static bool seesPoint(Vector2 point, Vector2 center, float rangeOfVision, float centerAngleDegrees, float angleSpreadDegrees) {
        return seesPoint(point, center, rangeOfVision, centerAngleDegrees, angleSpreadDegrees, (1 << LayerMask.NameToLayer("Default")));
    }
    public static bool seesPoint(Vector2 point, Vector2 center, float rangeOfVision, float centerAngleDegrees, float angleSpreadDegrees, int layerMask) {
        // false if not in sector of vision at all
        if (!Utilities.pointInSector(point, center, rangeOfVision, centerAngleDegrees * Mathf.PI / 180, angleSpreadDegrees * Mathf.PI / 180))
            return false;
        // check that point isn't behind a wall
        float ptDist = Vector2.Distance(point, center);
        RaycastHit2D rh2d = Physics2D.Raycast(center, point - center, rangeOfVision, layerMask);
        if (rh2d.distance < ptDist)
            return false;
        // point is seen
        return true;
    }


    void Awake() {
        timeUser = GetComponent<TimeUser>(); // optional
        Transform trans = transform.Find("spriteObject"); // optional
        if (trans)
            childSpriteObject = trans.gameObject;
	}

    void Start() {
        updateSightLayerMask();
    }

    void updateSightLayerMask() {
        sightLayerMask = 0;
        for (int i=0; i<sightLayers.Length; i++) {
            sightLayerMask |= (1 << LayerMask.NameToLayer(sightLayers[i]));
        }
    }
	
	void Update() {

        if (timeUser != null && timeUser.shouldNotUpdate)
            return;

        Player plr = Player.instance;
        if (plr == null) {
            _awareOfPlayer = false;
            return;
        }
        Vector2 plrPos = plr.rb2d.position;

        // aware from sight
        bool seesPlayerFromSight = false;
        if (sightRange > 0) {
            float angleRad = sightEyesAngle * Mathf.PI / 180;
            Vector3 center3 = sightEyesPosition;
            Vector3 angle3 = center3 + new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            if (childSpriteObject == null) {
                center3 = transform.TransformPoint(center3);
                angle3 = transform.TransformPoint(angle3);
            } else {
                // this else seems weird to me.  I feel like I should use transform again, but not is the accurate choice
                center3 = childSpriteObject.transform.TransformPoint(center3);
                angle3 = childSpriteObject.transform.TransformPoint(angle3);
            }
            angleRad = Mathf.Atan2(angle3.y - center3.y, angle3.x - center3.x);
            seesPlayerFromSight = seesPoint(plrPos, new Vector2(center3.x, center3.y), sightRange, angleRad * 180 / Mathf.PI, sightAngleSpread, sightLayerMask);
        }

        // aware from box
        bool awareBox = false;
        if (transform.localPosition.x - awareBoxWidth/2 <= plrPos.x && plrPos.x <= transform.localPosition.x + awareBoxWidth/2 &&
            transform.localPosition.y - awareBoxHeight/2 <= plrPos.y && plrPos.y <= transform.localPosition.y + awareBoxHeight / 2) {
            awareBox = true;
        }

        // setting aware flag
        if (seesPlayerFromSight || awareBox) {
            _awareOfPlayer = true;
            awareHiddenTime = 0;
        } else {
            awareHiddenTime += Time.deltaTime;
            if (awareHiddenTime >= hiddenPlayerAwareDuration) {
                _awareOfPlayer = false;
            }
        }

	}

    void OnSaveFrame(FrameInfo fi) {
        fi.bools["pa_aop"] = awareOfPlayer;
        fi.floats["pa_aht"] = awareHiddenTime;
        fi.bools["pa_aa"] = alwaysAware;
        if (!recordAboveInspectorValuesInTimeUser) return;
        fi.floats["pa_sepx"] = sightEyesPosition.x;
        fi.floats["pa_sepy"] = sightEyesPosition.y;
        fi.floats["pa_sea"] = sightEyesAngle;
        fi.floats["pa_sas"] = sightAngleSpread;
        fi.floats["pa_aaw"] = awareBoxWidth;
        fi.floats["pa_aah"] = awareBoxHeight;
    }

    void OnRevert(FrameInfo fi) {
        _awareOfPlayer = fi.bools["pa_aop"];
        awareHiddenTime = fi.floats["pa_aht"];
        alwaysAware = fi.bools["pa_aa"];
        if (!recordAboveInspectorValuesInTimeUser) return;
        sightEyesPosition.Set(fi.floats["pa_sepx"], fi.floats["pa_sepy"]);
        sightEyesAngle = fi.floats["pa_sea"];
        sightAngleSpread = fi.floats["pa_sas"];
        awareBoxWidth = fi.floats["pa_aaw"];
        awareBoxHeight = fi.floats["pa_aah"];
    }


    

    int sightLayerMask = 1;
    float awareHiddenTime = 9999;
    bool _awareOfPlayer = false;

    TimeUser timeUser;
    GameObject childSpriteObject;

}
