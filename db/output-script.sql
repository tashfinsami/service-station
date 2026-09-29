CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `ServiceTokens` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `TokenNumber` int NOT NULL,
    `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_ServiceTokens` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260921124623_InitialCreate', '9.0.20');

CREATE TABLE `QueueSettings` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `MaxWaiting` int NOT NULL,
    `MaxServing` int NOT NULL,
    CONSTRAINT `PK_QueueSettings` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260921125754_AddQueueSettings', '9.0.20');

INSERT INTO `QueueSettings` (`Id`, `MaxServing`, `MaxWaiting`)
VALUES (1, 2, 5);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260921130521_SeedQueueSettings', '9.0.20');

CREATE UNIQUE INDEX `IX_ServiceTokens_TokenNumber` ON `ServiceTokens` (`TokenNumber`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260923082852_MakeTokenNumberUnique', '9.0.20');

ALTER TABLE `ServiceTokens` MODIFY COLUMN `Status` varchar(255) CHARACTER SET utf8mb4 NOT NULL;

CREATE INDEX `IX_ServiceTokens_Status_CreatedAt_Id` ON `ServiceTokens` (`Status`, `CreatedAt`, `Id`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260929160825_AddServiceTokenQueueIndex', '9.0.20');

COMMIT;

