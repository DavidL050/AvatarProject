// Scripts/Traps/Trap.cs
using UnityEngine;

public class Trap : MonoBehaviour, IDamageSource
{
    [Header("Configuraci�n de Da�o")]
    public int damageAmount = 25;
    public float damageCooldown = 2f;

    public int GetDamageAmount() => damageAmount;
    public float GetCooldown() => damageCooldown;
}

