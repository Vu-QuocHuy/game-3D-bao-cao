# Kịch bản demo (mục 10 đặc tả)

## Thứ Ba: điều khiển và camera (~2:10)

1. Chỉ Inspector của **Player**: Character Controller, Player Input, PlayerController, PlayerStateMachine. Bật Gizmos để thấy viên nang.
2. **Sân rộng:** W đi về phía trước màn hình; xoay chuột, W vẫn đi theo camera.
3. **Bục cao:** lên bậc thang bên trái (bậc 0,2 m < Step Offset 0,3), nhảy lên bục 1,5 m, bước ra mép: rơi, `Grounded: False`, VelocityY âm dần.
4. **Bậc 0,6 m** bên phải: bị chặn. **Dốc 30°**: leo được. **Dốc 60°**: bị chặn (Slope Limit 45).
5. **Hành lang tường:** đứng giữa hành lang, xoay camera về phía tường: camera tiến sát nhân vật thay vì xuyên tường (Deoccluder).
6. **Đổi góc camera:** đứng yên, nhấn **T**: third person → first person (ẩn model) → top-down (thấy cả khu thử nghiệm) → third person; có blend, nhân vật không dịch chuyển. R luôn về third person.
7. **Joystick ảo:** Game view → Simulator, kéo joystick trái, nhấn JUMP, kéo nửa phải để xoay camera.

## Thứ Năm: trạng thái và animation (~2:00)

1. Khi đang Play, bỏ chọn **Animator** trên `Player/Model` rồi đi: nhân vật trượt, không cử động. Bật lại.
2. Mở `Animator Controller`, Blend Tree Locomotion: Idle 0, Walk 0,5, Run 1.
3. Đứng → giữ **Ctrl** đi bộ (`Speed` 0,5) → thả Ctrl chạy (`Speed` 1); hoặc đẩy joystick nhẹ rồi mạnh.
4. Nhảy và bước khỏi mép bục: HUD `State: Jump` → `Fall` → `Idle`.
5. (Mở rộng) **C** khom: `State: Crouch`; dưới thanh chắn thả C vẫn khom. Chuột trái gần dummy: `State: Attack`, dummy −25 HP; state mới thêm mà không sửa Idle/Move/Jump/Fall.

R đưa nhân vật về điểm xuất phát bất cứ lúc nào.
