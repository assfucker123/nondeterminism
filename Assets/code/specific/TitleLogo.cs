using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TitleLogo : MonoBehaviour {

    public float speed = 100;
    public float loopDist = 400;
    public float speedMultiplier = 1;

	void Awake() {
        bg = transform.Find("BG").GetComponent<Image>();
        startPos = bg.GetComponent<RectTransform>().localPosition.x;
	}
	
	void Update() {

        // set position
        float pos = bg.GetComponent<RectTransform>().localPosition.x;
        pos += speed * Time.unscaledDeltaTime;
        float diff = pos - startPos;
        diff = Utilities.fmod(diff, loopDist);
        bg.GetComponent<RectTransform>().localPosition = new Vector3(startPos + diff, bg.GetComponent<RectTransform>().localPosition.y);

    }

    float startPos = 0;

    Image bg;
}
