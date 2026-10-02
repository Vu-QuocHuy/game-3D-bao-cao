using UnityEngine;
namespace TrainingArena {
public sealed class CameraZoneTrigger : MonoBehaviour {
 public CameraCoordinator cameraCoordinator;
 void OnTriggerEnter(Collider other){if(other.GetComponent<PlayerBrain>())cameraCoordinator.SetZone(this,true);}
 void OnTriggerStay(Collider other){if(other.GetComponent<PlayerBrain>())cameraCoordinator.SetZone(this,true);}
 public bool Contains(Transform target){var volume=GetComponent<Collider>();var character=target.GetComponent<CharacterController>();return isActiveAndEnabled&&volume&&character&&volume.bounds.Intersects(character.bounds);}
 void OnTriggerExit(Collider other){if(other.GetComponent<PlayerBrain>())cameraCoordinator.SetZone(this,false);}
 void OnDisable(){if(cameraCoordinator)cameraCoordinator.SetZone(this,false);}
}}
