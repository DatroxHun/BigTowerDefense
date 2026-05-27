using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public TextMeshProUGUI componentName;
    public TextMeshProUGUI description;
    public TextMeshProUGUI price;
    public Image image;

    public Func<TowerComponent> factory;

    public void Pressed()
    {
        if (factory != null)
        {
            TowerComponent newComponent = factory.Invoke();
            
            if (BuildingManager.instance.Resources < newComponent.Price)
            {
                WarningSystem.DisplayWarningMessage("Insufficient funds!", .5f);
            }

            else if (!InventoryManager.AddComponent(newComponent))
            {
                WarningSystem.DisplayWarningMessage("Not enough space to buy!", .5f);
            }
            else
            {
                BuildingManager.instance.TrySubtractResources(newComponent.Price);
            }
        }
    }
}
