-- -----------------------------------------------------
-- LMPE Database
-- MariaDB / MySQL
-- -----------------------------------------------------

CREATE DATABASE IF NOT EXISTS LMPE;
USE LMPE;

-- -----------------------------------------------------
-- Table Users
-- -----------------------------------------------------
CREATE TABLE Users (
    Id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    Pseudo VARCHAR(50) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    UrlImage VARCHAR(255),
    IsAdmin BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- -----------------------------------------------------
-- Table GroupeConversation
-- -----------------------------------------------------
CREATE TABLE GroupeConversation (
    Id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- -----------------------------------------------------
-- Table interm diaire User_Groupe (relation many-to-many)
-- -----------------------------------------------------
CREATE TABLE User_Groupe (
    UserId BIGINT UNSIGNED NOT NULL,
    GroupeId BIGINT UNSIGNED NOT NULL,
    PRIMARY KEY (UserId, GroupeId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (GroupeId) REFERENCES GroupeConversation(Id) ON DELETE CASCADE
);

-- -----------------------------------------------------
-- Table Message
-- -----------------------------------------------------
CREATE TABLE Message (
    Id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    GroupeId BIGINT UNSIGNED NOT NULL,
    UserId BIGINT UNSIGNED NOT NULL,
    Type ENUM('texte','image','video','fichier') DEFAULT 'texte',
    Content TEXT,             -- pour texte ou URL du fichier
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (GroupeId) REFERENCES GroupeConversation(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- -----------------------------------------------------
-- Table Agenda (multi-utilisateurs)
-- -----------------------------------------------------
CREATE TABLE Agenda (
    Id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Description TEXT,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    CreatedBy BIGINT UNSIGNED,
    IsPublic BOOLEAN DEFAULT FALSE,  -- si TRUE = tout le monde voit
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE SET NULL
);

-- Table interm diaire Agenda_User pour agendas priv s ou partag s
CREATE TABLE Agenda_User (
    AgendaId BIGINT UNSIGNED NOT NULL,
    UserId BIGINT UNSIGNED NOT NULL,
    PRIMARY KEY (AgendaId, UserId),
    FOREIGN KEY (AgendaId) REFERENCES Agenda(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- -----------------------------------------------------
-- Table CourbeCA (li e   chaque utilisateur)
-- Amount positif = revenu, n gatif = d pense
-- Ne supprime pas les lignes si l'utilisateur est supprim 
-- -----------------------------------------------------
CREATE TABLE CourbeCA (
    Id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    UserId BIGINT UNSIGNED NOT NULL,
    DatePoint DATE NOT NULL,
    Amount DECIMAL(12,2) NOT NULL,
    Description VARCHAR(255),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE NO ACTION
);

-- -----------------------------------------------------
-- Table Bulletin
-- -----------------------------------------------------
CREATE TABLE Bulletin (
    Id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    UserId BIGINT UNSIGNED NOT NULL,       -- destinataire
    CreatedBy BIGINT UNSIGNED,             -- qui a cr   le bulletin
    Title VARCHAR(255) NOT NULL,           -- titre du bulletin
    ConventionCollective VARCHAR(255),     -- info sur la convention
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE SET NULL
);

-- Table Bulletin_Ligne
CREATE TABLE Bulletin_Ligne (
    Id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    BulletinId BIGINT UNSIGNED NOT NULL,
    Label VARCHAR(255) NOT NULL,           -- exemple : Salaire Brut, Prime, etc.
    Amount DECIMAL(12,2) NOT NULL,        -- positif ou n gatif
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (BulletinId) REFERENCES Bulletin(Id) ON DELETE CASCADE
);

-- -----------------------------------------------------
-- Index pour recherche rapide
-- -----------------------------------------------------
CREATE INDEX idx_message_groupe ON Message(GroupeId);
CREATE INDEX idx_message_user ON Message(UserId);
CREATE INDEX idx_agenda_startdate ON Agenda(StartDate);
CREATE INDEX idx_ca_user_date ON CourbeCA(UserId, DatePoint);
CREATE INDEX idx_bulletin_user ON Bulletin(UserId);

-- -----------------------------------------------------
-- Script termin 
-- -----------------------------------------------------
