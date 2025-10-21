using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CameraScroll : MonoBehaviour
{
    public GameObject CameraToMove;
    public float ScrollSpeed = 1f;
    public float Acceleration = 0.1f;
    public float MaxScrollSpeed = 5f;
    private float currentScrollSpeed;

    public GameObject Player1;
    public GameObject Player2;

    public GameObject DeathScreen;

    // How far below the camera before death triggers
    public float FallBuffer = 2f;

    private Camera cam;
    private bool someoneDied = false;

    void Start()
    {
        if (CameraToMove == null)
        {
            Debug.LogError("CameraToMove is not assigned! Disabling script.");
            enabled = false;
            return;
        }

        cam = CameraToMove.GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraToMove must have a Camera component!");
            enabled = false;
            return;
        }

        currentScrollSpeed = ScrollSpeed;
    }

    void Update()
    {
        // Accelerate the scroll speed
        currentScrollSpeed += Acceleration * Time.deltaTime;
        currentScrollSpeed = Mathf.Clamp(currentScrollSpeed, ScrollSpeed, MaxScrollSpeed);

        // Move the camera upward
        float newYPosition = CameraToMove.transform.position.y + currentScrollSpeed * Time.deltaTime;
        CameraToMove.transform.position = new Vector3(CameraToMove.transform.position.x, newYPosition, CameraToMove.transform.position.z);

        if (Player1 != null)
        {
            if (Player1.transform.Find("Body").position.y > CameraToMove.transform.position.y - 1f)
                CameraToMove.transform.position = new Vector3(CameraToMove.transform.position.x, Player1.transform.Find("Body").position.y + 1f, CameraToMove.transform.position.z);

        }

        if (Player2 != null)
        {
            if (Player2.transform.Find("Body").position.y > CameraToMove.transform.position.y - 1f)
                CameraToMove.transform.position = new Vector3(CameraToMove.transform.position.x, Player2.transform.Find("Body").position.y + 1f, CameraToMove.transform.position.z);
        }
        
        // Check for death (falling off screen)
        CheckPlayerBounds(Player1);
        CheckPlayerBounds(Player2);
    }

    void CheckPlayerBounds(GameObject player)
    {
        if (player == null || cam == null) return;

        Transform body = player.transform.Find("Body");
        if (body == null) return;

        // Find the bottom of the camera’s view in world space
        float camBottom = cam.transform.position.y - cam.orthographicSize;

        // If the player's body is below the bottom edge + buffer, "kill" them
        if (body.position.y < camBottom - FallBuffer)
        {
            Debug.Log(player.name + " fell off screen!");


            // Example death behavior:

            if (DeathScreen != null && !someoneDied)
            {
                DeathScreen.SetActive(true);
                var textTransform = DeathScreen.transform.Find("text");
                if (textTransform != null)
                {
                    var tmp = textTransform.GetComponent<TMP_Text>();
                    if (tmp != null)
                        tmp.text = player.name + " has died!";
                }

                void fart()
                {
                    SceneManager.LoadScene("Main", LoadSceneMode.Single);
                }

                DeathScreen.transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { fart(); });
            }

            Destroy(player);

            someoneDied = true;
            
            // or player.GetComponent<PlayerHealth>().Die();
        }
    }
}
