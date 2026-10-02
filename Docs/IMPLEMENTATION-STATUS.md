# Đối chiếu với plan T3/T5

Plan gốc: `../PLAN-TRIEN-KHAI-GAME-T3-T5.md` ở thư mục cha của project. Phạm vi sản phẩm là một training arena cho hai đề tài, không có slide.

| Nhóm | Triển khai |
|---|---|
| G0 | Unity 6000.3.23f1, URP, Input System, Cinemachine; Unity MCP và Pipeline; config asset, prefab, hai scene |
| G1 | Sân thử, bậc thang/platform, dốc, hành lang camera, thanh crouch, fixed zone, bẫy và dummy |
| G2 | CharacterController, camera-relative movement, đi/chạy/nhảy/gravity/ground check, chuẩn hóa input, reset và rơi khỏi map |
| G3 | TPS/FPS/top-down/fixed; Priority và blend; native Deoccluder/Impulse; damping, toggle, zone membership và reset |
| G4 | 10 Generic clips cho nhân vật chơi; Blend Tree; rig/visual tách motor; khu riêng gồm hai Humanoid Avatar và các clip dùng chung để minh họa retargeting |
| G5 | Lifecycle FSM; locomotion/action; Normal/Hit/Dead; bridge Animator; HUD logic, clip thực tế và history |
| G6 | Health, bẫy theo nhịp, dummy, Animation Events, một damage/target/đòn, upper-body mask, bước chân/landing, reset |
| G7 | Khu Humanoid và arena theo tuyến demo; không triển khai các khu so sánh P2 |
| G8 | Hai cấu hình scene/build; kiểm tra Play Mode và standalone; hướng dẫn chơi cùng bằng chứng kiểm tra trong Docs |

## Điều chỉnh asset so với plan

Thay cách nhập Mixamo bằng rig và animation tạo trực tiếp trong Unity để project không cần tài khoản/tải asset ngoài. Nhân vật điều khiển dùng Generic; Humanoid/retargeting được minh họa trong khu riêng. Đây là thay đổi so với giả định “nhân vật chính Humanoid” trong plan. Không có asset FBX import hoặc quy trình Configure Avatar từ FBX.

## Phần P2 không nằm trong bản chốt

Coyote time/jump buffer, khu nhân vật Rigidbody, khu Root Motion, strafe Blend Tree 2D. Có binding gamepad cho di chuyển/nhảy nhưng chưa xác nhận toàn bộ trải nghiệm gamepad; bàn phím/chuột là cách chơi chính.

Kết quả build và kiểm tra thực tế được ghi riêng trong `BUILD-NOTES.md`; không coi việc tạo script/asset là bằng chứng đã vượt kiểm tra.
