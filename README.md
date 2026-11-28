<table align="center" width="100%">
  <tr>
    <td align="center">
      <br>
      <h1>A U R E L I A .</h1>
      <i>Atelier &bull; Saigon — Hệ Thống Thương Mại Điện Tử &amp; Quản Lý Bán Hàng Thời Trang</i>
      <br><br>
    </td>
  </tr>
</table>

<p align="center">
  <img src="https://img.shields.io/badge/ASP.NET-MVC%205-512BD4?style=for-the-badge&logo=dotnet&logoColor=white">
  <img src="https://img.shields.io/badge/Entity%20Framework-6%20DB--First-68217A?style=for-the-badge">
  <img src="https://img.shields.io/badge/SQL%20Server-SQLEXPRESS-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white">
  <img src="https://img.shields.io/badge/Bootstrap-5-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white">
  <img src="https://img.shields.io/badge/Zero%20AJAX-Traditional%20MVC-111111?style=for-the-badge">
</p>

Website thương mại điện tử thời trang **AURELIA.** (Solution `ClothingShop`) xây dựng trên nền tảng **ASP.NET MVC 5**, **Entity Framework 6 (Database First `.edmx`)**, **SQL Server** và **Bootstrap 5**, theo phong cách **Fashion Editorial / Minimalist** và tuân thủ hoàn toàn luồng **MVC truyền thống (Không dùng AJAX)**.

---

## 🏗️ Tổng Quan Hệ Thống & Tính Năng

### 🛍️ 1. Phía Khách Hàng (Storefront)

| Trang / Module | Controller | Chức năng chính |
|:---------------|:-----------|:----------------|
| **Trang chủ** | `Home` | Hero lookbook, Promo/Voucher Banner (`SALE10`, `NEWUSER` kèm nút sao chép mã), dải cam kết dịch vụ (Trust Strip), khối sản phẩm mới & nổi bật. |
| **Điều hướng & Tìm kiếm** | `Menu` / `Product` | Header 1 hàng gọn gàng, menu danh mục 2 cấp, thanh tìm kiếm gợi ý tức thì từ dữ liệu server render sẵn (`window.SEARCH_PRODUCTS`, không AJAX) + form GET tìm kiếm. |
| **Danh mục & Chi tiết SP** | `Product` | Lọc theo danh mục cha/con, giới tính, khoảng giá, trạng thái Sale/New; sắp xếp; phân trang nâng cấp (nút trước/sau, `disabled`, rút gọn `...` khi `> 7` trang); chọn biến thể Size/Màu. |
| **Tài khoản & Sổ địa chỉ** | `Account` | Đăng nhập, Đăng ký (nút ẩn/hiện mật khẩu, đo độ mạnh mật khẩu, validation), Quên mật khẩu (cấp mật khẩu tạm), Hồ sơ & Sổ địa chỉ với Combobox 63 Tỉnh/Thành – Quận/Huyện – Phường/Xã. |
| **Yêu thích & Giỏ hàng** | `Favorite` / `Cart` | Thêm/xóa sản phẩm yêu thích, giỏ hàng theo phiên (`Session`), cập nhật số lượng qua Form POST thuần + thông báo `TempData`. |
| **Đặt hàng & Thanh toán** | `Order` | Progress Bar 3 bước đồng bộ giữa `Checkout` và `Success`; áp mã giảm giá; thanh toán COD hoặc VietQR; Combobox địa chỉ 3 cấp vừa chọn vừa tự nhập + Validation Client & Server đồng bộ; lịch sử & hủy đơn hàng. |
| **Footer** | `Shared/_Layout` | Thông tin thương hiệu `AURELIA.`, danh mục nhanh, thông tin liên hệ và bản đồ Google Maps nhúng (140 Lê Trọng Tấn, Tân Phú, TP.HCM). |

### 🔐 2. Phía Quản Trị (`Areas/Admin` — AURELIA. Studio)

| Module | Controller | Chức năng chính |
|:-------|:-----------|:----------------|
| **Tổng quan** | `Dashboard` | Thống kê doanh thu, đơn hàng, sản phẩm & tổng quan vận hành. |
| **Loại sản phẩm** | `Category` | Quản lý danh mục cha/con (CRUD), bật/tắt trạng thái hiển thị. |
| **Sản phẩm & Kho** | `Product` | CRUD sản phẩm, hỗ trợ **nhập URL ảnh online** hoặc **upload file** kèm xem trước ảnh real-time, quản lý biến thể Size/Màu & tồn kho. |
| **Đơn hàng** | `Order` | Xem chi tiết, cập nhật trạng thái đơn hàng & thanh toán. |
| **Khuyến mãi** | `Coupon` | Tạo/sửa/xóa mã giảm giá (`SALE10`, `NEWUSER`,...). |
| **Tài khoản & Quyền** | `User` | Quản lý người dùng và phân quyền (`Admin`, `Staff`, `Customer`). |

---

## 🧭 Kiến Trúc & Nguyên Tắc Kỹ Thuật

- **Luồng ASP.NET MVC 5 truyền thống (Zero AJAX):** Không sử dụng `$.ajax`, `$.post`, `$.get`, `fetch(...)`, `XMLHttpRequest` hay `JsonResult`. Mọi thao tác tạo/sửa/xóa đều dùng `@using (Html.BeginForm(...))` kèm `@Html.AntiForgeryToken()` / `[ValidateAntiForgeryToken]`, `DataAnnotations` + `ModelState` và `TempData`.
- **Dữ liệu địa giới hành chính 3 cấp tĩnh:** Toàn bộ 63 Tỉnh/Thành phố, Quận/Huyện, Phường/Xã Việt Nam được nạp tĩnh qua `Content/data/vietnam-provinces.js`, hỗ trợ tìm kiếm không dấu và nhập tự do.

| Lớp | Công nghệ |
|:----|:----------|
| **Backend** | ASP.NET MVC 5 (.NET Framework), Entity Framework 6 (Database First `.edmx`), LINQ |
| **Frontend** | Razor View, HTML5, CSS3 (`Content/Site.css`), Bootstrap 5, Bootstrap Icons, JavaScript thuần |
| **Font chữ** | Google Fonts: *Playfair Display*, *Urbanist*, *Nunito* |
| **Database** | SQL Server (`ClothingShopDB`) |

---

## 🚀 Hướng Dẫn Cài Đặt & Chạy Dự Án

1. **Clone & Mở Solution:**
   - Mở `ClothingShop.sln` bằng **Visual Studio**.
2. **Cấu hình Database (`Web.config`):**
   - Kiểm tra chuỗi kết nối `ClothingShopDBEntities` trong `ClothingShop/Web.config`:
     - Mặc định đang cấu hình cho SQL Server Express: `data source=.\SQLEXPRESS;initial catalog=ClothingShopDB;user id=sa;password=123;...`
     - Hoặc chỉnh lại `data source` phù hợp với instance SQL Server / `(localdb)\MSSQLLocalDB` trên máy của bạn.
3. **Chạy ứng dụng:**
   - Nhấn **F5** hoặc **Ctrl + F5** trên Visual Studio để khởi chạy bằng IIS Express.

---

## 🔑 Tài Khoản Kiểm Thử (Database `ClothingShopDB`)

Dữ liệu mẫu trong bảng `Users` ([ClothingShopDB.sql](ClothingShopDB.sql)) bao gồm các tài khoản sau:

| Vai trò (`Role`) | Họ tên | Email | Mật khẩu | Phạm vi truy cập |
|:----------------|:-------|:------|:---------|:-----------------|
| **Admin** | Quản trị viên | `admin@shop.com` | `Admin@123` | Toàn quyền hệ thống & khu vực `/Admin` (`Dashboard`, `Category`, `Product`, `Order`, `Coupon`, `User`) |
| **Staff** | Lê Minh Nhân Viên | `nhanvien@shop.com` | `Staff@123` | Khu vực quản lý vận hành (`/Admin/Order`, `/Admin/Product`, `/Admin/Category`) |
| **Customer** | Nguyễn Văn An | `khach1@shop.com` | `Khach@123` | Mua sắm, giỏ hàng, yêu thích, đặt hàng, hồ sơ & sổ địa chỉ |
| **Customer** | Trần Thị Bình | `khach2@shop.com` | *(đã hash trong DB)* | Tài khoản khách hàng mẫu thứ 2 trong `ClothingShopDB.sql` |

---

<p align="center"><i>AURELIA. Atelier &bull; TP. Hồ Chí Minh</i></p>
