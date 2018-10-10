using System.Collections.Generic;
using UnityEngine;

// Game Sector Class

public class GameSector : MonoBehaviour {

    public Vector2 SectorCoords; // Sector coordinates in Game World
    public bool InnerSector;     // Inner Sector indicator (always 4 Inner vs. 12 Outer)
    public List<GameCube> Cubes; // All Game Cubes in the Sector
}
