using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChamberPlatform : MonoBehaviour {

    public GameObject rayGameObject;
    public Vector2 rayCenterPos = new Vector2();
    public float raySpread = 4 / 16f;
    public int rayCount = 8;
    public float rayEdgeScaleMultiplier = .5f;
    public float rayPeriodMin = 1.0f;
    public float rayPeriodMax = 1.5f;
    public float rayScaleCenter = 1;
    public float rayScaleRange = .5f;

    public bool playerIsOnPlatform {
        get {
            if (Player.instance == null) return false;
            if (!Player.instance.GetComponent<ColFinder>().hitBottom) return false;
            Vector2 plrPos = Player.instance.rb2d.position;
            Bounds bounds = pc2d.bounds;
            if (plrPos.y < bounds.max.y || plrPos.y > bounds.max.y + 2) return false;
            return (bounds.min.x < plrPos.x && plrPos.x < bounds.max.x);
        }
    }

	void Awake() {
        pc2d = GetComponent<PolygonCollider2D>();
        timeUser = GetComponent<TimeUser>();
        createRays();
	}

    void createRays() {
        for (int i=0; i<rayCount; i++) {
            GameObject rGO = GameObject.Instantiate(rayGameObject);
            rGO.transform.SetParent(this.transform, true);
            rGO.transform.localPosition = new Vector2((i - (rayCount - 1) / 2f) * raySpread, 0) + rayCenterPos;
            Ray ray = new Ray();
            ray.gameObject = rGO;
            ray.period = timeUser.randomRange(rayPeriodMin, rayPeriodMax);
            ray.timeOffset = ray.period * timeUser.randomValue();
            float scaleMult = Utilities.easeLinear(Mathf.Abs(i - (rayCount - 1) / 2f), 1, rayEdgeScaleMultiplier - 1, (rayCount - 1) / 2f);
            ray.scaleCenter = rayScaleCenter * scaleMult;
            ray.scaleRange = rayScaleRange * scaleMult;
            rays.Add(ray);
        }
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        updateRays();
        

	}

    void updateRays() {

        foreach (Ray ray in rays) {
            float t = time + ray.timeOffset;
            float scale = ray.scaleCenter + Mathf.Sin(t / ray.period * Mathf.PI*2) * ray.scaleRange;
            ray.gameObject.transform.localScale = new Vector3(1, scale, 1);
        }

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        updateRays();
    }

    class Ray {
        public GameObject gameObject;
        public float timeOffset = 0;
        public float period = 1;
        public float scaleCenter = 1;
        public float scaleRange = .5f;
    }

    void OnDestroy() {
        foreach (Ray ray in rays) {
            GameObject.Destroy(ray.gameObject);
        }
        rays.Clear();
    }

    List<Ray> rays = new List<Ray>();

    PolygonCollider2D pc2d;
    TimeUser timeUser;
    float time = 0;
}
