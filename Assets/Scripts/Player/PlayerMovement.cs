using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public float movementSpeed = 16f;
    public float jumpHeight = 4f;

    public Transform perspective;
    public float sensitivity = 2f;

    CharacterController playerController;
    Vector3 playerVelocity;

    float yaw = 0f;
    float pitch = 0f;

    // bool isJumping;

    // Start is called before the first frame update
    void Start() {
        playerController = GetComponent<CharacterController>();
        playerVelocity = Vector3.zero;
        // gravitationalPull = 0f;

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update() {
        // Debug.Log("Grounded?: " + playerController.isGrounded);
        MovePlayer();
        RotatePlayer();

        // Debug.Log(playerController.isGrounded);
        // playerVelocity.x = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
        // playerVelocity.y = playerController.isGrounded ? -0.01f : playerVelocity.y + (Physics.gravity.y * Time.deltaTime);
        // playerVelocity.z = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;

        // if (playerController.isGrounded && Input.GetButtonDown("Jump")) {
        //     Debug.Log("Jump");
        //     playerVelocity.y += Mathf.Sqrt(jumpHeight * Physics.gravity.y * -2f);
        // }

        // playerController.Move(playerVelocity);

        // Character faces in direction it is currently moving in
        // if (playerVelocity.x != 0 && playerVelocity.z != 0) transform.forward = new Vector3(playerVelocity.x, 0, playerVelocity.z);
    }

    private void MovePlayer() {
        // TODO: Make player slide down slopes with angles > playerController.slopeLimit
        Vector3 localXMovement = Input.GetAxis("Horizontal") * transform.right * movementSpeed;
        Vector3 localYMovement = CalculateVerticalMovement();
        Vector3 localZMovement = Input.GetAxis("Vertical") * transform.forward * movementSpeed;

        playerVelocity = localXMovement + localYMovement + localZMovement;

        playerController.Move(playerVelocity * Time.deltaTime);

        // gravitationalPull = playerController.isGrounded ? -playerController.stepOffset / Time.deltaTime : gravitationalPull + Physics.gravity.y * Time.deltaTime;

        // Vector3 input = (transform.forward * forwardInput + transform.right * sidewaysInput) * movementSpeed;
        // Vector3 gravity = transform.up * gravitationalPull;

        // playerController.Move((input + gravity) * Time.deltaTime);


    }

    private void RotatePlayer() {
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        
        float maxUpAngle = -90f;
        float maxDownAngle = 45f;
        pitch = Mathf.Clamp(pitch, maxUpAngle, maxDownAngle);

        transform.localEulerAngles = new Vector3(0f, yaw, 0f);
        perspective.localEulerAngles = new Vector3(pitch, 0f, 0f);

        // Vector3 perspectiveAngle = perspective.localEulerAngles;
        // if (perspectiveAngle.x > 180f) perspectiveAngle.x -= 360f;
        // perspectiveAngle.x = Mathf.Clamp(perspectiveAngle.x, -75f, 75f);
        // perspective.localRotation = Quaternion.Euler(perspectiveAngle);
    }

    private Vector3 CalculateVerticalMovement() {
        if (playerController.isGrounded) {
            if (Input.GetButtonDown("Jump")) {
                float jumpForce = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
                playerVelocity.y = jumpForce;
            }
            else {
                playerVelocity.y = -playerController.stepOffset / Time.deltaTime;
            }
        }
        else {
            playerVelocity.y += Physics.gravity.y * Time.deltaTime;
        }

        return transform.up * playerVelocity.y;
    }
}
