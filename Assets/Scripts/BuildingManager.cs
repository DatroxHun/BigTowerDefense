using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingManager : MonoBehaviour
{
    private GameObject towerPrefab;

    [SerializeField]
    private LayerMask blockingLayers;

    private GameObject currentGhost;
    
    private BoxCollider2D prefabCollider;

    [SerializeField]
    Camera mainCamera;


    bool buildingMode = false;


    public void StartBuilding(GameObject tower)
    {
        if (buildingMode)
            return;

        towerPrefab = tower;

        buildingMode = true;
        
        currentGhost = Instantiate(towerPrefab);
        
        BoxCollider2D ghostCollider = (currentGhost.GetComponent<BoxCollider2D>());
        ghostCollider.enabled = false;
        prefabCollider = ghostCollider;

        var ghostSprite = currentGhost.GetComponent<SpriteRenderer>();
        ghostSprite.sortingLayerName = "air";

        Tower script = currentGhost.GetComponent<Tower>();
        script.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!buildingMode)
            return;


        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePosition);
        worldPos.z = 0;
        currentGhost.transform.position = worldPos;


        bool canPlace = CheckPlacement(worldPos);

        
        currentGhost.GetComponent<SpriteRenderer>().color = canPlace
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
        Instantiate(towerPrefab, position, Quaternion.identity);
        CancelBuilding();
    }

    void CancelBuilding()
    {
        buildingMode = false;
        if (currentGhost != null)
        {
            Destroy(currentGhost);
            Debug.Log("destroyed ghost");
        }
    }
}
