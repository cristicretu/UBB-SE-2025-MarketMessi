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

CREATE TABLE ReviewsPictures(
	id INT PRIMARY KEY IDENTITY(1,1),
	url NVARCHAR(256) NOT NULL,
	review_id INT NOT NULL,
	CONSTRAINT FK_Image_Reviews FOREIGN KEY (review_id) REFERENCES Reviews(id)
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



----- ORDERS ------
CREATE TABLE Orders (
    id INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
    cost FLOAT NOT NULL,
    created BIGINT NOT NULL,
    sellerId INT NOT NULL,
    buyerId INT NOT NULL,
    orderStatus NVARCHAR(50) NOT NULL
);


----- CHAT BOT TABLES ------
CREATE TABLE ChatBotNodes
(
	pid INT PRIMARY KEY,
	button_label NVARCHAR(100) NOT NULL,
	label_text NVARCHAR(MAX) NOT NULL,
	response_text NVARCHAR(MAX) NOT NULL,
);

CREATE TABLE ChatBotChildren
(
	cid INT PRIMARY KEY IDENTITY,
	parentID INT FOREIGN KEY REFERENCES ChatBotNodes(pid) NOT NULL,
	childID INT FOREIGN KEY REFERENCES ChatBotNodes(pid) NOT NULL
);

----- CHAT TABLES ------
CREATE TABLE Conversations (
        id INT PRIMARY KEY IDENTITY(1,1),
        user1 INT NOT NULL,
        user2 INT NOT NULL
);

CREATE TABLE Messages (
        id INT PRIMARY KEY IDENTITY(1,1),
        conversationId INT NOT NULL,
	    creator INT NOT NULL,
	    timestamp BIGINT NOT NULL,
        contentType NVARCHAR(50) NOT NULL,
        content NVARCHAR(MAX) NOT NULL,
);



------- DROPS -------
-- Drop Basket-related tables first
DROP TABLE IF EXISTS BasketItemsBuyProducts;
DROP TABLE IF EXISTS Baskets;

-- Drop Bids table (dependent on Users and AuctionProducts)
DROP TABLE IF EXISTS Bids;

-- Drop Auction-related tables
DROP TABLE IF EXISTS AuctionProductProductTags;
DROP TABLE IF EXISTS AuctionProductsImages;
DROP TABLE IF EXISTS AuctionProducts;

-- Drop Borrow-related tables
DROP TABLE IF EXISTS BorrowProductProductTags;
DROP TABLE IF EXISTS BorrowProductImages;
DROP TABLE IF EXISTS BorrowProducts;

-- Drop Buy-related tables
DROP TABLE IF EXISTS BuyProductProductTags;
DROP TABLE IF EXISTS BuyProductImages;
DROP TABLE IF EXISTS BuyProducts;

-- Drop Reviews table (dependent on Users)
DROP TABLE IF EXISTS ReviewsPictures
DROP TABLE IF EXISTS Reviews;

-- Drop product category, condition, and tag-related tables
DROP TABLE IF EXISTS ProductTags;
DROP TABLE IF EXISTS ProductCategories;
DROP TABLE IF EXISTS ProductConditions;

-- Drop Orders table
DROP TABLE IF EXISTS Orders;
-- Drop Chat-related tables
DROP TABLE IF EXISTS Messages;
DROP TABLE IF EXISTS Conversations;
-- Drop ChatBot-related tables
DROP TABLE IF EXISTS ChatBotChildren;
DROP TABLE IF EXISTS ChatBotNodes;

-- Drop Users table last since many tables reference it
DROP TABLE IF EXISTS .Users;







-- SQL QUERRIES FOR INSERTING DATA


-- ADD USERS FIRST
INSERT INTO Users (email, username, userType, balance, rating, passwordHash)
VALUES 
('alice@example.com', 'alice123', 1, 500.00, 4.5, 'hashedpassword1'),
('bob@example.com', 'bob321', 2, 1000.00, 4.8, 'hashedpassword2');

--ADD PRODUCT CONDITIONS, CATEGORIES AND TAGS
INSERT INTO ProductConditions (title, description)
VALUES 
('New', 'Brand new item, unopened'),
('Used', 'Item has been previously used');

INSERT INTO ProductCategories (title, description)
VALUES 
('Electronics', 'Devices like phones, laptops, etc.'),
('Furniture', 'Chairs, tables, beds, etc.');

INSERT INTO ProductTags (title)
VALUES 
('Tech'),
('Home'),
('Vintage');




-- INSERT QUERRIES FOR AUCTION PRODUCTS

INSERT INTO AuctionProducts (title, description, seller_id, condition_id, category_id,    start_datetime, end_datetime, starting_price, current_price)
VALUES 
('Used iPhone 12', 'Still works great. Minor scratches.', 1, 2, 1, '2025-03-25 10:00:00', '2025-03-30 10:00:00', 300.00, 300.00) , 	
('Gaming Laptop', 'High-end gaming laptop with RTX 3070.', 2, 1, 1, '2025-03-26 09:00:00', '2025-03-29 09:00:00', 800.00, 800.00),
('Antique Clock', 'Old collectible wall clock.', 1, 2, 2, '2025-03-24 15:00:00', '2025-03-28 15:00:00', 100.00, 100.00),
('Smartwatch', 'Waterproof smartwatch with GPS.', 2, 1, 1, '2025-03-27 14:00:00', '2025-03-31 14:00:00', 120.00, 120.00),
('Bluetooth Speaker', 'Compact speaker with deep bass.', 1, 1, 1, '2025-03-26 11:00:00', '2025-03-29 11:00:00', 40.00, 40.00);

INSERT INTO AuctionProductsImages (url, product_id)
VALUES 
('https://i.imgur.com/XBpDHa7.jpeg', 1),
('https://i.imgur.com/u9j0U5Y.jpeg', 1),
('https://i.imgur.com/YYXgjHM.jpeg' , 2),
('https://i.imgur.com/Yq7jIzr.jpeg' , 3),
('https://i.imgur.com/4XWqyj5.jpeg' , 3),
('https://i.imgur.com/ZC5UaQZ.jpeg' , 4),
('https://i.imgur.com/grOzWu8.jpeg' , 5),
('https://i.imgur.com/srDfm59.jpeg' , 5);


INSERT INTO AuctionProductProductTags (product_id, tag_id)
VALUES 
(1, 1),
(1, 3),
(2, 1),
(3, 3),
(4 , 1),
(4 , 2),
(5 , 1),
(5 , 2);

INSERT INTO Bids (bidder_id, product_id, price, timestamp)
VALUES 
(2, 1, 350.00, '2025-03-25 12:30:00');




-- INSERT QUERRIES FOR BORROW PRODUCTS

INSERT INTO BorrowProducts (title, description, seller_id, condition_id, category_id, time_limit, start_date, end_date, daily_rate, is_borrowed) 
VALUES 
('Professional Camera', 'DSLR camera perfect for events.',1, 1, 1,'2025-03-30', '2025-03-25', '2025-03-30',12.00, 1),
('Toolbox', 'Complete DIY toolkit for home repairs.',2, 2, 2,'2025-03-30', '2025-03-24', '2025-03-28',4.00, 1);



INSERT INTO BorrowProductImages (url, product_id)
VALUES 
('https://i.imgur.com/yVSgkw1.jpeg', 1),
('https://i.imgur.com/gMaRVZN.jpeg', 1),
('https://i.imgur.com/OTDwYjh.jpeg' , 2),
('https://i.imgur.com/WL5yCjH.jpeg' , 2);

INSERT INTO BorrowProductProductTags (product_id, tag_id)
VALUES 
(1, 2),
(1, 1),
(2, 2);


-- INSERT QUERRIES FOR BUY PRODUCTS

INSERT INTO BuyProducts (title, description, seller_id, condition_id, category_id, price
) VALUES 
('Wireless Headphones', 'Brand new noise-cancelling headphones with long battery life.',2, 1, 1, 120.00),
 ('Wooden Table', 'Solid oak wood dining table.', 1, 2, 2, 250.00),
 ('Gaming Mouse', 'RGB, programmable buttons.', 2, 1, 1, 45.00);

INSERT INTO BuyProductImages (url, product_id)
VALUES 
('https://i.imgur.com/LhlIBMt.jpeg', 1),
('https://i.imgur.com/GmWI7GM.jpeg' , 1),
('https://i.imgur.com/oTYyO1O.jpeg' , 2),
('https://i.imgur.com/DQbBuGr.jpeg' , 2),
('https://i.imgur.com/oDwPDES.jpeg' , 3);

INSERT INTO BuyProductProductTags (product_id, tag_id)
VALUES 
(1, 1),
(2, 2),
(2, 3),
(3, 1);


-- INSERT QUERIES FOR REVIEWS
INSERT INTO Reviews (reviewer_id, seller_id, description, rating)
VALUES 
(1, 2, 'Great seller! The product was exactly as described. My son loves it!', 5.0),
(2, 1, 'Fast shipping and great communication.', 4.5),
(1, 2, 'It said that this was a football, not a rugby ball or whatever. Damn you americans and your football/soccer thing. Football is football, a BALL', 1.0),
(2, 1, 'Excellent service, will buy again.', 5.0);

-- INSERT QUERIES FOR REVIEW IMAGES
INSERT INTO ReviewsPictures (url, review_id)
VALUES 
('https://i.imgur.com/Wr7FSB0.jpeg', 2),
('https://i.imgur.com/yOMoqoS.jpeg', 1),
('https://i.imgur.com/4ikPbty.jpeg', 1),
('https://i.imgur.com/vG2ozCz.jpeg', 3),
('https://i.imgur.com/r3q6HKj.jpeg', 4);

UPDATE BorrowProducts
SET is_borrowed = 0,
    start_date = '2025-03-27',
    time_limit='2025-04-20';

-- Insert bot data into nodes
INSERT INTO ChatBotNodes (pid, button_label, label_text, response_text)
VALUES 
(1, 'empty', 'empty', 'Hi! What can I help you with?'), -- Root node cid = 1
(2, 'Payment Issues', 'I have a problem with buying a new item.', 'Can you specify what issue you came accross during the buying process?'), -- Child 1 of Root cid = 2
(3, 'Selling Issues', 'I have a problem with selling an item.', 'Can you specify what issue you came accross during the selling process?'), -- Child 2 of Root cid = 3
(4, 'Account & Security', 'I have a problem with my account / I ran into a scammer.', 'Can you specify what issue you have regarding your account or security?'), -- Child 3 of Root cid = 4

(5, 'Item not as described', 'I bought an item but it''s different from what I listed. Can I get a refund?', 
 'If you bought an item that is different from the one listed you should go to the listing an immediately report it. If you want to negotiate a refund, please reach out to our live customer support. ' + CHAR(13) + CHAR(10) +
 'If your case is not available for a refund, try negotiating with the seller himself, if possible.'), -- Child 1 of Child 1 of Root cid = 5

(6, 'Seller not responding', 'The seller is not responding to any of my messages. What should I do?',  
 'Wait for the seller to respond, people can take a while to check their messages. If you already waited for a long time, you could report the seller for being inactive and we will try to contact them. ' + CHAR(13) + CHAR(10) +
 'Otherwise we will remove their listing as soon as possible.'), -- Child 2 of Child 1 of Root cid = 6

(7, 'Payment Issues', 'What are the payment methods? I payed for an item but I never received it.', 
 'The payment method depends on the seller. If it''s not clarified in the listing what payment methods are accepted by the seller, you should contact them.'), -- Child 3 of Child 1 of Root cid = 7

(8, 'Buyer not showing up', 'What should I do if a buyer agreed to meet but never showed up?', 
 'In case you agreed to meet with a buyer but they never showed up, you should try contacting them to find out what happened. You could arrange for a new meeting if both of you are open to it. ' + CHAR(13) + CHAR(10) +
 'In case you can''t reach them, just let them go. We understand it''s frustrating to have someone waste your time but unfortunately it happens.'), -- Child 1 of Child 2 of Root cid = 8

 (9, 'Payment disputes', 'The buyer claims they sent payment, but I never received the money. What should I do?', 
 'If the buyer says they sent payment, but you have not received the money, here are the steps you should follow.' + 
 '1. Wait some time, it''s possible that the bank transfer is still pending.' + CHAR(13) + CHAR(10) +
 '2. Make sure the buyer sent the money. Ask them for proof if possible.' + CHAR(13) + CHAR(10) +
 '3. Contact your bank to see if there are any pending transfers.' + CHAR(13) + CHAR(10) +
 'If none of these steps solved your problem, you are probably dealing with a scammer. Feel free to report them and we will review the case!'), -- Child 2 of Child 2 of Root cid = 9

(10, 'Listing getting removed or rejected', 'Why was my listing removed? What are the rules for selling an item?', 
 'If your listing was removed from the store, it''s possible that it did not comply with our policy, or it was a fraudulent listing.' + CHAR(13) + CHAR(10) +
 'To find out why your listing was removed, please contact our live customer support. You can also check out our rules for selling an item by reading our sellers policy statement.'), -- Child 3 of Child 2 of Root cid = 10

(11, 'Account access issues', 'I can''t log into my account or it was restricted. How can I fix this?', 
 'If you have issues logging in or your account was restricted here are the steps you could follow to potentially solve the issue:' + CHAR(13) + CHAR(10) +
 '1. Make sure your login credentials are correct when trying to log in.' + CHAR(13) + CHAR(10) +
 '2. Press the account recovery button and try to recover your account that way.' + CHAR(13) + CHAR(10) +
 '3. If you still can''t log in, or your account was restricted, please contact our live customer support for further help.'), -- Child 1 of Child 3 of Root cid = 11

(12, 'Suspicious messages & scams', 'I received a suspicious message from a buyer/seller. How can I stay safe?', 
 'Scammers are present everywhere, even on our store. If you think you bumped into a scammer or someone suspicious, you should report them to us and we will review their account. ' + CHAR(13) + CHAR(10) +
 'How can you stay safe from scammers? It is essential to not give any sensitive information away. Do not send people extra money other than what the price of the item is. ' + CHAR(13) + CHAR(10) +
 'If someone has scammed you, you should immediately report them and talk to our live customer support to see if you are available for a refund.'), -- Child 2 of Child 3 of Root cid = 12

(13, 'Scam or fraudulent seller', 'I paid for an item but never received it. What can I do?', 
 'If you paid for an item but never received it you should follow the following steps:' + CHAR(13) + CHAR(10) +
 '1. Try to find out more about your item''s whereabouts from the seller.' + CHAR(13) + CHAR(10) +
 '2. If they are ignoring you for a long time or blocked you, immediately report them for being fraudulent!' + CHAR(13) + CHAR(10) +
 '3. Go and talk to our live customer support to try and get a refund.'), -- Child 3 of Child 3 of Root cid = 13

(14, 'Fake or misleading listings', 'How can I report a listing that is scam or fake?', 
 'If you are sure that a listing is a scam or is fake, you could report it by clicking on the report button in the listing.'), -- Child 4 of Child 3 of Root cid = 14

 (15, 'Restart conversation', 'There are no further available options for you to choose from. Would you like to restart this conversation?', 
 'Hi! What can I help you with?'); -- Child of all the last nodes cid = 15

 -- Define parent-child relationships
INSERT INTO ChatBotChildren (parentID, childID) VALUES
(1, 2), (1, 3), (1, 4), -- Assign children to root
(2, 5), (2, 6), (2, 7), -- Assign children to child 1 of root
(3, 8), (3, 9), (3, 10), -- Assign children to child 2 of root
(4, 11), (4, 12), (4, 13), (4, 14), -- Assign children to child 3 of root
(5, 15), (6, 15), (7, 15), (8, 15), (9, 15), (10, 15), (11, 15), (12, 15), (13, 15), (14, 15), -- Assign the restart option to all of the end nodes
(15, 2), (15, 3), (15, 4) -- Assign the restart node the roots children

-- INSERT QUERIES FOR ORDERS
INSERT INTO Orders(name,description,cost,created,sellerId,buyerId,orderStatus) VALUES (
	'book',
	'a cool book',
	10.5,
	1743210476533,
	1,
	0,
	'shipped'
)

INSERT INTO Orders(name,description,cost,created,sellerId,buyerId,orderStatus) VALUES (
	'car',
	'best car',
	50000.0,
	1743210478533,
	0,
	1,
	'shipping'
)

INSERT INTO Orders(name,description,cost,created,sellerId,buyerId,orderStatus) VALUES (
	'iphone 15 pro max',
	'very good condition iphone',
	800.0,
	1743210486533,
	1,
	0,
	'shipping'
)

-- Not "completed" orders
INSERT INTO Orders(name,description,cost,created,sellerId,buyerId,orderStatus) VALUES (
	'bmw m4',
	'50k km , good condition',
	60000.0,
	1743210486533,
	1,
	-1,
	'waiting buyer'
)

INSERT INTO Orders(name,description,cost,created,sellerId,buyerId,orderStatus) VALUES (
	'jordan',
	'limited edition 1/100',
	1500.0,
	1743210496533,
	0,
	-1,
	'waiting buyer'
)
