using System;
using UnityEngine;
using UnityEngine.UI;

public class ScrollBuy : MonoBehaviour
{
    [SerializeField] private Transform verticalLayout;
    [SerializeField] private GameObject shopItemPrefab;

    void Start()
    {
        foreach (Func<TowerComponent> factory in ComponentLibrary.GetAll())
        {
            TowerComponent exampleInstance = factory.Invoke();

            GameObject newShopItem = Instantiate(shopItemPrefab);
            newShopItem.transform.SetParent(verticalLayout, false);

            if (!newShopItem.TryGetComponent<ShopItem>(out ShopItem item))
                throw new MissingComponentException("ShopItem component is missing!");

            item.componentName.text = exampleInstance.Name;
            item.description.text = exampleInstance.Description;
            item.price.text = $"{exampleInstance.Price}€";
            item.image.sprite = exampleInstance.Image;

            item.factory = factory;
        }
    }    
}
