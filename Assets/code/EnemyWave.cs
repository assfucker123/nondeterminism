using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyWave {

    public enum SpawnMethod {
        ANY, // spawns on any random appropriate Segment or Area
        EMPTY, // like ANY, but tries to pick a Segment or Area that's not occupied
        NAMED, // picks a random spot on a Segment or Area, named in EnemyWavePart.location
        NAMED_POINT, // picks a specific point, named in EnemyWavePart.location (not implemented yet)
        ABSOLUTE_POINT // picks a specific point, written out in EnemyWavePart.location
    }

    public enum FaceDirection {
        RANDOM,
        RIGHT,
        LEFT,
        UP,
        DOWN
    }

    [System.Serializable]
    public class EnemyWavePart {

        public EnemyInfo.ID enemy = EnemyInfo.ID.NONE;
        public SpawnMethod spawnMethod = SpawnMethod.ANY;
        public string location = "";
        public bool spawnPickups = true;
        public FaceDirection faceDirection = FaceDirection.RANDOM;
        public int count = 1;

    }

    public float maxDanger = 10; // max danger present at once
    public float dangerIncrease = 0; //how much maxDanger increases per second
    public List<EnemyWavePart> enemies = new List<EnemyWavePart>();
    
	
}
