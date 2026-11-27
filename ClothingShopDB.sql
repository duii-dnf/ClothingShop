-- ============================================================================
-- CLOTHING SHOP DATABASE SCRIPT (SQL SERVER - EF6 DATABASE FIRST / ADO.NET)
-- Server: DESKTOP-OVTMJB9 | Database: ClothingShopDB
-- 14 bảng (có bảng Favorites Yêu thích) | 50 sản phẩm khớp 100% ảnh sp-01..sp-50
-- Ràng buộc CHECK số điện thoại: đúng 10 chữ số, bắt đầu bằng số 0
-- ============================================================================

USE [master];
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ClothingShopDB')
BEGIN
    ALTER DATABASE [ClothingShopDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [ClothingShopDB];
END
GO

CREATE DATABASE [ClothingShopDB];
GO

USE [ClothingShopDB];
GO

-- ============================================================================
-- 1. TẠO CÁC BẢNG, KHÓA CHÍNH VÀ RÀNG BUỘC CHECK (14 BẢNG)
-- ============================================================================

CREATE TABLE [dbo].[Users] (
    [UserId]       INT IDENTITY(1,1) NOT NULL,
    [FullName]     NVARCHAR(100)     NOT NULL,
    [Email]        NVARCHAR(150)     NOT NULL,
    [PasswordHash] NVARCHAR(200)     NOT NULL,
    [Phone]        NVARCHAR(20)      NULL,
    [Address]      NVARCHAR(250)     NULL,
    [Gender]       NVARCHAR(10)      NULL,
    [BirthDate]    DATETIME          NULL,
    [Avatar]       NVARCHAR(300)     NULL,
    [IsActive]     BIT               NOT NULL CONSTRAINT [DF_Users_IsActive] DEFAULT (1),
    [Role]         NVARCHAR(20)      NOT NULL CONSTRAINT [DF_Users_Role] DEFAULT (N'Customer'),
    [CreatedAt]    DATETIME          NOT NULL CONSTRAINT [DF_Users_CreatedAt] DEFAULT (GETDATE()),
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT [UQ_Users_Email] UNIQUE NONCLUSTERED ([Email] ASC),
    CONSTRAINT [CK_Users_Role] CHECK ([Role] IN (N'Admin', N'Staff', N'Customer')),
    CONSTRAINT [CK_Users_Phone] CHECK ([Phone] IS NULL OR [Phone] = N'' OR (LEN([Phone]) = 10 AND [Phone] LIKE N'0[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'))
);
GO

CREATE TABLE [dbo].[UserAddresses] (
    [AddressId]     INT IDENTITY(1,1) NOT NULL,
    [UserId]        INT               NOT NULL,
    [ReceiverName]  NVARCHAR(100)     NOT NULL,
    [ReceiverPhone] NVARCHAR(20)      NOT NULL,
    [Province]      NVARCHAR(100)     NULL,
    [District]      NVARCHAR(100)     NULL,
    [Ward]          NVARCHAR(100)     NULL,
    [DetailAddress] NVARCHAR(255)     NOT NULL,
    [Notes]         NVARCHAR(255)     NULL,
    [IsDefault]     BIT               NOT NULL CONSTRAINT [DF_UserAddresses_IsDefault] DEFAULT (0),
    [CreatedAt]     DATETIME          NOT NULL CONSTRAINT [DF_UserAddresses_CreatedAt] DEFAULT (GETDATE()),
    CONSTRAINT [PK_UserAddresses] PRIMARY KEY CLUSTERED ([AddressId] ASC),
    CONSTRAINT [CK_UserAddresses_Phone] CHECK (LEN([ReceiverPhone]) = 10 AND [ReceiverPhone] LIKE N'0[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]')
);
GO

CREATE TABLE [dbo].[Categories] (
    [CategoryId] INT IDENTITY(1,1) NOT NULL,
    [Name]       NVARCHAR(100)     NOT NULL,
    [ParentId]   INT               NULL,
    [IsActive]   BIT               NOT NULL CONSTRAINT [DF_Categories_IsActive] DEFAULT (1),
    CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([CategoryId] ASC)
);
GO

CREATE TABLE [dbo].[ProductStyles] (
    [StyleId]     INT IDENTITY(1,1) NOT NULL,
    [Code]        NVARCHAR(50)      NOT NULL,
    [Name]        NVARCHAR(100)     NOT NULL,
    [Description] NVARCHAR(255)     NULL,
    [IsActive]    BIT               NOT NULL CONSTRAINT [DF_ProductStyles_IsActive] DEFAULT (1),
    CONSTRAINT [PK_ProductStyles] PRIMARY KEY CLUSTERED ([StyleId] ASC),
    CONSTRAINT [UQ_ProductStyles_Code] UNIQUE NONCLUSTERED ([Code] ASC)
);
GO

CREATE TABLE [dbo].[ProductSizes] (
    [SizeId]    INT IDENTITY(1,1) NOT NULL,
    [Code]      NVARCHAR(20)      NOT NULL,
    [Name]      NVARCHAR(50)      NOT NULL,
    [SortOrder] INT               NOT NULL CONSTRAINT [DF_ProductSizes_SortOrder] DEFAULT (0),
    [IsActive]  BIT               NOT NULL CONSTRAINT [DF_ProductSizes_IsActive] DEFAULT (1),
    CONSTRAINT [PK_ProductSizes] PRIMARY KEY CLUSTERED ([SizeId] ASC),
    CONSTRAINT [UQ_ProductSizes_Code] UNIQUE NONCLUSTERED ([Code] ASC)
);
GO

CREATE TABLE [dbo].[ProductColors] (
    [ColorId]  INT IDENTITY(1,1) NOT NULL,
    [Code]     NVARCHAR(30)      NOT NULL,
    [Name]     NVARCHAR(50)      NOT NULL,
    [HexCode]  NVARCHAR(20)      NOT NULL,
    [ImageUrl] NVARCHAR(300)     NULL,
    [IsActive] BIT               NOT NULL CONSTRAINT [DF_ProductColors_IsActive] DEFAULT (1),
    CONSTRAINT [PK_ProductColors] PRIMARY KEY CLUSTERED ([ColorId] ASC),
    CONSTRAINT [UQ_ProductColors_Code] UNIQUE NONCLUSTERED ([Code] ASC)
);
GO

CREATE TABLE [dbo].[Products] (
    [ProductId]     INT IDENTITY(1,1) NOT NULL,
    [Name]          NVARCHAR(200)     NOT NULL,
    [Description]   NVARCHAR(MAX)     NULL,
    [Price]         DECIMAL(18, 2)    NOT NULL,
    [DiscountPrice] DECIMAL(18, 2)    NULL,
    [ImageUrl]      NVARCHAR(300)     NULL,
    [Gender]        NVARCHAR(10)      NULL,
    [IsNew]         BIT               NOT NULL CONSTRAINT [DF_Products_IsNew] DEFAULT (0),
    [IsFeatured]    BIT               NOT NULL CONSTRAINT [DF_Products_IsFeatured] DEFAULT (0),
    [Sizes]         NVARCHAR(100)     NULL,
    [Colors]        NVARCHAR(100)     NULL,
    [Stock]         INT               NOT NULL CONSTRAINT [DF_Products_Stock] DEFAULT (0),
    [IsActive]      BIT               NOT NULL CONSTRAINT [DF_Products_IsActive] DEFAULT (1),
    [CreatedAt]     DATETIME          NOT NULL CONSTRAINT [DF_Products_CreatedAt] DEFAULT (GETDATE()),
    [CategoryId]    INT               NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([ProductId] ASC)
);
GO

CREATE TABLE [dbo].[ProductStyleMappings] (
    [ProductId] INT NOT NULL,
    [StyleId]   INT NOT NULL,
    CONSTRAINT [PK_ProductStyleMappings] PRIMARY KEY CLUSTERED ([ProductId] ASC, [StyleId] ASC)
);
GO

CREATE TABLE [dbo].[ProductVariants] (
    [VariantId] INT IDENTITY(1,1) NOT NULL,
    [ProductId] INT               NOT NULL,
    [SizeId]    INT               NULL,
    [ColorId]   INT               NULL,
    [SKU]       NVARCHAR(50)      NOT NULL,
    [Price]     DECIMAL(18, 2)    NULL,
    [Stock]     INT               NOT NULL CONSTRAINT [DF_ProductVariants_Stock] DEFAULT (0),
    [IsActive]  BIT               NOT NULL CONSTRAINT [DF_ProductVariants_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME          NOT NULL CONSTRAINT [DF_ProductVariants_CreatedAt] DEFAULT (GETDATE()),
    CONSTRAINT [PK_ProductVariants] PRIMARY KEY CLUSTERED ([VariantId] ASC)
);
GO

CREATE TABLE [dbo].[InventoryLogs] (
    [LogId]          INT IDENTITY(1,1) NOT NULL,
    [ProductId]      INT               NOT NULL,
    [VariantId]      INT               NULL,
    [ChangeType]     NVARCHAR(50)      NOT NULL,
    [QuantityChange] INT               NOT NULL,
    [StockAfter]     INT               NOT NULL,
    [ReferenceType]  NVARCHAR(50)      NULL,
    [ReferenceId]    INT               NULL,
    [Note]           NVARCHAR(500)     NULL,
    [CreatedAt]      DATETIME          NOT NULL CONSTRAINT [DF_InventoryLogs_CreatedAt] DEFAULT (GETDATE()),
    [CreatedBy]      NVARCHAR(100)     NULL,
    CONSTRAINT [PK_InventoryLogs] PRIMARY KEY CLUSTERED ([LogId] ASC)
);
GO

CREATE TABLE [dbo].[Coupons] (
    [CouponId]        INT IDENTITY(1,1) NOT NULL,
    [Code]            NVARCHAR(30)      NOT NULL,
    [DiscountPercent] INT               NOT NULL,
    [StartDate]       DATETIME          NOT NULL,
    [EndDate]         DATETIME          NOT NULL,
    [MinOrder]        DECIMAL(18, 2)    NOT NULL CONSTRAINT [DF_Coupons_MinOrder] DEFAULT (0),
    [UsageLimit]      INT               NOT NULL CONSTRAINT [DF_Coupons_UsageLimit] DEFAULT (100),
    [UsedCount]       INT               NOT NULL CONSTRAINT [DF_Coupons_UsedCount] DEFAULT (0),
    [IsActive]        BIT               NOT NULL CONSTRAINT [DF_Coupons_IsActive] DEFAULT (1),
    CONSTRAINT [PK_Coupons] PRIMARY KEY CLUSTERED ([CouponId] ASC),
    CONSTRAINT [UQ_Coupons_Code] UNIQUE NONCLUSTERED ([Code] ASC)
);
GO

CREATE TABLE [dbo].[Orders] (
    [OrderId]        INT IDENTITY(1,1) NOT NULL,
    [UserId]         INT               NOT NULL,
    [OrderDate]      DATETIME          NOT NULL CONSTRAINT [DF_Orders_OrderDate] DEFAULT (GETDATE()),
    [PaymentDate]    DATETIME          NULL,
    [PaymentStatus]  NVARCHAR(20)      NOT NULL CONSTRAINT [DF_Orders_PaymentStatus] DEFAULT (N'Unpaid'),
    [ReceiverName]   NVARCHAR(100)     NOT NULL,
    [Phone]          NVARCHAR(20)      NOT NULL,
    [Address]        NVARCHAR(250)     NOT NULL,
    [PaymentMethod]  NVARCHAR(30)      NULL,
    [Status]         NVARCHAR(20)      NOT NULL CONSTRAINT [DF_Orders_Status] DEFAULT (N'Pending'),
    [Note]           NVARCHAR(300)     NULL,
    [CouponId]       INT               NULL,
    [Subtotal]       DECIMAL(18, 2)    NOT NULL CONSTRAINT [DF_Orders_Subtotal] DEFAULT (0),
    [ShippingFee]    DECIMAL(18, 2)    NOT NULL CONSTRAINT [DF_Orders_ShippingFee] DEFAULT (0),
    [DiscountAmount] DECIMAL(18, 2)    NOT NULL CONSTRAINT [DF_Orders_DiscountAmount] DEFAULT (0),
    [TotalAmount]    DECIMAL(18, 2)    NOT NULL CONSTRAINT [DF_Orders_TotalAmount] DEFAULT (0),
    CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([OrderId] ASC),
    CONSTRAINT [CK_Orders_Phone] CHECK (LEN([Phone]) = 10 AND [Phone] LIKE N'0[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]')
);
GO

CREATE TABLE [dbo].[OrderDetails] (
    [OrderDetailId]    INT IDENTITY(1,1) NOT NULL,
    [OrderId]          INT               NOT NULL,
    [ProductId]        INT               NOT NULL,
    [ProductVariantId] INT               NULL,
    [Size]             NVARCHAR(20)      NULL,
    [Color]            NVARCHAR(30)      NULL,
    [Quantity]         INT               NOT NULL,
    [UnitPrice]        DECIMAL(18, 2)    NOT NULL,
    CONSTRAINT [PK_OrderDetails] PRIMARY KEY CLUSTERED ([OrderDetailId] ASC)
);
GO

-- Bảng 14: Sản phẩm yêu thích (Wishlist / Favorites)
CREATE TABLE [dbo].[Favorites] (
    [FavoriteId] INT IDENTITY(1,1) NOT NULL,
    [UserId]     INT               NOT NULL,
    [ProductId]  INT               NOT NULL,
    [CreatedAt]  DATETIME          NOT NULL CONSTRAINT [DF_Favorites_CreatedAt] DEFAULT (GETDATE()),
    CONSTRAINT [PK_Favorites] PRIMARY KEY CLUSTERED ([FavoriteId] ASC),
    CONSTRAINT [UQ_Favorites_User_Product] UNIQUE NONCLUSTERED ([UserId] ASC, [ProductId] ASC)
);
GO

-- ============================================================================
-- 2. TẠO KHÓA NGOẠI (FOREIGN KEYS)
-- ============================================================================

ALTER TABLE [dbo].[UserAddresses]
    ADD CONSTRAINT [FK_UserAddresses_Users]
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[Categories]
    ADD CONSTRAINT [FK_Categories_Parent]
    FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Categories] ([CategoryId]);
GO

ALTER TABLE [dbo].[Products]
    ADD CONSTRAINT [FK_Products_Categories]
    FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([CategoryId]);
GO

ALTER TABLE [dbo].[ProductStyleMappings]
    ADD CONSTRAINT [FK_ProductStyleMappings_Products]
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[ProductStyleMappings]
    ADD CONSTRAINT [FK_ProductStyleMappings_ProductStyles]
    FOREIGN KEY ([StyleId]) REFERENCES [dbo].[ProductStyles] ([StyleId]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[ProductVariants]
    ADD CONSTRAINT [FK_ProductVariants_Products]
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[ProductVariants]
    ADD CONSTRAINT [FK_ProductVariants_ProductSizes]
    FOREIGN KEY ([SizeId]) REFERENCES [dbo].[ProductSizes] ([SizeId]);
GO

ALTER TABLE [dbo].[ProductVariants]
    ADD CONSTRAINT [FK_ProductVariants_ProductColors]
    FOREIGN KEY ([ColorId]) REFERENCES [dbo].[ProductColors] ([ColorId]);
GO

ALTER TABLE [dbo].[InventoryLogs]
    ADD CONSTRAINT [FK_InventoryLogs_Products]
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId]);
GO

ALTER TABLE [dbo].[InventoryLogs]
    ADD CONSTRAINT [FK_InventoryLogs_ProductVariants]
    FOREIGN KEY ([VariantId]) REFERENCES [dbo].[ProductVariants] ([VariantId]);
GO

ALTER TABLE [dbo].[Orders]
    ADD CONSTRAINT [FK_Orders_Users]
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);
GO

ALTER TABLE [dbo].[Orders]
    ADD CONSTRAINT [FK_Orders_Coupons]
    FOREIGN KEY ([CouponId]) REFERENCES [dbo].[Coupons] ([CouponId]);
GO

ALTER TABLE [dbo].[OrderDetails]
    ADD CONSTRAINT [FK_OrderDetails_Orders]
    FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([OrderId]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[OrderDetails]
    ADD CONSTRAINT [FK_OrderDetails_Products]
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId]);
GO

ALTER TABLE [dbo].[OrderDetails]
    ADD CONSTRAINT [FK_OrderDetails_ProductVariants]
    FOREIGN KEY ([ProductVariantId]) REFERENCES [dbo].[ProductVariants] ([VariantId]);
GO

ALTER TABLE [dbo].[Favorites]
    ADD CONSTRAINT [FK_Favorites_Users]
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[Favorites]
    ADD CONSTRAINT [FK_Favorites_Products]
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId]) ON DELETE CASCADE;
GO

-- ============================================================================
-- 3. NẠP DỮ LIỆU MẪU (50 SẢN PHẨM KHỚP 100% ẢNH sp-01.jpg -> sp-50.jpg)
-- ============================================================================

-- 3.1 Users (Admin: admin@shop.com/Admin@123 | Staff: nhanvien@shop.com/Staff@123 | Customer: khach1@shop.com/Khach@123)
SET IDENTITY_INSERT [dbo].[Users] ON;
INSERT INTO [dbo].[Users] ([UserId], [FullName], [Email], [PasswordHash], [Phone], [Address], [Gender], [IsActive], [Role], [CreatedAt]) VALUES
(1, N'Quản trị viên', N'admin@shop.com', N'10000.r+v3EIb60XeSC8n2/Sg32Q==.TFb1w3uUa1rlOGZdTEXGwrQ0yGY6Kh1Ckn41X+RIxzQ=', N'0900000001', N'TP. Hồ Chí Minh', N'Nam', 1, N'Admin', GETDATE()),
(2, N'Lê Minh Nhân Viên', N'nhanvien@shop.com', N'10000.UlC+d6M1Wmy3Q+rcp82rxA==.Dj9m547xAVlEtjfrSg62yMmyjHHdUM+2wI5nnIUVniA=', N'0900000004', N'Quận Tân Phú, TP.HCM', N'Nam', 1, N'Staff', GETDATE()),
(3, N'Nguyễn Văn An', N'khach1@shop.com', N'10000.ryambZsAlOIknH7Au9lFmA==.fIWs6Ey57/XWzwYRLC7W5esM6rqmIjtFqW1Ai/DRTTk=', N'0900000002', N'12 Nguyễn Huệ, Quận 1, TP.HCM', N'Nam', 1, N'Customer', GETDATE()),
(4, N'Trần Thị Bình', N'khach2@shop.com', N'10000.R6GlmlPFcYC5viLC14olug==.R/033nlqMlztNrGoUKwfkAR0Ga7EMLlolBKvRbcF0kQ=', N'0900000003', N'45 Lê Lợi, Quận 3, TP.HCM', N'Nữ', 1, N'Customer', GETDATE());
SET IDENTITY_INSERT [dbo].[Users] OFF;
GO

-- 3.2 UserAddresses
SET IDENTITY_INSERT [dbo].[UserAddresses] ON;
INSERT INTO [dbo].[UserAddresses] ([AddressId], [UserId], [ReceiverName], [ReceiverPhone], [Province], [District], [Ward], [DetailAddress], [Notes], [IsDefault], [CreatedAt]) VALUES
(1, 3, N'Nguyễn Văn An', N'0900000002', N'TP. Hồ Chí Minh', N'Quận 1', N'Phường Bến Nghé', N'12 Nguyễn Huệ', N'Giao giờ hành chính', 1, GETDATE()),
(2, 4, N'Trần Thị Bình', N'0900000003', N'TP. Hồ Chí Minh', N'Quận 3', N'Phường Võ Thị Sáu', N'45 Lê Lợi', N'Gọi trước khi giao', 1, GETDATE());
SET IDENTITY_INSERT [dbo].[UserAddresses] OFF;
GO

-- 3.3 Categories (4 loại cha + 11 loại con)
SET IDENTITY_INSERT [dbo].[Categories] ON;
INSERT INTO [dbo].[Categories] ([CategoryId], [Name], [ParentId], [IsActive]) VALUES
(1, N'Áo', NULL, 1),
(2, N'Quần', NULL, 1),
(3, N'Váy - Đầm', NULL, 1),
(4, N'Phụ kiện', NULL, 1),
(5, N'Áo thun & Hoodie', 1, 1),
(6, N'Áo sơ mi & Len', 1, 1),
(7, N'Áo khoác', 1, 1),
(8, N'Quần jeans & Cargo', 2, 1),
(9, N'Quần short', 2, 1),
(10, N'Quần jogger & Tây', 2, 1),
(11, N'Chân váy', 3, 1),
(12, N'Đầm dự tiệc', 3, 1),
(13, N'Nón', 4, 1),
(14, N'Kính mát', 4, 1),
(15, N'Balo & Túi xách', 4, 1);
SET IDENTITY_INSERT [dbo].[Categories] OFF;
GO

-- 3.4 ProductStyles
SET IDENTITY_INSERT [dbo].[ProductStyles] ON;
INSERT INTO [dbo].[ProductStyles] ([StyleId], [Code], [Name], [Description], [IsActive]) VALUES
(1, N'BASIC',  N'Basic Minimal',  N'Tối giản, tinh tế, dễ phối đồ hằng ngày', 1),
(2, N'STREET', N'Streetwear GenZ',N'Năng động, cá tính, phom rộng thời thượng', 1),
(3, N'CHIC',   N'Hàn Quốc Chic',  N'Thanh lịch, trẻ trung chuẩn gu Hàn Quốc', 1),
(4, N'PARTY',  N'Dự tiệc & Dạo phố', N'Nổi bật, cuốn hút cho sự kiện và cuối tuần', 1);
SET IDENTITY_INSERT [dbo].[ProductStyles] OFF;
GO

-- 3.5 ProductSizes
SET IDENTITY_INSERT [dbo].[ProductSizes] ON;
INSERT INTO [dbo].[ProductSizes] ([SizeId], [Code], [Name], [SortOrder], [IsActive]) VALUES
(1, N'S',        N'Size S',   1, 1),
(2, N'M',        N'Size M',   2, 1),
(3, N'L',        N'Size L',   3, 1),
(4, N'XL',       N'Size XL',  4, 1),
(5, N'29',       N'Size 29',  5, 1),
(6, N'30',       N'Size 30',  6, 1),
(7, N'Freesize', N'Freesize', 7, 1);
SET IDENTITY_INSERT [dbo].[ProductSizes] OFF;
GO

-- 3.6 ProductColors
SET IDENTITY_INSERT [dbo].[ProductColors] ON;
INSERT INTO [dbo].[ProductColors] ([ColorId], [Code], [Name], [HexCode], [IsActive]) VALUES
(1,  N'BLACK',  N'Đen',         N'#18181b', 1),
(2,  N'WHITE',  N'Trắng',       N'#f8f9fa', 1),
(3,  N'GRAY',   N'Xám Tiêu',    N'#8b9099', 1),
(4,  N'DENIM',  N'Xanh Denim',  N'#3b5b82', 1),
(5,  N'BEIGE',  N'Kem Be',      N'#dfd3c3', 1),
(6,  N'OLIVE',  N'Xanh Rêu',    N'#5a6345', 1),
(7,  N'BROWN',  N'Nâu Đất',     N'#a6674c', 1),
(8,  N'PINK',   N'Hồng Pastel', N'#dfa094', 1),
(9,  N'RED',    N'Đỏ Đô',       N'#c81d25', 1),
(10, N'YELLOW', N'Vàng Nghệ',   N'#f4b400', 1);
SET IDENTITY_INSERT [dbo].[ProductColors] OFF;
GO

-- 3.7 Products (50 sản phẩm chuẩn hình sp-01.jpg -> sp-50.jpg)
SET IDENTITY_INSERT [dbo].[Products] ON;
INSERT INTO [dbo].[Products] ([ProductId], [Name], [Description], [Price], [DiscountPrice], [ImageUrl], [Gender], [IsNew], [IsFeatured], [Sizes], [Colors], [Stock], [IsActive], [CreatedAt], [CategoryId]) VALUES
(1,  N'Áo Thun Oversize Heavyweight Cotton 280GSM', N'Áo thun phom rộng boxy vải cotton 280GSM dày dặn, xử lý wash đen than cá tính, cổ bo dệt dày giữ phom bền đẹp chuẩn Streetwear Gen-Z.', 320000, 249000, N'sp-01.jpg', N'Unisex', 1, 1, N'M,L,XL', N'Đen', 36, 1, DATEADD(DAY,-1,GETDATE()), 5),
(2,  N'Áo Thun Cotton Supima Trắng Basic', N'Áo thun nam cổ tròn tối giản màu trắng tinh khiết, dệt từ 100% cotton Supima mềm mịn, thấm hút mồ hôi cực tốt.', 240000, 189000, N'sp-02.jpg', N'Nam', 1, 0, N'S,M,L,XL', N'Trắng', 40, 1, DATEADD(DAY,-2,GETDATE()), 5),
(3,  N'Áo Sơ Mi Chambray Xanh Họa Tiết Tim Nhỏ', N'Áo sơ mi vải chambray xanh denim mềm nhẹ thoáng khí, điểm nhấn họa tiết chấm tim trắng tinh tế cùng túi ngực tiện lợi.', 420000, 359000, N'sp-03.jpg', N'Unisex', 1, 1, N'S,M,L', N'Xanh Denim', 28, 1, DATEADD(DAY,-3,GETDATE()), 6),
(4,  N'Áo Khoác Da Biker Streetwear Cổ Bẻ', N'Áo khoác da PU cao cấp mềm mịn, thiết kế dáng biker cổ bẻ khuy bấm kim loại, khóa kéo chéo cá tính cùng hai túi ngực tiện dụng.', 950000, 790000, N'sp-04.jpg', N'Unisex', 1, 1, N'M,L,XL', N'Đen', 20, 1, DATEADD(DAY,-4,GETDATE()), 7),
(5,  N'Áo Hoodie Nỉ Bông Oversize Streetwear', N'Hoodie nỉ bông dày ấm tông xám tiêu cổ điển, phom rộng vai trễ chuẩn gu Gen-Z, mũ trùm hai lớp đứng dáng kèm túi kangaroo.', 490000, NULL, N'sp-05.jpg', N'Unisex', 1, 1, N'M,L,XL', N'Xám Tiêu', 32, 1, DATEADD(DAY,-5,GETDATE()), 5),
(6,  N'Quần Jeans Denim Ống Đứng Classic', N'Quần jeans chất denim nguyên bản bền màu, đường chỉ nổi sắc nét, phom ống đứng dễ mặc che khuyết điểm chân.', 590000, 499000, N'sp-06.jpg', N'Nam', 0, 1, N'29,30', N'Đen,Xanh Denim', 30, 1, DATEADD(DAY,-6,GETDATE()), 8),
(7,  N'Quần Cargo Túi Hộp Kaki Ống Rộng', N'Quần cargo túi hộp chất kaki cotton twill màu xanh rêu olive cực trendy, hai túi hộp lớn hai bên đùi kèm dây rút bo ống.', 540000, 449000, N'sp-07.jpg', N'Unisex', 1, 1, N'29,30,L', N'Xanh Rêu', 34, 1, DATEADD(DAY,-7,GETDATE()), 8),
(8,  N'Quần Short Jeans Wash Rách Xắn Gấu', N'Quần short jeans cạp cao tông xanh wash sáng trẻ trung, nhấn nhá chi tiết mài rách nhẹ và gấu xắn gọn gàng hack chân dài.', 350000, 289000, N'sp-08.jpg', N'Nữ', 1, 0, N'S,M,L', N'Xanh Denim', 26, 1, DATEADD(DAY,-8,GETDATE()), 9),
(9,  N'Set Hoodie Croptop & Quần Jogger Streetwear', N'Set đồ nỉ thể thao Gen-Z nổi bật tông vàng nghệ rực rỡ, gồm áo hoodie croptop kết hợp quần jogger cạp chun dây rút.', 680000, 569000, N'sp-09.jpg', N'Nữ', 1, 1, N'S,M,L', N'Vàng Nghệ', 24, 1, DATEADD(DAY,-9,GETDATE()), 5),
(10, N'Áo Khoác Len Lưới Poncho Tua Rua Boho', N'Áo choàng len dệt lưới thoáng nhẹ màu kem be phối viền tua rua phóng khoáng chuẩn phong cách Boho chic đi biển, du lịch.', 390000, NULL, N'sp-10.jpg', N'Nữ', 1, 0, N'Freesize', N'Kem Be', 22, 1, DATEADD(DAY,-10,GETDATE()), 6),
(11, N'Đầm Dạ Hội Satin Xòe Dài Cổ Yếm', N'Đầm dạ hội dáng dài tùng xòe bồng bềnh màu đỏ rực quyến rũ, chất liệu satin cao cấp bắt sáng sang trọng, cổ yếm tôn vai.', 1250000, 990000, N'sp-11.jpg', N'Nữ', 1, 1, N'S,M,L', N'Đỏ Đô', 14, 1, DATEADD(DAY,-11,GETDATE()), 12),
(12, N'Chân Váy Xếp Ly Xòe Cạp Cao Nơ Eo', N'Chân váy chữ A xếp ly to bản màu đen tuyền điểm nhấn nơ thắt eo duyên dáng, vải tuyết mưa đứng phom chuẩn đẹp.', 380000, 319000, N'sp-12.jpg', N'Nữ', 0, 1, N'S,M,L', N'Đen', 28, 1, DATEADD(DAY,-12,GETDATE()), 11),
(13, N'Quần Jogger Lụa Bo Gấu Túi Hộp Nữ', N'Quần jogger nữ chất vải lụa gân mềm mát tông hồng đất pastel ngọt ngào, cạp chun co giãn kèm hai túi nắp trước cá tính.', 440000, NULL, N'sp-13.jpg', N'Nữ', 1, 0, N'S,M,L', N'Hồng Pastel', 24, 1, DATEADD(DAY,-13,GETDATE()), 10),
(14, N'Áo Khoác Bomber Gió Minimalist Khóa Kéo', N'Áo khoác bomber vải gió trượt nước màu nâu đất thời thượng, bo cổ và bo tay dệt kim êm ái, có túi khóa kéo tay áo tiện lợi.', 620000, 519000, N'sp-14.jpg', N'Unisex', 1, 1, N'M,L,XL', N'Nâu Đất', 26, 1, DATEADD(DAY,-14,GETDATE()), 7),
(15, N'Balo Laptop Streetwear Chống Nước Tối Giản', N'Balo thiết kế tối giản màu đen tuyền, chất vải polyester dày chống thấm nước tốt, ngăn chống sốc đựng vừa laptop 15.6 inch.', 550000, 459000, N'sp-15.jpg', N'Unisex', 1, 1, N'Freesize', N'Đen', 30, 1, DATEADD(DAY,-15,GETDATE()), 15),
(16, N'Nón Lưỡi Trai Trucker Lưới Trắng Basic', N'Nón lưỡi trai dáng trucker phối lưới thoáng khí phía sau, tông trắng tinh khôi tối giản siêu dễ phối với mọi outfit dạo phố.', 190000, NULL, N'sp-16.jpg', N'Unisex', 1, 0, N'Freesize', N'Trắng', 35, 1, DATEADD(DAY,-16,GETDATE()), 13),
(17, N'Túi Đeo Chéo Da Trần Trám Khóa Vàng', N'Túi đeo chéo form hộp nhỏ gọn bằng da mềm trần chỉ chevron sang trọng, khóa kéo ánh vàng tinh tế cho đi tiệc hay café.', 890000, 749000, N'sp-17.jpg', N'Nữ', 1, 1, N'Freesize', N'Đen', 18, 1, DATEADD(DAY,-17,GETDATE()), 15),
(18, N'Kính Mát Gọng Kim Loại Tròn Retro', N'Kính mát dáng tròn cổ điển gọng kim loại mảnh ánh vàng thanh lịch, tròng kính râm chống tia UV400 bảo vệ mắt tối ưu.', 290000, 229000, N'sp-18.jpg', N'Unisex', 1, 1, N'Freesize', N'Đen', 40, 1, DATEADD(DAY,-18,GETDATE()), 14),
(19, N'Áo Thun In Mèo Thần Tài Japanese Streetwear', N'Áo thun cotton màu kem be in họa tiết Mèo Thần Tài phong cách Nhật Bản màu xanh cobalt sắc nét, vải thoáng mát không bong tróc.', 290000, 229000, N'sp-19.jpg', N'Unisex', 1, 1, N'S,M,L,XL', N'Kem Be', 30, 1, DATEADD(DAY,-19,GETDATE()), 5),
(20, N'Áo Thun Đen In Chữ Minimalist Form Suông', N'Áo thun đen cổ tròn form suông hiện đại với dòng chữ tối giản giữa ngực, chất cotton 2 chiều mềm mịn, mặc đơn hay layer đều đẹp.', 260000, 199000, N'sp-20.jpg', N'Unisex', 1, 0, N'M,L,XL', N'Đen', 34, 1, DATEADD(DAY,-20,GETDATE()), 5),
(21, N'Áo Thun Streetwear Logo Tròn 705 Đen', N'Áo thun đen in logo tròn 705 bên ngực trái cực gọn mắt, vải cotton co giãn tốt giữ màu sau nhiều lần giặt.', 280000, NULL, N'sp-21.jpg', N'Unisex', 0, 0, N'M,L,XL', N'Đen', 28, 1, DATEADD(DAY,-21,GETDATE()), 5),
(22, N'Áo Sweater Nỉ Trắng Cổ Tròn Oversize', N'Áo sweater nỉ bông cổ tròn màu trắng tinh khôi, bo tay bo gấu chắc chắn, mặc đi học máy lạnh hay dạo phố mùa thu đều siêu êm.', 420000, 349000, N'sp-22.jpg', N'Unisex', 1, 1, N'M,L,XL', N'Trắng', 26, 1, DATEADD(DAY,-22,GETDATE()), 5),
(23, N'Áo Sơ Mi Dài Tay Cotton Phối Màu Basic', N'Áo sơ mi dài tay vải cotton dệt gân nhẹ, nhiều gam màu trầm ấm dễ phối đồ công sở lẫn dạo phố cuối tuần.', 390000, NULL, N'sp-23.jpg', N'Nam', 0, 0, N'M,L,XL', N'Nâu Đất', 24, 1, DATEADD(DAY,-23,GETDATE()), 6),
(24, N'Áo Sơ Mi Trắng Công Sở Slimfit Chống Nhăn', N'Áo sơ mi trắng cổ đức đứng phom, chất vải bamboo chống nhăn thoáng mát, cực kỳ chỉn chu khi kết hợp cùng cà vạt và quần tây.', 460000, 389000, N'sp-24.jpg', N'Nam', 0, 1, N'S,M,L,XL', N'Trắng', 32, 1, DATEADD(DAY,-24,GETDATE()), 6),
(25, N'Áo Sơ Mi Oxford Dài Tay Cao Cấp Cổ Điển', N'Áo sơ mi dệt Oxford dày dặn mềm mại, đường may tỉ mỉ với 3 phiên bản màu Xanh Navy chấm bi, Kem sữa và Đỏ rượu vang sang trọng.', 480000, 399000, N'sp-25.jpg', N'Nam', 1, 1, N'M,L,XL', N'Xanh Denim,Kem Be', 30, 1, DATEADD(DAY,-25,GETDATE()), 6),
(26, N'Áo Sơ Mi Trắng Form Regular Nam Lịch Lãm', N'Áo sơ mi nam form regular thoải mái, cổ bẻ thanh lịch kèm logo thêu nhỏ tinh tế, xắn tay áo cực nam tính và hiện đại.', 440000, NULL, N'sp-26.jpg', N'Nam', 0, 0, N'M,L,XL', N'Trắng', 28, 1, DATEADD(DAY,-26,GETDATE()), 6),
(27, N'Quần Tây Nỉ Xám Ống Đứng Smart Casual', N'Quần tây chất nỉ mềm màu xám muối tiêu đứng phom, dễ dàng phối cùng áo thun đen và sneaker trắng theo phong cách Hàn Quốc.', 520000, 439000, N'sp-27.jpg', N'Nam', 1, 1, N'29,30', N'Xám Tiêu', 24, 1, DATEADD(DAY,-27,GETDATE()), 10),
(28, N'Áo Khoác Jeans Denim Wash Sáng Form Rộng', N'Áo khoác denim wash xanh sáng phong cách vintage, vải jeans dày dặn lên form chuẩn streetwear, cực hợp khoác ngoài áo thun.', 690000, 589000, N'sp-28.jpg', N'Unisex', 1, 1, N'M,L,XL', N'Xanh Denim', 22, 1, DATEADD(DAY,-28,GETDATE()), 7),
(29, N'Áo Khoác Măng Tô Dạ Dáng Dài Thắt Đai', N'Áo khoác măng tô dạ ép màu nâu camel sang chảnh, dáng dài qua gối kèm đai thắt eo tôn dáng, đậm khí chất thời trang châu Âu.', 1350000, 1090000, N'sp-29.jpg', N'Nữ', 1, 1, N'S,M,L', N'Nâu Đất', 14, 1, DATEADD(DAY,-29,GETDATE()), 7),
(30, N'Áo Khoác Phao Trần Bông Cổ Đứng Streetwear', N'Áo khoác phao trần bông siêu nhẹ giữ ấm tốt màu đen tuyền, khóa kéo kim loại bền bỉ, dễ dàng layer bên ngoài hoodie nổi bật.', 780000, 649000, N'sp-30.jpg', N'Nam', 1, 0, N'M,L,XL', N'Đen', 20, 1, DATEADD(DAY,-30,GETDATE()), 7),
(31, N'Quần Jeans Nữ Rách Gối Thêu Sticker Y2K', N'Quần jeans nữ dáng boyfriend năng động, chi tiết rách gối cá tính phối patch thêu sticker nổi bật đậm chất Y2K Gen-Z.', 560000, 469000, N'sp-31.jpg', N'Nữ', 1, 1, N'29,30', N'Xanh Denim', 24, 1, DATEADD(DAY,-31,GETDATE()), 8),
(32, N'Quần Kaki Chino Nam Dáng Slimfit Tôn Dáng', N'Quần kaki chino màu be rêu chất cotton co giãn nhẹ, xắn gấu mặc cùng sneaker trắng và áo khoác denim cực kỳ trẻ trung.', 450000, 379000, N'sp-32.jpg', N'Nam', 0, 1, N'29,30', N'Kem Be', 30, 1, DATEADD(DAY,-32,GETDATE()), 10),
(33, N'Quần Jeans Raw Denim Tối Màu Ống Suông', N'Quần jeans denim tối màu không mài rách, phom ống suông cổ điển bền bỉ, mặc đi làm văn phòng hay dạo phố đều lịch sự.', 580000, NULL, N'sp-33.jpg', N'Nam', 0, 0, N'29,30', N'Đen', 26, 1, DATEADD(DAY,-33,GETDATE()), 8),
(34, N'Quần Kaki Ống Lửng In Họa Tiết Trứng Ốp La GenZ', N'Quần kaki màu vàng nổi bật in họa tiết trứng ốp la vui nhộn độc đáo, lưng thun dây rút thoải mái dành cho tín đồ streetwear phá cách.', 490000, 399000, N'sp-34.jpg', N'Unisex', 1, 1, N'M,L', N'Vàng Nghệ', 20, 1, DATEADD(DAY,-34,GETDATE()), 8),
(35, N'Quần Ống Rộng Kẻ Sọc Dọc Cạp Cao Hack Dáng', N'Quần nữ ống rộng siêu hack chân với họa tiết kẻ sọc dọc đen trắng cá tính, vải mềm rủ tạo hiệu ứng kéo dài đôi chân tối đa.', 460000, 389000, N'sp-35.jpg', N'Nữ', 1, 0, N'S,M,L', N'Đen', 22, 1, DATEADD(DAY,-35,GETDATE()), 10),
(36, N'Đầm Maxi Hai Dây Xẻ Tà Đi Biển Xanh Pastel', N'Đầm maxi dáng dài màu xanh dương pastel dịu mắt, cổ chữ V buộc eo quyến rũ cùng đường xẻ tà bay bổng tuyệt đẹp khi đi biển.', 620000, 499000, N'sp-36.jpg', N'Nữ', 1, 1, N'S,M,L', N'Xanh Denim', 18, 1, DATEADD(DAY,-36,GETDATE()), 12),
(37, N'Đầm Hoa Nhí Đỏ Thắt Đai Cổ Chữ V Cổ Điển', N'Đầm xòe họa tiết hoa nhí nền đỏ rực rỡ kèm thắt lưng bản vừa tôn vòng eo con kiến, chất voan lụa mềm mát đầy nữ tính.', 590000, 479000, N'sp-37.jpg', N'Nữ', 1, 0, N'S,M,L', N'Đỏ Đô', 20, 1, DATEADD(DAY,-37,GETDATE()), 12),
(38, N'Đầm Trễ Vai Bèo Nhún Trắng Công Chúa', N'Đầm ngắn trễ vai màu trắng tinh khôi phối viền bèo nhún tiểu thư, chun eo co giãn dễ mặc tôn dáng mảnh mai ngày hè.', 540000, 449000, N'sp-38.jpg', N'Nữ', 1, 1, N'S,M,L', N'Trắng', 22, 1, DATEADD(DAY,-38,GETDATE()), 12),
(39, N'Đầm Body Trễ Vai Dạ Tiệc Tím Mận Quyến Rũ', N'Đầm ôm body dáng trễ vai tông đỏ mận sang trọng, chất thun co giãn dày dặn tôn trọn đường cong cho buổi tiệc tối.', 790000, NULL, N'sp-39.jpg', N'Nữ', 0, 1, N'S,M', N'Đỏ Đô', 16, 1, DATEADD(DAY,-39,GETDATE()), 12),
(40, N'Chân Váy Midi Xếp Ly Dáng Dài Thanh Lịch', N'Chân váy midi xếp ly nhuyễn màu nâu be ngọt ngào, độ dài qua gối thanh lịch chuẩn phong cách công sở và vintage Hàn Quốc.', 430000, 359000, N'sp-40.jpg', N'Nữ', 0, 1, N'S,M,L', N'Kem Be', 26, 1, DATEADD(DAY,-40,GETDATE()), 11),
(41, N'Túi Xách Tay Da Trơn Khóa Kim Loại Đỏ Cam', N'Túi xách tay form đứng sang trọng bằng da trơn bóng mịn màu đỏ cam nổi bật, khóa cài kim loại bạc tinh xảo đi kèm dây đeo vai.', 850000, 699000, N'sp-41.jpg', N'Nữ', 1, 1, N'Freesize', N'Đỏ Đô', 16, 1, DATEADD(DAY,-41,GETDATE()), 15),
(42, N'Túi Xách Đan Mây Phối Nắp Da Cam Đất Vintage', N'Túi xách phong cách resort kết hợp thân đan thủ công và nắp da mềm màu cam đất thời thượng, cực hợp diện cùng đầm maxi mùa hè.', 790000, 649000, N'sp-42.jpg', N'Nữ', 1, 0, N'Freesize', N'Nâu Đất', 18, 1, DATEADD(DAY,-42,GETDATE()), 15),
(43, N'Nón Lưỡi Trai Cotton Wash Xám Than Cổ Điển', N'Nón lưỡi trai vải kaki cotton wash bạc màu xám than cực bụi bặm, phom mềm ôm đầu tự nhiên hợp mọi khuôn mặt Nam & Nữ.', 210000, 169000, N'sp-43.jpg', N'Unisex', 1, 1, N'Freesize', N'Xám Tiêu', 38, 1, DATEADD(DAY,-43,GETDATE()), 13),
(44, N'Mũ Len Beanie Streetwear Mùa Đông Nhiều Màu', N'Mũ len gân dày ấm áp gắn tag vải cá tính, có đủ các tông màu trending Đen, Hồng Pastel và Vàng Nghệ cực dễ phối hoodie.', 180000, 149000, N'sp-44.jpg', N'Unisex', 1, 1, N'Freesize', N'Đen,Hồng Pastel', 40, 1, DATEADD(DAY,-44,GETDATE()), 13),
(45, N'Kính Mát Wayfarer Gọng Đen Classic UV400', N'Kính mát dáng vuông Wayfarer kinh điển gọng đen bóng chắc chắn, tròng chống chói UV400 phù hợp đi nắng, lái xe và chụp ảnh.', 320000, 259000, N'sp-45.jpg', N'Unisex', 0, 1, N'Freesize', N'Đen', 36, 1, DATEADD(DAY,-45,GETDATE()), 14),
(46, N'Kính Mát Mắt Mèo Gọng Kem Thời Trang Nữ', N'Kính mát gọng mắt mèo màu kem pastel kết hợp tròng chuyển màu nâu trà cực sang chảnh, giúp gương mặt thon gọn sắc nét hơn.', 310000, 249000, N'sp-46.jpg', N'Nữ', 1, 0, N'Freesize', N'Kem Be', 30, 1, DATEADD(DAY,-46,GETDATE()), 14),
(47, N'Áo Sơ Mi Trắng Thêu Hoa Cúc Buộc Vạt Vintage', N'Áo sơ mi tay ngắn màu trắng thêu họa tiết hoa cúc nổi tỉ mỉ, thiết kế buộc vạt trước eo cực ngọt ngào chuẩn vibe nàng thơ.', 390000, 319000, N'sp-47.jpg', N'Nữ', 1, 1, N'S,M,L', N'Trắng', 24, 1, DATEADD(DAY,-47,GETDATE()), 6),
(48, N'Áo Khoác Blazer Dáng Dài Nam Tông Nâu Camel', N'Áo khoác blazer dáng dài màu nâu camel cực bảnh bao, đường cắt may chuẩn xác tôn vai rộng, phối cực ăn ý với áo thun trắng.', 980000, 820000, N'sp-48.jpg', N'Nam', 1, 1, N'M,L,XL', N'Nâu Đất', 18, 1, DATEADD(DAY,-48,GETDATE()), 7),
(49, N'Áo Khoác Dạ Kẻ Ô Vuông Dáng Dài Mùa Thu', N'Áo khoác dạ dáng dài họa tiết mảng màu hình học nâu cam - xám ấm áp, nổi bật giữa phố thu đông đầy tính nghệ thuật.', 1150000, 920000, N'sp-49.jpg', N'Nữ', 1, 0, N'S,M,L', N'Nâu Đất', 14, 1, DATEADD(DAY,-49,GETDATE()), 7),
(50, N'Túi Đeo Vai Da Mềm Nạm Đinh Xám Pastel', N'Túi xách nắp gập tông xám ghi thanh lịch, nhấn nhá hàng đinh tán kim loại ánh vàng cùng dây xích đeo vai cực kỳ thời thượng.', 760000, 629000, N'sp-50.jpg', N'Nữ', 1, 1, N'Freesize', N'Xám Tiêu', 20, 1, DATEADD(DAY,-50,GETDATE()), 15);
SET IDENTITY_INSERT [dbo].[Products] OFF;
GO

-- 3.8 ProductStyleMappings (Quan hệ N-N cho 50 sản phẩm)
INSERT INTO [dbo].[ProductStyleMappings] ([ProductId], [StyleId])
SELECT p.ProductId,
       CASE
           WHEN p.CategoryId IN (12) THEN 4
           WHEN p.CategoryId IN (6, 11, 10) THEN 3
           WHEN p.CategoryId IN (5, 7, 8, 9) THEN 2
           ELSE 1
       END
FROM [dbo].[Products] p;
GO

-- 3.9 ProductVariants (Tự động sinh 2 biến thể chuẩn Size & Đúng Màu thực tế cho từng sản phẩm 1..50)
INSERT INTO [dbo].[ProductVariants] ([ProductId], [SizeId], [ColorId], [SKU], [Price], [Stock], [IsActive], [CreatedAt])
SELECT p.ProductId,
       CASE WHEN p.CategoryId IN (13,14,15) OR p.Sizes = N'Freesize' THEN 7
            WHEN p.CategoryId IN (8,10) AND p.Gender = N'Nam' THEN 5
            ELSE 2 END,
       CASE
           WHEN p.Colors LIKE N'Trắng%' THEN 2
           WHEN p.Colors LIKE N'Xám%' THEN 3
           WHEN p.Colors LIKE N'Xanh Denim%' THEN 4
           WHEN p.Colors LIKE N'Kem%' THEN 5
           WHEN p.Colors LIKE N'Xanh Rêu%' THEN 6
           WHEN p.Colors LIKE N'Nâu%' THEN 7
           WHEN p.Colors LIKE N'Hồng%' THEN 8
           WHEN p.Colors LIKE N'Đỏ%' THEN 9
           WHEN p.Colors LIKE N'Vàng%' THEN 10
           ELSE 1 END,
       'SP' + RIGHT('000' + CAST(p.ProductId AS VARCHAR(10)), 3) + '-V1',
       NULL,
       p.Stock / 2,
       1,
       GETDATE()
FROM [dbo].[Products] p;

INSERT INTO [dbo].[ProductVariants] ([ProductId], [SizeId], [ColorId], [SKU], [Price], [Stock], [IsActive], [CreatedAt])
SELECT p.ProductId,
       CASE WHEN p.CategoryId IN (13,14,15) OR p.Sizes = N'Freesize' THEN 7
            WHEN p.CategoryId IN (8,10) AND p.Gender = N'Nam' THEN 6
            ELSE 3 END,
       CASE
           WHEN p.Colors LIKE N'%Xanh Denim' THEN 4
           WHEN p.Colors LIKE N'%Hồng Pastel' THEN 8
           WHEN p.Colors LIKE N'%Kem Be' THEN 5
           WHEN p.Colors LIKE N'Trắng%' THEN 2
           WHEN p.Colors LIKE N'Xám%' THEN 3
           WHEN p.Colors LIKE N'Xanh Rêu%' THEN 6
           WHEN p.Colors LIKE N'Nâu%' THEN 7
           WHEN p.Colors LIKE N'Đỏ%' THEN 9
           WHEN p.Colors LIKE N'Vàng%' THEN 10
           ELSE 1 END,
       'SP' + RIGHT('000' + CAST(p.ProductId AS VARCHAR(10)), 3) + '-V2',
       NULL,
       p.Stock - (p.Stock / 2),
       1,
       GETDATE()
FROM [dbo].[Products] p
WHERE p.Sizes <> N'Freesize';
GO

-- 3.10 Coupons
SET IDENTITY_INSERT [dbo].[Coupons] ON;
INSERT INTO [dbo].[Coupons] ([CouponId], [Code], [DiscountPercent], [StartDate], [EndDate], [MinOrder], [UsageLimit], [UsedCount], [IsActive]) VALUES
(1, N'SALE10',  10, DATEADD(MONTH,-1,GETDATE()), DATEADD(MONTH,6,GETDATE()), 200000, 100, 1, 1),
(2, N'NEWUSER', 15, DATEADD(MONTH,-1,GETDATE()), DATEADD(MONTH,6,GETDATE()), 300000,  50, 0, 1),
(3, N'HETHAN5',  5, DATEADD(MONTH,-3,GETDATE()), DATEADD(MONTH,-1,GETDATE()),     0, 100, 0, 1);
SET IDENTITY_INSERT [dbo].[Coupons] OFF;
GO

-- 3.11 Orders (8 đơn hàng mẫu đa dạng trạng thái để thống kê doanh thu)
SET IDENTITY_INSERT [dbo].[Orders] ON;
INSERT INTO [dbo].[Orders] ([OrderId], [UserId], [OrderDate], [PaymentDate], [PaymentStatus], [ReceiverName], [Phone], [Address], [PaymentMethod], [Status], [Note], [CouponId], [Subtotal], [ShippingFee], [DiscountAmount], [TotalAmount]) VALUES
(1, 3, DATEADD(DAY,-25,GETDATE()), DATEADD(DAY,-23,GETDATE()), N'Paid',   N'Nguyễn Văn An', N'0900000002', N'12 Nguyễn Huệ, Quận 1, TP.HCM', N'COD',          N'Completed', NULL, NULL,  498000, 30000,      0,  528000),
(2, 4, DATEADD(DAY,-18,GETDATE()), DATEADD(DAY,-18,GETDATE()), N'Paid',   N'Trần Thị Bình', N'0900000003', N'45 Lê Lợi, Quận 3, TP.HCM',     N'Chuyển khoản', N'Completed', NULL,    1,  990000,     0,  99000,  891000),
(3, 3, DATEADD(DAY,-12,GETDATE()), DATEADD(DAY,-10,GETDATE()), N'Paid',   N'Nguyễn Văn An', N'0900000002', N'12 Nguyễn Huệ, Quận 1, TP.HCM', N'COD',          N'Completed', NULL, NULL, 1239000,     0,      0, 1239000),
(4, 4, DATEADD(DAY,-7, GETDATE()), DATEADD(DAY,-7, GETDATE()), N'Paid',   N'Trần Thị Bình', N'0900000003', N'45 Lê Lợi, Quận 3, TP.HCM',     N'Chuyển khoản', N'Completed', NULL, NULL, 1068000,     0,      0, 1068000),
(5, 3, DATEADD(DAY,-4, GETDATE()), NULL,                       N'Unpaid', N'Nguyễn Văn An', N'0900000002', N'12 Nguyễn Huệ, Quận 1, TP.HCM', N'COD',          N'Shipping',  NULL, NULL,  948000,     0,      0,  948000),
(6, 4, DATEADD(DAY,-2, GETDATE()), DATEADD(DAY,-2, GETDATE()), N'Paid',   N'Trần Thị Bình', N'0900000003', N'45 Lê Lợi, Quận 3, TP.HCM',     N'Chuyển khoản', N'Confirmed', NULL, NULL,  569000,     0,      0,  569000),
(7, 3, DATEADD(DAY,-1, GETDATE()), NULL,                       N'Unpaid', N'Nguyễn Văn An', N'0900000002', N'12 Nguyễn Huệ, Quận 1, TP.HCM', N'COD',          N'Pending',   N'Giao buổi sáng', NULL, 519000, 0, 0, 519000),
(8, 4, GETDATE(),                  NULL,                       N'Unpaid', N'Trần Thị Bình', N'0900000003', N'45 Lê Lợi, Quận 3, TP.HCM',     N'COD',          N'Cancelled', N'Khách đổi size', NULL, 289000, 30000, 0, 319000);
SET IDENTITY_INSERT [dbo].[Orders] OFF;
GO

-- 3.12 OrderDetails
INSERT INTO [dbo].[OrderDetails] ([OrderId], [ProductId], [ProductVariantId], [Size], [Color], [Quantity], [UnitPrice]) VALUES
(1,  1,  1, N'Size M',   N'Đen',       2, 249000),
(2, 11, 11, N'Size M',   N'Đỏ Đô',     1, 990000),
(3,  4,  4, N'Size M',   N'Đen',       1, 790000),
(3,  7,  7, N'Size M',   N'Xanh Rêu',  1, 449000),
(4, 17, 17, N'Freesize', N'Đen',       1, 749000),
(4, 12, 12, N'Size M',   N'Đen',       1, 319000),
(5,  6,  6, N'Size 29',  N'Đen',       1, 499000),
(5,  7,  7, N'Size M',   N'Xanh Rêu',  1, 449000),
(6,  9,  9, N'Size M',   N'Vàng Nghệ', 1, 569000),
(7, 14, 14, N'Size M',   N'Nâu Đất',   1, 519000),
(8,  8,  8, N'Size M',   N'Xanh Denim',1, 289000);
GO

-- 3.13 InventoryLogs
INSERT INTO [dbo].[InventoryLogs] ([ProductId], [VariantId], [ChangeType], [QuantityChange], [StockAfter], [ReferenceType], [ReferenceId], [Note], [CreatedAt], [CreatedBy]) VALUES
(1,  1, N'InitialStock', 20, 20, N'Seed',  1, N'Khởi tạo tồn kho ban đầu', DATEADD(DAY,-30,GETDATE()), N'Admin'),
(1,  1, N'OrderPlaced',  -2, 18, N'Order', 1, N'Đặt hàng #1',              DATEADD(DAY,-25,GETDATE()), N'Nguyễn Văn An'),
(4,  4, N'OrderPlaced',  -1, 10, N'Order', 3, N'Đặt hàng #3',              DATEADD(DAY,-12,GETDATE()), N'Nguyễn Văn An'),
(7,  7, N'OrderPlaced',  -1, 17, N'Order', 3, N'Đặt hàng #3',              DATEADD(DAY,-12,GETDATE()), N'Nguyễn Văn An');
GO

-- 3.14 Favorites (Sản phẩm yêu thích mẫu)
INSERT INTO [dbo].[Favorites] ([UserId], [ProductId], [CreatedAt]) VALUES
(3, 1,  DATEADD(DAY,-3,GETDATE())),
(3, 4,  DATEADD(DAY,-2,GETDATE())),
(3, 7,  DATEADD(DAY,-1,GETDATE())),
(4, 9,  DATEADD(DAY,-2,GETDATE())),
(4, 11, DATEADD(DAY,-1,GETDATE())),
(4, 17, GETDATE());
GO
