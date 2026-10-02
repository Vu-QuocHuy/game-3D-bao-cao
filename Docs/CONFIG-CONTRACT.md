# Hợp đồng cấu hình

- Editor: 6000.3.23f1, URP template.
- Layer 0: môi trường/obstacle; layer 8: Player và visual. Ground/Deoccluder chỉ dùng mask layer 0 và bỏ qua trigger.
- Input Actions: map `Player`, actions `Move`, `Look`, `Jump`, `Sprint`, `Crouch`, `Attack`.
- Motor config: `Assets/TrainingArena/Config/PlayerConfig.asset`.
- Animator parameter `Speed`: vận tốc ngang thực tế, m/s; ngưỡng Idle 0, Walk 3, Run 6.
- Base states: Locomotion, Jump, Fall, Land, Crouch, Hit, Dead.
- Layer `Upper Body Action`: Empty, Attack; Avatar Mask Generic theo đường dẫn Hips/UpperBody.
- Events trên Attack: OpenHitbox tại 0.16s, CloseHitbox tại 0.65s; state interruption/reset luôn đóng hitbox.
- Action duration 0.85s, cooldown 1.0s; damage mỗi đòn 25; damage zone mỗi 1s là 25; máu 100.
- Hit duration 0.45s; Dead không thoát bởi input gameplay; Backspace là reset ngoài FSM.
- Camera: CM TPS, CM FPS, CM Top Down, CM Fixed Zone; priority live 20, zone 30, standby 0; Brain blend EaseInOut 0.65s.
- Walk 3m/s, Run 6m/s, Crouch 1.6m/s, Jump 1.8m, gravity -22m/s²; capsule standing 1.8m, crouch 1.05m.
- Build target hiện dùng Linux x86_64 là module có sẵn trên máy triển khai. Windows cần module phù hợp và một lần build/kiểm tra riêng.
