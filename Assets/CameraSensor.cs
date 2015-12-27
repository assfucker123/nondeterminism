using UnityEngine;
using System.Collections;

public class CameraSensor : MonoBehaviour {

    void Awake() {
        bc2d = GetComponent<BoxCollider2D>();
    }

    void Update() {

        if (Player.instance == null) return;
        Vector3 plrPos = Player.instance.rb2d.position;

        if (bc2d.bounds.Contains(plrPos)) {
            
        }

    }

    void LateUpdate() {

        if (TimeUser.reverting ||
            Time.timeScale < .0001f ||
            HUD.instance.gameOverScreen.activated)
            return;
        
    }

    BoxCollider2D bc2d;
}
