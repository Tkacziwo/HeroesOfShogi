using UnityEngine;

public class BuildingController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                var script = hit.transform.GetComponent<InteractibleBuilding>();
                script.FindPathToBuilding();
            }
        }
    }
}