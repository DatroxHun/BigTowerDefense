using System;
using Unity.VisualScripting;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private int price = 1000;
    [SerializeField] private TooltipTrigger trigger;

    private void Start()
    {
        if (trigger != null)
            trigger.tooltipContent = $"{price}€";
    }

    /// <summary>
    /// Remove button event handler
    /// </summary>
    public void OnRemove()
    {

        // subtract money
        if (BuildingManager.instance.TrySubtractResources(price))
        {
            BuildingManager.instance.RemoveObsticle(this);
        }
        else
        {
            WarningSystem.DisplayWarningMessage("Insufficient funds!", .5f);
        }
    }
}
