# Tuyến chơi thử T3/T5

Phím: WASD đi, Shift chạy, Space nhảy, Ctrl khom, chuột trái đánh, chuột xoay camera, C TPS ↔ góc rộng, H/K (DEBUG) Hit/Dead, R reset, F1 HUD, Esc nhả chuột, click để khóa lại.

## T3 — điều khiển và camera

Chạy `Builds/T3/TrainingArena` (hoặc scene `TrainingArena_T3.unity`).

1. **A:** W đi, giữ Shift chạy. Xoay chuột 90°: W đổi hướng theo camera. W+D không nhanh hơn W (xem Speed trên HUD).
2. **B:** chạy lên bậc thang tới platform. Giữ Space: chỉ nhảy một lần. Bấm Space trên không: không nhảy thêm. Nhảy xuống, tiếp đất ổn định.
3. **C:** vào hành lang bên trái, xoay camera về phía tường. Camera không nằm trong tường, vẫn thấy nhân vật.
4. Đứng yên, nhấn C: blend sang góc rộng, nhân vật không đổi vị trí. C lần nữa về TPS.
5. R: về A, camera TPS.

## T5 — FSM và animation

Chạy `Builds/T5/TrainingArena` (hoặc scene `TrainingArena.unity`).

1. **A:** đứng yên (Idle thở), đi, chạy: Blend Tree theo Speed thực tế. Đâm vào tường: Speed về 0, không chạy tại chỗ.
2. **B:** nhảy: Grounded → Airborne (Jump khi lên, Fall khi rơi) → Land → Grounded. Space trên không: HUD báo "Jump blocked in air".
3. **D:** H: HP −25, Hit ~0,7 s, khóa di chuyển, tự về Grounded. H trên không: HUD báo "H only works when Grounded".
4. K: Dead, giữ tư thế nằm, mọi phím gameplay bị chặn. R: HP 100, Grounded, về A.
5. (Thêm) Ctrl dưới thanh chắn vàng: thả Ctrl vẫn khom vì có trần, HUD báo "Ceiling: cannot stand up"; ra ngoài tự đứng dậy.
6. (Thêm) Đứng sát dummy vàng, quay mặt vào, click trái: tay phải bổ xuống, dummy −25 HP. Giữ chuột chỉ đánh một lần; vừa đi vừa đánh được; trên không bị chặn; H giữa đòn hủy đòn. R hồi dummy.
