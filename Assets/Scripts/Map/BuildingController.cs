using System.Linq;
using UnityEngine;

public class BuildingController : MonoBehaviour
{
    private Camera currentCamera;

    private void OnEnable()
    {
        PlayerController.CameraChanged += HandleCameraChanged;
    }

    private void OnDisable()
    {
        PlayerController.CameraChanged -= HandleCameraChanged;

    }

    private void HandleCameraChanged(Camera changedCamera)
    {
        currentCamera = changedCamera;
    }

    private void Start()
    {
        currentCamera = Camera.main;
        
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(currentCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                var script = hit.transform.GetComponent<InteractibleBuilding>();
                script.FindPathToBuilding();
            }
        }
    }
}