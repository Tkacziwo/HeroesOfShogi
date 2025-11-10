using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSlot : MonoBehaviour, IDropHandler
{
    public DragDropController droppedUnit;

    public string droppedName;

    private void OnEnable()
    {
        DragDropController.OnPickup += HandlePickup;
    }

    private void OnDisable()
    {
        
        DragDropController.OnPickup -= HandlePickup;
    }

    public void HandlePickup(EntityId id) 
    {
        if (droppedUnit == null) return;

        if(id == droppedUnit.GetEntityId())
        {
            droppedUnit = null;
            droppedName = null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("on drop");

        if (eventData.pointerDrag != null)
        {
            droppedUnit = eventData.pointerDrag.GetComponent<DragDropController>();
            droppedName = droppedUnit.assignedUnit.UnitName.ToString();

            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = this.GetComponent<RectTransform>().anchoredPosition;
        }
    }
}
