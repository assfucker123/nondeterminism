using UnityEngine;
using System.Collections;

/* Makes a background.
 * I'm not 100% sure what this should actually bg. */
public class AutoBackground : MonoBehaviour {

    public bool vertical = false;
    public BGObject[] bgObjects;
    public int randSeed = 12347614;

    [System.Serializable]
    public class BGObject {
        public GameObject gameObject;
        public float spacing = 10;
        public float spacingVariability = 1; // random offset to spacing
        public float position = 0; // usually y position
        public float positionVariability = 0; // random offset to position
    }

    void spawnBGObjects() {
        int prevSeed = Random.seed;
        Random.seed = randSeed;
        Rect bounds = CameraControl.getMapBounds();
        foreach (BGObject bgo in bgObjects) {
            float x = bounds.xMin - bgo.spacing * Random.value;
            for (; x < bounds.xMax; ) {
                float y = bgo.position + bgo.positionVariability * (Random.value * 2 - 1);
                GameObject.Instantiate(bgo.gameObject, new Vector3(x, y), Quaternion.identity);

                // increment
                x += bgo.spacing + bgo.spacingVariability * (Random.value * 2 - 1);
            }
        }
        Random.seed = prevSeed;
    }

	void Awake() {
		
	}

    void Start() {
        spawnBGObjects();
    }
	
	void Update() {
		
	}
}
