using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using Unity.AI.Navigation;
using NavMeshPlus.Components;
using UnityEditor.Search;
using TMPro;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager instance;

    private GameObject towerPrefab;

    [SerializeField]
    private LayerMask blockingLayers;

    private GameObject currentGhost;
    
    private BoxCollider2D prefabCollider;

    bool buildingMode = false;

    [SerializeField]
    private float saleMultiplier = 0.8f;
    public float SaleMultiplier { get => saleMultiplier; }

    [SerializeField]
    Camera mainCamera;

    [SerializeField]
    public NavMeshPlus.Components.NavMeshSurface NavMeshSurface;

    [SerializeField]
    TextMeshProUGUI resourceText;


    [SerializeField]
    public int startingResources;

    private int resources;

    public int Resources
    {
        get { return resources; }
        set 
        { 
            resources = value;
            resourceText.text = $"{resources}€";
        }
    }


    public bool TrySubtractResources(int amount)
    {
        if (Resources < amount)
        {
            return false;
        }
        else
        {
            Resources -= amount;
            return true;
        }
    }

    /// <summary>
    /// Add resources multiplied by the sale multiplier.
    /// </summary>
    /// <param name="buyValue">Prchase value of the sold item.</param>
    public void SellForResources(int buyValue)
    {
        Resources += (int)Mathf.Floor(buyValue * saleMultiplier);
    }

    private void Start()
    {
        Resources = startingResources;    
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }


    public void StartBuilding(GameObject tower)
    {
        if (buildingMode)
            return;

        towerPrefab = tower;

        buildingMode = true;
        
        currentGhost = Instantiate(towerPrefab);
        
        Tower script = currentGhost.GetComponent<Tower>();
        script.enabled = false;
        TowerManager.instance.RemoveTower(script);
        
        BoxCollider2D ghostCollider = (currentGhost.GetComponent<BoxCollider2D>());
        ghostCollider.enabled = false;
        prefabCollider = ghostCollider;

        var ghostSprite = currentGhost.GetComponentInChildren<SpriteRenderer>();
        ghostSprite.sortingLayerName = "air";
    }

    void Update()
    {
        if (!buildingMode)
            return;


        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePosition);
        worldPos.z = 0;
        currentGhost.transform.position = worldPos;


        bool canPlace = CheckPlacement(worldPos);

        
        currentGhost.GetComponentInChildren<SpriteRenderer>().color = canPlace
            ? Color.green 
            : Color.red;

        if (Mouse.current.leftButton.wasPressedThisFrame && canPlace)
        {
            PlaceTower(worldPos);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelBuilding();
        }
    }

    bool CheckPlacement(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapBox(
            position + prefabCollider.offset,
            prefabCollider.size * 0.95f,
            0,
            blockingLayers
        );

        return hit == null;
    }

    

    void PlaceTower(Vector3 position)
    {
        int price = towerPrefab.GetComponent<Tower>().Price;

        if (instance.TrySubtractResources(price))
        {
            Instantiate(towerPrefab, position, Quaternion.identity);
            RefreshNavMesh();
        }
        else
        {
            WarningSystem.DisplayWarningMessage("Insufficient funds!", 1f);
        }

        CancelBuilding();
    }

    void CancelBuilding()
    {
        buildingMode = false;
        if (currentGhost != null)
        {
            Destroy(currentGhost);
            //Debug.Log("destroyed ghost");
        }
    }

    public void RefreshNavMesh()
    {
        NavMeshSurface.BuildNavMesh();
    }

    public void SellTower(Tower tower)
    {
        SellForResources(tower.Price);
        TowerManager.instance.RemoveTower(tower);
        Destroy(tower.gameObject);
        RefreshNavMesh();
    }
}
