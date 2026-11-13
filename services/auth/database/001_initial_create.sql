CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;
CREATE TABLE `usuarios` (
    `Id` char(36) NOT NULL,
    `Nome` varchar(120) NOT NULL,
    `Email` varchar(254) NOT NULL,
    `PasswordHash` varchar(100) NOT NULL,
    `CriadoEmUtc` datetime(6) NOT NULL,
    `AtualizadoEmUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `refresh_tokens` (
    `Id` char(36) NOT NULL,
    `UsuarioId` char(36) NOT NULL,
    `TokenHash` varchar(64) NOT NULL,
    `CriadoEmUtc` datetime(6) NOT NULL,
    `ExpiraEmUtc` datetime(6) NOT NULL,
    `RevogadoEmUtc` datetime(6) NULL,
    `SubstituidoPorTokenHash` varchar(64) NULL,
    `Version` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_refresh_tokens_usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE CASCADE
);

CREATE UNIQUE INDEX `IX_refresh_tokens_TokenHash` ON `refresh_tokens` (`TokenHash`);

CREATE INDEX `IX_refresh_tokens_UsuarioId` ON `refresh_tokens` (`UsuarioId`);

CREATE UNIQUE INDEX `IX_usuarios_Email` ON `usuarios` (`Email`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261110230259_InitialCreate', '10.0.14');

COMMIT;
