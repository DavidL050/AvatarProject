using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class DisableLocomotion : MonoBehaviour
{
    void Start()
    {
        FindFirstObjectByType<ContinuousMoveProvider>().enabled = false;
    }
}