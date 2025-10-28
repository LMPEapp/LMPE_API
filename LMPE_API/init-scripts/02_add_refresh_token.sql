-- -----------------------------------------------------
-- LMPE Database
-- MariaDB / MySQL
-- -----------------------------------------------------

USE LMPE;

CREATE TABLE RefreshTokens (
    Id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    UserId BIGINT UNSIGNED NOT NULL,
    RefreshToken VARCHAR(255) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

