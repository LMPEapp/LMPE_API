-- -----------------------------------------------------
-- Script : 03_add_table_notification.sql
-- Objectif : Ajout des tables de notifications pour Message, Agenda, CourbeCA et Bulletin
-- -----------------------------------------------------

USE LMPE;

-- -----------------------------------------------------
-- Table des messages non lus
-- -----------------------------------------------------
CREATE TABLE Notification_User_Message (
    UserId BIGINT UNSIGNED NOT NULL,
    MessageId BIGINT UNSIGNED NOT NULL,
    PRIMARY KEY (UserId, MessageId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (MessageId) REFERENCES Message(Id) ON DELETE CASCADE
);

CREATE INDEX idx_Notification_User_Message ON Notification_User_Message(UserId);

-- -----------------------------------------------------
-- Table des Bulletins non vus
-- -----------------------------------------------------
CREATE TABLE Notification_User_Bulletin (
    UserId BIGINT UNSIGNED NOT NULL,
    BulletinId BIGINT UNSIGNED NOT NULL,
    PRIMARY KEY (UserId, BulletinId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (BulletinId) REFERENCES Bulletin(Id) ON DELETE CASCADE
);

CREATE INDEX idx_Notification_User_Bulletin ON Notification_User_Bulletin(UserId);

-- -----------------------------------------------------
-- Script terminé
-- -----------------------------------------------------
