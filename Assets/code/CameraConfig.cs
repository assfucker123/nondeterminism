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

    // Tiled map stuff
    public static Rect getMapBounds() {
        GameObject map = GameObject.FindWithTag("Map");
        Debug.Assert(map != null);
        Tiled2Unity.TiledMap tiledMap = map.GetComponent<Tiled2Unity.TiledMap>();
        Debug.Assert(tiledMap != null);
        return new Rect(
            map.transform.position.x,
            -map.transform.position.y,
            tiledMap.GetMapWidthInPixelsScaled(),
            tiledMap.GetMapHeightInPixelsScaled()
            );
    }
    
    void Awake() {
        
	}

    void Update() {

        if (Input.GetKeyDown(KeyCode.F10)) {
            Screen.fullScreen = !Screen.fullScreen;
        }

    }
}
