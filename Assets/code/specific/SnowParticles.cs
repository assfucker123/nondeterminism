using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(TimeUser))]
public class SnowParticles : MonoBehaviour {

    public float particleDensity = .03f;
    public float particleHeading = -90;
    public float particleHeadingSpread = 10;
    public float particleSpeedMin = 5;
    public float particleSpeedMax = 10;
    public float particleWaveMagnitudeMin = 3;
    public float particleWaveMagnitudeMax = 2;
    public float particleWavePeriodMin = 2;
    public float particleWavePeriodMax = 2;
    public GameObject snowParticleGameObject;

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        // immediately reposition so that it's in center of the map
        Rect bounds = CameraControl.getMapBounds();
        width = bounds.width + 2;
        height = bounds.height + 2;
        transform.localPosition = bounds.min - new Vector2(1, 1);
        numParticles = Mathf.RoundToInt(width * height * particleDensity);
	}

    void Start() {
        // create particles
        while (particles.Count < numParticles) {
            Vector2 startPos = new Vector2(Random.Range(0, width), Random.Range(0, height));
            GameObject pGO = GameObject.Instantiate(snowParticleGameObject, startPos, Quaternion.identity) as GameObject;
            pGO.transform.SetParent(transform, false);
            SnowParticle part = pGO.GetComponent<SnowParticle>();
            part.startPos = startPos;
            part.speed = Random.Range(particleSpeedMin, particleSpeedMax);
            part.heading = particleHeading + Random.Range(-particleHeadingSpread / 2, particleHeadingSpread / 2);
            part.waveMagnitude = Random.Range(particleWaveMagnitudeMin, particleWaveMagnitudeMax);
            part.wavePeriod = Random.Range(particleWavePeriodMin, particleWavePeriodMax);
            part.waveTimeOffset = Random.Range(0, part.wavePeriod);
            part.setSprite();

            particles.Add(part);
        }
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        setParticlePositions();

	}

    void setParticlePositions() {

        foreach (SnowParticle part in particles) {
            Vector2 pos = part.getPosition(time);
            // fit pos in box
            pos.x = Utilities.fmod(pos.x, width);
            pos.y = Utilities.fmod(pos.y, height);
            // set position
            part.gameObject.transform.localPosition = pos;
        }

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        setParticlePositions();
    }


    void OnDestroy() {
        foreach (SnowParticle part in particles) {
            GameObject.Destroy(part.gameObject);
        }
        particles.Clear();
    }

    List<SnowParticle> particles = new List<SnowParticle>();

    TimeUser timeUser;
    float time = 0;

    float width = 42;
    float height = 23;
    int numParticles = 20;
}
