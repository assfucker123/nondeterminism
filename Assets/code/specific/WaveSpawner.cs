using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Feature to implement later: some waves can secretly be escaped from early.
 * */

[RequireComponent(typeof(TimeUser))]
public class WaveSpawner : MonoBehaviour {

    public GameObject portalGameObject;
    public bool autoStart = false;
    public List<GameObject> enemyGameObjects = new List<GameObject>();
    public List<EnemyWave> enemyWaves = new List<EnemyWave>();
    
    public int waveIndex { get { return _waveIndex; } }
    public int waveEnemyIndex { get { return _waveEnemyIndex; } }
    public int waveEnemyCountIndex { get { return _waveEnemyCountIndex; } }
    public bool notStarted { get { return waveIndex < 0;} }
    public bool finished { get { return waveIndex >= enemyWaves.Count; } }
    public EnemyWave currentWave {
        get {
            if (notStarted || finished) return null;
            return enemyWaves[waveIndex];
        }
    }
    public bool waveExhausted { // if there are no enemies left to spawn in the current wave, and just waiting for all enemies to be destroyed
        get {
            if (currentWave == null) return true;
            if (waveEnemyIndex >= currentWave.enemies.Count) return true;
            return false;
        }
    }
    [HideInInspector]
    public float currentDanger = 0;
    public float currentMaxDanger { get { return _currentMaxDanger; } }

    /* Starts the wave spawner */
    public void startSpawner() {
        if (enemyWaves.Count <= 0) {
            Debug.Log("Warning: there are no enemyWaves in this WaveSpawner.  Starting it doesn't do anything.");
            _waveIndex = 0;
            return;
        }
        newWave(0);
        
    }
    
    /////////////
    // PRIVATE //
    /////////////

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        Debug.Assert(portalGameObject != null);
		// sort enemyGameObjects into the mapping
        foreach (GameObject gO in enemyGameObjects) {
            EnemyInfo ei = gO.GetComponent<EnemyInfo>();
            Debug.Assert(ei != null);
            enemyMapping[ei.id] = gO;
        }
	}

    void Start() {
        if (autoStart && notStarted) {
            startSpawner();
        }
    }
	
	void Update() {

        segmentsSpawnedOnThisFrame.Clear();

        if (timeUser.shouldNotUpdate)
            return;

        if (notStarted || finished)
            return;

        // try to add more enemies until max danger is reached
        while (currentDanger < currentMaxDanger) {

            if (waveExhausted) {
                // wave has no more enemies to spawn, so wait until all enemies die
                if (currentDanger < .0001f) {
                    newWave(waveIndex + 1);
                    if (finished) {
                        Debug.Log("Wave spawner finished");
                    }
                }
                break;
            }

            // check next enemy
            EnemyWave.EnemyWavePart ewp = currentWave.enemies[waveEnemyIndex];
            GameObject eGO = getGameObject(ewp.enemy);
            EnemyInfo ei = eGO.GetComponent<EnemyInfo>();
            if (currentDanger + ei.danger <= currentMaxDanger) {
                
                // spawn enemy
                Vector2 spawnPos = new Vector2();
                bool spawnWithPortal = true;
                Portal portal = null;
                List<Segment> possibleSegments = null;
                Segment seg = null;

                // set spawnPos based on spawnMethod
                switch (ewp.spawnMethod) {
                case EnemyWave.SpawnMethod.ANY:
                case EnemyWave.SpawnMethod.EMPTY:
                case EnemyWave.SpawnMethod.NAMED:

                    // spawn at a random position on a random (or not) Segment
                    switch (ei.spawnLocation) {
                    case EnemyInfo.SpawnLocation.BOTTOM_SEGMENT:
                        possibleSegments = Segment.bottomSegments;
                        if (possibleSegments.Count == 0) {
                            Debug.LogError("Can't spawn because there are no bottom segments");
                        }
                        break;
                    case EnemyInfo.SpawnLocation.TOP_SEGMENT:
                        possibleSegments = Segment.topSegments;
                        if (possibleSegments.Count == 0) {
                            Debug.LogError("Can't spawn because there are no top segments");
                        }
                        break;
                    case EnemyInfo.SpawnLocation.VERT_SEGMENT:
                        possibleSegments = new List<Segment>();
                        possibleSegments.AddRange(Segment.leftSegments);
                        possibleSegments.AddRange(Segment.rightSegments);
                        if (possibleSegments.Count == 0) {
                            Debug.LogError("Can't spawn because there are no vert segments");
                        }
                        break;
                    case EnemyInfo.SpawnLocation.AREA:
                        Debug.Log("TODO: area spawning");
                        break;
                    }

                    if (possibleSegments != null) {
                        // pick one of the possible segments
                        switch (ewp.spawnMethod) {
                        case EnemyWave.SpawnMethod.ANY:
                            seg = Segment.weightedRandom(possibleSegments, timeUser.randomValue());
                            break;
                        case EnemyWave.SpawnMethod.EMPTY:
                            seg = Segment.weightedRandom(findUnoccupiedSegments(possibleSegments), timeUser.randomValue());
                            break;
                        case EnemyWave.SpawnMethod.NAMED:
                            foreach (Segment possibleSeg in possibleSegments) {
                                if (possibleSeg.name == ewp.location) {
                                    seg = possibleSeg;
                                }
                            }
                            if (seg == null) {
                                Debug.Log("Segment named " + ewp.location + " not found.");
                                seg = Segment.weightedRandom(possibleSegments, timeUser.randomValue());
                            }
                            break;
                        }
                    }

                    // spawning with segment, find spawn position
                    if (seg != null) {
                        segmentsSpawnedOnThisFrame.Add(seg);
                        spawnPos = seg.interpolate(timeUser.randomValue());
                        switch (seg.wall) {
                        case Segment.Wall.BOTTOM:
                            spawnPos.y += ei.spawnDist;
                            break;
                        case Segment.Wall.TOP:
                            spawnPos.y -= ei.spawnDist;
                            break;
                        case Segment.Wall.LEFT:
                            spawnPos.x += ei.spawnDist;
                            break;
                        case Segment.Wall.RIGHT:
                            spawnPos.x -= ei.spawnDist;
                            break;
                        }
                    }
                    break;

                case EnemyWave.SpawnMethod.NAMED_POINT:

                    // spawn at a named point in the editor somewhere
                    Debug.Log("EnemyWave.SpawnMethod.NAMED_POINT not implemented yet");

                    break;
                case EnemyWave.SpawnMethod.ABSOLUTE_POINT:

                    // spawn at a point defined typed out in EnemyWavePart.location
                    int index = ewp.location.IndexOf(",");
                    if (index == -1) {
                        Debug.LogError("Spawn location point is invalid");
                    }
                    string xStr = ewp.location.Substring(0, index);
                    string yStr = ewp.location.Substring(index + 1);
                    spawnPos.x = float.Parse(xStr);
                    spawnPos.y = float.Parse(yStr);
                    break;

                }

                // spawnPos set.  Now spawn the enemy
                
                if (spawnWithPortal) {
                    // create SpawnInfo for the portal
                    SpawnInfo spawnInfo = new SpawnInfo();
                    spawnInfo.faceRight = (timeUser.randomValue() < .5f);
                    // spawn with a portal
                    GameObject pGO = GameObject.Instantiate(
                        portalGameObject,
                        new Vector3(spawnPos.x, spawnPos.y, 0),
                        Quaternion.identity) as GameObject;
                    portal = pGO.GetComponent<Portal>();
                    portal.gameObjectToSpawn = eGO;
                    portal.spawnInfo = spawnInfo;
                    portal.waveSpawnerRef = this;

                } else {
                    Debug.Log("Spawning without a portal is not supported yet.");
                }

                // increase amount of danger
                currentDanger += ei.danger;

                // increment variables, possibly get ready for next wave
                _waveEnemyCountIndex++;
                if (waveEnemyCountIndex >= ewp.count) {
                    // move on to next enemy
                    _waveEnemyCountIndex = 0;
                    _waveEnemyIndex++;
                }
                // if waveEnemyIndex >= currentWave.enemies.Count, then waveExhausted will be true and wait until enemies are dead before starting new wave

            } else {
                // currentDanger would go too high
                break;
            }
        }

        // gradually increase maxDanger
        if (!finished) {
            _currentMaxDanger += currentWave.dangerIncrease * Time.deltaTime;
        }

	}

    void newWave(int waveIndex) {
        _waveIndex = waveIndex;
        _waveEnemyIndex = 0;
        _waveEnemyCountIndex = 0;
        currentDanger = 0;
        if (currentWave != null) {
            _currentMaxDanger = currentWave.maxDanger;
        }
    }

    GameObject getGameObject(EnemyInfo.ID enemyID) {
        if (!enemyMapping.ContainsKey(enemyID)) {
            Debug.LogError("Error: enemyID " + enemyID + " not found in enemyMapping for this wave.");
            return null;
        }
        return enemyMapping[enemyID];
    }

    /* Searches through a list of segments and returns only those that aren't occupied.
     * HOWEVER: if all segments are occupied, then returns the same list again. */
    List<Segment> findUnoccupiedSegments(List<Segment> segments) {
        List<Segment> ret = new List<Segment>();
        foreach (Segment seg in segments) {
            bool occupied = false;
            foreach (EnemyInfo ei in EnemyInfo.enemyInfos) {
                if (isSegmentOccupiedByEnemy(seg, ei)) {
                    occupied = true;
                    break;
                }
            }
            foreach (Segment segAlready in segmentsSpawnedOnThisFrame) {
                if (seg == segAlready) {
                    occupied = true;
                    break;
                }
            }
            if (!occupied) {
                ret.Add(seg);
            }
        }
        if (ret.Count == 0) {
            return segments;
        }
        return ret;
    }

    /* This function isn't super reliable
     * */
    bool isSegmentOccupiedByEnemy(Segment segment, EnemyInfo enemy) {
        
        TimeUser eTU = enemy.GetComponent<TimeUser>();
        if (eTU != null) {
            if (!eTU.exists)
                return false;
        }
        /*
        VisionUser eVU = enemy.GetComponent<VisionUser>();
        if (eVU != null) {
            if (eVU.isVision)
                return false;
        }
        */

        Vector2 enemyPos = new Vector2();
        Rigidbody2D rb2d = enemy.GetComponent<Rigidbody2D>();
        if (rb2d == null) {
            enemyPos.x = enemy.transform.localPosition.x;
            enemyPos.y = enemy.transform.localPosition.y;
        } else {
            enemyPos = rb2d.position;
        }
        switch (enemy.spawnLocation) {
        case EnemyInfo.SpawnLocation.BOTTOM_SEGMENT:
            if (segment.wall != Segment.Wall.BOTTOM)
                return false;
            if (segment.p0.x <= enemyPos.x && enemyPos.x <= segment.p1.x &&
                segment.p0.y < enemyPos.y){
                return true;
            }
            break;
        case EnemyInfo.SpawnLocation.TOP_SEGMENT:
            if (segment.wall != Segment.Wall.TOP)
                return false;
            if (segment.p0.x <= enemyPos.x && enemyPos.x <= segment.p1.x &&
                segment.p0.y > enemyPos.y) {
                return true;
            }
            break;
        case EnemyInfo.SpawnLocation.VERT_SEGMENT:
            if (segment.wall == Segment.Wall.LEFT) {
                if (segment.p0.y <= enemyPos.y && enemyPos.y <= segment.p1.y &&
                    segment.p0.x < enemyPos.x) {
                    return true;
                }
            } else if (segment.wall == Segment.Wall.RIGHT) {
                if (segment.p0.y <= enemyPos.y && enemyPos.y <= segment.p1.y &&
                    segment.p0.x > enemyPos.x) {
                    return true;
                }
            }
            
            break;
        case EnemyInfo.SpawnLocation.AREA:
            return false;
        }
        return false;
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.ints["wi"] = waveIndex;
        fi.ints["wei"] = waveEnemyIndex;
        fi.ints["weci"] = waveEnemyCountIndex;
        fi.floats["cd"] = currentDanger;
        fi.floats["cmd"] = currentMaxDanger;
    }
    void OnRevert(FrameInfo fi) {
        _waveIndex = fi.ints["wi"];
        _waveEnemyIndex = fi.ints["wei"];
        _waveEnemyCountIndex = fi.ints["weci"];
        currentDanger = fi.floats["cd"];
        _currentMaxDanger = fi.floats["cmd"];
    }

    private Dictionary<EnemyInfo.ID, GameObject> enemyMapping = new Dictionary<EnemyInfo.ID, GameObject>();
    int _waveIndex = -1;
    int _waveEnemyIndex = 0;
    int _waveEnemyCountIndex = 0;
    float _currentMaxDanger = 0;
    List<Segment> segmentsSpawnedOnThisFrame = new List<Segment>();

    TimeUser timeUser;
	
}
