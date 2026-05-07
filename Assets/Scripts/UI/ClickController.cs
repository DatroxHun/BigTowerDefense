using UnityEngine;

public class ClickController : MonoBehaviour
{
    [SerializeField] private Interaction interaction;

    public void ToggleInteraction()
    {
        interaction.SetVisibility(!interaction.Visible);
    }
}
