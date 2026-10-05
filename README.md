# Training Arena

Game 3D dùng cho hai đề tài: **T3 — điều khiển và camera**, **T5 — trạng thái và character animation**.

## Mở và chơi

1. Unity Hub → Add project from disk → chọn thư mục này.
2. Dùng Unity **6000.3.23f1**; đợi Package Manager và import hoàn tất.
3. Mở `Assets/TrainingArena/Scenes/TrainingArena.unity` cho T5, hoặc `TrainingArena_T3.unity` cho T3.
4. Nhấn Play. R reset; Esc mở/khóa chuột.

| Phím | Chức năng |
|---|---|
| WASD / Shift / Space | Đi / chạy / nhảy (chỉ khi Grounded) |
| Chuột | Xoay camera TPS, pitch giới hạn -30°..70° |
| Ctrl | Khom; tự giữ thấp nếu có trần |
| Chuột trái | Attack (chỉ khi Grounded, cooldown 1 s) |
| C | TPS ↔ góc rộng (có blend) |
| H | DEBUG: -25 HP, vào Hit (chỉ khi Grounded) |
| K | DEBUG: HP về 0, vào Dead |
| R | Reset: về spawn, HP 100, Grounded, camera TPS |
| F1 | Ẩn/hiện HUD |

Animator `Player.controller` theo `Mode` (0 Grounded, 1 Airborne, 2 Hit, 3 Dead), `Speed`, `VerticalSpeed`. Transition được tạo bởi `Training Arena/Wire Player Animator` (`AnimatorWiring.cs`).

## Tuyến demo

Bố cục theo đặc tả `03-dac-ta-demo-unity-dung-chung.md`, nhãn trên sàn:

- **A START**: điểm xuất phát và điểm reset.
- **B JUMP + STEPS**: bậc thang lên platform, nhảy xuống để thấy Jump/Fall/Land. Dốc bên phải.
- **C CAMERA CORRIDOR**: hành lang bên trái, thử Deoccluder sát tường.
- **D STATE TEST H / K**: nền đỏ bên phải gần A, thử Hit/Dead/R.
- **CROUCH (CTRL)**: thanh chắn vàng bên trái spawn (tính năng thêm, ngoài D01–D13).
- **ATTACK (LMB)**: dummy vàng bên phải spawn, 100 HP, mỗi đòn −25 (tính năng thêm).

Tuyến T3: A → B → C → C (góc rộng). Tuyến T5: A → B → D.

## Kiến trúc

- `PlayerInputHandler`: Move, Look, Jump, Sprint, Crouch từ Input Actions.
- `PlayerMotor`: CharacterController, gravity, ground check, jump, crouch, kiểm tra trần trước khi đứng.
- `PlayerBrain`: FSM 4 state Grounded/Airborne/Hit/Dead, HP, ưu tiên Dead > Hit > Jump, lý do chặn lệnh, lịch sử 5 chuyển. Crouch là mức tốc độ trong Grounded.
- `PlayerAnimator`: ghi `Mode`, `Speed`, `VerticalSpeed`, `Crouch` vào Animator. Không tự đổi state gameplay.
- `CameraCoordinator`: TPS + góc rộng, Brain blend, Deoccluder; điểm nhìn hạ thấp khi khom.
- `PlayerCombat`: Attack là action chạy song song trên layer Upper Body, không phải state FSM thứ 5. C# giữ thời lượng 0,85 s, cooldown 1 s, hủy khi Hit/Dead; Animation Event `OpenHitbox`/`CloseHitbox` trên clip mở/đóng hitbox; mỗi target nhận damage một lần mỗi đòn.
- `DemoController`: H/K/R/F1/Esc, R hồi máu dummy; `DebugOverlay`: HUD và lịch sử.

## Asset animation

Bản demo sử dụng **robot được dựng và hoạt họa bằng asset Unity trong project**, không cần tải Mixamo hoặc đăng nhập dịch vụ ngoài. Rig hiện tại là **Generic**, với clip transform và Avatar Mask theo đường dẫn xương. Các clip Idle/Walk/Run/Jump/Fall/Land/Crouch/CrouchWalk/Hit/Dead được tạo bởi `AnimatorWiring.cs` (menu `Training Arena/Wire Player Animator`); controller có thể mở trong Editor.

Khu bên trái spawn bổ sung hai robot **Humanoid** với Avatar khác nhau, cùng controller và cùng các muscle clips Idle/Walk/Run. Robot đích có chân dài hơn và tay ngắn hơn để quan sát retargeting. Asset Avatar, clip và controller nằm trong `Animations/`. Không dùng asset Mixamo. Khi thay model nhân vật chơi, cần thay clip/controller tương thích với rig và giữ bridge/state gameplay hiện có.

## Công cụ và build

- `Training Arena/Apply Spec Scene Setup`: nối Animator, dọn scene theo khu A/B/C/D, chạy lại an toàn nhiều lần.
- `Training Arena/Generate Demo Scene` dựng lại demo bằng Editor API, rồi tự chạy Apply Spec Scene Setup. **Thao tác này tạo lại scene/animation của demo; không chạy trên scene đã chỉnh tay mà chưa lưu bản sao.**
- `ProjectBootstrap.PackageInstaller.Install`: cài package bằng UPM Client API.
- `ProjectBootstrap.ArenaBuilder.BuildLinux`: tạo bản T3/T5 Linux dưới `Builds/`; binaries T3/T5 đã được lưu sẵn.
- `DemoVerifier`: chạy build với `--verify-demo --verification-report <file>.json` (thêm `--verification-shots <dir>` để chụp ảnh). Không gắn vào scene.
- Unity MCP đã là dependency project; bật server trong `Window → MCP for Unity` khi cần. Codex dùng URL local `http://localhost:8080/mcp`.
- Project hiện có uvx trong `Tools/uv/bin` (bỏ qua khỏi Git) để Unity MCP có thể tải/chạy server local trên máy đã cài project. Chạy Unity Hub project trước khi dùng công cụ MCP.

Xem [tuyến demo](Docs/DEMO-RUNBOOK.md), [cấu hình](Docs/CONFIG-CONTRACT.md) và [kết quả kiểm tra](Docs/BUILD-NOTES.md).
