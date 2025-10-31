using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;

    [SerializeField] private TextMeshProUGUI stoneText;

    [SerializeField] private TextMeshProUGUI woodText;

    [SerializeField] private TextMeshProUGUI turnText;

    [SerializeField] private Canvas canvasRef;

    [SerializeField] private CharacterPanelController playerCharacterPanel;


    private void OnEnable()
    {
        PlayerController.PlayerSpawned += UpdatePlayerCharacterPanels;
    }

    private void OnDisable()
    {
        PlayerController.PlayerSpawned -= UpdatePlayerCharacterPanels;
    }

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

    public void UpdatePlayerCharacterPanels(PlayerModel player)
    {
        if (!player.isRealPlayer) return;

        float size = playerCharacterPanel.GetComponent<RectTransform>().rect.width;
        float posX = 60f;

        float posY = -60f;

        foreach (var character in player.GetPlayerCharacters())
        {
            var obj = Instantiate(playerCharacterPanel, new(posX, posY), Quaternion.identity);
            obj.transform.SetParent(canvasRef.transform, false);

            var panelScript = obj.GetComponent<CharacterPanelController>();
            panelScript.SetPlayer(character);
            //obj.transform.position = new Vector3(posX, posY);
            posX += size;
        }
    }
}