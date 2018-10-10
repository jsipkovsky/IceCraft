using UnityEngine;

// Game Cube Class

public class GameCube : MonoBehaviour {

    public Vector2 ParentSector; // GameSector where Cube belongs
    public bool PlayerMade;      // Created by Player (vs. generated terrain)
    public int Endurance;        // Determine how long takes to destroy it

}
