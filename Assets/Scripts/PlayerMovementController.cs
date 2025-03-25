using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    public float movementSpeed = 5.0f;

    public float rotationSpeed = 120.0f;

    private Rigidbody body;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        // Move player based on vertical input.
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = transform.forward * moveVertical * movementSpeed * Time.fixedDeltaTime;
        body.MovePosition(body.position + movement);

        // Rotate player based on horizontal input.
        float turn = Input.GetAxis("Horizontal") * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        body.MoveRotation(body.rotation * turnRotation);
    }
}