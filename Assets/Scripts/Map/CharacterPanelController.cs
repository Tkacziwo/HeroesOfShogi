using System;
using TMPro;
using UnityEngine;

public class CharacterPanelController : MonoBehaviour
{
    PlayerCharacterController character;

    public static Action<PlayerCharacterController> PlayerChanged;

    public void SetPlayer(PlayerCharacterController character)
    {
        this.character = character;
        var characterId = character.characterId;
        this.GetComponentInChildren<TextMeshProUGUI>().text = characterId.ToString();
    }

    public void OnPlayerPanelClicked()
    {
        if (character != null)
        {
            PlayerChanged?.Invoke(character);
        }
    }
}
