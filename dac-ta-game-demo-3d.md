# ĐẶC TẢ GAME MÔ PHỎNG 3D DÙNG CHO DEMO
## Hai buổi báo cáo: (Thứ Ba) Điều khiển nhân vật và camera · (Thứ Năm) Trạng thái và animation

Tài liệu mô tả những gì cần có trong **một project Unity duy nhất** để demo đầy đủ nội dung của cả hai buổi. Project dựng một lần, dùng lại cho cả hai ngày. Các mục được đánh dấu **[Thứ Ba]** và **[Thứ Năm]** cho biết phần nào phục vụ buổi nào.

---

## 1. Mục tiêu của bản demo

Một scene thử nghiệm nhỏ (không cần cốt truyện hay màn chơi hoàn chỉnh), trong đó một nhân vật 3D có thể:

| Buổi | Nội dung cần chứng minh được trong demo |
|---|---|
| Thứ Ba | Di chuyển bằng Character Controller · hướng đi theo camera · trọng lực và nhảy · leo bậc thang, dốc · camera third person bám theo và tránh vật cản · điều khiển bằng joystick ảo |
| Thứ Năm | Có 4 trạng thái (Idle, Move, Jump, Fall) · hiển thị tên trạng thái hiện tại · animation Idle/Walk/Run/Jump/Fall · Blend Tree theo `Speed` · Animator Controller hoạt động theo parameter |

**Nguyên tắc dựng:** ưu tiên *dễ nhìn thấy* hơn *đẹp*. Mỗi nội dung trong slide phải có một điểm trong scene để chỉ vào và quan sát được ngay.

---

## 2. Thiết lập project

| Mục | Yêu cầu |
|---|---|
| Phiên bản Unity | Unity 2022 LTS hoặc Unity 6; chọn một phiên bản và thống nhất cho cả hai buổi |
| Template | 3D (Built-in hoặc URP đều được; URP thường nhẹ hơn khi chạy trên Android) |
| Gói cần cài | **Input System**, **Cinemachine**, **Unity UI** (hoặc TextMeshPro) |
| Input handling | Project Settings → Player → *Active Input Handling*: **Input System Package (New)** (hoặc *Both* nếu có script cũ) |
| Nền tảng | Có thể chạy trong Editor; để kiểm tra giao diện cảm ứng bật **Device Simulator** (Game view → chọn Simulator). Nếu muốn trình bày bản chạy thật thì Build ra Android |
| Độ phân giải Game view | Đặt cố định (ví dụ 1920×1080) để hình ảnh khi chia sẻ màn hình ổn định |

---

## 3. Bố cục scene

Một khu thử nghiệm nằm trên mặt phẳng lớn, gồm các khu vực nhỏ, mỗi khu phục vụ một nội dung trong slide. Mọi khối đều có thể dựng từ các khối cơ bản (Cube, Plane, Cylinder) của Unity.

```
        [Tường chắn camera]
              ||
   [Dốc thoải]   [Dốc quá dốc]
        \            /
 [Bậc thang]---[ VỊ TRÍ XUẤT PHÁT ]---[Bục cao có mép]
                      |
                [Sân phẳng rộng]
```

| Khu vực | Cách dựng | Phục vụ nội dung |
|---|---|---|
| **Sân phẳng rộng** | Plane hoặc Cube lớn, kích thước khoảng 40×40, gán vật liệu có họa tiết ô vuông để thấy rõ chuyển động | Di chuyển, xoay nhân vật, hướng đi theo camera [Thứ Ba] |
| **Bậc thang** | 4–5 Cube xếp liên tiếp, mỗi bậc cao khoảng **0.2 m** (thấp hơn *Step Offset* = 0.3) | Step Offset [Thứ Ba] |
| **Một bậc quá cao** | Một Cube cao khoảng **0.6 m** (cao hơn Step Offset) | Chứng minh bậc cao quá thì bị chặn [Thứ Ba] |
| **Dốc thoải** | Cube xoay quanh trục X khoảng **30°** (nhỏ hơn *Slope Limit* = 45) | Slope Limit, nhân vật leo được [Thứ Ba] |
| **Dốc quá dốc** | Cube xoay khoảng **60°** (lớn hơn Slope Limit) | Nhân vật bị chặn / trượt xuống [Thứ Ba] |
| **Bục cao có mép** | Cube lớn cao khoảng **1.5 m**, đủ rộng để đứng; có thể nhảy lên từ bậc thang | Rời mép bục, trọng lực, trạng thái Fall [Thứ Ba, Thứ Năm] |
| **Tường chắn camera** | Hai Cube mỏng, cao khoảng 3 m, đặt gần nhau để tạo một hành lang hẹp | Camera tránh vật cản (Deoccluder) [Thứ Ba] |
| **Vật cản nhỏ** (tùy chọn) | Vài Cube, Cylinder rải rác | Tạo cảm giác không gian, kiểm tra va chạm |

**Layer:** đặt các khối môi trường (sàn, bậc, dốc, tường) vào layer **Environment**. Bộ phận tránh vật cản của camera chỉ cần kiểm tra layer này (không kiểm tra Player), tránh việc camera tự va vào nhân vật.

**Ánh sáng:** một Directional Light, bóng đổ bật, đủ để thấy rõ nhân vật và địa hình khi quay hình.

---

## 4. Nhân vật

### 4.1. Cấu trúc đối tượng (Hierarchy)

```
Player                         ← Character Controller, Player Input, PlayerController, StateMachine
 ├─ Model                      ← model nhân vật Humanoid + Animator
 └─ CameraTarget               ← đối tượng rỗng, độ cao vai/đầu (khoảng y = 1.5)
CinemachineCamera              ← Tracking Target: CameraTarget
Main Camera                    ← Cinemachine Brain
Canvas                         ← HUD + joystick ảo + nút nhảy
EventSystem
```

### 4.2. Model và animation

| Mục | Yêu cầu |
|---|---|
| Model | Một model nhân vật dạng Humanoid (tải từ Mixamo, Asset Store hoặc dùng Starter Assets). Rig → Animation Type: **Humanoid** |
| Clip cần có | **Idle, Walk, Run, Jump (khởi nhảy), Fall (rơi/lơ lửng)**. Có thể thêm Land (chạm đất) nếu muốn đẹp, không bắt buộc |
| Loop Time | Bật cho Idle, Walk, Run, Fall; tắt cho Jump |
| Model dự phòng | Nếu chưa có model, dùng Capsule để demo phần thứ Ba; phần thứ Năm bắt buộc có model Humanoid |

### 4.3. Character Controller (Inspector của Player)

| Thông số | Giá trị gợi ý | Ghi chú |
|---|---|---|
| Height | 1.8 | Khớp chiều cao model |
| Radius | 0.3 | |
| Center | (0, 0.9, 0) | Đặt viên nang vừa với model |
| Step Offset | 0.3 | Bậc 0.2 m leo được, bậc 0.6 m bị chặn |
| Slope Limit | 45 | Dốc 30° leo được, 60° bị chặn |
| Skin Width | 0.08 | Giữ mặc định, không đặt quá nhỏ |

---

## 5. Đầu vào (Input Actions)

Tạo một Input Action Asset (ví dụ `PlayerControls`) với các hành động sau.

| Action | Kiểu | Nguồn (binding) | Ghi chú |
|---|---|---|---|
| **Move** | Value / Vector2 | WASD; Left Stick (tay cầm); **On-Screen Stick** | Giá trị x, y trong khoảng −1 đến 1 |
| **Look** | Value / Vector2 | Mouse Delta; Right Stick; vùng kéo trên màn hình cảm ứng | Điều khiển xoay camera |
| **Jump** | Button | Space; Button South (tay cầm); **On-Screen Button** | |
| **Walk** (tùy chọn) | Button (giữ) | Left Ctrl | Giữ để đi bộ chậm, dùng cho demo Blend Tree |
| **ToggleWorldMove** | Button | Phím **T** | Chuyển giữa "theo camera" và "theo trục thế giới" (chỉ để demo) |

**Giao diện cảm ứng (Canvas, chế độ Screen Space – Overlay):**
- Joystick ảo ở góc dưới bên trái, gắn **On-Screen Stick**, *Control Path* = `<Gamepad>/leftStick`
- Nút nhảy ở góc dưới bên phải, gắn **On-Screen Button**, *Control Path* = `<Gamepad>/buttonSouth`
- Vùng nửa màn hình bên phải (trừ nút nhảy) dùng để kéo xoay camera, gắn **On-Screen Stick** thứ hai cho *Look* hoặc xử lý riêng
- Cả hai được kiểm tra trong Device Simulator

---

## 6. Script cần có

### 6.1. `PlayerController` — [Thứ Ba] và nền tảng cho [Thứ Năm]

Trách nhiệm: đọc đầu vào, tính vận tốc, gọi `CharacterController.Move`, xoay nhân vật và công bố dữ liệu cho các thành phần khác.

| Chức năng | Mô tả |
|---|---|
| Đọc `Move`, `Jump`, `Walk`, `ToggleWorldMove` | Qua Input System |
| Hướng đi theo camera | `forward`/`right` của Main Camera, bỏ thành phần y, chuẩn hóa; `dir = f * input.y + r * input.x` |
| Chế độ đối chiếu | Biến `moveRelativeToCamera`; phím **T** đảo giá trị để demo sự khác biệt so với trục thế giới |
| Vận tốc ngang | `dir * speed` (speed = 5 khi chạy, 2 khi giữ Walk) |
| Trọng lực | `gravity = -20` (nhanh hơn -9.81 cho cảm giác nhảy gọn); khi chạm đất giữ `velocityY = -2` |
| Nhảy | Chỉ khi `isGrounded`; `velocityY = sqrt(jumpHeight * -2 * gravity)`, `jumpHeight = 1.2` |
| Xoay nhân vật | Xoay dần về hướng đi bằng `Quaternion.RotateTowards`/`Slerp`, tốc độ khoảng 720°/giây |
| Công bố dữ liệu | Thuộc tính công khai: `InputMagnitude`, `HorizontalSpeed`, `VelocityY`, `IsGrounded`, `JumpPressed` |
| Gọi `Move` | Mỗi khung hình đúng một lần: `controller.Move((dir*speed + Vector3.up*velocityY) * Time.deltaTime)` |

### 6.2. `StateMachine` và các trạng thái — [Thứ Năm]

| Thành phần | Mô tả |
|---|---|
| `IState` | Giao diện với `Enter()`, `Tick()`, `Exit()` |
| `StateMachine` | Giữ `current`, có `ChangeState(next)` (gọi `Exit` → gán → `Enter`) và `Update()` gọi `Tick` |
| `IdleState` | Enter: `animator.SetFloat("Speed", 0)`. Tick: nếu có đầu vào → Move; nếu nhấn nhảy khi chạm đất → Jump; nếu `!IsGrounded` → Fall |
| `MoveState` | Tick: cập nhật `Speed`; nếu hết đầu vào → Idle; nếu nhấn nhảy khi chạm đất → Jump; nếu `!IsGrounded` → Fall |
| `JumpState` | Enter: `animator.SetTrigger("Jump")`. Tick: nếu `VelocityY <= 0` → Fall |
| `FallState` | Enter: `animator.SetBool("IsGrounded", false)`. Tick: nếu `IsGrounded` → Idle hoặc Move tùy còn đầu vào hay không |

**Lưu ý thứ tự cập nhật:** `PlayerController` cập nhật dữ liệu trước, `StateMachine.Update()` đọc dữ liệu đó sau (ví dụ gọi `StateMachine.Update()` ở cuối `Update` của PlayerController, hoặc đặt *Script Execution Order*).

### 6.3. `AnimatorBridge` (tùy chọn) — [Thứ Năm]

Nếu muốn tách gọn, một script nhỏ nhận dữ liệu từ `PlayerController` và gán parameter cho Animator:

```csharp
animator.SetFloat("Speed", horizontalSpeedNormalized, 0.1f, Time.deltaTime);
animator.SetBool("IsGrounded", controller.isGrounded);
```

`Speed` được chuẩn hóa về khoảng 0–1 (0 = đứng, 0.5 = đi bộ, 1 = chạy).

### 6.4. `DebugHUD` — [Thứ Ba, Thứ Năm]

Một Text (TextMeshPro) góc trên bên trái màn hình, cập nhật mỗi khung hình:

```
State: Run
Speed: 5.0 m/s
Grounded: True
VelocityY: -2.0
Move mode: Camera-relative
```

- Dòng `State` chỉ có ý nghĩa ở buổi Thứ Năm; ẩn bớt các dòng không cần khi quay hình.
- Cỡ chữ đủ lớn để đọc được khi chia sẻ màn hình trực tuyến.

---

## 7. Camera (Cinemachine)

| Thành phần | Thiết lập |
|---|---|
| **Main Camera** | Gắn *Cinemachine Brain* |
| **CinemachineCamera** | *Tracking Target* = `CameraTarget`; chế độ third person (Third Person Follow hoặc Orbital Follow tùy phiên bản) |
| Khoảng cách | Distance khoảng **4 m**; điều chỉnh để nhìn thấy nhân vật và phía trước |
| Damping | Khoảng 0.1–0.3 s để camera bám mượt, không bám cứng |
| Xoay camera | Dùng action *Look*: kéo chuột hoặc kéo ngón tay để xoay quanh nhân vật (Input Axis Controller với Cinemachine 3.x) |
| **Tránh vật cản** | Thêm *Cinemachine Deoccluder* (3.x) hoặc *Cinemachine Collider* (2.x); *Collide Against* = layer **Environment**; *Camera Radius* khoảng 0.2 |
| Con trỏ chuột | Khóa và ẩn con trỏ khi Play (`Cursor.lockState = CursorLockMode.Locked`) để xoay camera mượt; phím Esc mở lại con trỏ |

---

## 8. Animator Controller

### 8.1. Parameter

| Tên | Kiểu | Ý nghĩa |
|---|---|---|
| `Speed` | Float | 0 (đứng) – 0.5 (đi bộ) – 1 (chạy) |
| `IsGrounded` | Bool | Có chạm đất hay không |
| `Jump` | Trigger | Khởi động động tác nhảy |

### 8.2. State và Transition

| State | Nội dung | Ghi chú |
|---|---|---|
| **Locomotion** | Blend Tree 1D theo `Speed`: Idle (0), Walk (0.5), Run (1) | State mặc định |
| **Jump** | Clip Jump | |
| **Fall** | Clip Fall (lặp) | |

| Chuyển | Điều kiện | Has Exit Time | Transition Duration |
|---|---|---|---|
| Locomotion → Jump | `Jump` (Trigger) | Tắt | khoảng 0.05 s |
| Jump → Fall | `IsGrounded = false` (hoặc Exit Time ngắn) | Tắt | khoảng 0.1 s |
| Locomotion → Fall | `IsGrounded = false` | Tắt | khoảng 0.1 s |
| Fall → Locomotion | `IsGrounded = true` | Tắt | khoảng 0.1 s |

**Cài đặt Animator component:** *Apply Root Motion* = **tắt**.

---

## 9. Chức năng phục vụ riêng cho demo

| Chức năng | Cách thực hiện | Dùng ở slide |
|---|---|---|
| Đảo chế độ hướng đi | Phím **T** (`moveRelativeToCamera`) | Thứ Ba, slide 12–13 |
| Tắt/bật animation | Bỏ chọn/chọn component **Animator** ngay trong Inspector khi đang Play | Thứ Năm, slide 13 |
| Hiển thị tên trạng thái | `DebugHUD` | Thứ Năm, slide 13 |
| Đặt lại vị trí nhân vật | Phím **R** đưa Player về điểm xuất phát (tránh mất thời gian khi nhân vật rơi hoặc kẹt) | Cả hai buổi |
| Hiển thị viên nang | Bật *Gizmos* trong Scene view hoặc cửa sổ Scene đặt cạnh Game view | Thứ Ba, slide 5 |

---

## 10. Đối chiếu demo với kịch bản trong slide

### Thứ Ba (khoảng 2 phút 10 giây)

| Bước trong kịch bản | Khu vực / chức năng cần có |
|---|---|
| Chỉ các component trên Player | Player với Character Controller, Player Input, PlayerController |
| Di chuyển, xoay camera, so với trục thế giới | Sân phẳng rộng, phím **T**, `DebugHUD` dòng *Move mode* |
| Nhảy, rơi khỏi mép bục | Bục cao có mép, `isGrounded`, trọng lực |
| Bậc thang, dốc | Bậc 0.2 m, bậc 0.6 m, dốc 30°, dốc 60° |
| Camera tránh vật cản | Hành lang tường hẹp, Deoccluder bật |
| Joystick ảo | Canvas với On-Screen Stick, Device Simulator |

### Thứ Năm (khoảng 2 phút)

| Bước trong kịch bản | Khu vực / chức năng cần có |
|---|---|
| Tắt Animator để thấy nhân vật trượt | Component Animator có thể tắt khi Play |
| Mở Animator Controller, Blend Tree | Animator Controller đã dựng như mục 8 |
| Đứng → đi → chạy | Phím giữ **Ctrl** để đi bộ, hoặc kéo joystick nhẹ |
| Nhảy, rơi khỏi mép bục | Bục cao có mép, state Jump và Fall |
| Hiển thị tên trạng thái | `DebugHUD` dòng *State* |

---

## 11. Danh sách kiểm tra trước buổi báo cáo

**Dựng scene**
- [ ] Scene đủ các khu vực ở mục 3, đã phân layer Environment
- [ ] Nhân vật Humanoid có Animator, Avatar hợp lệ (dấu tích xanh)
- [ ] Input Actions đầy đủ, đã gán vào Player Input
- [ ] Canvas có joystick ảo và nút nhảy hoạt động trong Device Simulator

**Kiểm tra chức năng**
- [ ] Nhấn W luôn đi về phía trước màn hình, dù camera xoay hướng nào
- [ ] Phím T đảo được chế độ hướng đi, `DebugHUD` đổi theo
- [ ] Nhảy chỉ được khi chạm đất; không nhảy được khi đang ở trên không
- [ ] Bước khỏi mép bục: nhân vật rơi và vào trạng thái Fall
- [ ] Bậc 0.2 m leo được, bậc 0.6 m bị chặn; dốc 30° leo được, dốc 60° bị chặn
- [ ] Camera tiến lại gần khi tường nằm giữa camera và nhân vật
- [ ] Blend Tree: `Speed` tăng thì chuyển mượt Idle → Walk → Run
- [ ] `DebugHUD` hiển thị đúng tên trạng thái trong mọi chuyển trạng thái

**Chuẩn bị trình bày**
- [ ] Layout Unity cố định: Game view (lớn), Animator (cạnh bên), Inspector (khi cần)
- [ ] Phóng to chữ trong Unity Editor (Preferences) để người xem trực tuyến đọc được
- [ ] Đã quay sẵn video demo từng buổi làm phương án dự phòng
- [ ] Đã tắt thông báo hệ thống trước khi chia sẻ màn hình
- [ ] Đã chụp sẵn các ảnh minh họa cho slide (Inspector, Hierarchy, Animator, Input Actions...) từ chính project này, theo mô tả trong hai file báo cáo

---

## 12. Mở rộng (chỉ làm khi còn thời gian)

- Thêm hiệu ứng bụi hoặc âm thanh bước chân bằng **Animation Event** để minh họa nội dung Animation Event.
- Thêm trạng thái **Attack** để minh họa việc mở rộng State Machine mà không phải sửa các trạng thái cũ.
- Thêm một Capsule thứ hai dùng **Rigidbody** để so sánh trực quan với Character Controller ở slide 4 buổi Thứ Ba.
- Build APK và chạy trên điện thoại thật để trình bày bản chạy trên Android.
