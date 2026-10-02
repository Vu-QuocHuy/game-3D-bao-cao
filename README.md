# Training Arena

Game 3D dùng cho hai đề tài: **T3 — điều khiển và camera**, **T5 — trạng thái và character animation**.

## Mở và chơi

1. Unity Hub → Add project from disk → chọn thư mục này.
2. Dùng Unity **6000.3.23f1**; đợi Package Manager và import hoàn tất.
3. Mở `Assets/TrainingArena/Scenes/TrainingArena.unity` cho T5, hoặc `TrainingArena_T3.unity` cho T3.
4. Nhấn Play. Backspace reset; Esc mở/khóa chuột.

| Phím | Chức năng |
|---|---|
| WASD / Shift / Space | Đi / chạy / nhảy |
| Chuột | Xoay camera TPS/FPS |
| Ctrl | Crouch; tự giữ thấp nếu không đủ chỗ đứng |
| Chuột trái | Attack trong T5 |
| C | TPS → FPS → top-down |
| F1 / F2 | Bảng phím / debug |
| F3 / F4 / F5 | Damping / Deoccluder / Impulse |
| F6 | Chuyển T3/T5 và reset |
| Backspace | Reset toàn bộ demo |

## Tuyến demo

- Từ spawn đi thẳng đến bậc thang, lên platform và nhảy xuống để quan sát Fall/Land/Impulse.
- Phía trái: hành lang camera và thanh chắn crouch.
- Phía phải gần spawn: dummy để đánh, bẫy đỏ gây damage theo nhịp đến Hit/Dead.
- Góc phải phía xa: zone camera cố định; ra khỏi zone trở lại camera đã chọn.

## Kiến trúc

- `PlayerInputHandler`: một nguồn input, dùng asset Input Actions.
- `PlayerMotor`: CharacterController, gravity, ground probe, jump, crouch và kiểm tra chỗ đứng.
- `PlayerBrain`: locomotion FSM, action FSM và ưu tiên Normal/Hit/Dead.
- `PlayerAnimator`: bridge logic → Animator. Animator có Blend Tree 1D và upper-body layer với Avatar Mask.
- `PlayerCombat`: hitbox mở/đóng bởi Animation Events; mỗi target chỉ nhận damage một lần mỗi đòn.
- `CameraCoordinator`: bốn Cinemachine Camera; Brain chọn bằng Priority, zone blend, Deoccluder và Impulse.
- `DemoController`: topic và reset; `DebugOverlay`: dữ liệu thực tế của nhân vật.

## Asset animation

Bản demo sử dụng **robot được dựng và hoạt họa bằng asset Unity trong project**, không cần tải Mixamo hoặc đăng nhập dịch vụ ngoài. Rig hiện tại là **Generic**, với clip transform và Avatar Mask theo đường dẫn xương. Các clip Idle/Walk/Run/Jump/Fall/Land/Crouch/Attack/Hit/Dead, controller và mask đều có thể mở trong Editor.

Khu bên trái spawn bổ sung hai robot **Humanoid** với Avatar khác nhau, cùng controller và cùng các muscle clips Idle/Walk/Run. Robot đích có chân dài hơn và tay ngắn hơn để quan sát retargeting. Asset Avatar, clip và controller nằm trong `Animations/`. Không dùng asset Mixamo. Khi thay model nhân vật chơi, cần thay clip/controller tương thích với rig và giữ bridge/state gameplay hiện có.

## Công cụ và build

- `Training Arena/Generate Demo Scene` dựng lại demo bằng Editor API. **Thao tác này tạo lại scene/animation của demo; không chạy trên scene đã chỉnh tay mà chưa lưu bản sao.**
- `ProjectBootstrap.PackageInstaller.Install`: cài package bằng UPM Client API.
- `ProjectBootstrap.ArenaBuilder.BuildLinux`: tạo bản T3/T5 Linux dưới `Builds/`; binaries T3/T5 đã được lưu sẵn.
- `DemoVerifier`: kiểm tra tích hợp có chủ động bật; không gắn vào scene phát hành.
- Unity MCP đã là dependency project; bật server trong `Window → MCP for Unity` khi cần. Codex dùng URL local `http://localhost:8080/mcp`.
- Project hiện có uvx trong `Tools/uv/bin` (bỏ qua khỏi Git) để Unity MCP có thể tải/chạy server local trên máy đã cài project. Chạy Unity Hub project trước khi dùng công cụ MCP.

Xem [tuyến demo](Docs/DEMO-RUNBOOK.md), [cấu hình](Docs/CONFIG-CONTRACT.md) và [kết quả kiểm tra](Docs/BUILD-NOTES.md).
