using UnityEngine;

public class MeleeSlime : Enemy
{
    

    // Actions
    protected override void Action()
    {
        Debug.Log("Enemy Action");

        if (CurrentTarget is EntityTarget target && target.entity != null)
        {
            DamageObj dmg = AttackDamage;

            target.entity.ApplyEffect(Effects.InstantDamage(target.entity, dmg));

            animator.SetTrigger("attack");

            AudioManager.PlaySFX(Clip.BasicAttack, 1f, 1f, 1.1f);
        }
    }
}
