using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderChangeValue : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private Slider slider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSliderChangeValue()
    {
        var val = slider.value;
        text.text = val.ToString() + "/10";
    }
}
