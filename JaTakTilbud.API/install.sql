USE master;
GO

/* =========================================================
   DROP DATABASE IF EXISTS (SAFE)
   ========================================================= */

IF DB_ID('JaTakTilbudDb') IS NOT NULL
BEGIN
    ALTER DATABASE JaTakTilbudDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE JaTakTilbudDb;
END
GO

/* =========================================================
   CREATE DATABASE
   ========================================================= */

CREATE DATABASE JaTakTilbudDb;
GO

USE JaTakTilbudDb;
GO

/* =========================================================
   TABLES
   ========================================================= */

-- =========================
-- USERS
-- =========================
CREATE TABLE [Users] (
    Id INT PRIMARY KEY IDENTITY,

    username NVARCHAR(100) NOT NULL,
    passwordHash NVARCHAR(255) NOT NULL,
    email NVARCHAR(255),
    phone NVARCHAR(50),
    accountStatus NVARCHAR(50) NOT NULL,

    createdAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- =========================
-- STORES
-- =========================
CREATE TABLE Stores (
    Id INT PRIMARY KEY IDENTITY,

    name NVARCHAR(255) NOT NULL,
    address NVARCHAR(255) NOT NULL
);

-- =========================
-- USERSTORE (MANY-TO-MANY)
-- role:
-- 0 = User
-- 1 = Admin
-- =========================
CREATE TABLE UserStores (
    Id INT PRIMARY KEY IDENTITY,

    userId_FK INT NOT NULL,
    storeId_FK INT NOT NULL,

    FOREIGN KEY (userId_FK) REFERENCES [Users](Id),
    FOREIGN KEY (storeId_FK) REFERENCES Stores(Id),

    role INT NOT NULL,
    createdAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT UQ_UserStore UNIQUE (userId_FK, storeId_FK)
);

-- =========================
-- PRODUCTS
-- =========================
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY,

    name NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
	imageBlob VARBINARY(MAX) NULL
    isActive BIT NOT NULL DEFAULT 1
);

-- =========================
-- PRODUCT PRICES (TEMPORAL)
-- =========================
CREATE TABLE ProductPrices (
    Id INT PRIMARY KEY IDENTITY,

    productId_FK INT NOT NULL,
    FOREIGN KEY (productId_FK) REFERENCES Products(Id),

    amount DECIMAL(10,2) NOT NULL,
    validFrom DATETIME2 NOT NULL,
    validTo DATETIME2 NULL
);

-- =========================
-- CAMPAIGNS (BELONG TO STORE)
-- =========================
CREATE TABLE Campaigns (
    Id INT PRIMARY KEY IDENTITY,

    storeId_FK INT NOT NULL,
    FOREIGN KEY (storeId_FK) REFERENCES Stores(Id),

    title NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
	imageBlob VARBINARY(MAX) NULL
    createdAt DATETIME2 NOT NULL,
    startTime DATETIME2 NOT NULL,
    endTime DATETIME2 NOT NULL,
    isActive BIT NOT NULL
);

-- =========================
-- CAMPAIGN PRODUCTS
-- =========================
CREATE TABLE CampaignProducts (
    Id INT PRIMARY KEY IDENTITY,

    campaignId_FK INT NOT NULL,
    productId_FK INT NOT NULL,

    FOREIGN KEY (campaignId_FK) REFERENCES Campaigns(Id),
    FOREIGN KEY (productId_FK) REFERENCES Products(Id),

    offerPrice DECIMAL(10,2) NOT NULL,
    quantity INT NOT NULL,
    reservedQuantity INT NOT NULL DEFAULT 0,
	imageBlob VARBINARY(MAX) NULL
	
    CONSTRAINT UQ_CampaignProduct UNIQUE (campaignId_FK, productId_FK)
);

-- =========================
-- RESERVATIONS
-- =========================
CREATE TABLE Reservations (
    Id INT PRIMARY KEY IDENTITY,

    userId_FK INT NOT NULL,
    campaignProductId_FK INT NOT NULL,

    FOREIGN KEY (userId_FK) REFERENCES [Users](Id),
    FOREIGN KEY (campaignProductId_FK) REFERENCES CampaignProducts(Id),

    collectingNumber NVARCHAR(100) NOT NULL,
    reservedAt DATETIME2 NOT NULL,
    quantity INT NOT NULL,
    status NVARCHAR(50) NOT NULL
);

-- =========================
-- FAVORITES
-- =========================
CREATE TABLE Favorites (
    Id INT PRIMARY KEY IDENTITY,

    userId_FK INT NOT NULL,
    campaignProductId_FK INT NOT NULL,

    FOREIGN KEY (userId_FK) REFERENCES [Users](Id),
    FOREIGN KEY (campaignProductId_FK) REFERENCES CampaignProducts(Id),

    savedAt DATETIME2 NOT NULL,

    CONSTRAINT UQ_Favorite UNIQUE (userId_FK, campaignProductId_FK)
);

GO

/* =========================================================
   MINIMAL SEED DATA
   ========================================================= */

-- Admin user
INSERT INTO [Users] (username, passwordHash, accountStatus)
VALUES ('admin', 'hashed_password_here', 'Active');

-- Default store
INSERT INTO Stores (name, address)
VALUES ('Default Store', 'Main Street 1');

-- Link admin to store (role = 1 = Admin)
INSERT INTO UserStores (userId_FK, storeId_FK, role)
VALUES (1, 1, 1);

-- Default campaign
INSERT INTO Campaigns (storeId_FK, title, description, createdAt, startTime, endTime, isActive)
VALUES (
    1,
    'Standard Kampagne',
    'Default kampagne oprettet ved system start',
    GETDATE(),
    GETDATE(),
    DATEADD(DAY, 7, GETDATE()),
    1
);

GO