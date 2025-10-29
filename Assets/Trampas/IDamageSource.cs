using UnityEngine;

public interface IDamageSource
{
    int GetDamageAmount();      // Cu�nto da�o hace la trampa
    float GetCooldown();        // Tiempo de espera entre da�os al mismo objeto
}
