# Hợp đồng cấu hình

- Editor: 6000.3.23f1, URP. Input System 1.20, Cinemachine 3.1.7.
- Layer 0: môi trường; layer 8: Player và visual. Ground check, trần và Deoccluder chỉ dùng layer 0, bỏ qua trigger.
- Input Actions (`Input/PlayerControls.inputactions`): Move, Look, Jump, Sprint, Crouch. H/K/R/F1/C/Esc đọc trực tiếp từ bàn phím trong `DemoController`/`CameraCoordinator`.
- Motor (`Config/PlayerConfig.asset`): Walk 3 m/s, Run 6 m/s, Crouch 1,6 m/s, Jump 1,8 m, gravity −22 m/s². Capsule đứng 1,8 m, khom 1,05 m. Step offset 0,45 m khi đứng, 0,1 m khi khom.
- FSM: Grounded=0, Airborne=1, Hit=2, Dead=3. HP 100, H −25 (chỉ Grounded), Hit 0,7 s, K → HP 0. Ưu tiên Dead > Hit > Jump trong cùng frame.
- Animator `Player.controller`, Root Motion tắt:
  - Parameter: `Mode` (int), `Speed` (float, m/s thực tế), `VerticalSpeed` (float), `Crouch` (bool), `Attack` (trigger).
  - Locomotion: Blend Tree 1D Idle 0 / Walk 3 / Run 6. Crouch: Blend Tree Crouch 0 / CrouchWalk 1,6.
  - Any State → Dead (`Mode==3`), Any State → Hit (`Mode==2`), không tự chuyển lại chính nó.
  - Locomotion/Crouch/Land → Jump (`Mode==1`, VerticalSpeed > 0,1) hoặc Fall (VerticalSpeed < 0,1); Jump → Fall; Jump/Fall → Land (`Mode==0`) → Locomotion.
  - Hit → Locomotion (`Mode==0`) hoặc Fall (`Mode==1`); Hit/Dead tắt loop.
  - Layer `Upper Body Action` (Avatar Mask Hips/UpperBody, weight 1, Override): Empty → Attack (trigger), Attack → Empty theo exit time 0,95 hoặc khi `Mode > 1`.
- Attack: Event OpenHitbox 0,16 s, CloseHitbox 0,65 s; thời lượng 0,85 s, cooldown 1 s, damage 25, tầm 1,1 m bán kính 0,6 m; chỉ Grounded. Dummy 100 HP, R hồi đầy.
- Camera: CM TPS (priority 20 khi chọn) và CM Top Down làm góc rộng (priority 20 khi chọn), Brain blend EaseInOut 0,65 s, Deoccluder trên TPS. Pitch −30°..70°.
