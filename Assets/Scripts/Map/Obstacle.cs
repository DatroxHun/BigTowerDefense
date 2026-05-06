using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField]
    BuildingManager buildingManager;

    /// <summary>
    /// Remove button event handler
    /// </summary>
    public void OnRemove()
    {

        // subtract money

        buildingManager.RefreshNavMesh();
        
        // play sfx

        // maybe play vfx

        Destroy(gameObject);
    }
}
