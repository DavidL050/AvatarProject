using UnityEngine;
using System.Collections;

public class XRBootSnap : MonoBehaviour {
    public Transform xrOrigin;   // XR Origin (XR Rig)
    public Transform hmd;        // Main Camera (HMD)
    public Transform defaultTarget; // tu target inicial (vista al avatar)
    public CharacterController cc;  // opcional

    IEnumerator Start() {
        yield return null; // espera 1 frame a que el HMD reporte pose
        TeleportTo(defaultTarget);
    }

    public void TeleportTo(Transform target) {
        if (!xrOrigin || !hmd || !target) return;
        if (cc) cc.enabled = false;

        var hmdOffset = hmd.position - xrOrigin.position;
        var endRot = target.rotation;
        var rotatedOffset = endRot * Quaternion.Inverse(xrOrigin.rotation) * hmdOffset;
        var endPos = target.position - rotatedOffset;

        xrOrigin.SetPositionAndRotation(endPos, endRot);
        if (cc) cc.enabled = true;
    }
}
