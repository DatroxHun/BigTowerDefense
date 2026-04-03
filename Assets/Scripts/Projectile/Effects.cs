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
        for (int i = 0; i < count; i++)
        {
            yield return new WaitForSeconds(delay);
            entity.ApplyDamage(dobj);
        }
    }
}
