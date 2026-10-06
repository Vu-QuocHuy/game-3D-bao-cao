# Training Arena

Game mô phỏng 3D cho hai buổi báo cáo: **Thứ Ba: điều khiển nhân vật và camera**, **Thứ Năm: trạng thái và animation**. Một project, một scene, theo đặc tả [`dac-ta-game-demo-3d.md`](dac-ta-game-demo-3d.md).

## Mở và chơi

1. Unity Hub → Add project from disk → chọn thư mục này. Dùng Unity **6000.3.23f1** (URP, Input System, Cinemachine 3.1, TextMeshPro).
2. Mở `Assets/TrainingArena/Scenes/TrainingArena.unity` → Play. Hoặc chạy bản build `Builds/TrainingArena/TrainingArena`.
3. Thử giao diện cảm ứng: Game view → Simulator.

| Phím / thao tác | Chức năng |
|---|---|
| WASD / joystick ảo | Di chuyển (mặc định chạy 5 m/s) |
| Giữ Ctrl / đẩy joystick nhẹ | Đi bộ 2 m/s |
| Chuột / kéo nửa phải màn hình | Xoay camera |
| Space / nút JUMP | Nhảy (chỉ khi chạm đất) |
| T | Đổi góc camera: third person → first person → top-down (giống commit đầu tiên) |
| C | Khom (mở rộng), tự giữ thấp nếu có trần |
| Chuột trái | Attack (mở rộng), chỉ khi chạm đất |
| R | Về điểm xuất phát |
| F1 | Ẩn/hiện HUD |
| Esc | Nhả con trỏ; click để khóa lại |

## Bố cục scene (mục 3)

Xuất phát ở giữa, nhìn về phía +Z. Trái: bậc thang 5 × 0,2 m dẫn tới bục cao 1,5 m. Phải: bậc 0,6 m (bị chặn). Phía trước: dốc 30° (leo được) và dốc 60° (bị chặn), xa hơn là hành lang tường 3 m để thử camera. Phía sau: sân rộng, thanh chắn khom (trái) và dummy để đánh (phải). Môi trường nằm ở layer **Environment**.

## Kiến trúc (mục 6)

| Script | Vai trò |
|---|---|
| `PlayerController` | Đọc Input (Player Input), tính vận tốc, gọi `CharacterController.Move` một lần/khung hình, xoay nhân vật 720°/s, công bố `InputMagnitude`, `HorizontalSpeed`, `VelocityY`, `IsGrounded`, `JumpPressed` |
| `StateMachine.cs` | `IState`, `StateMachine`, `IdleState`, `MoveState`, `JumpState`, `FallState`; mở rộng `CrouchState`, `AttackState` không sửa 4 state gốc |
| `PlayerStateMachine` | Chạy FSM sau `PlayerController` (Script Execution Order 10) |
| `AnimatorBridge` | Ghi `Speed` (0–1), `IsGrounded`, `Crouch` vào Animator; nhận Animation Event |
| `CameraOrbitInput` | Đưa action `Look` vào Cinemachine Orbital Follow |
| `CameraModeSwitcher` | T xoay vòng Priority giữa CinemachineCamera third person, `CM First Person` (tầm mắt, ẩn model) và `CM Top Down` (cao 14 m, lùi 5 m, nghiêng 70°); không di chuyển nhân vật |
| `PlayerCombat` | Attack: Animation Event mở/đóng hitbox, C# giữ cooldown |
| `DebugHUD`, `DemoController` | HUD góc trên trái; R / F1 / Esc |

Animator `Player.controller`: Locomotion (Blend Tree Idle 0 / Walk 0,5 / Run 1), Jump, Fall, Crouch; layer Upper Body (Avatar Mask) cho Attack. Root Motion tắt. Model Humanoid dựng bằng khối cơ bản, Avatar hợp lệ; clip là muscle clip tạo bằng `HumanPoseHandler`.

## Công cụ

- `Training Arena/Build Demo Scene` (`DemoSceneBuilder.cs`): dựng lại scene, model, Avatar, clip, Animator Controller, Input Actions. **Ghi đè scene; sửa tay trong scene sẽ mất.**
- `Training Arena/Build Linux Player`: build vào `Builds/TrainingArena/`.
- `DemoVerifier`: `Builds/TrainingArena/TrainingArena -batchmode --verify-demo --verification-report out.json [--verification-shots dir]`.

Xem [tuyến demo](Docs/DEMO-RUNBOOK.md), [cấu hình](Docs/CONFIG-CONTRACT.md), [đối chiếu đặc tả](Docs/IMPLEMENTATION-STATUS.md), [kết quả kiểm tra](Docs/BUILD-NOTES.md).
