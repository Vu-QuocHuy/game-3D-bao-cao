using UnityEngine;
namespace TrainingArena {
public sealed class CameraZoneTrigger : MonoBehaviour {
    // Dummy class to prevent broken references in Unity scene
    // Camera zones are explicitly removed by the T3/T5 specs.
    public CameraCoordinator cameraCoordinator;
    public bool Contains(Transform player) => false;
}
}
