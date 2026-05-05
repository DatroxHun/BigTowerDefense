using UnityEngine;

public class Obstacle : MonoBehaviour
{
    /// <summary>
    /// Remove button event handler
    /// </summary>
    public void OnRemove()
    {
        // re-bake navmesh

        // subtract money
        
        // play sfx

        // maybe play vfx

        Destroy(gameObject);
    }
}
