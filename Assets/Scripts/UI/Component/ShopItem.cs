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
            if (InventoryManager.AddComponent(factory.Invoke()))
            {
                // money management needed here!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            }
            else
            {
                WarningSystem.DisplayWarningMessage("Not enough space to buy!", 1f);
            }
        }
    }
}
