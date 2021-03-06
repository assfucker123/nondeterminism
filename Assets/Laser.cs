﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Laser : MonoBehaviour {

    bool damagesPlayer = true;
    int damage = 2;
    public float maxDistance = 70;
    public float spriteObjectBaseWidth = 4;
    public float rayParticleSpacing = .6f;
    public float rayParticlesVertSpacing = .2f;
    public float rayParticleSpeed = 6;
    public GameObject rayParticleGameObject;
    public float particleSpawnPeriod = .08f;
    public float particleSpawnRadius = 1;
    public float particleSpread = 120;
    public float particleSpeed = 15;
    public GameObject particleGameObject;

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
        float normal = Mathf.Atan2(rh2d.normal.y, rh2d.normal.x) * Mathf.Rad2Deg;

        // scale sprite object
        spriteObject.transform.localScale = new Vector3(distance / spriteObjectBaseWidth, 1, 1);
        updateRayParticles();

        // spawn particles
        if (hit) {
            particleTime += Time.deltaTime;
            while (particleTime > particleSpawnPeriod) {
                UnityEngine.Random.seed = timeUser.randSeed;
                Vector2 point = UnityEngine.Random.insideUnitCircle;
                timeUser.setRandSeed(UnityEngine.Random.seed);

                point = point * particleSpawnRadius;
                point += endpoint;
                GameObject part = GameObject.Instantiate(particleGameObject, point, Quaternion.identity) as GameObject;
                float scale = timeUser.randomRange(.5f, 1.0f);
                part.transform.localScale = new Vector3(scale, scale, 1);
                float heading = normal;
                heading += timeUser.randomRange(-particleSpread / 2, particleSpread / 2);
                part.transform.localRotation = Utilities.setQuat(heading);
                VisualEffect ve = part.GetComponent<VisualEffect>();
                ve.speed.Set(particleSpeed * Mathf.Cos(heading * Mathf.Deg2Rad), particleSpeed * Mathf.Sin(heading * Mathf.Deg2Rad));

                
                particleTime -= particleSpawnPeriod;
            }
            
        }

        checkDamaging();

    }

    void checkDamaging() {

        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = new Vector2(Mathf.Cos(rotation * Mathf.Deg2Rad), Mathf.Sin(rotation * Mathf.Deg2Rad));

        if (damagesPlayer) {
            int layerMask = 1 << LayerMask.NameToLayer("Players");
            RaycastHit2D rh2d = Physics2D.Raycast(origin, direction, distance, layerMask);
            if (rh2d) {
                Vector2 hitPoint = rh2d.point;
                Vector2 plrPoint = Player.instance.rb2d.position - Player.instance.rb2d.velocity * Time.deltaTime;
                Vector2 rayPoint = Utilities.closestPointOnLineToPoint(origin, origin+direction, plrPoint);
                AttackInfo ai = new AttackInfo();
                ai.damage = damage;
                ai.impactPoint = hitPoint;
                ai.impactHeading = Mathf.Atan2(plrPoint.y - rayPoint.y, plrPoint.x - rayPoint.x) * Mathf.Rad2Deg;
                ai.impactMagnitude = 1;
                Player.instance.GetComponent<ReceivesDamage>().dealDamage(ai);
            }
        }
        
    }

    void Awake() {
        timeUser = GetComponent<TimeUser>();
        visionUser = GetComponent<VisionUser>();
        spriteObject = transform.Find("spriteObject").gameObject;
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        localRotation += 20 * Time.deltaTime;

        rayTime += Time.deltaTime;
        updateRay();

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["lr"] = localRotation;
        fi.floats["rt"] = rayTime;
        fi.floats["pt"] = particleTime;
        fi.floats["d"] = distance;
    }

    void OnRevert(FrameInfo fi) {
        localRotation = fi.floats["lr"];
        rayTime = fi.floats["rt"];
        particleTime = fi.floats["pt"];
        distance = fi.floats["d"];
        updateRayParticles();
    }
    
    void updateRayParticles() {

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
    float particleTime = 0;
    List<GameObject> particles = new List<GameObject>();

    List<GameObject> recycledParticles = new List<GameObject>();

}
