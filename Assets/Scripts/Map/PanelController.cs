using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    PlayerCharacterController character;

    City city;

    [SerializeField] private Button focusButton;

    private int MaxMovementPoints { get; set; }

    private int PlayerId { get; set; }

    public static Action<City> CityOpened;

    public static Action<Transform> OnFocusOnPlayer;

    private void OnEnable()
    {
        PlayerEvents.OnPlayerEndMove += UpdateMovementPointsString;
        OverworldMapController.onTurnEnd += ResetMovementPointsString;
    }

    private void OnDisable()
    {
        PlayerEvents.OnPlayerEndMove -= UpdateMovementPointsString;
        OverworldMapController.onTurnEnd -= ResetMovementPointsString;
    }

    public void SetPlayer(PlayerCharacterController character)
    {
        this.character = character;
        PlayerId = character.playerId;
        MaxMovementPoints = character.GetMovementPoints();
        UpdateMovementPointsString(character);
    }

    public void UpdateMovementPointsString(PlayerCharacterController character)
    {
        if (PlayerId != character.playerId) return;

        string movementPointsString = $"{character.GetRemainingMovementPoints()} / {character.GetMovementPoints()}";

        this.GetComponentInChildren<TextMeshProUGUI>().text = movementPointsString;
    }

    public void ResetMovementPointsString()
    {
        string movementPointsString = $"{MaxMovementPoints} / {MaxMovementPoints}";

        this.GetComponentInChildren<TextMeshProUGUI>().text = movementPointsString;
    }

    public void SetCity(City city)
    {
        this.city = city;
        var cityName = city.cityName;
        this.GetComponentInChildren<TextMeshProUGUI>().text = cityName;
        focusButton.gameObject.SetActive(false);
    }

    public void OnClick()
    {
        if(character == null)
        {
            OnCityPanelClicked();
        }
    }

    public void FocusOnPlayer()
        => OnFocusOnPlayer?.Invoke(character.transform);

    public void OnCityPanelClicked()
    {
        if (city != null)
        {
            CityOpened?.Invoke(city);
        }
    }
}
