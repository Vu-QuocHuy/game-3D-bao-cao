# Kết quả build và kiểm tra

- Unity 6000.3.23f1, build Linux x86_64 `Builds/TrainingArena/TrainingArena`: `Succeeded`.
- Kiểm tra tự động: `DemoVerifier` giả lập bàn phím, chuột và thao tác chạm trên joystick/nút ảo, rồi đọc lại PlayerController, FSM, Animator, xương Humanoid, camera và HUD. Kết quả **71/71** (`Verification.json`), gồm toàn bộ mục "Kiểm tra chức năng" ở mục 11 đặc tả, các bảng thiết lập (mục 4.3, 5, 7, 8) và phần mở rộng Crouch/Attack.
- Ảnh do verifier chụp trong lúc chạy: `VerifyShots/`.

## Chưa kiểm tự động

- Esc nhả/khóa con trỏ: `-batchmode` không khóa được con trỏ; verifier đọc chuột không qua cổng khóa. Cần thử tay.
- Device Simulator, độ phân giải Game view, Gizmos, layout Editor: thao tác trong Editor.
- Video dự phòng, ảnh minh họa slide, build Android: chưa làm.
