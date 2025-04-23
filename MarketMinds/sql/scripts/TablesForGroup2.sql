-- Create ReturnRequests table
CREATE TABLE ReturnRequests (
    id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    product_id INT NULL,
    return_type NVARCHAR(50) NOT NULL,
    description NVARCHAR(1000) NOT NULL,
    request_date DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_ReturnRequests_Users FOREIGN KEY (user_id) REFERENCES Users(id)
);
