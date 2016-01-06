using UnityEngine;
using System.Collections;

public class PitfallSensor : MonoBehaviour {

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
                killPlayer();
                playerContained = true;
            }
        } else {
            playerContained = false;
        }

    }
    
    void killPlayer() {
        if (timeUser.shouldNotUpdate)
            return;

        AttackInfo attackInfo = new AttackInfo();
        attackInfo.damage = 9999;
        attackInfo.ignoreMercyInvincibility = true;
        attackInfo.message = "pitfall";

        Player.instance.GetComponent<ReceivesDamage>().dealDamage(attackInfo);

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.bools["pc"] = playerContained;
    }

    void OnRevert(FrameInfo fi) {
        bool prevPlayerContained = playerContained;
        playerContained = fi.bools["pc"];
        if (playerContained && !prevPlayerContained) {
            killPlayer();
        }
    }

    TimeUser timeUser;
    BoxCollider2D bc2d;
    ScriptRunner scriptRunner;
    bool playerContained = false;
}
