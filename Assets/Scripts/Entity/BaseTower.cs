using UnityEngine;

public class BaseTower : Tower
{


    protected override void Action()
    {
        
    }

    protected override void Target()
    {
        
    }

    protected override void JustDied()
    {
        base.JustDied();

        GameOver.Instance.Toggle();
    }
}
