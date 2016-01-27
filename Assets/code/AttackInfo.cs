using UnityEngine;
using System.Collections;

public class AttackInfo {

    public int damage = 0;
    public Vector2 impactPoint = new Vector2();
    public float impactHeading = 0;
    public float impactMagnitude = 0;
    public bool ignoreMercyInvincibility = false;
    public bool breaksChargeShotBarriers = false;
    public bool breaksGrenadeBarriers = false;
    public bool breaksMissileBarriers = false;
    public bool breaksFlareDiveBarriers = false;
    public string message = "";

    public AttackInfo clone() {
        AttackInfo ret = new AttackInfo();
        ret.damage = damage;
        ret.impactPoint = impactPoint;
        ret.impactHeading = impactHeading;
        ret.impactMagnitude = impactMagnitude;
        ret.ignoreMercyInvincibility = ignoreMercyInvincibility;
        ret.breaksChargeShotBarriers = breaksChargeShotBarriers;
        ret.breaksGrenadeBarriers = breaksGrenadeBarriers;
        ret.breaksMissileBarriers = breaksMissileBarriers;
        ret.breaksFlareDiveBarriers = breaksFlareDiveBarriers;
        ret.message = message;
        return ret;
    }

    public bool impactToRight() {
        float h = impactHeading - 360 * Mathf.Floor(impactHeading / 360);
        return h < 90 || h > 270;
    }

}
