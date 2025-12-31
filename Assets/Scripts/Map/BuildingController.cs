using UnityEngine;
public class BuildingController : MonoBehaviour
{
    private Camera currentCamera;

    private bool PlayerInCity { get; set; } = false;

    private void OnEnable()
    {
        CityEvents.OnPlayerInCity += HandleOnPlayerInCity;
    }

    private void OnDisable()
    {
        CityEvents.OnPlayerInCity -= HandleOnPlayerInCity;
    }

    private void HandleOnPlayerInCity(bool res)
        => PlayerInCity = res;
    public void SetCamera(Camera camera)
        => currentCamera = camera;

    private void Update()
    {
        if (currentCamera == null) return;
        if (PlayerInCity) return;

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