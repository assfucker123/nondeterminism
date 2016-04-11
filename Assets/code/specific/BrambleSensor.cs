using UnityEngine;
using System.Collections;

public class BrambleSensor : MonoBehaviour {

    public int damage = 1;
    public KnockbackDirection knockbackDirection = KnockbackDirection.LEFT;

    public enum KnockbackDirection {
        LEFT,
        RIGHT
    }

	void Awake() {
		
	}
	
	void Update() {
		
	}

    void OnTriggerStay2D(Collider2D c2d) {
        if (TimeUser.reverting)
            return;
        if (!enabled) return;
        GameObject gO = c2d.gameObject;
        ReceivesDamage rd = gO.GetComponent<ReceivesDamage>();
        if (rd == null)
            return;
        rd.dealDamage(damage, knockbackDirection == KnockbackDirection.RIGHT);
        SendMessage("OnDealDamage", rd, SendMessageOptions.DontRequireReceiver);
    }
}
