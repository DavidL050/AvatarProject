using UnityEngine;

public interface IDamageSource
{
    int GetDamageAmount();      // Cuánto daño hace la trampa
    float GetCooldown();        // Tiempo de espera entre daños al mismo objeto
}
