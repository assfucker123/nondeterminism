using UnityEngine;
using System.Collections;

/* triggers an ambush when player triggers the sensor */
public class AmbushSensor : MonoBehaviour {

    public GameObject ambushRef = null; // if left null, will activate closest ambush

	void Awake() {
		
	}
	
	void Update() {
		
	}

    void OnTriggerEnter2D(Collider2D c2d) {
        if (c2d.gameObject == null) return;
        if (c2d.gameObject != Player.instance.gameObject) return;
        if (TimeUser.reverting) return;

        if (ambushRef == null)
            ambushRef = findClosestAmbush();
        if (ambushRef == null)
            return;
        if (ambushRefA == null)
            ambushRefA = ambushRef.GetComponent<Ambush>();
        if (ambushRefA == null)
            return;

        ambushRefA.activate();
    }

    GameObject findClosestAmbush() {
        Ambush[] ambushes = GameObject.FindObjectsOfType<Ambush>();
        if (ambushes.Length == 0) return null;
        if (ambushes.Length == 1) return ambushes[0].gameObject;
        Ambush ret = ambushes[0];
        float dist = Vector3.SqrMagnitude(ret.transform.localPosition - transform.localPosition);
        foreach (Ambush ambush in ambushes) {
            float d = Vector3.SqrMagnitude(ambush.transform.localPosition - transform.localPosition);
            if (d < dist) {
                dist = d;
                ret = ambush;
            }
        }
        return ret.gameObject;
    }

    Ambush ambushRefA = null;

}
