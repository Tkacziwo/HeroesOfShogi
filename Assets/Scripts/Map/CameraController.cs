using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static event Action<Transform> OnCameraZoom;

    public event Action<int> OnCameraDrag;

    public event Action<int> OnCameraMove;

    public Vector3 cameraOffset;

    private Camera cam;

    private Vector3 rotationPivot;

    private float cameraDistance = 6;

    private readonly float maxCameraDistance = 10;

    private readonly float minCameraDistance = 2;

    private readonly float maxCameraXAngle = 75;
    private readonly float minCameraXAngle = 55;

    [SerializeField] private float mouseSensitivity = 10f;

    public bool isCameraFocusedOnPlayer;

    public bool battleStarted = false;

    private void OnEnable()
    {
        BattleDeploymentController.OnBattleStarted += HandleBattleStarted;
        GameOverController.OnBackToMap += HandleBattleEnded;
        PanelController.OnFocusOnPlayer += FocusCameraOnCharacter;
    }

    private void OnDisable()
    {
        BattleDeploymentController.OnBattleStarted -= HandleBattleStarted;
        GameOverController.OnBackToMap -= HandleBattleEnded;
        PanelController.OnFocusOnPlayer -= FocusCameraOnCharacter;
    }

    public void SetCameraPosition(Transform transform)
    {
        rotationPivot = transform.position;
        UpdateCameraPosition();
    }

    public void UpdateCameraPosition()
    {
        var offset = new Vector3(0, 0, -cameraDistance);
        offset = cam.transform.rotation * offset;
        cam.transform.position = rotationPivot + offset + new Vector3(0, cameraOffset.y, 0);
    }

    public void InitCamera()
    {
        cam = this.GetComponentInChildren<Camera>();
        cam.transform.SetPositionAndRotation(new(0, 10, 0), Quaternion.Euler(60, 0, 0));
    }

    public void FocusCameraOnCharacter(Transform transform)
    {
        isCameraFocusedOnPlayer = true;
        SetCameraPosition(transform);
    }

    private void Update()
    {
        if (battleStarted) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Input.GetMouseButton(1))
        {
            float angleX = -mouseX * mouseSensitivity;
            float angleY = mouseY * mouseSensitivity;

            cam.transform.Rotate(angleY, angleX, 0f);
            var euler = cam.transform.eulerAngles;
            euler.z = 0;
            euler.x = Mathf.Clamp(euler.x, minCameraXAngle, maxCameraXAngle);
            cam.transform.rotation = Quaternion.Euler(euler);

            if (isCameraFocusedOnPlayer)
            {
                UpdateCameraPosition();
            }
        }

        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0.01f || scroll < -0.01f)
        {
            var newCameraDistance = cameraDistance - scroll * mouseSensitivity;

            if (newCameraDistance <= maxCameraDistance && newCameraDistance >= minCameraDistance)
            {
                cameraDistance = newCameraDistance;
                cameraOffset.y -= scroll * mouseSensitivity;
                UpdateCameraPosition();
            }
        }

        var rotationForMoving = cam.transform.rotation.eulerAngles.y;

        var radians = Mathf.Deg2Rad * rotationForMoving;

        var offsetX = Mathf.Sin(radians) * 0.2f;
        var offsetZ = Mathf.Cos(radians) * 0.2f;

        if (Input.GetKey(KeyCode.W))
        {
            isCameraFocusedOnPlayer = false;
            cam.transform.position = new Vector3(cam.transform.position.x + offsetX, cam.transform.position.y, cam.transform.position.z + offsetZ);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            isCameraFocusedOnPlayer = false;
            cam.transform.position = new Vector3(cam.transform.position.x - offsetX, cam.transform.position.y, cam.transform.position.z - offsetZ);
        }
        if (Input.GetKey(KeyCode.A))
        {
            isCameraFocusedOnPlayer = false;
            cam.transform.position = new Vector3(cam.transform.position.x - offsetZ, cam.transform.position.y, cam.transform.position.z + offsetX);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            isCameraFocusedOnPlayer = false;
            cam.transform.position = new Vector3(cam.transform.position.x + offsetZ, cam.transform.position.y, cam.transform.position.z - offsetX);
        }
    }

    private void HandleBattleStarted(bool r)
        => battleStarted = true;

    private void HandleBattleEnded()
        => battleStarted = false;
}