using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//--------------------------------------
// Define Cube structures

// One each Cube created by Player 
[System.Serializable]
public struct PlayerCubesData 
{
    public SerializableVector3 CubeCoords;
    public int Endurance;
}

// Cubes in each Sector created by Player

[System.Serializable]
public struct PlayerCubes 
{
    public SerializableVector2 sectorCoord;
    public List<PlayerCubesData> CubeData;
}

// GAME CONTROLLER CLASS

public class GameController : MonoBehaviour
{
    //--------------------------------------

    private int tst = 1;
    // General Setting & Objects
    public int GameID;                      // New Game = random number (0-10000), Load Game = stored Game ID
    public int SectorSize = 16;             // Sector size (should stay 16)

    public Player Player;                   // Player (= Main Camera)

    public GameObject SectorPrefab;         // Prefab Game Object for each Sector
    public List<GameObject> CubePrefabs;    // Prefab Game Objects for Game Cubes
    public List<GameObject> PlayerCubePfbs; // Prefab Game Objects for Player Cubes
    public Transform ObjectsParent;         // Parent of all Sectors and Game Cubes

    // UI
    public Canvas GameUI;                   // Canvas with all Game UI   
    public Text SectorCoords;               // Current Player Sector coordinates 
    public Text Direction;                  // Current Player direction
    public Text ActionText;                 // 'Build'/'Destroy'
    public Text GameTimeTxt;                // Total Game time for current Game
    public Text SumPlCubes;                 // Total Player Cubes build in current Game

    // Game State
    public bool BuildMode;                  // Build/Destroy modes
    public bool ActionAllowed;              // Switch on when ray hits a Cube
    public bool MenuActive;                 // F1-4 keys activated (Save, New, Load, Quit)

    // Sectors & Cubes
    public List<GameSector> GameSectors;    // All current Game Sectors
    private List<GameSector> SectorsTmp;    // Temporary list for Sector's operations
    private GameSector SectorTmp;           // Temporary Game Sector
    private GameCube MarkedCube;            // Cube marked to be destroyed

    public List<PlayerCubes> PlayerMade;    // List of all Player Cube data
    private PlayerCubes PlayerCube;         // Player Cube data for current Sector

    // Game Attributes
    private Vector3 NewCube;                // Coordinates of potential new Player Cube
    private GameSector NewCubeSector;       // Parent Sector of potential new Player Cube

    private float GameTime;                 // Counts total Game time for current Game
    private int SumPlayerCubes;             // Counts total Player Cubes build in current Game

    private int CubeIndex;                  // Index for selecting Player Cube Prefab
    private int GCIndex;                    // Index for selecting Game Cube Prefab
    private float TimeToDestroy;            // Time to destroy Cube based on Endurance
    private float DestroyCounter;           // When this valeu reach 'TimeToDestroy', remove marked Player Cube 

    readonly int layerMask = 1 << 1;        // Only detect collisions with Cubes when raycasting

    private Color FrameColor;               // Preview frame Color

    //--------------------------------------

    // Game Initialization  

    void Start()
    {
        StartGame(StaticTools.LoadedGame);
    }

    // Generate Game (New/Load modes)

    private void StartGame(bool IsLoadedGame)
    {
        // Initialize Sectors
        GameSectors = new List<GameSector>();
        SectorsTmp = new List<GameSector>();
        PlayerMade = new List<PlayerCubes>();

        FrameColor = new Color32(116, 231, 86, 255); // set default preview Frame Color

        // Start point for Sector's creation (New Game only)
        int xStart = -2;
        int zStart = -2; //(y = z for us)

        if (IsLoadedGame == true) // in case of loaded Game, get Game data
        {
            PlayerMade = StaticTools.GameData.Sectors;
            Player.transform.position = StaticTools.GameData.PlayerPosition;
            GameID = StaticTools.GameData.GameID;
            GameTime = StaticTools.GameData.TotalTime;
            SumPlayerCubes = StaticTools.GameData.TotalCubes;
            xStart = (int)StaticTools.GameData.CurrentZero.x;
            zStart = (int)StaticTools.GameData.CurrentZero.y; //(y = z for us)
            StaticTools.LoadedGame = false; // remove 'Loaded' indicator

        } else
        {
            GameID = Random.Range(0, 10000); // Generate Game ID
        }

        // Build initial Start Secors
        for (int x = xStart; x <= (xStart + 3); x++)
        {
            for (int z = zStart; z <= (zStart + 3); z++)
            {
                bool IsInner = false;

                // Mark 4 Sectors in the middle as Inner Sectors
                if (x == xStart + 1 && z == zStart + 1 || x == xStart + 1 && z == zStart + 2 ||
                    x == xStart + 2 && z == zStart + 1 || x == xStart + 2 && z == zStart + 2)
                    IsInner = true;
               
                CreateSector(new Vector2(x, z), IsInner); // Create Sector
            }
        }
    }

    //--------------------------------------
    // Controller Update method

    private void Update()
    {
        // Permanently active 'Keys'

        if (Input.GetKeyDown("q")) // Build/Destroy mode switch
        {
            if (BuildMode == false) // change mode and update UI
            {
                GameUI.transform.Find("BuildPanel").gameObject.SetActive(true);
                ActionText.text = "Destroy [Q]";
                BuildMode = true;
            }
            else // change mode and update UI
            {
                GameUI.transform.Find("BuildPanel").gameObject.SetActive(false);
                ActionText.text = "Build [Q]";
                BuildMode = false;
            }
        }

        if (Input.GetKeyDown("1")) 
        {
            if (MenuActive == false) // change mode and update UI
            {
                GameUI.transform.Find("GameOptions").gameObject.SetActive(true);
                MenuActive = true;
            }
            else // change mode and update UI
            {
                GameUI.transform.Find("GameOptions").gameObject.SetActive(false);
                MenuActive = false;
            }
        }

        // Activated 'Keys'

        if (Input.GetKeyDown("f1") && MenuActive == true) // Save Game
            SaveGame();

        if (Input.GetKeyDown("f2") && MenuActive == true) // Start New Game
            SceneManager.LoadScene("GameScene");

        if (Input.GetKeyDown("f3") && MenuActive == true) // Load Saved Game
            LoadGame();

        if (Input.GetKeyDown("f4") && MenuActive == true) // Quit Game
            Application.Quit();

        if (BuildMode == true) // check for switch Player Cube prefab
        {
            if (Input.GetKeyDown("e")) // Player Cube lvl1
            {
                CubeIndex = 0;
                FrameColor = new Color32(116, 231, 86, 255);
            }
            if (Input.GetKeyDown("r")) // Player Cube lvl2
            {
                CubeIndex = 1;
                FrameColor = new Color32(201, 229, 107, 255);
            }
            if (Input.GetKeyDown("f")) // Player Cube lvl3
            {
                CubeIndex = 2;
                FrameColor = new Color32(231, 235, 77, 255);
            }
            if (Input.GetKeyDown("g")) // Player Cube lvl4
            {
                CubeIndex = 3;
                FrameColor = new Color32(231, 178, 71, 255);
            }
        }

        // Handle mouse actions

        if (Input.GetMouseButtonDown(0)) // Create Player Cube
        {
            if (BuildMode == true)
            {
                if (ActionAllowed == true)
                {
                    CreatePlayerCube(NewCubeSector);
                }
            }
        }

        // Count mouse down time and destroy Player Cube when endurance reached
        if (Input.GetMouseButton(0) && BuildMode == false && MarkedCube == true)
        {
            DestroyCounter += Time.deltaTime; 
            var time = (int)DestroyCounter;
            if (time == MarkedCube.Endurance)
            {
                DestroyCube(MarkedCube);
                MarkedCube = null;
                DestroyCounter = 0;
            }
        } else {
            DestroyCounter = 0;
        }

        if (Input.GetMouseButtonUp(0))
            DestroyCounter = 0;

        // Check hit of some Cube & update Game
        GameStateUpdates();
    }

    // Draw outline of potential new Player Cube

    private void OnRenderObject()
    {
        if (ActionAllowed == true && BuildMode == true)
            StaticTools.DrawSquare(NewCube, 1, FrameColor);
    }

    // Method for update Game World when any Outer Sector is reached

    public void UpdateSectors(int xDirection, int zDirection)
    {
        var xCoord = Player.PlayerSector.x; // get current Player coords (y = z for us)
        var zCoord = Player.PlayerSector.y; 

        if (xDirection != 0) // change on x-axes -> shift Sectors horizontal
        {
            SectorsTmp = GameSectors.FindAll(n => n.SectorCoords.x == xCoord + (3 * xDirection));

            for (int i = 0; i < SectorsTmp.Count; i++) // destoy 'left' Sectors & build new in Pl. direction
            {
                GameSectors.Remove(SectorsTmp[i]); 
                Destroy(SectorsTmp[i].gameObject); 
                var newCoord = new Vector2((xCoord + (xDirection * -1)), (SectorsTmp[i].SectorCoords.y));
                CreateSector(newCoord, false);
            }

            SectorsTmp = GameSectors.FindAll(n => n.SectorCoords.x == xCoord + (2 * xDirection) && n.InnerSector == true);
            for (int i = 0; i < SectorsTmp.Count; i++) // update Inner/Outer Sector statuses
            {
                SectorTmp = GameSectors.FindLast(n => n.SectorCoords.x == xCoord &&
                            n.SectorCoords.y == SectorsTmp[i].SectorCoords.y);
                SectorTmp.InnerSector = true;
                SectorsTmp[i].InnerSector = false;
            }

        }
        else // change on z-ares -> shift Sectors vertical
        {
            SectorsTmp = GameSectors.FindAll(n => n.SectorCoords.y == zCoord + (3 * zDirection));

            for (int i = 0; i < SectorsTmp.Count; i++) // destoy 'left' Sectors & build new in Pl. direction
            {
                GameSectors.Remove(SectorsTmp[i]);
                Destroy(SectorsTmp[i].gameObject);
                var newCoord = new Vector2(SectorsTmp[i].SectorCoords.x, zCoord + (zDirection * -1));
                CreateSector(newCoord, false);
            }

            SectorsTmp = GameSectors.FindAll(n => n.SectorCoords.y == zCoord + (2 * zDirection) && n.InnerSector == true);
            for (int i = 0; i < SectorsTmp.Count; i++) // update Inner/Outer Sector statuses
            {
                SectorTmp = GameSectors.FindLast(n => n.SectorCoords.y == zCoord &&
                            n.SectorCoords.x == SectorsTmp[i].SectorCoords.x);
                SectorTmp.InnerSector = true;
                SectorsTmp[i].InnerSector = false;
            }
        }
    }

    //---------------------------
    // Method for Sector Creation

    private void CreateSector(Vector2 sectorCoords, bool IsInner)
    {
        var xCoord = (int)sectorCoords.x * SectorSize; // get x-start point coords
        var zCoord = (int)sectorCoords.y * SectorSize; // get x-start point coords (y = z for us)

        GameObject newSectorGO = Instantiate(SectorPrefab, new Vector3(xCoord, 0, zCoord), Quaternion.identity) as GameObject;
        GameSector newSector = newSectorGO.GetComponent<GameSector>();
        newSector.SectorCoords.x = sectorCoords.x;
        newSector.SectorCoords.y = sectorCoords.y;

        if (IsInner == true) // mark Sector as Inner Sector
        {
            newSector.InnerSector = true;
        }

        for (int x = xCoord; x < SectorSize + xCoord; x++) // Generate Game Cubes
        {
            for (int z = zCoord; z < SectorSize + zCoord; z++)
            {
                var position = new Vector2(x, z);
                CreateGameCube(newSector, position); // Create Game Cube
            }
        }

        // Create Sector Player Cubes
        PlayerCube = PlayerMade.Find(n => n.sectorCoord == sectorCoords);
        if (PlayerCube.CubeData != null)
        {
            for (int i = 0; i < PlayerCube.CubeData.Count; i++)
            {
                RecreatePlayerCube(newSector, PlayerCube.CubeData[i]); // build prev. created Player Cube
            }
        }

        // Add Sector to lists
        GameSectors.Add(newSector);
        newSector.transform.parent = ObjectsParent;
    }

    //------------------------------
    // Method for Game Cube Creation
    // (Dynamic batching swithed on for reducing number of batches)

    private void CreateGameCube(GameSector sector, Vector2 position)
    {
        int yCoord = NoiseEffect(position.x, position.y); // create 'terrain' effect

        GCIndex = 0; // set Cube color according to y-position
        if (yCoord > 2)
            GCIndex = 1;
        if (yCoord > 5)
            GCIndex = 2;

        GameObject newCubeGO = Instantiate(CubePrefabs[GCIndex], new Vector3(position.x, yCoord, position.y), Quaternion.identity)
            as GameObject;
        GameCube newCube = newCubeGO.GetComponent<GameCube>();
        newCubeGO.transform.parent = sector.transform;
        newCube.ParentSector = sector.SectorCoords;
        sector.Cubes.Add(newCube);
    }

    //-------------------------------------
    // Method for Player made Cube Creation

    private void CreatePlayerCube(GameSector sector)
    {
        // Create Cube from relevant Prefab
        GameObject newCubeGO = Instantiate(PlayerCubePfbs[CubeIndex], new Vector3(NewCube.x, NewCube.y, NewCube.z),
            Quaternion.identity) as GameObject;
        GameCube newCube = newCubeGO.GetComponent<GameCube>();
        newCubeGO.transform.parent = sector.transform;
        newCube.ParentSector = sector.SectorCoords;
        sector.Cubes.Add(newCube); // probably not needed
        SumPlayerCubes += 1;       // increase Total

        // Process Player Made list
        PlayerCube = PlayerMade.FindLast(n => n.sectorCoord == sector.SectorCoords);
        if (PlayerCube.CubeData == null) // no Player Cube data for Sector yet
        {
            PlayerCube.sectorCoord = sector.SectorCoords;
            PlayerCube.CubeData = new List<PlayerCubesData>();
            PlayerCubesData cubedata = new PlayerCubesData
            {
                CubeCoords = NewCube,
                Endurance = newCube.Endurance
            };
            PlayerCube.CubeData.Add(cubedata);
            PlayerMade.Add(PlayerCube);
        }
        else // only add new Player Cube to list
        {
            PlayerCubesData cubedata = new PlayerCubesData
            {
                CubeCoords = NewCube,
                Endurance = newCube.Endurance
            };
            PlayerCube.CubeData.Add(cubedata);
        }
    }

    //-------------------------------
    // Method for rebuild Player Cube

    private void RecreatePlayerCube(GameSector sector, PlayerCubesData cubeData)
    {
        // Create Player Cube according to loaded data about this
        var cubePos = cubeData.CubeCoords;
        GameObject newCubeGO = Instantiate(PlayerCubePfbs[cubeData.Endurance - 1], 
            new Vector3(cubePos.x, cubePos.y, cubePos.z), Quaternion.identity) as GameObject;
        GameCube newCube = newCubeGO.GetComponent<GameCube>();
        newCubeGO.transform.parent = sector.transform;
        newCube.ParentSector = sector.SectorCoords;
        sector.Cubes.Add(newCube);
    }

    // Method for destroy marked Player Cube

    private void DestroyCube(GameCube toRemove)
    {
        // Remove from list
        PlayerCube = PlayerMade.FindLast(n => n.sectorCoord == toRemove.ParentSector);
        var cubeData = PlayerCube.CubeData.Find(n => n.CubeCoords == toRemove.transform.position);
        PlayerCube.CubeData.Remove(cubeData);
        SumPlayerCubes -= 1; // decrease Total

        Destroy(toRemove.gameObject); // destroy Cube Game Object
    }

    // Perlin noise effect

    int NoiseEffect(float x, float y)
    {
        float xred = (float)x / SectorSize + GameID;
        float yred = (float)y / SectorSize + GameID;

        float pNoise = Mathf.PerlinNoise(xred, yred);

        var yCoord = Mathf.RoundToInt((pNoise * pNoise) * 10);
        return yCoord;
    }

    // Gets the mouse position is world space (& check for Cube hits)

    private void GameStateUpdates()
    {
        MarkedCube = null; // reset marked Cube
        UpdateUI();        // update Player UI information

        // Shoot ray into middle of screen (marked by sight)
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, layerMask))
        {
            var cube = hit.transform.GetComponent<GameCube>(); // get Cube
            var cubeNormal = hit.normal.ToString();            // get side where Cube was hit

            switch (cubeNormal) // determine position for potential new Cube
            {
                case "(1.0, 0.0, 0.0)":
                    NewCube = hit.transform.position + new Vector3(1, 0, 0);
                    break;
                case "(-1.0, 0.0, 0.0)":
                    NewCube = hit.transform.position + new Vector3(-1, 0, 0);
                    break;
                case "(0.0, 1.0, 0.0)":
                    NewCube = hit.transform.position + new Vector3(0, 1, 0);
                    break;
                case "(0.0, 0.0, 1.0)":
                    NewCube = hit.transform.position + new Vector3(0, 0, 1);
                    break;
                case "(0.0, 0.0, -1.0)":
                    NewCube = hit.transform.position + new Vector3(0, 0, -1);
                    break;
                default:
                    // no action needed
                    break;
            }

            NewCubeSector = GameSectors.FindLast(n => n.SectorCoords == cube.ParentSector);
            ActionAllowed = true; // allow Build/Destroy actions

            if (cube.PlayerMade == true && BuildMode == false) // update time needed to destroy Cube
                MarkedCube = cube;
        }
        else
        {
            ActionAllowed = false; // no hit = disable Build/Destroy actions
        }
    }

    private void UpdateUI() // Update Game data diplayed on Screen Canvas
    {
        // Update Total time and Player Cubes
        GameTime += Time.deltaTime;
        GameTimeTxt.text = "Total Game Time: " + (int)GameTime;
        SumPlCubes.text = "Total Player Cubes: " + SumPlayerCubes.ToString(); 

        // Update position data
        SectorCoords.text = "Sector: [" + Player.PlayerSector.x.ToString() + "," + Player.PlayerSector.y.ToString() + "]";
        var angle = Player.transform.eulerAngles.y;
        if (angle > 315 || angle <= 45)
        {
            Direction.text = "Head: North";
            Player.HorizontalMove = false; 
        }
        else if (angle > 45 && angle <= 135)
        {
            Direction.text = "Head: East";
            Player.HorizontalMove = true;
        }
        else if (angle > 135 && angle <= 225)
        {
            Direction.text = "Head: South";
            Player.HorizontalMove = false;
        }
        else if (angle > 225 && angle < 315) 
        { 
            Direction.text = "Head: West";
            Player.HorizontalMove = true;
        }
    }

    // Method for Save current Game state into file

    public void SaveGame()
    {
        string destination = Application.persistentDataPath + "/save.dat";
        FileStream file;

        if (File.Exists(destination)) file = File.OpenWrite(destination);
        else file = File.Create(destination);

        GameData data = PrepareData(); // gather current Game data
 
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        file.Close();
    }

    // Method for preparing current Game state to be saved

    private GameData PrepareData()
    {
        // Get start point for furher loaded Sectors creation
        var zeroSector = GameSectors.OrderBy(n => n.SectorCoords.x).ThenBy(n => n.SectorCoords.y).First();
        GameData currentData = new GameData // Create Game data
        {
            GameID = GameID,
            TotalTime = (int)GameTime,
            TotalCubes = SumPlayerCubes,
            CurrentZero = zeroSector.SectorCoords,
            PlayerPosition = Player.transform.position + new Vector3(0, 0.2f, 0),
            Sectors = PlayerMade
        };
        return currentData;
    }

    // Method for loading previously saved Game

    public void LoadGame()
    {
        // Try to load saved Game data
        string destination = Application.persistentDataPath + "/save.dat";
        FileStream file;

        if (File.Exists(destination)) file = File.OpenRead(destination);
        else
        { 
            return; // message
        }

        BinaryFormatter bf = new BinaryFormatter();
        StaticTools.GameData = (GameData)bf.Deserialize(file);
        file.Close();

        // Start loaded Game
        StaticTools.LoadedGame = true;
        SceneManager.LoadScene("GameScene");
    }
}
