using NUnit.Framework;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Pyromancer : Enemy
{
    [SerializeField]
    private CircleCollider2D explosionRange;
    protected override void Action()
    {
        Detonate();
        HitPoints = 0;
        JustDied();
    }

    protected void Detonate()
    {
        List<Tower> towersInRange = TowerManager.instance.Towers
            .Where(t =>
                t != null &&
                t.IsAlive &&
                !t.Hiding &&
                explosionRange.OverlapPoint(t.transform.position))
            .ToList();

        DamageObj dmg = AttackDamage;

        foreach (Tower tower in towersInRange)
        {
            tower.ApplyEffect(
                    Effects.InstantDamage(tower, dmg)
            );
        }

        // replace with bigger explosion effect
        ParticlePool.Emit(transform.position, ParticleType.Smoke, sizeMultiplier: 2);
        AudioManager.PlaySFX(Clip.Boom, 1f, .95f, 1.05f);
    }
}
