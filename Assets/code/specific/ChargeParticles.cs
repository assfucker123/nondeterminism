using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* For the particle effect that happens behind Oracle as she charges her gun. */
public class ChargeParticles : MonoBehaviour {

    public float initialDelay = .4f;
    public float spawnRadius = 1.0f;
    public float spawnPeriod = .07f;
    public float speedInitial = 40;
    public float speedFinal = 300;
    public int simultaneousBullets = 3;
    public float spinSpeed = 350;
    public GameObject chargeParticleGameObject = null;

    public static Color CHARGE_FLASH_COLOR = new Color(1f, .60f, 1f);
    public static float CHARGE_FLASH_DURATION = .2f;

    public bool spawningParticlesPastDelay { get { return started && time > initialDelay; } }

    bool _tiny = false;
    public bool tiny {
        get { return _tiny; }
        set {
            if (value == tiny) return;
            _tiny = value;
            if (tiny) {
                // convert all particles to tiny
                foreach (GameObject GO in particles) {
                    GO.GetComponent<Animator>().Play("tiny");
                }
            } else {
                // convert all particles to normal
                foreach (GameObject GO in particles) {
                    GO.GetComponent<Animator>().Play("normal");
                }
            }
        }
    }

    public void startSpawning() {
        if (started) return;
        started = true;
        time = 0;
        spawnTime = 0;
    }

    public void stopSpawning() {
        if (!started) return;
        // destroy all particles
        /*
        while (particles.Count > 0) {
            destroyParticle(particles[particles.Count - 1]);
        }
        particleLocations.Clear();
        */
        started = false;
    }

	
	void Awake() {
        timeUser = GetComponent<TimeUser>();
	}
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        if (started) {
            time += Time.deltaTime;
            if (spawningParticlesPastDelay) {
                // past initialDelay, now make particles
                spawnTime += Time.deltaTime;
                while (spawnTime >= spawnPeriod) {

                    for (int i = 0; i < simultaneousBullets; i++) {
                        float angle = time * spinSpeed;
                        angle += i * 360.0f / simultaneousBullets;
                        angle *= Mathf.PI / 180;
                        Vector2 pos = new Vector2(spawnRadius * Mathf.Cos(angle), spawnRadius * Mathf.Sin(angle));
                        particleLocations.Add(pos);
                    }
                    
                    spawnTime -= spawnPeriod;
                }
            }
        }

        // animate particles
        for (int i = 0; i < particleLocations.Count; i++) {
            Vector2 pos = particleLocations[i];
            float l = pos.magnitude;
            float speed = Utilities.easeLinear(spawnRadius - l, speedInitial, speedFinal - speedInitial, spawnRadius);
            float lNew = l - speed * Time.deltaTime;
            if (lNew <= 0) { // delete particle
                particleLocations.RemoveAt(i);
                i--;
                continue;
            }
            pos *= lNew / l;
            particleLocations[i] = pos;
        }

        // sync particles with locations
        syncParticles();

	}

    void syncParticles() {
        while (particles.Count < particleLocations.Count) {
            makeParticle();
        }
        while (particles.Count > particleLocations.Count) {
            destroyParticle(particles[particles.Count - 1]);
        }
        for (int i = 0; i < particles.Count; i++) {
            GameObject partGO = particles[i];
            Vector2 pos = particleLocations[i];
            partGO.transform.localPosition = pos;
            partGO.transform.localRotation = Utilities.setQuat(Mathf.Atan2(pos.y, pos.x) * 180 / Mathf.PI);
        }
    }

    GameObject makeParticle() {
        GameObject part;
        if (recycledParticles.Count > 0) {
            part = recycledParticles[recycledParticles.Count - 1];
            recycledParticles.RemoveAt(recycledParticles.Count - 1);
        } else {
            part = GameObject.Instantiate(chargeParticleGameObject) as GameObject;
        }
        SpriteRenderer sr = part.GetComponent<SpriteRenderer>();
        sr.enabled = true;
        part.transform.SetParent(transform, false);
        part.transform.SetAsFirstSibling();
        if (tiny) {
            part.GetComponent<Animator>().Play("tiny");
        } else {
            part.GetComponent<Animator>().Play("normal");
        }
        particles.Add(part);
        return part;
    }
    void destroyParticle(GameObject part) {
        particles.Remove(part);
        SpriteRenderer sr = part.GetComponent<SpriteRenderer>();
        sr.enabled = false;
        recycledParticles.Add(part);
    }

    void OnDestroy() {
        foreach (GameObject part in particles) {
            GameObject.Destroy(part);
        }
        particles.Clear();
        foreach (GameObject part in recycledParticles) {
            GameObject.Destroy(part);
        }
        recycledParticles.Clear();
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.bools["s"] = started;
        fi.floats["t"] = time;
        fi.floats["st"] = spawnTime;
        fi.strings["pl"] = particleLocationsToString();
        fi.bools["tiny"] = tiny;
    }

    void OnRevert(FrameInfo fi) {
        started = fi.bools["s"];
        time = fi.floats["t"];
        spawnTime = fi.floats["st"];
        stringToParticleLocations(fi.strings["pl"]);
        syncParticles();
        tiny = fi.bools["tiny"];
    }

    string particleLocationsToString() {
        string str = "";
        for (int i = 0; i < particleLocations.Count; i++) {
            Vector2 v = particleLocations[i];
            string loc = "" + v.x + "," + v.y;
            if (i < particleLocations.Count - 1) {
                loc += " ";
            }
            str += loc;
        }
        return str;
    }

    void stringToParticleLocations(string str) {
        particleLocations.Clear();
        char[] delimiters = {' '};
        string[] strs = str.Split(delimiters);
        for (int i = 0; i < strs.Length; i++) {
            Vector2 v = new Vector2();
            string s = strs[i];
            int index = s.IndexOf(',');
            if (index == -1) continue;
            string xStr = s.Substring(0, index);
            string yStr = s.Substring(index + 1);
            v.x = float.Parse(xStr);
            v.y = float.Parse(yStr);
            particleLocations.Add(v);
        }
    }

    List<GameObject> particles = new List<GameObject>(); // particles being displayed
    List<Vector2> particleLocations = new List<Vector2>(); // stores locations of all particles
    List<GameObject> recycledParticles = new List<GameObject>(); // particles hidden

    bool started = false;
    float time = 0;
    float spawnTime = 0;

    TimeUser timeUser;
	
}
