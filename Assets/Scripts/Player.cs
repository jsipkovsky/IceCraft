using UnityEngine;

// Player Controller Class

public class Player : MonoBehaviour {

    //--------------------------------------

    // Game data
    private bool GameOn = false;                 // Start detecting Sector entries on true
    public GameController Controller;            // Reference on Game Controller
    public CharacterController PlayerController; // Reference on Controller Component

    // Movement attributes
    public Vector2 PlayerSector;                 // Sector where Player is currently located

    public float Speed = 6.0f;                   // Player movement speed
    public float JumpSpeed = 8.0f;               // Player jump speed
    public float Gravity = 20.0f;                // World Gravity

    private CollisionFlags flags;
    private Vector3 MoveDir = Vector3.zero;
    private bool IsGrounded = false;

    public bool HorizontalMove; // Determines main direction of Player movement

    //--------------------------------------

    private void Start()
    {
        PlayerController = GetComponent<CharacterController>();
    }

    // Method to check Player position in Game Wolrd 
    // Force update of Game World if any Outer Sector reached

    private void OnTriggerEnter(Collider other)
    {
        var sector = other.GetComponent<GameSector>();

        if (GameOn == false) // only get Sector Coords
        {
            PlayerSector = sector.SectorCoords;
            GameOn = true;
        }
        else
        {
            if (other.GetComponent<GameSector>().InnerSector == false) // Outer Sector entered
            {
                var direction = PlayerSector - sector.SectorCoords;
                PlayerSector = sector.SectorCoords;

                // check for diagonal movement (not needed for corners because of capsule shape)
                if (direction.x != 0 && direction.y != 0)
                {
                    if (HorizontalMove == true)
                    {
                        direction.y = 0; 
                    }
                    else
                    {
                        direction.x = 0;
                    }
                }

                if (direction.x != 0) // shift Sectors horizontal
                {
                    Controller.UpdateSectors(Mathf.RoundToInt(direction.x), 0);

                }
                else // shift Sectors vertical
                {
                    Controller.UpdateSectors(0, Mathf.RoundToInt(direction.y));
                }
            }
            else // Inner Sector entered
            {
                PlayerSector = sector.SectorCoords; // only update Player's Sector
            }
        }
    }

    // Method to handle Player movement

    void FixedUpdate()
    {
        if (IsGrounded) // process new movement Inputs only if Player is on the ground
        {
            MoveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            MoveDir = Quaternion.AngleAxis(transform.localEulerAngles.y, Vector3.up) * MoveDir;
            MoveDir *= Speed;

            if (Input.GetButton("Jump"))
            {
                MoveDir.y = JumpSpeed;
            }
        }

        // Player Controller updates
        MoveDir.y -= Gravity * Time.deltaTime;
        flags = PlayerController.Move(MoveDir * Time.deltaTime);
        IsGrounded = (flags & CollisionFlags.CollidedBelow) != 0;
    }
}
