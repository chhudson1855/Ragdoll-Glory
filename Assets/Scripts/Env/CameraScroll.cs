using UnityEngine;

public class CameraScroll : MonoBehaviour
{

    public GameObject CameraToMove; // Assign the camera you want to move in the Inspector
    public float ScrollSpeed = 1f; // Base scroll speed
    public float Acceleration = 0.1f; // Acceleration amount per second (smaller is usually better)
    public float MaxScrollSpeed = 5f; // Optional:  Limit the maximum scroll speed
    private float currentScrollSpeed; // The actual speed that changes with acceleration

    public GameObject Player1; // Reference to the player object
    public GameObject Player2;

    // Start is called before the first frame update
    void Start()
    {
        // Check if the camera is assigned.  Important for preventing errors.
        if (CameraToMove == null)
        {
            Debug.LogError("CameraToMove is not assigned in the Inspector! Disabling script.");
            enabled = false; // Disable the script to prevent errors
            return;
        }

        currentScrollSpeed = ScrollSpeed; // Initialize current speed to the base speed
    }

    // Update is called once per frame
    void Update()
    {
        currentScrollSpeed += Acceleration * Time.deltaTime;

        currentScrollSpeed = Mathf.Clamp(currentScrollSpeed, ScrollSpeed, MaxScrollSpeed); //Ensure speed is always at least ScrollSpeed

        float newYPosition = CameraToMove.transform.position.y + currentScrollSpeed * Time.deltaTime;
        CameraToMove.transform.position = new Vector3(CameraToMove.transform.position.x, newYPosition, CameraToMove.transform.position.z);

        if (Player1.transform.Find("Body").position.y > CameraToMove.transform.position.y - 1f)
        {
            CameraToMove.transform.position = new Vector3(CameraToMove.transform.position.x, Player1.transform.Find("Body").position.y + 1f, CameraToMove.transform.position.z);
        }
        if (Player2.transform.Find("Body").position.y > CameraToMove.transform.position.y - 1f)
        {
            CameraToMove.transform.position = new Vector3(CameraToMove.transform.position.x, Player2.transform.Find("Body").position.y + 1f, CameraToMove.transform.position.z);
        }
    }
}