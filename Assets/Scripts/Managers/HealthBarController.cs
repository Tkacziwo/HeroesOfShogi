using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Slider healthBarSlider;

    public void InitHealthBar(int health)
    {
        healthBarSlider.maxValue = health;
        healthBarSlider.minValue = 0;
        healthBarSlider.value = health;
    }

    public void UpdateHealthBar(float health)
    {
        healthBarSlider.value = health;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
