using System.Linq;
using UnityEngine;

public class BuildingController : MonoBehaviour
{
    private Camera currentCamera;

    private bool PlayerInCity { get; set; } = false;

    private void OnEnable()
    {
        PlayerController.CameraChanged += HandleCameraChanged;
        CityEvents.OnPlayerInCity += HandleOnPlayerInCity;
    }

    private void OnDisable()
    {
        PlayerController.CameraChanged -= HandleCameraChanged;
        CityEvents.OnPlayerInCity -= HandleOnPlayerInCity;
    }

    private void HandleOnPlayerInCity(bool res)
        => PlayerInCity = res;

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
        if (PlayerInCity) { return; }

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(currentCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                if (hit.transform.TryGetComponent<InteractibleBuilding>(out InteractibleBuilding script))
                {
                    script.FindPathToBuilding();
                }
            }
        }
    }
}