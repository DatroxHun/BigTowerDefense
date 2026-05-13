using UnityEngine;

public class ManicPyromancer : Pyromancer
{
    protected override void JustDied()
    {
        Detonate();
        base.JustDied();
    }
}
