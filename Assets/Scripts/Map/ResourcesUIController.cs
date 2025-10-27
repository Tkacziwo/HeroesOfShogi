using TMPro;
using UnityEngine;

public class ResourceUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;

    [SerializeField] private TextMeshProUGUI stoneText;

    [SerializeField] private TextMeshProUGUI woodText;

    [SerializeField] private TextMeshProUGUI turnText;

    private uint turnNumber = 1;

    private void Start()
    {
        turnText.text = $"Turn: {turnNumber}";
    }

    public void UpdateResourcesUI(PlayerResources resources)
    {
        goldText.text = $"Gold: {resources.Gold}";
        stoneText.text = $"Stone: {resources.Stone}";
        woodText.text = $"Wood: {resources.Wood}";
    }

    public void IncrementTurnNumber()
    {
        turnNumber++;
        turnText.text = $"Turn: {turnNumber}";
    }
}