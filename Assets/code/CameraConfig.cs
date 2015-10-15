using UnityEngine;
using System.Collections;

public class CameraConfig : MonoBehaviour {

    public static float PIXEL_PER_UNIT = 32;
    public static float PIXEL_PER_UNIT_SCALE = 2;
    public static float ORTHOGRAPHIC_SIZE = 6; //screen height = 768
    //screen size: 1366px x 768px
    //tile size: 16px x 16px
    //room tile size: 44 x 25
    //practical room tile size: 42 x 23 (edge tiles are obscured)
    //orthographic size = screen height / (pixel per unit * pixel per unit scale) / 2
    
    void Awake() {
        
	}

    void Update() {

        if (Input.GetKeyDown(KeyCode.F10)) {
            Screen.fullScreen = !Screen.fullScreen;
        }

    }
}
