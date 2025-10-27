using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DoubleClickHandler : MonoBehaviour, IPointerDownHandler
{
    private int clicked = 0;

    [SerializeField] private float delay;

    float clickTime = 0f;

    private PointerEventData eventData;

    public static event Action<DoubleClickHandler> OnDoubleClick;

    public void OnPointerDown(PointerEventData eventData)
    {
        clicked++;

        if (clicked == 1) clickTime = Time.time;

        if (clicked == 2 && Time.time - clickTime < delay)
        {
            clicked = 0;
            clickTime = 0f;
            Debug.Log("DoubleClick");
            OnDoubleClick?.Invoke(this);
        }
        else if (clicked > 2 || Time.time - clickTime > delay)
        {
            clicked = 0;
            clickTime = 0f;
        }
    }

    private void Start()
    {
        eventData = new PointerEventData(EventSystem.current);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnPointerDown(eventData);
        }
    }
}
