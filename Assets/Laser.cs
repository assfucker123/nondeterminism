using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Laser : MonoBehaviour {

    public float maxDistance = 70;
    public float spriteObjectBaseWidth = 4;
    public float rayParticleSpacing = .6f;
    public float rayParticlesVertSpacing = .2f;
    public float rayParticleSpeed = 6;
    public GameObject rayParticleGameObject;

    public float rotation {
        get {
            return Utilities.get2DRot(transform.rotation);
        }
        set {
            transform.rotation = Utilities.setQuat(value);
        }
    }
    public float localRotation {
        get {
            return Utilities.get2DRot(transform.localRotation);
        }
        set {
            transform.localRotation = Utilities.setQuat(value);
        }
    }

    public float distance { get; private set; }

    void updateRay() {

        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = new Vector2(Mathf.Cos(rotation * Mathf.Deg2Rad), Mathf.Sin(rotation * Mathf.Deg2Rad));
        int layerMask = 1 << LayerMask.NameToLayer("Default");
        // |= more layer masks?
        
        RaycastHit2D rh2d = Physics2D.Raycast(origin, direction, maxDistance, layerMask);

        bool hit = rh2d;
        distance = hit ? rh2d.distance : maxDistance;
        this.distance = distance;
        Vector2 endpoint = rh2d.point;

        // scale sprite object
        spriteObject.transform.localScale = new Vector3(distance / spriteObjectBaseWidth, 1, 1);
        updateParticles();

    }

    void Awake() {
        timeUser = GetComponent<TimeUser>();
        visionUser = GetComponent<VisionUser>();
        spriteObject = transform.Find("spriteObject").gameObject;
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        localRotation += 100 * Time.deltaTime;

        rayTime += Time.deltaTime;
        updateRay();

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["lr"] = localRotation;
        fi.floats["rt"] = rayTime;
        fi.floats["d"] = distance;
    }

    void OnRevert(FrameInfo fi) {
        localRotation = fi.floats["lr"];
        rayTime = fi.floats["rt"];
        distance = fi.floats["d"];
        updateParticles();
    }
    
    void updateParticles() {

        float offset = Utilities.fmod(rayTime * rayParticleSpeed, rayParticleSpacing * 2);
        int numParticles = Mathf.FloorToInt((distance - offset) / rayParticleSpacing);
        GameObject part;

        // remove particles not needed
        while (particles.Count > numParticles) {
            part = particles[particles.Count - 1];
            particles.RemoveAt(particles.Count - 1);
            part.GetComponent<SpriteRenderer>().enabled = false;
            recycledParticles.Add(part);
        }
        // add needed particles
        while (particles.Count < numParticles) {
            if (recycledParticles.Count > 0) {
                part = recycledParticles[recycledParticles.Count - 1];
                recycledParticles.RemoveAt(recycledParticles.Count - 1);
            } else {
                part = GameObject.Instantiate(rayParticleGameObject);
                part.transform.SetParent(transform, false);
            }
            part.GetComponent<SpriteRenderer>().enabled = true;
            particles.Add(part);
        }
        // position particles
        Vector2 pos = new Vector2();
        for (int i=0; i<particles.Count; i++) {
            pos.y = i % 2 == 0 ? rayParticlesVertSpacing : -rayParticlesVertSpacing;
            pos.x = offset + i * rayParticleSpacing;
            particles[i].transform.localPosition = pos;
        }

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

    TimeUser timeUser;
    VisionUser visionUser;
    GameObject spriteObject;

    float rayTime = 0;
    List<GameObject> particles = new List<GameObject>();

    List<GameObject> recycledParticles = new List<GameObject>();

}
