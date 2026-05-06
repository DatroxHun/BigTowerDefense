using System.Collections;
using UnityEngine;

public static class Effects
{
    public static IEnumerator InstantDamage(Entity entity, DamageObj dobj)
    {
        //                           V--- get actual DMGObject that gets updated upon component updates
        // enemy.applydamage(new DMGOBJ(physical, 10))

        entity.ApplyDamage(dobj);

        yield return null;
    }

    public static IEnumerator RecurrentDamage(Entity entity, DamageObj dobj, int count, float delay)
    {
        var waitDelay = new WaitForSeconds(delay);
        for (int i = 0; i < count; i++)
        {
            yield return waitDelay;
            entity.ApplyDamage(dobj);
        }
    }
    public static IEnumerator InstantHeal(Entity entity, float hitPoints)
    {
        entity.ApplyHeal(hitPoints);

        yield return null;
    }
}
