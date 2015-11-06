using UnityEngine;
using System.Collections;

/* Parses EdgeCollider2D components from the GameObject this is attached to,
 * and creates Segments from them.
 * Each EdgeCollider2D should have 2 points and be horizontal or vertical.
 * */

public class SegmentParser : MonoBehaviour {
	
	void Awake() {
        ec2ds = GetComponents<EdgeCollider2D>();
        foreach (EdgeCollider2D ec2d in ec2ds) {
            ec2d.enabled = false;
            if (ec2d.pointCount != 2){
                Debug.Log("Edge colliders for SegmentParser need to have 2 points");
                continue;
            }
            Segment seg = new Segment();
            seg.name = gameObject.name;
            seg.p0 = ec2d.points[0] + new Vector2(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y);
            seg.p1 = ec2d.points[1] + new Vector2(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y);
            seg.snapToWall();
            Segment.addSegment(seg);
        }
	}

    void OnDestroy() {
        Segment.removeAll();
    }
	
	// components
    EdgeCollider2D[] ec2ds;
}
