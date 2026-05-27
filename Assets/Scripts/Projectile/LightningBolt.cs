using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class LightningBolt : MonoBehaviour
{
    [SerializeField]
    private float flashTime = 0.2f;

    [SerializeField]
    private int segments = 5;

    [SerializeField]
    private float perturbance = 0.5f;

    [SerializeField]
    private float impactRadius = 0.5f;

    [SerializeField]
    private LineRenderer lr;

    [SerializeField]
    private ParticleSystem ps;
    public GameObject Object => gameObject;
    
    public void Launch(Vector3 start, Vector3 end, Action<Entity> impactEffect, ObjectPool<LightningBolt> pool)
    {
        AudioManager.PlaySFX(Clip.Electro, 1f, .95f, 1.05f);

        RenderLine(start, end);

        Strike(end, impactEffect);

        StartCoroutine(ReleaseWithDelay(pool));
    }

    private void Strike(Vector3 target, Action<Entity> impactEffect)
    {
        List<Enemy> enemies = EnemyManager.instance.Enemies;

        enemies
            .Where(t =>
                Vector3.Distance(target, t.transform.position) < impactRadius)
            .ToList()
            .ForEach(enemy => impactEffect?.Invoke(enemy));
    }


    private IEnumerator ReleaseWithDelay(ObjectPool<LightningBolt> pool)
    {
        yield return new WaitForSeconds(flashTime);
        lr.enabled = false;
        yield return new WaitUntil(() => ps == null || !ps.isEmitting);
        pool.Release(this);
    }

    public void RenderLine(Vector3 start, Vector3 end)
    {
        lr.enabled = true;
        lr.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 pos = Vector3.Lerp(start, end, t);

            if (0 < i && i < segments)
            {
                pos.x += UnityEngine.Random.Range(-perturbance, perturbance);
                pos.y += UnityEngine.Random.Range(-perturbance, perturbance);
            }
            lr.SetPosition(i, pos);
        }

        
        ps.transform.position = end;
        ps.Play();

    }
}
