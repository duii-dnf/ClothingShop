<table align="center" width="100%">
  <tr>
    <td align="center">
      <br>
      <h1>C L O T H I N G S H O P</h1>
      <i>Hệ Thống Quản Lý &amp; Bán Hàng Thời Trang</i>
      <br><br>
    </td>
  </tr>
</table>

<p align="center">
  <img src="https://img.shields.io/badge/ASP.NET-MVC%205-512BD4?style=for-the-badge&logo=dotnet&logoColor=white">
  <img src="https://img.shields.io/badge/Entity%20Framework-6%20Code%20First-68217A?style=for-the-badge">
  <img src="https://img.shields.io/badge/SQL%20Server-LocalDB-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white">
  <img src="https://img.shields.io/badge/Bootstrap-5-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white">
</p>

Website thương mại điện tử thời trang xây dựng trên nền tảng **ASP.NET MVC 5**, **Entity Framework 6 (Code First)** và **SQL Server LocalDB**, mang phong cách thiết kế tối giản, hiện đại (Editorial UI/UX) với tông màu chủ đạo `--ink` và `--wine`.

---

## 🏗️ Tổng Quan Hệ Thống & Tính Năng

### 🛍️ 1. Phía Khách Hàng (Storefront)

| Trang | Controller | Chức năng chính |
|:------|:-----------|:----------------|
| **Trang chủ** | `Home` | Banner động mượt mà, lọc danh mục sản phẩm (không tải lại trang), khối sản phẩm mới và nổi bật. |
| **Danh mục & Tìm kiếm** | `Product` | Kệ hàng có phân trang, lọc kết hợp danh mục, tìm kiếm từ khóa, sắp xếp theo giá / ngày tạo. |
| **Chi tiết sản phẩm** | `Details` | Hiển thị trực quan, quản lý số lượng tồn kho, thêm giỏ hàng bất đồng bộ, gợi ý sản phẩm liên quan. |
| **Giỏ hàng & Thanh toán** | `Cart` / `Checkout` | Giỏ hàng theo phiên (`Session`), tăng giảm số lượng cập nhật thành tiền và tổng cộng ngay lập tức, xử lý an toàn dữ liệu nhạy cảm (`Request.Form`). |

### 🔐 2. Phía Quản Trị (Admin Management)

| Module | Controller | Chức năng chính |
|:-------|:-----------|:----------------|
| **Quản lý danh mục** | `Categories` | Thêm, Sửa, Xóa, Xem chi tiết (**CRUD**) kèm kiểm tra tính hợp lệ dữ liệu. |

---

## 🧭 Kiến Trúc Hệ Thống

<table align="center" width="100%">
  <tr>
    <td colspan="3" align="center">
      <b>🌐 Trình duyệt (Client)</b><br>
      HTML5 · CSS3 · Bootstrap 5 · JavaScript (AJAX)
    </td>
  </tr>
  <tr>
    <td colspan="3" align="center">⬇️ HTTP Request &nbsp;/&nbsp; Response ⬆️</td>
  </tr>
  <tr>
    <td colspan="3" align="center"><b>⚙️ ASP.NET MVC 5 (IIS Express)</b></td>
  </tr>
  <tr>
    <td align="center"><b>View</b><br>Razor</td>
    <td align="center"><b>Controller</b><br>Home · Product · Cart · Categories</td>
    <td align="center"><b>Model</b><br>Entities</td>
  </tr>
  <tr>
    <td colspan="3" align="center">⬇️ LINQ</td>
  </tr>
  <tr>
    <td colspan="3" align="center">
      <b>🧩 Entity Framework 6</b><br>
      Code First
    </td>
  </tr>
  <tr>
    <td colspan="3" align="center">⬇️</td>
  </tr>
  <tr>
    <td colspan="3" align="center">
      <b>🗄️ SQL Server LocalDB</b><br>
      <code>ClothingShopDB</code>
    </td>
  </tr>
</table>

---

## 🛠️ Công Nghệ Sử Dụng

| Lớp | Công nghệ |
|:----|:----------|
| **Backend** | ASP.NET MVC 5, Entity Framework 6 (Code First), LINQ |
| **Frontend** | HTML5, CSS3 (Custom Grid, Flexbox, Animation), Bootstrap 5, JavaScript (Fetch) |
| **Font chữ** | Google Fonts: *Playfair Display*, *Cormorant Garamond*, *Inter* |
| **Database** | SQL Server LocalDB (`ClothingShopDB`) |

---

## 🚀 Hướng Dẫn Cài Đặt & Chạy Dự Án

> [!NOTE]
> **📦 Bước 1: Clone dự án và mở bằng Visual Studio**
> 1. Clone repository về máy và mở file solution `ClothingShop.sln` bằng **Visual Studio**.
> 2. Kiểm tra chuỗi kết nối (`ConnectionString`) trong `Web.config` đã trỏ đến LocalDB của bạn: `(localdb)\MSSQLLocalDB`.

> [!IMPORTANT]
> **🗄️ Bước 2: Cập nhật Database & Nạp dữ liệu mẫu (Seed Data)**
>
> Để hệ thống hiển thị đầy đủ **24 sản phẩm** thời trang kèm hình ảnh mẫu:
> 1. Mở **Visual Studio** → **View** → **SQL Server Object Explorer**.
> 2. Mở rộng `(localdb)\MSSQLLocalDB` → `Databases` → `ClothingShopDB`.
> 3. Chuột phải vào `ClothingShopDB` → chọn **New Query...**
> 4. Mở file **`SeedProducts.sql`** (có sẵn ở thư mục gốc của project), copy toàn bộ nội dung dán vào khung query rồi bấm **Execute** (mũi tên xanh hoặc `Ctrl + Shift + E`) để nạp danh mục và sản phẩm mẫu.

> [!TIP]
> **▶️ Bước 3: Chạy ứng dụng**
>
> Nhấn nút **Play / IIS Express** trên Visual Studio (hoặc `Ctrl + F5`) để khởi chạy trang web và trải nghiệm hệ thống!

---

<p align="center"><i>Cảm ơn bạn đã quan tâm đến ClothingShop ✨</i></p>
