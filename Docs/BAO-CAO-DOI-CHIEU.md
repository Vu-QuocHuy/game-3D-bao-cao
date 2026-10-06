# Báo cáo đối chiếu với `dac-ta-game-demo-3d.md`

Ngày kiểm: 05/10/2026 · Unity 6000.3.23f1 · build Linux `Builds/TrainingArena/TrainingArena` · nhánh `feature/spec-game-demo-3d`.

## 1. Kết luận

**Phần dựng project và chức năng: đáp ứng đủ.** Mọi mục bắt buộc ở mục 3–9 và toàn bộ checklist "Kiểm tra chức năng" ở mục 11 đã được kiểm bằng chạy thật: **79/79 mục đạt** (71 mục đã đạt hai lần liên tiếp trước khi thêm góc rộng; 8 mục góc rộng chạy một lần) (`Verification.json`, ảnh trong `VerifyShots/`).

**Chưa hoàn thành (không thuộc phạm vi code, hoặc cần người làm tay):**

| # | Việc còn lại | Lý do |
|---|---|---|
| 1 | Phím Esc nhả/khóa con trỏ | `-batchmode` không khóa được con trỏ, verifier không kiểm được. Cần thử tay |
| 2 | Joystick ảo trong Device Simulator | Test mô phỏng thao tác chạm bằng code; chưa chạy trong Simulator thật của Editor |
| 3 | Mục "Chuẩn bị trình bày" ở mục 11 (layout Editor, cỡ chữ Editor, tắt thông báo) | Thao tác trong Editor và máy báo cáo |
| 4 | Video demo dự phòng, ảnh minh họa cho slide | Chưa quay/chụp. `VerifyShots/` là ảnh kiểm tra tự động, không phải ảnh slide |
| 5 | Mục 12: Capsule Rigidbody so sánh, build APK | Tùy chọn; chưa làm |
| 6 | Asset model | Model Humanoid dựng bằng khối cơ bản, clip tự tạo, không dùng Mixamo (xem mục 5) |

## 2. Đối chiếu từng mục đặc tả

Cột "Bằng chứng" ghi tên mục kiểm trong `Verification.json`.

### Mục 2 – Thiết lập project

| Yêu cầu | Trạng thái | Bằng chứng / ghi chú |
|---|---|---|
| Unity 2022 LTS hoặc Unity 6 | Đạt | Unity 6000.3.23f1 |
| Template 3D, URP | Đạt | URP 17.3 |
| Input System, Cinemachine, UI/TMP | Đạt | `Packages/manifest.json` |
| Active Input Handling = New | Đạt | `activeInputHandler: 1` |
| Device Simulator | Chưa kiểm | Mục 1 ở bảng trên |
| Game view 1920×1080 | Một phần | Cửa sổ build mặc định 1920×1080; Game view trong Editor chỉnh tay |

### Mục 3 – Bố cục scene

| Khu vực | Yêu cầu | Trạng thái | Đo được |
|---|---|---|---|
| Sân phẳng | ~40×40, họa tiết ô vuông | Đạt | Floor 40×40, ô 1 m |
| Bậc thang | 4–5 bậc, mỗi bậc ~0,2 m | Đạt | 5 bậc × 0,2 m; leo lên đỉnh 1,08 m, không nháy Fall |
| Bậc quá cao | ~0,6 m | Đạt | Bị chặn: dừng ở x = 3,70, y = 0,08 |
| Dốc thoải | ~30° | Đạt | Leo tới độ cao 1,76 m |
| Dốc quá dốc | ~60° | Đạt | Bị chặn: độ cao tối đa 0,08 m |
| Bục cao có mép | ~1,5 m, nhảy lên từ bậc thang | Đạt | Đứng ở y = 1,58; nhảy từ bậc thang lên được |
| Tường chắn camera | 2 Cube ~3 m tạo hành lang hẹp | Đạt | Camera kéo sát còn 0,78 m (bán kính 4 m) |
| Vật cản nhỏ | Tùy chọn | Đạt | 3 trụ Cylinder |
| Layer Environment | Sàn, bậc, dốc, tường | Đạt | layer 6; Deoccluder chỉ va layer này |
| Ánh sáng | Directional Light, bóng đổ | Đạt | |

### Mục 4 – Nhân vật

| Yêu cầu | Trạng thái | Bằng chứng / ghi chú |
|---|---|---|
| Hierarchy `Player/Model` + `Player/CameraTarget` (y 1,5) | Đạt | `4.1 Hierarchy…` |
| Model Humanoid, Avatar hợp lệ | Đạt | `4.2 Humanoid model, valid Avatar…` |
| Clip Idle/Walk/Run/Jump/Fall; Loop đúng | Đạt | Idle, Walk, Run, Fall loop; Jump không loop |
| Chuyển động xương thật (không đứng hình) | Đạt | Idle: cột sống 1,2°; Walk: đùi 46°, gối 31°, tay 38°; Run đùi 66° |
| Chân chạm sàn | Đạt | cổ chân thấp nhất −0,03 m |
| Character Controller: 1,8 / 0,3 / (0,0,9,0) / 0,3 / 45 / 0,08 | Đạt | `4.3 Character Controller values` |

### Mục 5 – Input

| Yêu cầu | Trạng thái | Bằng chứng / ghi chú |
|---|---|---|
| Move, Look, Jump, Walk, ToggleWorldMove | Đạt | `5 Player Input…` (kèm Crouch, Attack) |
| Binding bàn phím/chuột/tay cầm | Đạt | WASD, Mouse Delta, Space, Left Ctrl, T; Left/Right Stick, Button South |
| Joystick ảo (On-Screen Stick `<Gamepad>/leftStick`) | Đạt | Đẩy lên: nhân vật tiến 3,0 m, state Move |
| Nút nhảy (On-Screen Button `<Gamepad>/buttonSouth`) | Đạt | Nhảy được |
| Vùng kéo xoay camera (nửa phải) | Đạt | Kéo 160 px: camera xoay 80° |
| Kiểm tra trong Device Simulator | Chưa kiểm | Mục 2 ở bảng trên |

### Mục 6 – Script

| Yêu cầu | Trạng thái | Bằng chứng / ghi chú |
|---|---|---|
| Hướng đi theo camera, bỏ y, chuẩn hóa | Đạt | W đi đúng hướng camera (dot 1,00), kể cả sau khi xoay 90° |
| Phím T đảo chế độ hướng đi | Đạt | Camera nhìn +X, W vẫn đi +Z; HUD đổi `World axes` |
| Tốc độ 5 (chạy) / 2 (Ctrl) m/s | Đạt | 5,00 / 2,00 |
| Đi chéo không nhanh hơn | Đạt | W 5,00; W+D 5,00 (đo trên sàn trống) |
| Trọng lực −20, chạm đất giữ −2 | Đạt | VelocityY khi đứng −2,33 (đã cộng gia tốc 1 khung) |
| Nhảy chỉ khi chạm đất; cao 1,2 m | Đạt | Giữ Space nhảy 1 lần; đỉnh 1,14 m (lệch do lấy mẫu theo khung hình); nhảy trên không bị chặn |
| Xoay nhân vật 720°/s | Đạt | Cấu hình `turnSpeed = 720`; hướng nhìn khớp hướng đi |
| `Move` đúng một lần mỗi khung | Đạt | Mã nguồn `PlayerController.Update` (hai lệnh `Move` phụ chỉ là bám sàn khi xuống bậc) |
| Công bố InputMagnitude, HorizontalSpeed, VelocityY, IsGrounded, JumpPressed | Đạt | |
| `IState`, `StateMachine`, Idle/Move/Jump/Fall | Đạt | Chuỗi `Idle > Move > Jump > Fall > Move > Idle` |
| Thứ tự cập nhật (PlayerController trước, StateMachine sau) | Đạt | Execution Order 0 → 10 → 20 → 30 |
| `AnimatorBridge`, Speed chuẩn hóa 0–1 | Đạt | walk 0,50, run 1,00 |
| `DebugHUD`: State, Speed, Grounded, VelocityY, Move mode | Đạt | HUD khớp FSM ở mọi mẫu; cỡ chữ 34 |

### Mục 7 – Camera

| Yêu cầu | Trạng thái | Bằng chứng / ghi chú |
|---|---|---|
| Cinemachine Brain trên Main Camera | Đạt | |
| Tracking Target = CameraTarget; Orbital Follow | Đạt | |
| Distance ~4 m, damping 0,1–0,3 | Đạt | 4 m; 0,2 |
| Xoay bằng action Look | Đạt | Chuột và nửa phải màn hình |
| Deoccluder, layer Environment, radius ~0,2 | Đạt | Camera không xuyên tường, không bị che (`11 Camera moves closer…`) |
| Khóa/ẩn con trỏ, Esc mở lại | Chưa kiểm tự động | Có trong mã (`DemoController`); cần thử tay |

### Mục 8 – Animator Controller

| Yêu cầu | Trạng thái | Bằng chứng / ghi chú |
|---|---|---|
| Parameter `Speed` (float), `IsGrounded` (bool), `Jump` (trigger) | Đạt | |
| Locomotion: Blend Tree 1D Idle 0 / Walk 0,5 / Run 1 | Đạt | Speed tăng đơn điệu, 13 mẫu trung gian |
| State Jump, Fall; Locomotion mặc định | Đạt | |
| Locomotion → Jump (trigger) | Đạt | |
| Jump → Fall | Đạt, khác chi tiết | Dùng Exit Time 0,7 thay cho `IsGrounded = false` (đặc tả cho phép "hoặc Exit Time ngắn"), vì `IsGrounded` đã false ngay lúc bật nhảy nên điều kiện đó sẽ cắt clip Jump |
| Locomotion → Fall, Fall → Locomotion | Đạt | Bước khỏi mép bục: vào Fall; chạm đất về Idle |
| Root Motion tắt | Đạt | |

### Mục 9 – Chức năng riêng cho demo

| Yêu cầu | Trạng thái | Bằng chứng / ghi chú |
|---|---|---|
| Phím T | Đạt | |
| Tắt/bật Animator khi Play | Đạt | Tắt Animator: đi 4,00 m, chân không cử động (0,0°) |
| Hiển thị tên trạng thái | Đạt | |
| Phím R | Đạt | Về xuất phát, Idle, hồi dummy, về chế độ camera-relative |
| Hiển thị viên nang | Chưa kiểm | Bật Gizmos trong Editor |

### Mục 10 – Kịch bản demo

Các bước Thứ Ba và Thứ Năm đều có điểm trong scene và đã kiểm bằng cách tương ứng ở trên. Chi tiết thao tác: `DEMO-RUNBOOK.md`. Riêng bước "kéo joystick nhẹ" cho Walk: đẩy nửa cần cho `Speed` 0,57 (2,34 m/s).

### Mục 11 – Checklist

**Dựng scene:** 3/4 đạt. Còn lại: joystick trong Device Simulator.

**Kiểm tra chức năng (8/8 đạt):**

| Mục | Kết quả |
|---|---|
| W luôn đi về phía trước màn hình khi camera xoay | Đạt |
| T đảo chế độ hướng đi, HUD đổi theo | Đạt |
| Nhảy chỉ khi chạm đất | Đạt |
| Bước khỏi mép bục: vào Fall | Đạt |
| Bậc 0,2 m leo được, 0,6 m bị chặn; dốc 30° leo được, 60° bị chặn | Đạt |
| Camera tiến lại gần khi tường chắn | Đạt |
| Blend Tree mượt Idle → Walk → Run | Đạt |
| HUD đúng tên trạng thái trong mọi chuyển | Đạt |

**Chuẩn bị trình bày:** 0/5, toàn bộ là việc làm tay (layout Editor, cỡ chữ Editor, video dự phòng, tắt thông báo, ảnh slide).

### Mục 12 – Mở rộng

| Mở rộng | Trạng thái |
|---|---|
| Animation Event (bước chân) | Đạt: `Footstep` trên Walk/Run; âm thanh `Audio/Footstep.wav` |
| State Attack | Đạt: thêm không sửa 4 state gốc |
| Capsule Rigidbody so sánh | Chưa làm |
| Build APK | Chưa làm |

## 3. Phần thêm ngoài đặc tả (theo yêu cầu: giữ Crouch, Attack và góc rộng)

| Tính năng | Hành vi | Kiểm |
|---|---|---|
| Crouch (phím **C**; Ctrl dành cho Walk) | Khom 1,6 m/s, cấm nhảy; dưới thanh chắn thả C vẫn giữ thấp; camera hạ theo, không kẹt trong thanh | 9 mục |
| Góc rộng (phím **V**, khôi phục từ bản trước) | Đổi third person ↔ camera cao 10 m phía sau, nghiêng 45°, có blend 0,5 s; không đổi vị trí nhân vật; R về third person; HUD dòng `Camera:` | 8 mục |
| Attack (chuột trái) | Event mở/đóng hitbox; 0,85 s, cooldown 1 s, −25 HP dummy; giữ chuột chỉ đánh một lần; đi được khi đánh; chặn trên không | 8 mục |

Hai tính năng không ảnh hưởng 4 state gốc: Idle/Move/Jump/Fall giữ nguyên điều kiện chuyển ở mục 6.2.

## 4. Khác biệt có chủ đích so với đặc tả

1. Crouch dùng **C**, không dùng Ctrl (Ctrl = Walk theo mục 5).
2. Jump → Fall dùng Exit Time (mục 8 ở trên).
3. Layer Upper Body (Attack) có weight 0 khi không đánh và được `AnimatorBridge` bật khi đang đánh; layer Humanoid dù ở state rỗng vẫn ghi đè thân trên và làm đơ tay (lỗi đã gặp và sửa khi test).
4. Khi xuống bậc 0,2 m, `PlayerController` thêm một bước bám sàn trong tầm Step Offset để không nháy sang Fall; đặc tả không nói đến chi tiết này.

## 5. Rủi ro và giới hạn của chính bộ kiểm

- Verifier chạy `-batchmode`: không khóa được con trỏ, nên chuột (look, attack) được đọc không qua cổng khóa. Logic khóa/nhả chưa được kiểm.
- Giả lập đầu vào: bàn phím, chuột, tương tác chạm với On-Screen Stick/Button bằng `PointerEventData`, không phải màn hình cảm ứng thật.
- Một vài ngưỡng có dung sai (độ cao nhảy 1,2 ± 0,15 m; tốc độ ± 0,6 m/s) vì lấy mẫu theo khung hình.
- Model là các khối cơ bản; hình ảnh minh họa chưa phải ảnh slide. Nếu cần model nhân vật đẹp (Mixamo, Starter Assets), thay Avatar và clip, giữ nguyên PlayerController/AnimatorBridge/Animator parameter.
- Build chỉ Linux x86_64; máy báo cáo Windows cần build lại (module Windows Build Support).
- `Docs/Diagrams.puml` chưa được render thử (máy không có PlantUML/Java); cú pháp đã rà thủ công.

## 6. Cách tái kiểm

```bash
Unity -batchmode -nographics -projectPath . -executeMethod ProjectBootstrap.DemoSceneBuilder.BuildAll
Builds/TrainingArena/TrainingArena -batchmode --verify-demo --verification-report out.json --verification-shots shots/
```

Exit code 0 khi mọi mục đạt; `out.json` liệt kê từng mục và số đo.
