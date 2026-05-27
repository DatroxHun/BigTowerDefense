using NUnit.Framework;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
[Serializable]
public class ScrollBuy : MonoBehaviour
{
    [SerializeField] private Transform verticalLayout;
    [SerializeField] private GameObject shopItemPrefab;
    private List<ShopItem> currentShop = new();
    private List<String> names = new();

    void Start()
    {
        foreach (Func<TowerComponent> factory in ComponentLibrary.GetAll()) // setup the shop
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
            currentShop.Add(item);
        }
        names = currentShop.Select(x => x.componentName.text).ToList();
    }

    public void Refresh(List<ComponentType> allowedTypes) // only togggle stuff
    {
        foreach (var shopItem in currentShop)
        {
            shopItem.gameObject.SetActive(false);
        }
        Debug.Log(currentShop.Count);
        foreach (Func<TowerComponent> factory in ComponentLibrary.GetAll())
        {
            TowerComponent exampleInstance = factory.Invoke();
            if(!allowedTypes.Contains(ComponentType.All) && !exampleInstance.Types.All(x => allowedTypes.Contains(x))) { continue; }
            currentShop[names.IndexOf(exampleInstance.Name)].gameObject.SetActive(true);
        }
    }
}
