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
-- Table des événements Agenda non vus
-- -----------------------------------------------------
CREATE TABLE Notification_User_Agenda (
    UserId BIGINT UNSIGNED NOT NULL,
    AgendaId BIGINT UNSIGNED NOT NULL,
    PRIMARY KEY (UserId, AgendaId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (AgendaId) REFERENCES Agenda(Id) ON DELETE CASCADE
);

CREATE INDEX idx_Notification_User_Agenda ON Notification_User_Agenda(UserId);

-- -----------------------------------------------------
-- Table des CourbeCA non vus (par exemple nouvelles lignes ajoutées)
-- -----------------------------------------------------
CREATE TABLE Notification_User_CourbeCA (
    UserId BIGINT UNSIGNED NOT NULL,
    CourbeCAId BIGINT UNSIGNED NOT NULL,
    PRIMARY KEY (UserId, CourbeCAId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (CourbeCAId) REFERENCES CourbeCA(Id) ON DELETE CASCADE
);

CREATE INDEX idx_Notification_User_CourbeCA ON Notification_User_CourbeCA(UserId);

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
