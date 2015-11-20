﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Toucade : MonoBehaviour {

    public bool fruitFacade = false;

    public float flyWidth = 14;
    public float flyHeight = 4;
    public float flyHorizPeriod = 6; // in seconds to complete a horizontal sin wave
    public float flyVertPeriod = 10; // in seconds to complete a vertical sin wave

    public float projectileRadiusW = 3;
    public float projectileRadiusH = 2;
    public float projectileRevolveSpeed = 100;
    public float projectileWaitDuration = 1;
    public float projectileGrowDuration = 1;

    public float waitUntilAutoThrow = 2.0f;
    public float throwInitialInterval = .2f;
    public float throwInterval = .4f;
    public float throwSpeed = 12f;

    public float postThrowIdleDuration = .4f;

    public bool startFlyRight = false;
    public bool startFlyUp = false;
    public bool spinClockwise = false;

    Vector2 startPos = new Vector2();

    public State state = State.IDLE;
    public ProjectileState projectileState = ProjectileState.NOT_CREATED;

    public GameObject bubbleGameObject;
    public GameObject grenadeGameObject;



    public enum State {
        IDLE,
        THROWING,
        POST_THROW,
        DEAD //don't do anything; DefaultDeath takes care of this
    }
    public enum ProjectileState {
        NOT_CREATED,
        GROWING,
        SPINNING,
        THROWING,
        POST_THROW
    }

    public bool flippedHoriz {
        get { return spriteRenderer.transform.localScale.x < 0; }
        set {
            if (value == flippedHoriz)
                return;
            spriteRenderer.transform.localScale = new Vector3(
                -spriteRenderer.transform.localScale.x,
                spriteRenderer.transform.localScale.y,
                spriteRenderer.transform.localScale.z);
        }
    }

    public void beginThrow() {
        if (projectileState == ProjectileState.THROWING ||
            state == State.THROWING ||
            state == State.DEAD) return;

        state = State.THROWING; // time will freeze
        rb2d.velocity = Vector2.zero;
        projectileState = ProjectileState.THROWING;
        projectileTime = 0;
        beginThrowFromGettingDamaged = false;

        // ready projctiles
        foreach (GameObject pGO in projectiles) {
            if (pGO == null) continue;
            if (!pGO.GetComponent<TimeUser>().exists) continue;
            Grenade pGrenade = pGO.GetComponent<Grenade>();
            if (pGrenade != null) {
                pGrenade.toWarning();
                pGrenade.GetComponent<ReceivesDamage>().mercyInvincibility(9999); // make grenade unable to be shot
            }
        }

        Player plr = Player.instance;
        if (plr != null) {
            flippedHoriz = (plr.rb2d.position.x < rb2d.position.x);
        }
        animator.Play("attack");
        throwIndex = throwIndexStart;
        throwInitialWait = true;
    }

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        spriteObject = transform.Find("spriteObject").gameObject;
        spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        animator = spriteObject.GetComponent<Animator>();
        colFinder = GetComponent<ColFinder>();
        timeUser = GetComponent<TimeUser>();
        receivesDamage = GetComponent<ReceivesDamage>();
        visionUser = GetComponent<VisionUser>();
        defaultDeath = GetComponent<DefaultDeath>();
        enemyInfo = GetComponent<EnemyInfo>();
    }

    void Start() {
        // attach to Area
        startPos = rb2d.position;
        area = Area.findArea(rb2d.position);
        if (area != null) {
            startFlyRight = (rb2d.position.x < area.center.x);
            startFlyUp = (rb2d.position.y < area.center.y);
        }
    }

    /* Called when being spawned from a Portal */
    void OnSpawn(SpawnInfo si) {
        flippedHoriz = !si.faceRight;
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;
        if (defaultDeath.activated)
            return;

        time += Time.deltaTime;
        projectileTime += Time.deltaTime;

        switch (state) {
        case State.IDLE:
            Vector2 expectedPos = getExpectedPosition(time);
            rb2d.MovePosition(expectedPos);
            break;
        case State.THROWING:
        case State.POST_THROW:
            time -= Time.deltaTime; // freeze time
            rb2d.velocity = Vector2.zero;
            break;
        }

        switch (projectileState) {
        case ProjectileState.NOT_CREATED:
            if (projectileTime >= projectileWaitDuration) {
                // create projectiles
                projectileAngle = timeUser.randomValue() * 90;
                grenadeIndex = Mathf.FloorToInt(timeUser.randomValue() * 4);
                spinClockwise = (timeUser.randomValue() < .5f);
                for (int i = 0; i < 4; i++) {
                    GameObject GOToClone = bubbleGameObject;
                    if (i == grenadeIndex) {
                        GOToClone = grenadeGameObject;
                    }
                    GameObject GO = GameObject.Instantiate(
                        GOToClone,
                        transform.localPosition,
                        Quaternion.identity) as GameObject;
                    projectiles.Add(GO);
                    if (fruitFacade) {
                        Debug.Log("create fruit facades");
                    }
                }
                // go to grow state
                projectileTime -= projectileWaitDuration;
                projectileState = ProjectileState.GROWING;
            }
            break;
        case ProjectileState.GROWING:
            if (spinClockwise) {
                projectileAngle -= projectileRevolveSpeed * Time.deltaTime;
            } else {
                projectileAngle += projectileRevolveSpeed * Time.deltaTime;
            }
            positionProjectiles();
            if (projectileTime > projectileGrowDuration) {
                projectileTime -= projectileGrowDuration;
                projectileState = ProjectileState.SPINNING;
            }
            // die if grenade was shot
            foreach (GameObject projGO in projectiles) {
                TimeUser pTimeUser = projGO.GetComponent<TimeUser>();
                if (!pTimeUser.exists) {
                    if (projGO.GetComponent<Grenade>()) { // if grenade was destroyed, kill self
                        receivesDamage.health = 0;
                        flippedHoriz = (projGO.transform.localPosition.x < rb2d.position.x);
                        die();
                    }
                    break;
                }
            }
            break;
        case ProjectileState.SPINNING:
            if (spinClockwise) {
                projectileAngle -= projectileRevolveSpeed * Time.deltaTime;
            } else {
                projectileAngle += projectileRevolveSpeed * Time.deltaTime;
            }
            positionProjectiles();
            if (projectileTime >= waitUntilAutoThrow ||
                beginThrowFromGettingDamaged) {
                // begin throw after taking too long or from taking damage
                beginThrow();
            } else {
                // begin throw after a projectile was destroyed
                foreach (GameObject projGO in projectiles) {
                    TimeUser pTimeUser = projGO.GetComponent<TimeUser>();
                    if (!pTimeUser.exists) {
                        if (projGO.GetComponent<Grenade>()) { // but if grenade was destroyed, kill self
                            receivesDamage.health = 0;
                            flippedHoriz = (projGO.transform.localPosition.x < rb2d.position.x);
                            die();
                        }
                        beginThrow();
                        break;
                    }
                }
            }
            break;
        case ProjectileState.THROWING:
            float interval = throwInterval;
            if (throwInitialWait) {
                interval = throwInitialInterval;
            }
            if (projectileTime >= interval) {
                // throw projectile

                bool thrown = false;
                int checkCount = 0;
                while (throwInitialWait || throwIndex != throwIndexStart) {
                    Player plr = Player.instance;
                    GameObject projGO = projectiles[throwIndex];
                    if (projGO == null || !projGO.GetComponent<TimeUser>().exists) {
                        incrementThrowIndex();
                    } else {
                        if (thrown)
                            break;
                        // throwing the projectile
                        Rigidbody2D pRB2D = projGO.GetComponent<Rigidbody2D>();
                        Vector2 dir = plr.rb2d.position - pRB2D.position;
                        if (projGO.GetComponent<Grenade>() != null) {
                            dir.y += 1f; // aim to offset gravity
                        }
                        dir.Normalize();
                        dir = dir * throwSpeed;
                        pRB2D.MovePosition(pRB2D.position + dir * Time.fixedDeltaTime);
                        Grenade pGrenade = projGO.GetComponent<Grenade>();
                        Bubble pBubble = projGO.GetComponent<Bubble>();
                        if (pGrenade != null) {
                            pGrenade.explodeOnContact = true;
                        }
                        if (pBubble != null) {
                            pBubble.popOnContact = true;
                        }

                        incrementThrowIndex();
                        thrown = true;
                    }
                    // failsafe to prevent infinite loop
                    checkCount++;
                    if (checkCount > projectiles.Count) {
                        throwIndex = throwIndexStart;
                        break;
                    }
                }

                throwInitialWait = false;
                projectileTime -= interval;
                
                if (throwIndex == throwIndexStart) {
                    // at the begin point, all projectiles have been thrown
                    projectiles.Clear();
                    projectileFruits.Clear();
                    state = State.POST_THROW;
                    projectileState = ProjectileState.POST_THROW;
                    projectileTime = 0;
                }
                
            }
            if (spinClockwise) {
                projectileAngle -= projectileRevolveSpeed * Time.deltaTime;
            } else {
                projectileAngle += projectileRevolveSpeed * Time.deltaTime;
            }
            positionProjectiles();
            break;
        case ProjectileState.POST_THROW:
            if (projectileTime >= postThrowIdleDuration) {
                // go to idle state
                animator.Play("idle");
                state = State.IDLE;
                projectileTime -= postThrowIdleDuration;
                projectileState = ProjectileState.NOT_CREATED;
                beginThrowFromGettingDamaged = false;
            }
            break;

        }

        // travel across a segment:
        /*
        x = segment.travelClamp(rb2d.position.x, speed, Time.fixedDeltaTime);
        rb2d.MovePosition(new Vector2(x, rb2d.position.y));
        */

        // create a vision:
        /*
        GameObject vGO = visionUser.createVision(VisionUser.VISION_DURATION);
        */

        // spawn a bullet:
        /*
        GameObject bulletGO = GameObject.Instantiate(bulletGameObject,
            rb2d.position + relSpawnPosition,
            Utilities.setQuat(heading)) as GameObject;
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        bullet.heading = heading;
        if (visionUser.isVision) { //make bullet a vision if this is also a vision
            VisionUser bvu = bullet.GetComponent<VisionUser>();
            bvu.becomeVisionNow(visionUser.duration - visionUser.time, visionUser);
        }
        */

    }

    void incrementThrowIndex() {
        throwIndex++;
        if (throwIndex >= projectiles.Count) {
            throwIndex = 0;
        }
    }

    Vector2 getExpectedPosition(float time) {
        Vector2 ret = startPos;
        float x = flyWidth * (1 - Mathf.Cos(time * Mathf.PI * 2 / flyHorizPeriod)) / 2;
        if (!startFlyRight)
            x *= -1;
        float y = flyHeight * (1 - Mathf.Cos(time * Mathf.PI * 2 / flyVertPeriod)) / 2;
        if (!startFlyUp)
            y *= -1;
        ret.x += x;
        ret.y += y;
        return ret;
    }

    void positionProjectiles() {
        for (int i = 0; i < projectiles.Count; i++) {
            GameObject pGO = projectiles[i];
            if (pGO == null) continue;
            Rigidbody2D prb2d = pGO.GetComponent<Rigidbody2D>();

            bool wasThrown = false;
            if (projectileState == ProjectileState.THROWING && !throwInitialWait) {
                if (throwIndexStart <= throwIndex) {
                    if (throwIndexStart <= i && i < throwIndex)
                        wasThrown = true;
                } else {
                    if (throwIndexStart <= 0)
                        wasThrown = true;
                    if (i < throwIndex)
                        wasThrown = true;
                }
            }
            if (wasThrown)
                continue;

            float angle = (projectileAngle + i * 90) * Mathf.PI / 180;
            float x = Mathf.Cos(angle) * projectileRadiusW;
            float y = Mathf.Sin(angle) * projectileRadiusH;
            if (projectileState == ProjectileState.GROWING) {
                x *= Utilities.easeOutQuadClamp(projectileTime, 0, 1, projectileGrowDuration);
                y *= Utilities.easeOutQuadClamp(projectileTime, 0, 1, projectileGrowDuration);
            }
            Vector2 pos = rb2d.position;
            pos.x += x;
            pos.y += y;
            prb2d.MovePosition(pos);
            
        }
    }

    /* called when this becomes a vision */
    void TimeSkip(float timeInFuture) {

        // Start() hasn't been called yet
        Start();

        // increment time
        time += timeInFuture;
    }

    /* called when this takes damage */
    void OnDamage(AttackInfo ai) {
        beginThrowFromGettingDamaged = true;
        if (receivesDamage.health <= 0) {
            // death
            flippedHoriz = ai.impactToRight();
            die();
        }
    }

    void die() {
        animator.Play("damage");
        foreach (GameObject projGO in projectiles) {
            Grenade pGrenade = projGO.GetComponent<Grenade>();
            Bubble pBubble = projGO.GetComponent<Bubble>();
            if (pGrenade != null) {
                pGrenade.explode();
            }
            if (pBubble != null) {
                pBubble.pop();
            }
        }
        defaultDeath.activate(flippedHoriz);
        projectiles.Clear();
        state = State.DEAD;
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.ints["pState"] = (int)projectileState;
        fi.floats["time"] = time;
        fi.floats["pTime"] = projectileTime;
        fi.floats["pAngle"] = projectileAngle;
        fi.ints["gi"] = grenadeIndex;
        fi.bools["pCreated"] = projectilesCreated;
        fi.bools["spinClockwise"] = spinClockwise;
        fi.ints["throwI"] = throwIndex;
        fi.ints["throwIS"] = throwIndexStart;
        fi.bools["tiw"] = throwInitialWait;
        fi.bools["btfgd"] = beginThrowFromGettingDamaged;

        fi.ints["numProjectiles"] = projectiles.Count;
        fi.ints["numProjectileFruits"] = projectileFruits.Count;
        for (int i = 0; i < projectiles.Count; i++) {
            fi.gameObjects["p" + i] = projectiles[i];
        }
        for (int i = 0; i < projectileFruits.Count; i++) {
            fi.gameObjects["pf" + i] = projectileFruits[i];
        }
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        projectileState = (ProjectileState)fi.ints["pState"];
        time = fi.floats["time"];
        projectileTime = fi.floats["pTime"];
        projectileAngle = fi.floats["pAngle"];
        grenadeIndex = fi.ints["gi"];
        spinClockwise = fi.bools["spinClockwise"];
        throwIndex = fi.ints["throwI"];
        throwIndexStart = fi.ints["throwIS"];
        throwInitialWait = fi.bools["tiw"];
        beginThrowFromGettingDamaged = fi.bools["btfgd"];
        
        projectilesCreated = fi.bools["pCreated"];

        projectiles.Clear();
        projectileFruits.Clear();
        int numProjectiles = fi.ints["numProjectiles"];
        int numProjectileFruits = fi.ints["numProjectileFruits"];
        for (int i = 0; i < numProjectiles; i++) {
            projectiles.Add(fi.gameObjects["p" + i]);
        }
        for (int i = 0; i < numProjectileFruits; i++) {
            projectiles.Add(fi.gameObjects["pf" + i]);
        }

    }

    float time;
    float projectileTime;
    float projectileAngle = 0;
    bool projectilesCreated = false;
    int throwIndex = 0;
    int throwIndexStart = 0;
    bool throwInitialWait = false;
    bool beginThrowFromGettingDamaged = false;
    Area area;

    List<GameObject> projectiles = new List<GameObject>();
    List<GameObject> projectileFruits = new List<GameObject>();
    int grenadeIndex = 0;

    

    // components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    Animator animator;
    ColFinder colFinder;
    TimeUser timeUser;
    ReceivesDamage receivesDamage;
    VisionUser visionUser;
    DefaultDeath defaultDeath;
    EnemyInfo enemyInfo;

}
