# Tuyến chơi thử T3/T5

## T3 — điều khiển và camera

Mở `TrainingArena_T3.unity` rồi Play, hoặc chạy bản `Builds/T3/TrainingArena`.

1. WASD đi; giữ Shift chạy; Space nhảy. Đi thẳng tới cầu thang, lên platform rồi nhảy xuống để quan sát airborne/landing.
2. Di chuột để xoay camera; kiểm tra W luôn theo hướng nhìn. Esc thả chuột, Esc lần nữa khóa chuột.
3. Đi vào hành lang bên trái cầu thang, xoay camera sát tường. F4 so sánh xử lý vật cản; F3 so sánh damping.
4. C lần lượt chuyển TPS/FPS/top-down. FPS ẩn visual của người chơi.
5. Đi tới nền xanh ở góc phải phía xa: camera chuyển sang góc cố định. Nhấn C trong zone, rời zone để thấy mode mới đã được ghi nhớ.
6. F5 bật/tắt rung khi đáp mạnh; Backspace khôi phục vị trí, camera và các toggle.

## T5 — FSM và animation

Mở `TrainingArena.unity` rồi Play, hoặc chạy bản `Builds/T5/TrainingArena`.

1. Đi/chạy/nhảy để xem Idle/Walk/Run/Jump/Fall/Land cùng history. Animator dùng tốc độ dịch chuyển thực tế cho Blend Tree.
2. Giữ Ctrl rồi đi dưới thanh màu vàng bên trái. Thả Ctrl dưới thanh: nhân vật vẫn thấp; ra ngoài mới đứng lên. Crouch không cho nhảy.
3. Đứng sát dummy màu vàng ở bên phải spawn, quay mặt về dummy rồi click trái. Mỗi đòn trừ 25 HP; giữ chuột không tự đánh liên tục. Có thể đi/chạy trong khi đánh; không nhận Attack trên không.
4. Đứng trên nền đỏ để lần lượt nhận Hit và Dead. Khi chết, input di chuyển/nhảy/đánh bị khóa. Backspace khôi phục cả player và dummy.
5. Đi về phía trái spawn để xem hai robot Humanoid cùng controller/clip nhưng khác Avatar và chiều dài chi. Mở asset Avatar, clip HumanoidWalk và HumanoidRetarget.controller để kiểm tra retargeting.
6. F1 ẩn/hiện phím, F2 ẩn/hiện dữ liệu; F6 đổi đề tài và reset.

Các animation của nhân vật chơi là Generic tự tạo; khu Humanoid dùng muscle clips và Avatar riêng. Không cần tài khoản Mixamo hoặc asset tải ngoài.
