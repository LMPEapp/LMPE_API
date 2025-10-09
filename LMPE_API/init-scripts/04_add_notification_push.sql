-- -----------------------------------------------------
-- Objectif : Ajout des tables de notifications pour Message, Agenda, CourbeCA et Bulletin
-- -----------------------------------------------------

USE LMPE;

-- -----------------------------------------------------
-- Table PushSubscription
-- -----------------------------------------------------
CREATE TABLE PushSubscription (
    Id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    UserId BIGINT UNSIGNED NOT NULL,      -- utilisateur cible
    Endpoint TEXT NOT NULL,               -- endpoint du navigateur
    P256dh VARCHAR(255) NOT NULL,         -- clé publique
    Auth VARCHAR(255) NOT NULL,           -- clé auth
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    
    -- ?? Empêche les doublons d’endpoint (on limite l'index sur 255 caractères)
    UNIQUE KEY idx_push_endpoint_unique (Endpoint(255))
);

-- Index pour recherche rapide par utilisateur
CREATE INDEX idx_push_user ON PushSubscription(UserId);

