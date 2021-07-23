using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public float movementSpeed = 16f;
    public float jumpHeight = 4f;
    public float slideSpeed = 25f;

    public Transform perspective;
    public float sensitivity = 2f;

    CharacterController playerController;
    Vector3 playerVelocity;
    
    Vector3 groundNormal;

    float yaw = 0f;
    float pitch = 0f;

    void Start() {
        playerController = GetComponent<CharacterController>();
        playerVelocity = Vector3.zero;
        
        groundNormal = Vector3.up;
        // gravitationalPull = 0f;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        // Debug.Log("Grounded?: " + playerController.isGrounded);
        MovePlayer();
        RotatePlayer();
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        if (hit.moveDirection == Vector3.down && groundNormal != hit.normal) {
            // Debug.Log("collidercontrollerhit!");
            groundNormal = hit.normal;
        }
    }

    private void MovePlayer() {
        Vector3 localXMovement = Input.GetAxis("Horizontal") * transform.right * movementSpeed;
        Vector3 localYMovement = CalculateVerticalMovement();
        Vector3 localZMovement = Input.GetAxis("Vertical") * transform.forward * movementSpeed;

        playerVelocity = localXMovement + localYMovement + localZMovement;

        // FIXME: fix jittery sliding when colliding with multiple planes of differing normals
        float normalAngle = Vector3.Angle(transform.up, groundNormal);
        if (playerController.isGrounded && normalAngle > playerController.slopeLimit) {
            playerVelocity.x += (1f - groundNormal.y) * groundNormal.x * slideSpeed;
            playerVelocity.z += (1f - groundNormal.y) * groundNormal.z * slideSpeed;
        }

        playerController.Move(playerVelocity * Time.deltaTime);
    }

    private void RotatePlayer() {
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        
        float maxUpAngle = -90f;
        float maxDownAngle = 45f;
        pitch = Mathf.Clamp(pitch, maxUpAngle, maxDownAngle);

        transform.localEulerAngles = new Vector3(0f, yaw, 0f);
        perspective.localEulerAngles = new Vector3(pitch, 0f, 0f);
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
