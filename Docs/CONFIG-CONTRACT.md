# Cấu hình thực tế

| Mục | Giá trị |
|---|---|
| Unity | 6000.3.23f1, URP; Input System 1.20 (Active Input Handling: New); Cinemachine 3.1.7; TextMeshPro |
| Layer | 6 Environment (sàn, bậc, dốc, tường, thanh chắn); 8 Player |
| Character Controller | Height 1,8; Radius 0,3; Center (0, 0,9, 0); Step Offset 0,3 (0,1 khi khom); Slope Limit 45; Skin Width 0,08 |
| PlayerController | run 5, walk 2, crouch 1,6 m/s; gravity −20; jumpHeight 1,2 m; xoay 720°/s; chạm đất giữ VelocityY −2 |
| Input Actions `Input/PlayerControls` | Move (WASD, Left Stick), Look (Mouse Delta, Right Stick), Jump (Space, Button South), Walk (Left Ctrl), ToggleWorldMove (T), Crouch (C), Attack (Left Mouse) |
| UI cảm ứng | On-Screen Stick `<Gamepad>/leftStick` (góc dưới trái), On-Screen Button `<Gamepad>/buttonSouth` (góc dưới phải), vùng kéo On-Screen Stick `<Gamepad>/rightStick` (nửa phải), EventSystem + Input System UI Input Module |
| Camera | CinemachineCamera, Tracking Target `Player/CameraTarget` (y 1,5; 0,95 khi khom); Orbital Follow Sphere, bán kính 4 m, damping 0,2; Rotation Composer damping 0,1; Deoccluder chỉ va layer Environment, Camera Radius 0,2, bỏ qua tag Player; pitch −20°..70° |
| Animator | Parameter `Speed` (0–1), `IsGrounded`, `Jump` (trigger); mở rộng `Crouch`, `Attack` (trigger). Locomotion → Jump (Jump), Jump → Fall (exit time 0,7), Locomotion → Fall (!IsGrounded), Fall → Locomotion (IsGrounded); Locomotion ↔ Crouch (Crouch). Layer Upper Body (mask thân trên) cho Attack, weight tăng khi đang đánh. Root Motion tắt |
| Clip | Idle, Walk, Run, Fall, Crouch, CrouchWalk loop; Jump, Attack không loop. Event: Footstep (Walk/Run), OpenHitbox 0,16 s / CloseHitbox 0,65 s (Attack) |
| Attack | 0,85 s, cooldown 1 s, −25 HP, tầm 1,1 m; dummy 100 HP, R hồi đầy |
