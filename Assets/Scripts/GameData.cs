using System;
using System.Collections.Generic;
using UnityEngine;

// Prepare serializable Vector 3 structure
// TEST

[System.Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;
    public SerializableVector3(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }
    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}]", x, y, z);
    }
    public static implicit operator Vector3(SerializableVector3 rValue)
    {
        return new Vector3(rValue.x, rValue.y, rValue.z);
    }

    public static implicit operator SerializableVector3(Vector3 rValue)
    {
        return new SerializableVector3(rValue.x, rValue.y, rValue.z);
    }
}

// Prepare serializable Vector 2 structure

[System.Serializable]
public struct SerializableVector2
{
    public float x;
    public float y;
    public SerializableVector2(float rX, float rY)
    {
        x = rX;
        y = rY;
    }
    public override string ToString()
    {
        return String.Format("[{0}, {1}]", x, y);
    }
    public static implicit operator Vector2(SerializableVector2 rValue)
    {
        return new Vector3(rValue.x, rValue.y);
    }

    public static implicit operator SerializableVector2(Vector2 rValue)
    {
        return new SerializableVector2(rValue.x, rValue.y);
    }
}

//--------------------------------------------
// Game Data Class (Save&Load processing)

[System.Serializable]
public class GameData
{
    public int GameID;                         // Game ID (determines terrain design)                            
    public int TotalCubes;                     // Total Player Cubes
    public int TotalTime;                      // Total Game Time
    public SerializableVector3 PlayerPosition; // Current Player position in time of save
    public SerializableVector2 CurrentZero;    // Current start point for Sector's creation (at StartGame)
    public List<PlayerCubes> Sectors;          // Data of all Player Cubes in the Game
}