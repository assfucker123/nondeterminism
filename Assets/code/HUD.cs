using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUD : MonoBehaviour {

    public static HUD instance { get { return _instance; } }

    public GameObject healthHeartGameObject;
    public GameObject phaseMeterGameObject;

    public int health { get { return _health; } }
    public int maxHealth { get { return _maxHealth; } }
    public PhaseMeter phaseMeter { get { return _phaseMeter; } }

    public void setMaxHealth(int maxHealth){
        if (_maxHealth == maxHealth)
            return;

        _maxHealth = maxHealth;
        int numHH = maxHealth / 2;
        while (healthHearts.Count > numHH) {
            HealthHeart hh = healthHearts[healthHearts.Count - 1];
            GameObject.Destroy(hh.gameObject);
            healthHearts.RemoveAt(healthHearts.Count - 1);
        }
        while (healthHearts.Count < numHH) {
            GameObject hhGO = GameObject.Instantiate(healthHeartGameObject) as GameObject;
            hhGO.transform.SetParent(canvas.transform, false);
            HealthHeart hh = hhGO.GetComponent<HealthHeart>();
            int index = healthHearts.Count;
            healthHearts.Add(hh);

            hh.setPosition(new Vector2(hh.startPosition.x + hh.spacing * index, hh.startPosition.y));
        }
        
    }

    public void setHealth(int health) {
        if (_health == health)
            return;

        if (health < 0) health = 0;
        if (health > maxHealth) health = maxHealth;

        if (health < this.health) {
            //jostle hearts
            foreach (HealthHeart hh in healthHearts) {
                hh.jostle();
            }
        } else if (health > this.health) {
            //shine hearts
            foreach (HealthHeart hh in healthHearts) {
                hh.shine();
            }
        }

        _health = health;

        if (health < 0 || health > healthHearts.Count*2)
            return;
        for (int i = 0; i < healthHearts.Count; i++) {
            HealthHeart.State state = HealthHeart.State.EMPTY;
            if (health >= (i+1)*2) {
                state = HealthHeart.State.FULL;
            }
            if (health == (i+1)*2-1) {
                state = HealthHeart.State.HALF;
            }
            healthHearts[i].state = state;
        }

    }
	
	void Awake() {
        _instance = this;
        canvas = GetComponent<Canvas>();
        //create Phase Meter
        GameObject pmGO = GameObject.Instantiate(phaseMeterGameObject) as GameObject;
        pmGO.transform.SetParent(canvas.transform, false);
        _phaseMeter = pmGO.GetComponent<PhaseMeter>();
        _phaseMeter.setUp();
	}

    void Start() {
        
    }
	
	void Update() {
		
	}

    void OnDestroy() {
        _instance = null;
        foreach (HealthHeart hh in healthHearts) {
            GameObject.Destroy(hh.gameObject);
        }
        healthHearts.Clear();
    }

    private static HUD _instance;

    private int _maxHealth = -1;
    private int _health = -1;

    private List<HealthHeart> healthHearts = new List<HealthHeart>();
    private PhaseMeter _phaseMeter;
	
	// components
    Canvas canvas;
}
