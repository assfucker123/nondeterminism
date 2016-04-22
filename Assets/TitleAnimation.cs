using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TitleAnimation : MonoBehaviour {

    public float platformY = -2;
    public float railsDistFromPlatforms = 4.44f;
    public float oracleDistFromPlatforms = 2;
    public float railsParallax = .5f;
    public float minDisplayWidth = 20;

    public Sprite[] platformSprites;
    public GameObject platformGameObject;
    public GameObject railsGameObject;

	void Awake() {
        larvaOracle = transform.Find("LarvaOracle").gameObject;
	}

    void Start() {
        generateLoopers(0);
        larvaOracle.transform.localPosition = new Vector2(0, platforms[0].transform.localPosition.y + oracleDistFromPlatforms);
    }

    float dist = 0;
	void Update() {

        dist -= 1.5f * Time.unscaledDeltaTime;
        generateLoopers(dist);
        

	}

    /// <summary>
    /// generates the platforms and rails needed
    /// </summary>
    /// <param name="dist">how far larva oracle has travelled</param>
    void generateLoopers(float dist) {

        float platformWidth = platformGameObject.GetComponent<SpriteRenderer>().sprite.rect.width / platformGameObject.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
        float railWidth = railsGameObject.GetComponent<SpriteRenderer>().sprite.rect.width / railsGameObject.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;

        int numPlatforms = Mathf.CeilToInt(minDisplayWidth / platformWidth);
        int numRails = Mathf.CeilToInt(minDisplayWidth / railWidth);

        // create items if not created yet
        while (platforms.Count < numPlatforms) {
            GameObject platform = GameObject.Instantiate(platformGameObject);
            platform.transform.SetParent(transform, false);
            platforms.Add(platform);
        }
        while (rails.Count < numRails) {
            GameObject rail = GameObject.Instantiate(railsGameObject);
            rail.transform.SetParent(transform, false);
            rails.Add(rail);
        }

        // position items
        GameObject GO;
        for (int i=0; i<platforms.Count; i++) {
            GO = platforms[i];
            float x = dist + i * platformWidth;
            int sprite = (i % 3 < 2 ? 0 : 1);
            Vector2 pos = new Vector2(x, platformY);

            // sprite
            GO.GetComponent<SpriteRenderer>().sprite = platformSprites[sprite];

            // wrapping
            pos.x = Utilities.fmod(pos.x, minDisplayWidth) - minDisplayWidth / 2;

            GO.transform.localPosition = pos;
        }
        for (int i = 0; i < rails.Count; i++) {
            GO = rails[i];
            float x = dist*railsParallax + i * railWidth;
            Vector2 pos = new Vector2(x, platformY + railsDistFromPlatforms);
            
            // wrapping
            pos.x = Utilities.fmod(pos.x, minDisplayWidth) - minDisplayWidth / 2;

            GO.transform.localPosition = pos;
        }

    }

    void OnDestroy() {
        foreach (GameObject GO in platforms) {
            GameObject.Destroy(GO);
        }
        platforms.Clear();
        foreach (GameObject GO in rails) {
            GameObject.Destroy(GO);
        }
        rails.Clear();
    }


    List<GameObject> platforms = new List<GameObject>();
    List<GameObject> rails = new List<GameObject>();

    GameObject larvaOracle;

}
