USE MarketPlace;
GO

CREATE TABLE Users (
    id INT PRIMARY KEY IDENTITY(1,1),
    email NVARCHAR(255) NOT NULL,
    username NVARCHAR(50) NOT NULL,
    userType INT NOT NULL,
	balance FLOAT NOT NULL,
	rating FLOAT NULL,
    passwordHash NVARCHAR(255) NOT NULL,
    CONSTRAINT UQ_Users_Email UNIQUE (email),
    CONSTRAINT UQ_Users_Username UNIQUE (username)
);

CREATE TABLE Reviews (
	id INT PRIMARY KEY IDENTITY(1,1),
	reviewer_id INT NOT NULL,
	seller_id INT NOT NULL,
	description NVARCHAR(500) NULL,
	rating FLOAT NOT NULL,
	CONSTRAINT FK_Reviews_ReviewerUsers FOREIGN KEY (reviewer_id) REFERENCES Users(id),
	CONSTRAINT FK_Reviews_SellerUsers FOREIGN KEY (seller_id) REFERENCES Users(id),
);

CREATE TABLE ProductConditions (
    id INT PRIMARY KEY IDENTITY(1,1),
    title NVARCHAR(100) NOT NULL,
    description NVARCHAR(500) NULL,
	CONSTRAINT UQ_ProductConditions_Title UNIQUE (title),
);

CREATE TABLE ProductCategories (
    id INT PRIMARY KEY IDENTITY(1,1),
    title NVARCHAR(100) NOT NULL,
    description NVARCHAR(500) NULL,
	CONSTRAINT UQ_ProductCategories_Title UNIQUE (title),
);

CREATE TABLE ProductTags (
    id INT PRIMARY KEY IDENTITY(1,1),
    title NVARCHAR(100) NOT NULL,
	CONSTRAINT UQ_ProductTags_Title UNIQUE (title),
);

----- PRODUCTS ------
-- BUY PRODUCTS --

CREATE TABLE BuyProducts (
	id INT PRIMARY KEY IDENTITY(1,1),
    	title NVARCHAR(100) NOT NULL,
	description NVARCHAR(3000) NULL,
	seller_id INT NOT NULL,
	condition_id INT NOT NULL,
	category_id INT NOT NULL,
    	price FLOAT NOT NULL,
	CONSTRAINT FK_Products_Users FOREIGN KEY (seller_id) REFERENCES Users(id),
	CONSTRAINT FK_Products_ProductConditions FOREIGN KEY (condition_id) REFERENCES ProductConditions(id),
	CONSTRAINT FK_Products_ProductCategories FOREIGN KEY (category_id) REFERENCES ProductCategories(id),
);

CREATE TABLE BuyProductImages (
	id INT PRIMARY KEY IDENTITY(1,1),
	url NVARCHAR(256) NOT NULL,
	product_id INT NOT NULL,
	CONSTRAINT FK_Image_BuyProductsProduct FOREIGN KEY (product_id) REFERENCES BuyProducts(id)
);

CREATE TABLE BuyProductProductTags (
	id INT PRIMARY KEY IDENTITY(1,1),
	product_id INT NOT NULL,
	tag_id INT NOT NULL,
	CONSTRAINT FK_BuyProductProductTags_BuyProducts FOREIGN KEY (product_id) REFERENCES BuyProducts(id),
	CONSTRAINT FK_BuyProductProductTags_ProductTags FOREIGN KEY (tag_id) REFERENCES ProductTags(id)
);

-- BORROW PRODUCTS --
CREATE TABLE BorrowProducts (
    id INT PRIMARY KEY IDENTITY(1,1),
    title NVARCHAR(100) NOT NULL,
    description NVARCHAR(3000) NULL,
    seller_id INT NOT NULL,
    condition_id INT NOT NULL,
    category_id INT NOT NULL,
    time_limit DATE NOT NULL,
    start_date DATE NULL,
    end_date DATE NULL,
    daily_rate FLOAT NOT NULL,
    is_borrowed BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_BorrowProducts_Users FOREIGN KEY (seller_id) REFERENCES Users(id),
    CONSTRAINT FK_BorrowProducts_ProductConditions FOREIGN KEY (condition_id) REFERENCES ProductConditions(id),
    CONSTRAINT FK_BorrowProducts_ProductCategories FOREIGN KEY (category_id) REFERENCES ProductCategories(id)
)

CREATE TABLE BorrowProductImages (
	id INT PRIMARY KEY IDENTITY(1,1),
	url NVARCHAR(256) NOT NULL,
	product_id INT NOT NULL,
	CONSTRAINT FK_Image_BorrowProduct FOREIGN KEY (product_id) REFERENCES BorrowProducts(id)
);

CREATE TABLE BorrowProductProductTags (
	id INT PRIMARY KEY IDENTITY(1,1),
	product_id INT NOT NULL,
	tag_id INT NOT NULL,
	CONSTRAINT FK_BorrowProductProductTags_BorrowProducts FOREIGN KEY (product_id) REFERENCES BorrowProducts(id),
	CONSTRAINT FK_BorrowProductProductTags_ProductTags FOREIGN KEY (tag_id) REFERENCES ProductTags(id)
);

-- AUCTION PRODUCTS --
CREATE TABLE AuctionProducts (
	id INT PRIMARY KEY IDENTITY(1,1),
    	title NVARCHAR(100) NOT NULL,
	description NVARCHAR(3000) NULL,
	seller_id INT NOT NULL,
	condition_id INT NOT NULL,
	category_id INT NOT NULL,
	start_datetime DATETIME NOT NULL,
	end_datetime DATETIME NOT NULL,
	starting_price FLOAT NOT NULL,
	current_price FLOAT NOT NULL,
	CONSTRAINT FK_AuctionProducts_Seller FOREIGN KEY (seller_id) REFERENCES Users(id),
	CONSTRAINT FK_AuctionProducts_ProductConditions FOREIGN KEY (condition_id) REFERENCES ProductConditions(id),
	CONSTRAINT FK_AuctionProducts_ProductCategories FOREIGN KEY (category_id) REFERENCES ProductCategories(id),
);

CREATE TABLE AuctionProductsImages (
	id INT PRIMARY KEY IDENTITY(1,1),
	url NVARCHAR(256) NOT NULL,
	product_id INT NOT NULL,
	CONSTRAINT FK_Image_AuctionProduct FOREIGN KEY (product_id) REFERENCES AuctionProducts(id)
);

CREATE TABLE AuctionProductProductTags (
	id INT PRIMARY KEY IDENTITY(1,1),
	product_id INT NOT NULL,
	tag_id INT NOT NULL,
	CONSTRAINT FK_AuctionProductProductTags_AuctionProducts FOREIGN KEY (product_id) REFERENCES AuctionProducts(id),
	CONSTRAINT FK_AuctionProductProductTags_ProductTags FOREIGN KEY (tag_id) REFERENCES ProductTags(id)
);

CREATE TABLE Bids (
	id INT PRIMARY KEY IDENTITY(1,1),
	bidder_id INT NOT NULL,
	product_id INT NOT NULL,
	price FLOAT NOT NULL,
	timestamp DATETIME NOT NULL,
	CONSTRAINT FK_Bids_Users FOREIGN KEY (bidder_id) REFERENCES Users(id),
	CONSTRAINT FK_Bids_AuctionProducts FOREIGN KEY (product_id) REFERENCES AuctionProducts(id)
);

----- BASKET ------
CREATE TABLE Baskets (
	id INT PRIMARY KEY IDENTITY(1,1),
	buyer_id INT NOT NULL,
	CONSTRAINT FK_Baskets_Users FOREIGN KEY (buyer_id) REFERENCES Users(id)
);

CREATE TABLE BasketItemsBuyProducts (
	id INT PRIMARY KEY IDENTITY(1,1),
	basket_id INT NOT NULL,
	product_id INT NOT NULL,
	quantity INT NOT NULL DEFAULT(1),
	price FLOAT NOT NULL,
	CONSTRAINT FK_BasketItems_Basket FOREIGN KEY (basket_id) REFERENCES Baskets(id),
	CONSTRAINT FK_BasketItems_Product FOREIGN KEY (product_id) REFERENCES BuyProducts(id),
);