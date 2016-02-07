using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PickupSpawner : MonoBehaviour {

    public GameObject healthSmallPickupGameObject;
    public GameObject healthPickupGameObject;
    public GameObject healthBigPickupGameObject;
    public GameObject phasePickupGameObject;
    public GameObject phaseBigPickupGameObject;
	
	void Awake() {
		timeUser = GetComponent<TimeUser>(); // not needed
	}
    
    /* The number represents how many pickups will spawn */
    public enum BurstSize {
        NONE,
        SMALL,
        MEDIUM,
        LARGE
    }
    public static int SIZE_NONE = 0;
    public static int SIZE_SMALL = 1;
    public static int SIZE_MEDIUM = 3;
    public static int SIZE_LARGE = 5;

    /* Multiplied by the chance of health appearing */
    public static float HEALTH_RARITY = .4f;

    /* Pickups that spawn are dependant on Player's health and phase */
    public void burstSpawn(Vector2 position, BurstSize burstSize, float velocityMultiplier = 1, float velocityRotation = 0) {
        if (burstSize == BurstSize.NONE)
            return;
        
        float percentHealth = .5f;
        float percentPhase = .5f;
        if (Player.instance != null) {
            percentHealth = 1 - Player.instance.health * 1.0f / Player.instance.maxHealth;
            percentPhase = 1 - Player.instance.phase / Player.instance.maxPhase;
        }
        float randSpread = .4f;
        if (timeUser == null) {
            percentHealth *= Random.Range(1 - randSpread, 1 + randSpread);
            percentPhase *= Random.Range(1 - randSpread, 1 + randSpread);
        } else {
            percentHealth *= 1 + ((timeUser.randomValue() * 2 - 1) * randSpread);
            percentPhase *= 1 + ((timeUser.randomValue() * 2 - 1) * randSpread);
        }
        percentHealth *= HEALTH_RARITY;

        float tot = percentHealth + percentPhase;
        if (tot < .001f) {
            percentHealth = HEALTH_RARITY;
            percentPhase = 1 - percentHealth;
        } else {
            percentHealth /= tot;
            percentPhase /= tot;
        }

        int totalPickups = 0;
        switch (burstSize) {
        case BurstSize.SMALL: totalPickups = SIZE_SMALL; break;
        case BurstSize.MEDIUM: totalPickups = SIZE_MEDIUM; break;
        case BurstSize.LARGE: totalPickups = SIZE_LARGE; break;
        }
        
        int numSmallHealth = Mathf.RoundToInt(totalPickups * percentHealth);
        int numPhase = totalPickups - numSmallHealth;

        //convert some pickups to their bigger versions
        int numHealth = 0;
        int numBigHealth = 0;
        int numBigPhase = 0;
        int healthRatio = Mathf.RoundToInt(
            healthPickupGameObject.GetComponent<Pickup>().amount / healthSmallPickupGameObject.GetComponent<Pickup>().amount);
        int bigHealthRatio = Mathf.RoundToInt(
            healthBigPickupGameObject.GetComponent<Pickup>().amount / healthPickupGameObject.GetComponent<Pickup>().amount);
        if (numSmallHealth >= healthRatio) {
            if (timeUser == null) {
                numHealth = Mathf.RoundToInt(Random.value * (numSmallHealth / healthRatio));
            } else {
                numHealth = Mathf.RoundToInt(timeUser.randomValue() * (numSmallHealth / healthRatio));
            }
            numSmallHealth -= numHealth * healthRatio;
        }
        if (numHealth >= bigHealthRatio) {
            if (timeUser == null) {
                numBigHealth = Mathf.RoundToInt(Random.value * (numHealth / bigHealthRatio));
            } else {
                numBigHealth = Mathf.RoundToInt(timeUser.randomValue() * (numHealth / bigHealthRatio));
            }
            numHealth -= numBigHealth * bigHealthRatio;
        }
        int bigPhaseRatio = Mathf.RoundToInt(
            phaseBigPickupGameObject.GetComponent<Pickup>().amount / phasePickupGameObject.GetComponent<Pickup>().amount);
        if (numPhase >= bigPhaseRatio) {
            if (timeUser == null) {
                numBigPhase = Mathf.RoundToInt(Random.value * (numPhase / bigPhaseRatio));
            } else {
                numBigPhase = Mathf.RoundToInt(timeUser.randomValue() * (numPhase / bigPhaseRatio));
            }
            numPhase -= numBigPhase * bigPhaseRatio;
        }
        
        //spawn pickups
        List<Pickup> pickups = new List<Pickup>();
        int i = 0;
        GameObject gO;
        for (i = 0; i < numSmallHealth; i++) {
            gO = GameObject.Instantiate(healthSmallPickupGameObject, new Vector3(position.x, position.y), Quaternion.identity) as GameObject;
            if (Player.instance != null &&
                Player.instance.health % 2 == 1) {
                gO.transform.localScale = new Vector3(-gO.transform.localScale.x, gO.transform.localScale.y, gO.transform.localScale.z);
            }
            pickups.Add(gO.GetComponent<Pickup>());
        }
        for (i = 0; i < numHealth; i++) {
            gO = GameObject.Instantiate(healthPickupGameObject, new Vector3(position.x, position.y), Quaternion.identity) as GameObject;
            pickups.Add(gO.GetComponent<Pickup>());
        }
        for (i = 0; i < numBigHealth; i++) {
            gO = GameObject.Instantiate(healthBigPickupGameObject, new Vector3(position.x, position.y), Quaternion.identity) as GameObject;
            pickups.Add(gO.GetComponent<Pickup>());
        }
        for (i = 0; i < numPhase; i++) {
            gO = GameObject.Instantiate(phasePickupGameObject, new Vector3(position.x, position.y), Quaternion.identity) as GameObject;
            pickups.Add(gO.GetComponent<Pickup>());
        }
        for (i = 0; i < numBigPhase; i++) {
            gO = GameObject.Instantiate(phaseBigPickupGameObject, new Vector3(position.x, position.y), Quaternion.identity) as GameObject;
            pickups.Add(gO.GetComponent<Pickup>());
        }

        //set pickup velocities
        for (i = 0; i < pickups.Count; i++) {
            Pickup p = pickups[i];
            Rigidbody2D rb2d = p.GetComponent<Rigidbody2D>();

            Vector2 v = new Vector2();
            if (p.type == Pickup.Type.HEALTH) {
                if (timeUser == null) {
                    v.x = 2 * (Random.value * 2 - 1);
                } else {
                    v.x = 2 * (timeUser.randomValue() * 2 - 1);
                }
                v.y = 5;
            } else {
                if (timeUser == null) {
                    v.x = 5 * (Random.value * 2 - 1);
                    v.y = 10 + 5 * (Random.value * 2 - 1);
                } else {
                    v.x = 5 * (timeUser.randomValue() * 2 - 1);
                    v.y = 10 + 5 * (timeUser.randomValue() * 2 - 1);
                }
            }

            v *= velocityMultiplier;
            v = Utilities.rotateAroundPoint(v, Vector2.zero, velocityRotation * Mathf.PI / 180);

            rb2d.velocity = v;
        }

        pickups.Clear();
    }
	
	// components
    TimeUser timeUser;
}
