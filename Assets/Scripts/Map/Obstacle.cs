using UnityEngine;

public class Obstacle : MonoBehaviour
{
    /// <summary>
    /// Remove button event handler
    /// </summary>
    public void OnRemove()
    {

        // subtract money

        Destroy(gameObject);
        

        // play sfx

        // play vfx (dust particles?)

        BuildingManager.instance.RefreshNavMesh();
    }
}
