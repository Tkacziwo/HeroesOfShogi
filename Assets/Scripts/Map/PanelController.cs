using System;
using TMPro;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    PlayerCharacterController character;

    City city;

    public static Action<PlayerCharacterController> PlayerChanged;

    public static Action<City> CityOpened;

    public void SetPlayer(PlayerCharacterController character)
    {
        this.character = character;
        var characterId = character.characterId;
        this.GetComponentInChildren<TextMeshProUGUI>().text = characterId.ToString();
    }

    public void SetCity(City city)
    {
        this.city = city;
        var cityName = city.cityName;
        this.GetComponentInChildren<TextMeshProUGUI>().text = cityName;
    }

    public void OnClick()
    {
        if(character != null)
        {
            OnPlayerPanelClicked();
        }
        else
        {
            OnCityPanelClicked();
        }
    }

    public void OnPlayerPanelClicked()
    {
        if (character != null)
        {
            PlayerChanged?.Invoke(character);
        }
    }

    public void OnCityPanelClicked()
    {
        if (city != null)
        {
            CityOpened?.Invoke(city);
        }
    }
}
