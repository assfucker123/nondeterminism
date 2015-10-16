using UnityEngine;
using System.Collections;

public class AttackObject : MonoBehaviour {

    /* Use this to make collider(s) on a gameobject deal damage.
     * Make an empty child object, and add the collider(s) to it.
     * Then add this component to the child object. */

    public int damage = 2;

	void Awake(){
        rb2d = transform.parent.GetComponent<Rigidbody2D>();
        visionUser = transform.parent.GetComponent<VisionUser>(); //it's okay if this is null
	}
	
	void Update () {
		
	}

    void OnTriggerStay2D(Collider2D c2d){
        if (visionUser != null && visionUser.isVision) //visions can't deal damage
            return;
        GameObject gO = c2d.gameObject;
        Rigidbody2D gOrb2d = gO.GetComponent<Rigidbody2D>();
        ReceivesDamage rd = gO.GetComponent<ReceivesDamage>();
        if (rd == null)
            return;
        bool toRight = (gOrb2d.position.x > rb2d.position.x);
        rd.dealDamage(damage, toRight);
    }
	
	// components
    Rigidbody2D rb2d;
    VisionUser visionUser;
}
