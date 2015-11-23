using UnityEngine;
using System.Collections;

/* Parses BoxCollider2D components from the GameObject this is attached to,
 * and creates Areas from them.
 * */

public class AreaParser : MonoBehaviour {

    void Awake() {
        bc2ds = GetComponents<BoxCollider2D>();
        foreach (BoxCollider2D bc2d in bc2ds) {
            
            Area area = new Area();
            area.name = gameObject.name;
            area.left = bc2d.bounds.min.x + gameObject.transform.localPosition.x;
            area.bottom = bc2d.bounds.min.y + gameObject.transform.localPosition.y;
            area.right = bc2d.bounds.max.x + gameObject.transform.localPosition.x;
            area.top = bc2d.bounds.max.y + gameObject.transform.localPosition.y;
            
            Area.addArea(area);

            bc2d.enabled = false;
        }
    }

    void OnDestroy() {
        Area.removeAll();
    }

    // components
    BoxCollider2D[] bc2ds;
}
