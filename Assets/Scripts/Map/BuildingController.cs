using UnityEngine;

public class BuildingController : MonoBehaviour
{
    private Camera currentCamera;

    private void OnEnable()
    {
        OverworldMapController.OnCameraChange += HandleCameraChanged;
    }

    private void OnDisable()
    {
        OverworldMapController.OnCameraChange -= HandleCameraChanged;
    }

    private void HandleCameraChanged(Camera cam)
    {
        currentCamera = cam;
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