# Đối chiếu đặc tả `dac-ta-game-demo-3d.md`

| Mục | Trạng thái |
|---|---|
| 2 Thiết lập | Unity 6, URP, Input System (New), Cinemachine, TextMeshPro. Device Simulator và độ phân giải Game view chỉnh trong Editor |
| 3 Bố cục | Sân 40×40 ô vuông 1 m, bậc 5 × 0,2 m, bậc 0,6 m, dốc 30° và 60°, bục 1,5 m, hành lang tường 3 m, vật cản trụ; layer Environment; Directional Light có bóng |
| 4 Nhân vật | Player (CharacterController, Player Input, PlayerController, PlayerStateMachine) / Model (Humanoid, Avatar hợp lệ, Animator) / CameraTarget; thông số CC đúng bảng 4.3 |
| 5 Input | Move, Look, Jump, Walk, ToggleCamera (T) (+ Crouch, Attack); joystick ảo, nút nhảy, vùng kéo xoay camera. ToggleWorldMove bỏ theo yêu cầu |
| 6 Script | PlayerController, IState/StateMachine/Idle/Move/Jump/Fall, AnimatorBridge, DebugHUD |
| 7 Camera | Brain, CinemachineCamera Orbital Follow 5,2 m (thông số commit đầu tiên), damping, Deoccluder layer Environment radius 0,25, khóa con trỏ + Esc; T đổi third person / first person / top-down |
| 8 Animator | Speed/IsGrounded/Jump, Locomotion Blend Tree 0/0,5/1, Jump, Fall, Root Motion tắt |
| 9 Chức năng demo | T, tắt Animator khi Play, HUD State, R |
| 12 Mở rộng | Animation Event (Footstep, hitbox); state Attack và Crouch thêm mà không sửa state gốc; đổi góc camera bằng T như commit đầu tiên. Chưa làm: Rigidbody so sánh, APK |

Khác biệt có chủ đích: crouch dùng phím **C** vì Ctrl là Walk theo đặc tả. Jump → Fall dùng exit time ngắn vì `IsGrounded` đã false ngay khi bật nhảy (đặc tả cho phép). Theo yêu cầu: phím T đổi góc camera thay cho đổi hướng đi; camera dùng thông số commit đầu tiên (5,2 m) thay cho ~4 m.
