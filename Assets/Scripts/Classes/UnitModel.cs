using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitModel : MonoBehaviour
{
    public Unit Unit { get; set; } = new();

    public GameObject Model;

    private List<Vector3> curvePath = new();

    private int pathIterator = 0;

    private float movementSpeed = 20f;

    public static Action UnitFinishedMoving;

    public GameObject promotionEffect;

    public GameObject healthBar;

    public GameObject pieceIcon;

    public void InitUnit(string name, int[] moveset, int x, int y, bool isSpecial, float movementSpeed, Unit template)
    {
        Unit.InitPiece(name, moveset, x, y, isSpecial);
        Unit.InitUnit(template);
        this.movementSpeed = movementSpeed;
        promotionEffect = Resources.Load("Prefabs/ParticleEffects/PromotionEffect") as GameObject;
        promotionEffect = Instantiate(promotionEffect);
        promotionEffect.transform.position = this.transform.position;
        promotionEffect.GetComponent<ParticleSystem>().Stop();
        promotionEffect.GetComponent<ParticleSystem>().Clear();
        this.healthBar = Instantiate(Resources.Load("Prefabs/Units/HealthBar") as GameObject);
        //healthBar.transform.SetParent(this.Model.transform);
        UpdateHealthBarPosition();
        var healthBarController = healthBar.GetComponent<HealthBarController>();
        healthBarController.InitHealthBar(template.HealthPoints);
    }

    public void SetPath(List<Vector3> path)
    {
        this.curvePath = path;
        pathIterator = 0;
    }

    private void Update()
    {
        if (curvePath.Count == 0) return;

        if (Vector3.Distance(Model.transform.position, curvePath[pathIterator]) >= 0000.1f)
        {
            float step = Time.deltaTime * movementSpeed;
            MakeStep(curvePath[pathIterator], step);
        }
        else
        {
            pathIterator++;

            if (pathIterator >= curvePath.Count)
            {
                curvePath.Clear();
                UnitFinishedMoving?.Invoke();
            }
        }
    }

    public void PromoteUnit(int[] changedMoveset)
    {
        this.Unit.Promote(changedMoveset);
        promotionEffect.GetComponent<ParticleSystem>().Play();
    }

    public void DemoteUnit()
    {
        promotionEffect.GetComponent<ParticleSystem>().Stop();
        promotionEffect.GetComponent<ParticleSystem>().Clear();
        Unit.Demote();
    }

    private void MakeStep(Vector3 targetPosition, float step)
    {
        Model.transform.position = Vector3.MoveTowards(Model.transform.position, targetPosition, step);
        promotionEffect.transform.position = Model.transform.position;
        UpdateHealthBarPosition();
    }

    public void UpdateHealthBarPosition()
        => healthBar.transform.SetPositionAndRotation(new(Model.transform.position.x + 0.8f, Model.transform.position.y + 0.5f, Model.transform.position.z), Quaternion.Euler(45, -90, 0));

    public void UpdateParticleSystemPosition()
        => promotionEffect.transform.SetPositionAndRotation(new(Model.transform.position.x, Model.transform.position.y, Model.transform.position.z), Quaternion.identity);
}