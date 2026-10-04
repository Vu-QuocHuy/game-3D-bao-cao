# Đặc tả demo Unity dùng chung cho T3 và T5

**Bộ tài liệu:** `01-khung-bao-cao-T3.md`, `02-khung-bao-cao-T5.md`, tài liệu này.  
**Lịch hiểu theo ngày lập 03/10/2026:** T3 là 06/10/2026; T5 là 08/10/2026.  
**Trạng thái:** thiết kế đề xuất để nhóm thực hiện, chưa có source project, asset hay kết quả chạy thử được cung cấp.

## 1. Mục tiêu và phạm vi

Tạo một sân tập 3D nhỏ tên **Training Arena**, một nhân vật người chơi, một motor, một camera rig. T3 giải thích nhân vật di chuyển và camera hoạt động ra sao. T5 giải thích quy tắc hành vi và cách animation phản ánh chúng. Không cần xây hai game hoặc hai scene riêng.

Đầu ra cho mỗi buổi là một bài nói 10 phút gồm **9 slide chính**, 2:30 demo nằm trong 9:20 nội dung và 0:40 đệm. Khi tạo PPTX sau này có thể thêm 3 slide phụ lục ẩn mỗi file; tổng 12 slide/file, chỉ trình chiếu 9 slide. Bước hiện tại chỉ tạo tài liệu MD.

**Giả định làm việc:** báo cáo cho lớp đã biết cơ bản Unity; dùng máy Windows với bàn phím/chuột; hỏi đáp ngoài 10 phút. Chưa biết số thành viên, phiên bản project, asset đang có và mẫu slide của khoa. Các giả định này không làm thay đổi mạch báo cáo; nhóm điền thông tin thực tế trước khi dựng PPTX.

### Những thay đổi so với khung MD gốc

| Điểm trong khung gốc | Điều chỉnh | Lý do |
| --- | --- | --- |
| T3: 4 góc camera, đổi motor, shake, trigger camera | TPS + góc rộng, một CharacterController | Đủ minh họa chủ đề, giảm tính năng và rủi ro live |
| T5: nhiều state, ngồi, đánh, layers, events | 4 state chính, Blend Tree, Hit và Dead | Có thời gian giải thích quy tắc và chứng minh |
| T5: “một nhân vật = một trạng thái” rồi vừa chạy vừa đánh | Một state trong FSM chính của demo | Tránh khái quát sai về hành vi song song |
| Root Motion bật/tắt theo state | Clip in-place, tắt Root Motion xuyên suốt | Giữ quyền điều khiển vị trí nhất quán với T3 |
| Bảng thời lượng T5 cộng thành 10:20 | 9:20 nội dung + 0:40 đệm | Khớp giới hạn 10 phút |
| “Lỗi nhóm đã gặp” khi chưa có dữ liệu | “Lỗi có thể gặp”, ghi kết quả thật sau test | Không biến giả định thành trải nghiệm đã xảy ra |
| Hướng dẫn cài plugin dựng slide | Bỏ khỏi báo cáo, giữ quy tắc bàn giao PPTX | Tập trung nội dung và demo mà người dùng yêu cầu |

## 2. Cấu hình đề xuất

- Giữ phiên bản Unity đang chạy ổn của nhóm. Nếu bắt đầu mới, có thể dùng Unity 6 với Input System và Cinemachine 3.1 tương thích. Ghi số phiên bản thực tế, không nâng package ngay trước báo cáo.
- Thuật ngữ camera trong hai khung theo Cinemachine 3.1: Cinemachine Camera, Brain, Decollider, Deoccluder. Nếu project dùng 2.x, sửa tên component và ảnh chụp theo phiên bản; không trộn hướng dẫn 2.x và 3.x.
- Nhân vật dùng CharacterController; code tính vận tốc ngang/dọc, Animator không điều khiển vị trí gốc.
- Model Humanoid và các clip tương thích: Idle, Walk, Run, Jump hoặc Jump/Fall, Hit, Death. Ưu tiên bộ asset nhóm đã có quyền sử dụng, ghi nguồn và điều kiện sử dụng vào credits.
- Dùng clip in-place, Apply Root Motion tắt. Collider và kích thước model phải tương ứng; tránh chân nổi hoặc lún.
- Không bắt buộc gamepad, âm thanh, nhân vật địch, chiến đấu, ngồi, kho đồ hay menu chính.

## 3. Scene duy nhất và tuyến demo

| Khu vực | Thành phần tối thiểu | Dùng cho buổi nào |
| --- | --- | --- |
| A: điểm xuất phát | Sàn rộng, ký hiệu hướng, điểm reset | Cả hai; đứng/đi/chạy, quan sát state |
| B: thử nhảy | Bục thấp và bậc/dốc nhẹ có collider | T3: nhảy/va chạm; T5: Airborne/Grounded |
| C: thử camera | Tường và hành lang vừa đủ cho nhân vật đi | T3: camera và vật cản |
| D: kiểm thử trạng thái | Khoảng sàn phẳng gần A, nền tương phản | T5: Hit, Dead, reset |

Tuyến T3: A → B → C → góc nhìn rộng. Tuyến T5: A → B → D. Mọi điểm phải nằm đủ gần để di chuyển trong các mốc 2:30 đã quy định. Không làm bản đồ rộng phải chạy lâu giữa các phép thử. Góc rộng dùng để quan sát; các thao tác chính thực hiện ở TPS.

## 4. Chức năng bắt buộc và tiêu chí nghiệm thu

Đây là checklist cần chạy, **chưa đánh dấu hoàn thành**. Cột “Đạt khi” là bằng chứng để nhóm tự kiểm tra trước khi chụp ảnh và quay demo.

| Mã | Chức năng | Đạt khi | Slide liên quan |
| --- | --- | --- | --- |
| D01 | Một scene và nhân vật chung | Hai buổi dùng cùng layout/model; reset về A | T3-1, T5-1 |
| D02 | Input Action: Move, Look, Jump, Sprint | WASD, chuột, Space, Shift hoạt động; không nhận lặp Jump do giữ phím | T3-2,3,8 |
| D03 | Di chuyển theo camera | Xoay camera 90° làm hướng W đổi theo; W+D không nhanh hơn W trên sàn | T3-4,8 |
| D04 | Motor, nhảy và va chạm | Không xuyên tường trên tuyến; lên bục được; bấm Space trên không không tạo nhảy thứ hai; tiếp đất ổn định | T3-5,8; T5-3,8 |
| D05 | Camera TPS | Theo được nhân vật, chuột xoay có giới hạn pitch, không lật camera | T3-6,8 |
| D06 | Hai góc nhìn | C đổi TPS/góc rộng có blend; đổi camera không đổi vị trí nhân vật | T3-6,8 |
| D07 | Xử lý vật cản camera | Camera không nằm trong tường trên tuyến C; giảm che khuất nhân vật ở các vị trí demo đã chọn | T3-7,8 |
| D08 | FSM chính và chặn lệnh | Chỉ một state chính; Jump bị chặn trên không; Dead chặn Move/Sprint/Jump | T5-2,3,4,8 |
| D09 | Animator và Blend Tree | Idle/Walk/Run đổi theo Speed; animation trên không theo state; không T-pose/mất clip | T5-5,6,8 |
| D10 | Hit có đường thoát | H khi Grounded giảm HP 25, vào Hit, khóa điều khiển tạm rồi về Grounded nếu còn HP | T5-3,7,8 |
| D11 | Dead và reset | K đặt HP 0; vào Dead, không nhận input gameplay; R phục hồi HP 100 và trạng thái ban đầu | T5-2,7,8 |
| D12 | HUD và lịch sử | Hiện State, HP, Speed, Grounded, VerticalSpeed; log 5 chuyển gần nhất; báo lý do chặn lệnh | T5-4,6,8 |
| D13 | Trình diễn độc lập | Có build chạy offline, R reset, hai video dự phòng 2:30 chạy được trên máy báo cáo | Cả hai slide 8 |

**Không bắt buộc:** đổi Rigidbody/CharacterController; FPS; camera shake; vùng trigger đổi camera; crouch; attack/combo; Avatar Mask; Animation Events; ragdoll; IK. Chỉ thêm sau khi D01–D13 đã ổn định, không tăng thời lượng báo cáo. Không tự đưa tính năng thêm vào hai kịch bản chính.

## 5. Phím điều khiển thống nhất

| Phím | Chức năng | Ghi chú |
| --- | --- | --- |
| WASD | Di chuyển | Tương đối với hướng ngang camera |
| Chuột | Xoay camera TPS | Pitch có giới hạn |
| Shift giữ | Chạy | Chỉ khi state cho phép |
| Space nhấn | Nhảy | Chỉ Grounded; không nhảy liên tục do giữ phím |
| C | TPS ↔ góc rộng | Ưu tiên dùng khi đứng yên trong demo |
| H | Giả lập nhận 25 sát thương | Chỉ Grounded; công cụ test, không phải hệ chiến đấu |
| K | Giả lập HP về 0 | Mọi state còn sống, kiểm thử ưu tiên Dead |
| R | Reset toàn bộ lượt demo | Đưa về A, HP 100, Grounded, camera TPS |
| F1 | Bật/tắt HUD | T3 có thể thu gọn; T5 bật đầy đủ |
| Esc | Nhả con trỏ | Để chuyển cửa sổ; click lại để tiếp tục |

HUD dùng cỡ chữ đọc được trên máy chiếu. T3 ưu tiên Speed, Grounded, camera mode; T5 ưu tiên State, HP, Speed và lý do chặn lệnh. H/K được ghi rõ là **DEBUG**. Không cần hiển thị cả lịch sử input chi chít.

## 6. Quy tắc FSM chính

### 6.1. Ý nghĩa state

| State | Ý nghĩa | Input gameplay được phép | Hình ảnh |
| --- | --- | --- | --- |
| Grounded | Đang trên mặt đất, còn sống | Đi/chạy/nhảy | Blend Tree Idle/Walk/Run |
| Airborne | Còn sống, đang trên không | Điều khiển ngang có giới hạn nếu nhóm chọn; cấm nhảy tiếp | Jump/Fall hoặc một clip trên không |
| Hit | Phản ứng nhận sát thương không chí mạng trên đất | Khóa Move/Sprint/Jump tạm | Clip Hit một lần |
| Dead | HP bằng 0 | Khóa input gameplay; R vẫn hoạt động như công cụ test | Clip Death một lần, giữ tư thế cuối |

Đề xuất cho dễ kiểm thử: Airborne giữ điều khiển ngang như Grounded nhưng không cho nhảy tiếp. Không thêm cơ chế air dash hay double jump. Đứng/đi/chạy là các mức tốc độ trong Grounded; đi lên/rơi là dấu vận tốc dọc trong Airborne.

### 6.2. Bảng chuyển và ưu tiên

| Mức ưu tiên | Từ | Điều kiện | Sang | Hành động |
| --- | --- | --- | --- | --- |
| Công cụ ngoài gameplay | Bất kỳ | Nhấn R | Grounded tại A | Khởi tạo HP, motor, Animator, camera, HUD |
| 1 | Mọi state còn sống | HP ≤ 0 hoặc K | Dead | Khóa input, xóa lệnh nhảy đang chờ |
| 2 | Grounded | H và sau giảm HP vẫn > 0 | Hit | Giảm 25 HP, bắt đầu bộ đếm phản ứng |
| 2 | Grounded | H và sau giảm HP ≤ 0 | Dead | Giảm HP về 0, bỏ qua Hit |
| 3 | Grounded | Nhảy hợp lệ hoặc mất tiếp xúc đất | Airborne | Chỉ đặt vận tốc nhảy nếu do lệnh nhảy |
| 3 | Airborne | Chạm đất khi đang hạ xuống | Grounded | Hoàn tất tiếp đất |
| 3 | Hit | Mất tiếp xúc đất, vẫn còn sống | Airborne | Hủy khóa Hit, tiếp tục rơi |
| 4 | Hit | Hết thời gian phản ứng và đang grounded | Grounded | Mở khóa điều khiển |

- Khi H và Jump cùng lượt xử lý ở Grounded, xử lý Hit trước và hủy Jump. Khi K cùng bất kỳ lệnh khác, Dead thắng.
- H ở Airborne, Hit hoặc Dead không gây thêm sát thương trong bản demo tối thiểu; HUD báo “H chỉ dùng khi Grounded”. Đây là phạm vi của công cụ test, không khẳng định mọi game phải xử lý sát thương như vậy.
- Thời gian Hit đề xuất khoảng 0,6–0,8 giây, chỉnh theo clip thật. C# sở hữu thời gian phục hồi; không đồng thời để Animator tự đưa gameplay về Grounded.
- Dead khóa vận tốc ngang theo luật demo; trọng lực và va chạm vẫn được xử lý nếu chết giữa không trung. Đường trình diễn chính dùng K khi đang đứng trên đất.
- Reset xóa vận tốc, lệnh chờ, bộ đếm Hit, log cũ; đưa Animator về Grounded và reset camera để chạy lặp lại được.

## 7. Phân chia trách nhiệm trong project

Tên dưới đây là gợi ý, không phải yêu cầu phải tạo đúng số script.

| Thành phần | Sở hữu dữ liệu / trách nhiệm | Không nên làm |
| --- | --- | --- |
| InputReader | Move, Look, JumpPressed, SprintHeld | Tự đổi state hoặc gọi Animator trực tiếp |
| CharacterStateMachine | CurrentState, HP, luật ưu tiên, bộ đếm Hit | Tự di chuyển Transform ở nhiều chỗ |
| CharacterMotor | Vận tốc ngang/dọc, grounded, CharacterController.Move | Tự quyết định nhận sát thương |
| AnimatorBridge | Ghi parameter từ snapshot gameplay/motor | Tự mở khóa gameplay khi thấy clip kết thúc |
| CameraRig | TPS, góc rộng, blend, vật cản | Sửa vị trí nhân vật |
| DemoDebug | H/K/R/F1, HUD, lịch sử | Trộn phím thử nghiệm vào luật chiến đấu chưa có |

**Nhịp xử lý đề xuất:** đọc input → giải quyết yêu cầu/ưu tiên state → motor thực hiện chuyển động → cập nhật tiếp đất/mất đất → ghi snapshot cho Animator/HUD. Camera dùng nhịp cập nhật phù hợp với motor và cấu hình Brain, kiểm tra rung bằng chạy thử. Nếu nhảy vừa bắt đầu, không để kết quả grounded cũ lập tức kéo state trở về Grounded.

Đây là luồng thiết kế, không phải yêu cầu tất cả script phải chạy trong các callback độc lập. Có thể dùng một bộ điều phối để kiểm soát thứ tự và tránh phụ thuộc thứ tự MonoBehaviour ngẫu nhiên.

## 8. Hợp đồng giữa FSM và Animator

| Parameter | Kiểu | Giá trị / cách tính | Công dụng |
| --- | --- | --- | --- |
| Mode | int | Grounded=0, Airborne=1, Hit=2, Dead=3 | Chọn animation state chính |
| Speed | float | Độ lớn vận tốc ngang thực tế từ motor, m/s | Blend Idle/Walk/Run; không lấy chỉ từ phím khi đang đâm tường |
| VerticalSpeed | float | Vận tốc dọc từ motor | Phân biệt đi lên/rơi nếu có đủ clip |

- Blend Tree 1D: ngưỡng Idle 0; Walk và Run đặt theo tốc độ thiết kế thực tế. Ví dụ 2 và 5 m/s chỉ là điểm bắt đầu tinh chỉnh, không phải chuẩn Unity.
- Animator có Grounded (Blend Tree), Airborne, Hit, Dead. Nếu Airborne dùng Jump/Fall, đặt trong phần animation riêng, không nhân đôi state gameplay.
- Transition do Mode điều khiển. Hit thoát khi Mode đổi, không để bộ đếm C# và exit condition của Animator cạnh tranh.
- Dead có transition ưu tiên đủ nhanh để thể hiện khóa hành vi. Nếu dùng Any State, cấu hình để không liên tục tự chuyển lại vào Dead và khởi động lại clip.
- Death/Hit tắt loop. Clip locomotion loop phù hợp; tinh chỉnh transition duration bằng clip thật.
- Root Motion tắt toàn bộ; không trộn hai nơi điều khiển vị trí. Blend có thể phát trộn hai clip trong lúc đổi state, đây là hành vi hình ảnh bình thường.
- Chưa yêu cầu Event tạo hitbox hay gây damage; HP đổi từ công cụ test H/K. Nếu bổ sung Event sau này, cần xác định rõ bên nào sở hữu gameplay.

## 9. Ảnh và video cần chuẩn bị

Tên file chi tiết nằm trong từng khung slide. Không phải tạo tất cả ảnh thành tệp riêng nếu dựng sơ đồ trực tiếp trong PPTX; cần giữ nội dung và bố cục đã mô tả.

| Nhóm tài nguyên | Cách chuẩn bị | Tiêu chí |
| --- | --- | --- |
| Ảnh scene/model | Chụp build ở A/B/C, TPS và góc rộng | Cùng project thật, không lẫn asset minh họa từ game khác |
| Input/Controller | Chụp Editor, crop Input Actions và component | Tên Action/phiên bản đúng project |
| Sơ đồ T3 | Kiến trúc, hướng theo camera, nhảy/grounded | Nhãn ngắn, vector rõ, không dùng ảnh AI cho sơ đồ kỹ thuật |
| Camera trước/sau | Chụp cùng vị trí trên tuyến C ở hai cấu hình | Điều kiện so sánh tương đương, không khẳng định mọi địa hình |
| Sơ đồ T5 | FSM 4 state, lifecycle, mapping | Khớp bảng state, Mode và code thực tế |
| Blend Tree/Animator | Chụp đồ thị và parameter sau khi chạy đúng | Crop đủ đọc, không toàn màn hình Editor thu nhỏ |
| Settings animation | Chụp Root Motion tắt và tư thế Hit/Dead | Dùng cấu hình thật đã nghiệm thu |
| `t3_demo_backup.mp4` | Quay theo bảng T3, 2:30 | 1920×1080 nếu máy đáp ứng; phím/HUD rõ |
| `t5_demo_backup.mp4` | Quay theo bảng T5, 2:30 | Thấy chuyển state và lệnh bị chặn |

Ảnh tĩnh ưu tiên PNG, sơ đồ vector có thể SVG hoặc đối tượng sửa được khi dựng PPTX. Video lưu cục bộ, kiểm tra codec/phát offline trên máy trình chiếu. Video có thể không có lời để người báo cáo nói trực tiếp; tránh vừa tiếng thu sẵn vừa tiếng người thuyết trình.

## 10. Chia việc và lịch chuẩn bị

Chia theo vai trò, một người có thể kiêm nhiều vai; không giả định nhóm có bốn thành viên.

| Vai trò | Sản phẩm bàn giao | Điều cần phối hợp |
| --- | --- | --- |
| Điều khiển/camera | D02–D07, ảnh T3 | Thống nhất motor, đơn vị tốc độ và camera hướng ngang |
| State/animation | D08–D12, ảnh T5 | Thống nhất Mode, HP, điều kiện chuyển và Root Motion |
| Tích hợp/kiểm thử | Scene chung, build, reset, checklist | Không ghép hai PlayerController cùng điều khiển nhân vật |
| Nội dung/trình diễn | Speaker notes, ảnh/sơ đồ, video, tập bấm giờ | Chỉ đưa chức năng đã chạy vào bài báo cáo |

### Các mốc đề xuất

1. **03–04/10:** chốt phiên bản, asset và hợp đồng state/parameter. Làm scene, motor, camera; thêm animation locomotion cơ bản ngay để T3 không phải đổi nhân vật về sau.
2. **05/10:** khóa bản demo T3; chụp ảnh, quay video, tập toàn bài. Giữ một bản build riêng để thay đổi T5 không làm hỏng bản đã ổn định.
3. **06/10:** báo cáo T3. Sau buổi chỉ bổ sung/sửa FSM, Hit/Dead và HUD trên cùng project; giữ motor/camera đã nghiệm thu.
4. **07/10:** khóa T5, quay video, thử lại reset và ưu tiên Dead; tập toàn bài.
5. **08/10:** báo cáo T5. Mở sẵn slide, build và video trước lượt nhóm.

Nếu thời gian làm hạn chế, bỏ toàn bộ tính năng ngoài D01–D13. Không bỏ HUD hay reset để dành thời gian làm hiệu ứng trang trí vì đây là bằng chứng và công cụ giúp demo rõ ràng.

## 11. Checklist trước khi báo cáo

- [ ] Mỗi hàng D01–D13 đã có người kiểm và ghi đạt/chưa đạt.
- [ ] Chạy toàn tuyến của từng buổi ít nhất hai lượt; kiểm R và thử lại sau Dead.
- [ ] Ảnh chụp, tên state, parameter, phím bấm trên slide khớp build.
- [ ] T3 không giải thích lại FSM; T5 chỉ nhắc motor/camera trong khoảng 20 giây đầu.
- [ ] Tập cả chuyển cửa sổ và lời nói trong 9:20; 40 giây còn lại là đệm, không thêm nội dung.
- [ ] Mở sẵn build/video; video offline chạy được, con trỏ khóa/nhả được.
- [ ] Nếu live lỗi quá 10 giây, nói “Nhóm chuyển sang video cùng kịch bản” rồi phát video; không debug tại lớp.
- [ ] Nếu video cũng lỗi, dùng 3 ảnh dự phòng theo chuỗi: di chuyển–nhảy–camera cho T3, Grounded–Hit–Dead cho T5.
- [ ] Không ghi “đã kiểm thử”, “nhóm đã gặp” khi chưa có ghi nhận thật.
- [ ] Nguồn model/clip, tài liệu Unity và thông tin nhóm được bổ sung vào credits/phụ lục trước khi dựng PPTX.

## 12. Quy tắc bàn giao để tạo PPTX sau này

- Dựng hai file riêng từ hai khung, cùng hệ thống bố cục; T3 xanh ngọc, T5 cam.
- Chỉ lấy chữ ở “Hiển thị trên slide” và nhãn hình lên trang; lấy “Nói khi thuyết trình” vào notes. Phần “Liên hệ demo” là checklist sản xuất, không đưa lên slide.
- 9 slide chính + 3 phụ lục ẩn mỗi file. Các bảng kịch bản demo nằm trong notes hoặc tài liệu nhóm, không tạo thêm slide dài.
- Thêm tên thành viên, lớp, môn, giảng viên khi nhóm cung cấp; không tự điền dữ liệu chưa biết.
- Ảnh thiếu thì đánh dấu khung ảnh cần bổ sung ở bản nháp; không bịa ảnh chụp Unity hoặc kết quả chạy.
- Khi dựng PPTX, render và kiểm tra dấu tiếng Việt, chữ tràn, ảnh đọc được ở kích thước trình chiếu, link/video offline. Chưa thực hiện các bước này trong bộ tài liệu MD hiện tại.

## Nguồn kỹ thuật đối chiếu

Các nguồn sau hỗ trợ khái niệm Unity. Tên script, luật FSM, HP, phím debug và lịch chuẩn bị là thiết kế đề xuất của bộ tài liệu.

- [CharacterController.Move](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/CharacterController.Move.html)
- [Input System Actions](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.11/manual/Actions.html)
- [Cinemachine Brain 3.1](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineBrain.html)
- [Cinemachine Decollider 3.1](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineDecollider.html)
- [Cinemachine Deoccluder 3.1](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineDeoccluder.html)
- [Animation Blend Trees](https://docs.unity3d.com/6000.0/Documentation/Manual/class-BlendTree.html)
- [Animation Parameters](https://docs.unity3d.com/6000.0/Documentation/Manual/AnimationParameters.html)
