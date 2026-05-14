using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerManager : MonoBehaviour
{
    public static TowerManager instance;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            towers = new List<Tower>();
        }
        else
            Destroy(this);
    }

    private List<Tower> towers;

    public List<Tower> Towers
    {
        get => new List<Tower>(towers);
    }

    public void AddTower(Tower e) => towers.Add(e);

    public bool RemoveTower(Tower e) => towers.Remove(e);

    public void RepairTowers()
    {
        foreach (Tower t in towers)
        {
            t.OnRepair();
        }
    }

    private int clickableLayerMask;

    private void Start()
    {
        clickableLayerMask = LayerMask.GetMask("Clickable");
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // The mouse is over a UI Canvas element. 
            // Return out of the function so we don't click the tower.
            return;
        }

        // Clicking clickable objects
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, clickableLayerMask);

            if (hit.collider != null)
            {
                if (hit.collider.TryGetComponent<ClickController>(out ClickController controller))
                {
                    controller.ToggleInteraction();
                }
            }
            else
            {
                Interaction.CloseAll();
            }
        }
    }
}
