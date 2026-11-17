-- ============================================================================
-- SeedProducts.sql
-- Script nap du lieu mau Danh muc & 24 San pham cho ClothingShopDB (SQL Server)
-- Chay file nay trong SQL Server Object Explorer -> New Query tren ClothingShopDB
-- ============================================================================

INSERT INTO Categories (Name, ParentId, IsActive)
SELECT v.Name, NULL, 1
FROM (VALUES 
    (N'Quần Âu & Jeans'),
    (N'Áo Khoác & Blazer'),
    (N'Váy & Đầm'),
    (N'Phụ Kiện Thời Trang')
) AS v(Name)
WHERE NOT EXISTS (SELECT 1 FROM Categories c WHERE c.Name = v.Name);

DECLARE @CatAoThun  INT = (SELECT TOP 1 CategoryId FROM Categories WHERE Name LIKE N'%Thun%');
DECLARE @CatAoSoMi  INT = (SELECT TOP 1 CategoryId FROM Categories WHERE Name LIKE N'%Sơ Mi%');
DECLARE @CatQuan    INT = (SELECT TOP 1 CategoryId FROM Categories WHERE Name = N'Quần Âu & Jeans');
DECLARE @CatKhoac   INT = (SELECT TOP 1 CategoryId FROM Categories WHERE Name = N'Áo Khoác & Blazer');
DECLARE @CatVay     INT = (SELECT TOP 1 CategoryId FROM Categories WHERE Name = N'Váy & Đầm');
DECLARE @CatPhuKien INT = (SELECT TOP 1 CategoryId FROM Categories WHERE Name = N'Phụ Kiện Thời Trang');

INSERT INTO Products (Name, Price, DiscountPrice, Description, ImageUrl, Stock, CategoryId, IsActive, IsFeatured, IsNew, CreatedAt)
SELECT v.Name, v.Price, v.DiscountPrice, v.Description, v.ImageUrl, v.Stock, v.CategoryId, 1, v.IsFeatured, v.IsNew, v.CreatedAt
FROM (VALUES
    (N'Áo Thun Cotton Supima Cổ Tròn', 250000, 199000, N'Chất liệu cotton Supima mềm mịn, thoáng khí, giữ phom sau nhiều lần giặt.', 'sp-01.jpg', 35, @CatAoThun, 1, 1, DATEADD(HOUR, -1, GETDATE())),
    (N'Áo Thun Oversize In Chữ Minimal', 290000, NULL, N'Phom rộng hiện đại, vải cotton 2 chiều dày dặn đứng dáng.', 'sp-02.jpg', 28, @CatAoThun, 0, 1, DATEADD(HOUR, -2, GETDATE())),
    (N'Áo Thun Polo Dệt Kim Phối Cổ', 380000, 320000, N'Thiết kế cổ dệt thanh lịch, phù hợp cả đi làm lẫn dạo phố.', 'sp-03.jpg', 20, @CatAoThun, 1, 0, DATEADD(HOUR, -3, GETDATE())),
    (N'Áo Thun Tay Dài Gân Tăm Slimfit', 270000, NULL, N'Co giãn 4 chiều ôm nhẹ tôn dáng, dễ phối cùng quần cạp cao.', 'sp-04.jpg', 18, @CatAoThun, 0, 1, DATEADD(HOUR, -4, GETDATE())),

    (N'Áo Sơ Mi Lụa Trắng Cổ Đức', 450000, 390000, N'Vải lụa nhân tạo ít nhăn, bề mặt bóng nhẹ sang trọng.', 'sp-05.jpg', 22, @CatAoSoMi, 1, 1, DATEADD(HOUR, -5, GETDATE())),
    (N'Áo Sơ Mi Linen Tay Ngắn Cổ Cuba', 420000, NULL, N'Chất vải lanh tự nhiên mát mẻ, mang phong cách nghỉ dưỡng phóng khoáng.', 'sp-06.jpg', 30, @CatAoSoMi, 0, 1, DATEADD(HOUR, -6, GETDATE())),
    (N'Áo Sơ Mi Oxford Kẻ Sọc Xanh', 460000, 410000, N'Vải Oxford dệt dày dặn, đường may tỉ mỉ chuẩn công sở.', 'sp-07.jpg', 15, @CatAoSoMi, 1, 0, DATEADD(HOUR, -7, GETDATE())),
    (N'Áo Sơ Mi Denim Wash Bạc Nhẹ', 520000, NULL, N'Chất denim mềm đã qua xử lý wash, nút bấm kim loại cổ điển.', 'sp-08.jpg', 12, @CatAoSoMi, 0, 0, DATEADD(HOUR, -8, GETDATE())),

    (N'Quần Tây Ống Suông Xếp Ly Đôi', 580000, 490000, N'Phom ống suông rủ đứng dáng, che khuyết điểm và tôn chiều cao.', 'sp-09.jpg', 25, @CatQuan, 1, 1, DATEADD(HOUR, -9, GETDATE())),
    (N'Quần Jeans Selvedge Ống Đứng', 650000, NULL, N'Denim biên đỏ cao cấp, bền màu và càng mặc càng mềm.', 'sp-10.jpg', 19, @CatQuan, 1, 0, DATEADD(HOUR, -10, GETDATE())),
    (N'Quần Kaki Chino Dáng Slim', 480000, 420000, N'Vải kaki cotton pha spandex nhẹ giúp vận động thoải mái cả ngày.', 'sp-11.jpg', 27, @CatQuan, 0, 1, DATEADD(HOUR, -11, GETDATE())),
    (N'Quần Short Linen Lưng Thun', 340000, NULL, N'Thiết kế tối giản với dây rút ẩn bên trong, phù hợp mùa hè.', 'sp-12.jpg', 32, @CatQuan, 0, 1, DATEADD(HOUR, -12, GETDATE())),

    (N'Áo Khoác Blazer Hai Hàng Khuy', 980000, 850000, N'Dựng vai chuẩn mực, lớp lót lụa thoáng khí bên trong.', 'sp-13.jpg', 10, @CatKhoac, 1, 1, DATEADD(HOUR, -13, GETDATE())),
    (N'Áo Khoác Bomber Da Lộn Mỏng', 820000, NULL, N'Bề mặt da lộn mịn mượt, bo viền dệt kim chắc chắn.', 'sp-14.jpg', 14, @CatKhoac, 1, 0, DATEADD(HOUR, -14, GETDATE())),
    (N'Áo Khoác Trench Coat Dáng Dài', 1250000, 1090000, N'Thiết kế cổ điển phong cách châu Âu kèm đai thắt eo tinh tế.', 'sp-15.jpg', 8, @CatKhoac, 1, 1, DATEADD(HOUR, -15, GETDATE())),
    (N'Áo Khoác Cardigan Len Merino', 620000, NULL, N'Len dệt mỏng nhẹ giữ ấm tốt, không gây kích ứng da.', 'sp-16.jpg', 16, @CatKhoac, 0, 0, DATEADD(HOUR, -16, GETDATE())),

    (N'Đầm Midi Lụa Xếp Nếp Eo', 750000, 640000, N'Đường cắt rập tinh xảo tạo độ rủ tự nhiên theo từng bước chân.', 'sp-17.jpg', 15, @CatVay, 1, 1, DATEADD(HOUR, -17, GETDATE())),
    (N'Chân Váy Chữ A Dáng Dài', 460000, NULL, N'Cạp cao tôn vòng eo, chất vải tuyết mưa đứng phom.', 'sp-18.jpg', 24, @CatVay, 0, 1, DATEADD(HOUR, -18, GETDATE())),
    (N'Đầm Sơ Mi Thắt Đai Cổ Điển', 680000, 590000, N'Kết hợp nét thanh lịch của sơ mi và sự nữ tính của dáng đầm xòe nhẹ.', 'sp-19.jpg', 18, @CatVay, 1, 0, DATEADD(HOUR, -19, GETDATE())),
    (N'Đầm Hai Dây Lụa Satin Dự Tiệc', 790000, NULL, N'Bề mặt satin ánh ngọc trai nhẹ nhàng, dây vai tăng chỉnh linh hoạt.', 'sp-20.jpg', 11, @CatVay, 0, 1, DATEADD(HOUR, -20, GETDATE())),

    (N'Túi Xách Da Đeo Vai Tối Giản', 690000, 580000, N'Da tổng hợp cao cấp chống trầy, ngăn chứa rộng rãi.', 'sp-21.jpg', 20, @CatPhuKien, 1, 1, DATEADD(HOUR, -21, GETDATE())),
    (N'Thắt Lưng Da Bò Khóa Kim Mảnh', 350000, NULL, N'Da bò thật nguyên miếng, mặt khóa hợp kim mạ mờ chống gỉ.', 'sp-22.jpg', 40, @CatPhuKien, 0, 0, DATEADD(HOUR, -22, GETDATE())),
    (N'Kính Mát Gọng Acetate Cổ Điển', 490000, 420000, N'Tròng kính chống tia UV400, gọng nhẹ ôm vừa khuôn mặt.', 'sp-23.jpg', 25, @CatPhuKien, 1, 1, DATEADD(HOUR, -23, GETDATE())),
    (N'Khăn Lụa Vuông Họa Tiết Nghệ Thuật', 280000, NULL, N'Dùng buộc cổ, thắt quai túi xách hoặc làm băng đô thời trang.', 'sp-24.jpg', 30, @CatPhuKien, 0, 1, DATEADD(HOUR, -24, GETDATE()))
) AS v(Name, Price, DiscountPrice, Description, ImageUrl, Stock, CategoryId, IsFeatured, IsNew, CreatedAt)
WHERE NOT EXISTS (SELECT 1 FROM Products p WHERE p.Name = v.Name);