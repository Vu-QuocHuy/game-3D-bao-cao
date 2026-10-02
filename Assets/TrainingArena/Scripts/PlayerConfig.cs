using UnityEngine;
namespace TrainingArena {
[CreateAssetMenu(menuName="Training Arena/Player Configuration")]
public sealed class PlayerConfig : ScriptableObject {
 public float walkSpeed=3,runSpeed=6,crouchSpeed=1.6f,jumpHeight=1.8f,gravity=-22,turnRate=650;
}
}
