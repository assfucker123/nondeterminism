using UnityEngine;
using System.Collections;

public class AttackObject : MonoBehaviour {

    /* Use this to make collider(s) on a gameobject deal damage.
     * Make an empty child object, and add the collider(s) to it.
     * Then add this component to the child object. */

    // MESSAGES:
    // void OnDealDamage(ReceivesDamage rd);

    public int damage = 1;

	void Awake() {
        if (transform.parent == null) {
            rb2d = GetComponent<Rigidbody2D>();
            timeUser = GetComponent<TimeUser>(); //it's okay if this is null
            visionUser = GetComponent<VisionUser>(); //it's okay if this is null
            defaultDeath = GetComponent<DefaultDeath>(); //it's okay if this is null
        } else {
            rb2d = transform.parent.GetComponent<Rigidbody2D>();
            timeUser = transform.parent.GetComponent<TimeUser>(); //it's okay if this is null
            visionUser = transform.parent.GetComponent<VisionUser>(); //it's okay if this is null
            defaultDeath = transform.parent.GetComponent<DefaultDeath>(); //it's okay if this is null
        }
	}
	
	void Update () {
		
	}

    void OnTriggerStay2D(Collider2D c2d){
        if (timeUser != null && timeUser.shouldNotUpdate)
            return;
        if (visionUser != null && visionUser.isVision) //visions can't deal damage
            return;
        if (defaultDeath != null && defaultDeath.activated) //don't deal damage while dying
            return;
        GameObject gO = c2d.gameObject;
        Rigidbody2D gOrb2d = gO.GetComponent<Rigidbody2D>();
        ReceivesDamage rd = gO.GetComponent<ReceivesDamage>();
        if (rd == null)
            return;
        bool toRight = (gOrb2d.position.x > rb2d.position.x);

        rd.dealDamage(damage, toRight);
        SendMessage("OnDealDamage", rd, SendMessageOptions.DontRequireReceiver);
    }
	
	// components
    Rigidbody2D rb2d;
    VisionUser visionUser;
    DefaultDeath defaultDeath;
    TimeUser timeUser;
}
