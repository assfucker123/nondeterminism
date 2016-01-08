using UnityEngine;
using System.Collections;

public class IceRock : MonoBehaviour {

    public int damage = 1;
    public float speed = 40;
    public float heading = 0;
    public float headingSpeed = 100;
    public bool positiveHeading = true;
    public float headingSwitchDurationMin = .4f;
    public float headingSwitchDurationMax = .9f;
    public float spinMaxSpeed = 100f;
    public bool hitsSherivice = false; // go on scripted path if this is true
    public GameObject trailGameObject;
    public float trailPeriod = .3f;
    public GameObject explosionGameObject;

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        visionUser = GetComponent<VisionUser>();
        rb2d = GetComponent<Rigidbody2D>();
	}

    void Start() {
        // set heading
        if (hitsSherivice) {

        } else {
            positiveHeading = timeUser.randomValue() > .5f;
            headingSwitchDuration = timeUser.randomValue() * (headingSwitchDurationMax - headingSwitchDurationMin) + headingSwitchDurationMin;
        }
        headingSwitchTime = 0;

        spinSpeed = (timeUser.randomValue()*2-1) * spinMaxSpeed;
    }
	
	void Update() {
		
        if (timeUser.shouldNotUpdate)
            return;

        headingSwitchTime += Time.deltaTime;
        // detect reverse headings
        if (headingSwitchTime >= headingSwitchDuration) {
            positiveHeading = !positiveHeading;
            headingSwitchTime -= headingSwitchDuration;
            // get next time heading will reverse
            if (hitsSherivice) {

            } else {
                headingSwitchDuration = timeUser.randomValue() * (headingSwitchDurationMax - headingSwitchDurationMin) + headingSwitchDurationMin;
            }
            
        }

        if (positiveHeading) {
            heading += headingSpeed * Time.deltaTime;
        } else {
            heading -= headingSpeed * Time.deltaTime;
        }
        rb2d.velocity = speed * new Vector2(Mathf.Cos(heading * Mathf.PI / 180), Mathf.Sin(heading * Mathf.PI / 180));

        // spinning
        rb2d.rotation += spinSpeed * Time.deltaTime;

        // spawning trails
        trailTime += Time.deltaTime;
        if (trailTime >= trailPeriod) {
            GameObject trailGO = (GameObject.Instantiate(trailGameObject, transform.localPosition, transform.localRotation) as GameObject);
            if (visionUser.isVision) {
                VisionUser tvu = trailGO.GetComponent<VisionUser>();
                tvu.becomeVisionNow(visionUser.duration - visionUser.time, visionUser);
            }
            trailTime -= trailPeriod;
        }

        // destroy if left map
        if (!CameraControl.pointContainedInMapBounds(rb2d.position, 4)) {
            timeUser.timeDestroy();
        }

	}

    void OnCollisionEnter2D(Collision2D c2d) {

        if (timeUser.shouldNotUpdate)
            return;
        
        if (!visionUser.isVision &&
            c2d.gameObject == Player.instance.gameObject) {
            // deal damage to player if hit
            Player.instance.GetComponent<ReceivesDamage>().dealDamage(damage, rb2d.position.x > Player.instance.rb2d.position.x);
        }

        // destroy
        GameObject explosionGO = (GameObject.Instantiate(explosionGameObject, transform.localPosition, transform.localRotation) as GameObject);
        if (visionUser.isVision) {
            VisionUser evu = explosionGO.GetComponent<VisionUser>();
            evu.becomeVisionNow(visionUser.duration - visionUser.time, visionUser);
        }
        timeUser.timeDestroy();
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["h"] = heading;
        fi.floats["hst"] = headingSwitchTime;
        fi.floats["hsd"] = headingSwitchDuration;
        fi.bools["ph"] = positiveHeading;
        fi.floats["tt"] = trailTime;
    }

    void OnRevert(FrameInfo fi) {
        heading = fi.floats["h"];
        headingSwitchTime = fi.floats["hst"];
        headingSwitchDuration = fi.floats["hsd"];
        positiveHeading = fi.bools["ph"];
        trailTime = fi.floats["tt"];
    }

    TimeUser timeUser;
    VisionUser visionUser;
    Rigidbody2D rb2d;

    float headingSwitchTime = 0;
    float headingSwitchDuration = 9999;
    float trailTime = 0;
    float spinSpeed = 0;

}
