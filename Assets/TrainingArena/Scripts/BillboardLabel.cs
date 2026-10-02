using UnityEngine;
namespace TrainingArena {
public sealed class BillboardLabel : MonoBehaviour {
 void LateUpdate(){if(Camera.main)transform.rotation=Camera.main.transform.rotation;}
}}
