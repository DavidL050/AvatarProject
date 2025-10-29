// Scripts/Player/PlayerTrapCollision.cs
using UnityEngine;
using System.Collections.Generic;

public class PlayerTrapCollision : MonoBehaviour
{
    public Health playerHealth;

    private Dictionary<GameObject, float> lastDamageTime = new Dictionary<GameObject, float>();

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.TryGetComponent<IDamageSource>(out var damageSource) && playerHealth != null)
        {
            GameObject trapObject = hit.collider.gameObject;
            lastDamageTime.TryGetValue(trapObject, out float lastTime);

            if (Time.time - lastTime >= damageSource.GetCooldown())
            {
                playerHealth.TakeDamage(damageSource.GetDamageAmount());
                lastDamageTime[trapObject] = Time.time;
            }
        }
    }
}
