using UnityEngine;

// Mouse Controller Class

public class MouseControl : MonoBehaviour
{
    //--------------------------------------

    // Mouse sensitivity
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    // Min/max angles
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;

    // Rotation 
    float rotationX = 0F;
    float rotationY = 0F;

    Quaternion originalRotation;

    //--------------------------------------

    // Mouse Control - initialization

    void Start()
    {
        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().freezeRotation = true;
        }

        originalRotation = transform.localRotation;

        // Lock Cursor on Start Game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Mouse Control - update

    void Update()
    {

        rotationX += Input.GetAxis("Mouse X") * sensitivityX;
        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

        rotationX = ClampAngle(rotationX, minimumX, maximumX);
        rotationY = ClampAngle(rotationY, minimumY, maximumY);

        Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);

        transform.localRotation = originalRotation * xQuaternion * yQuaternion;
    }

    // Clamp rotation angle

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
        {
            angle += 360F;
        }
        if (angle > 360F)
        {
            angle -= 360F;
        }
        return Mathf.Clamp(angle, min, max);
    }
}
