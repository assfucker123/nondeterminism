using UnityEngine;
using System.Collections;

public class LethalExplosion : MonoBehaviour {

    public int damage = 2;
    public float startRadius = 1;
    public float endRadius = 1;
    public float duration = .2f;
    public float impactMagnitude = 10; // value given in AttackInfo.impactMagnitude
    public bool breaksChargeShotBarriers = true;
	
	void Awake() {
        timeUser = GetComponent<TimeUser>();
        visionUser = GetComponent<VisionUser>();
	}
	
	void Update() {

        if (timeUser != null && timeUser.shouldNotUpdate)
            return;
        if (visionUser != null && visionUser.isVision)
            return;

        time += Time.deltaTime;

        // deal damage
        if (time < duration || time == Time.deltaTime) {
            float rad = startRadius;
            if (duration < .001f) {
                rad = endRadius;
            } else {
                rad = Utilities.easeOutQuad(time, startRadius, endRadius - startRadius, duration);
            }

            Collider2D[] c2ds = Physics2D.OverlapCircleAll(
                new Vector2(transform.localPosition.x, transform.localPosition.y),
                rad,
                ColFinder.getLayerCollisionMask(gameObject.layer));

            foreach (Collider2D c2d in c2ds) {
                GameObject GO = c2d.gameObject;
                if (GO == null) continue;
                ReceivesDamage rd = GO.GetComponent<ReceivesDamage>();
                if (rd == null) continue;
                Rigidbody2D cRB2D = GO.GetComponent<Rigidbody2D>();

                AttackInfo ai = new AttackInfo();
                ai.damage = damage;
                if (cRB2D == null) {
                    ai.impactPoint = new Vector2(transform.localPosition.x, transform.localPosition.y);
                    ai.impactHeading = Mathf.Atan2(GO.transform.localPosition.y - transform.localPosition.y, GO.transform.localPosition.x - transform.localPosition.x) * 180 / Mathf.PI;
                } else {
                    ai.impactPoint = new Vector2(transform.localPosition.x, transform.localPosition.y);
                    ai.impactHeading = Mathf.Atan2(cRB2D.position.y - transform.localPosition.y, cRB2D.position.x - transform.localPosition.x) * 180/Mathf.PI;
                }
                ai.impactMagnitude = impactMagnitude;
                ai.breaksChargeShotBarriers = breaksChargeShotBarriers;

                rd.dealDamage(ai);

            }
        }

	}

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["explT"] = time;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        time = fi.floats["explT"];
    }

    float time = 0;

    TimeUser timeUser;
    VisionUser visionUser;
	
}
