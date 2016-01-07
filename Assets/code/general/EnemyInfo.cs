﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyInfo : MonoBehaviour {

    public static List<EnemyInfo> enemyInfos = new List<EnemyInfo>();

    public enum ID {
        NONE,
        DUMMY,
        PENGRUNT,
        SEALIME,
        SEALIME_PASSIVE,
        MAGOOM,
        TOUCADE,
        TOUCADE_PASSIVE,
        TOUCADE_NO_FRUIT,
        SHERIVICE
    }

    public enum SpawnLocation {
        BOTTOM_SEGMENT,
        TOP_SEGMENT,
        VERT_SEGMENT,
        AREA
    }

    void Awake() {
        visionUser = GetComponent<VisionUser>();
    }
    void Start() {
        enemyInfos.Add(this);
    }
    void OnDestroy() {
        enemyInfos.Remove(this);
    }
    void OnRevertExist() {
        if (visionUser != null && visionUser.isVision)
            return;
        if (waveSpanwerRef != null) {
            waveSpanwerRef.currentDanger += danger;
        }
    }
    void OnTimeDestroy() {
        if (visionUser != null && visionUser.isVision)
            return;
        if (waveSpanwerRef != null) {
            waveSpanwerRef.currentDanger -= danger;
        }
    }

    public ID id = ID.NONE;
    public ID[] variations = { };
    public float danger = 5;
    public SpawnLocation spawnLocation = SpawnLocation.BOTTOM_SEGMENT;
    public float spawnDist = 0;
    public int score = 0; //score gotten for killing the enemy (only applicable in arcade mode)
    [HideInInspector]
    public WaveSpawner waveSpanwerRef = null;

    VisionUser visionUser = null; // not required
}
