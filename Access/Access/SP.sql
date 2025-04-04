-- =============================================
-- Stored Procedures for AspNetRoles
-- =============================================
IF OBJECT_ID('dbo.sp_InsertAspNetRole', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertAspNetRole;
GO
CREATE PROCEDURE dbo.sp_InsertAspNetRole
    @Id NVARCHAR(450),
    @Name NVARCHAR(256),
    @NormalizedName NVARCHAR(256),
    @ConcurrencyStamp NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (@Id, @Name, @NormalizedName, @ConcurrencyStamp);
END;
GO

IF OBJECT_ID('dbo.sp_GetAspNetRoles', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAspNetRoles;
GO
CREATE PROCEDURE dbo.sp_GetAspNetRoles
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.AspNetRoles;
END;
GO

-- =============================================
-- Stored Procedures for AspNetUsers
-- =============================================
IF OBJECT_ID('dbo.sp_InsertAspNetUser', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertAspNetUser;
GO
CREATE PROCEDURE dbo.sp_InsertAspNetUser
    @Id NVARCHAR(450),
    @UserName NVARCHAR(256),
    @NormalizedUserName NVARCHAR(256),
    @Email NVARCHAR(256),
    @NormalizedEmail NVARCHAR(256),
    @EmailConfirmed BIT,
    @PasswordHash NVARCHAR(MAX),
    @SecurityStamp NVARCHAR(MAX),
    @ConcurrencyStamp NVARCHAR(MAX),
    @PhoneNumber NVARCHAR(MAX),
    @PhoneNumberConfirmed BIT,
    @TwoFactorEnabled BIT,
    @LockoutEnd DATETIMEOFFSET = NULL,
    @LockoutEnabled BIT,
    @AccessFailedCount INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AspNetUsers
    (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount)
    VALUES
    (@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed, @PasswordHash, @SecurityStamp, @ConcurrencyStamp, @PhoneNumber, @PhoneNumberConfirmed, @TwoFactorEnabled, @LockoutEnd, @LockoutEnabled, @AccessFailedCount);
END;
GO

IF OBJECT_ID('dbo.sp_GetAspNetUsers', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAspNetUsers;
GO
CREATE PROCEDURE dbo.sp_GetAspNetUsers
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.AspNetUsers;
END;
GO

-- =============================================
-- Stored Procedures for AspNetRoleClaims
-- =============================================
IF OBJECT_ID('dbo.sp_InsertAspNetRoleClaim', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertAspNetRoleClaim;
GO
CREATE PROCEDURE dbo.sp_InsertAspNetRoleClaim
    @RoleId NVARCHAR(450),
    @ClaimType NVARCHAR(MAX),
    @ClaimValue NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AspNetRoleClaims (RoleId, ClaimType, ClaimValue)
    VALUES (@RoleId, @ClaimType, @ClaimValue);
END;
GO

IF OBJECT_ID('dbo.sp_GetAspNetRoleClaims', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAspNetRoleClaims;
GO
CREATE PROCEDURE dbo.sp_GetAspNetRoleClaims
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.AspNetRoleClaims;
END;
GO

-- =============================================
-- Stored Procedures for AspNetUserClaims
-- =============================================
IF OBJECT_ID('dbo.sp_InsertAspNetUserClaim', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertAspNetUserClaim;
GO
CREATE PROCEDURE dbo.sp_InsertAspNetUserClaim
    @UserId NVARCHAR(450),
    @ClaimType NVARCHAR(MAX),
    @ClaimValue NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AspNetUserClaims (UserId, ClaimType, ClaimValue)
    VALUES (@UserId, @ClaimType, @ClaimValue);
END;
GO

IF OBJECT_ID('dbo.sp_GetAspNetUserClaims', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAspNetUserClaims;
GO
CREATE PROCEDURE dbo.sp_GetAspNetUserClaims
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.AspNetUserClaims;
END;
GO

-- =============================================
-- Stored Procedures for AspNetUserLogins
-- =============================================
IF OBJECT_ID('dbo.sp_InsertAspNetUserLogin', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertAspNetUserLogin;
GO
CREATE PROCEDURE dbo.sp_InsertAspNetUserLogin
    @LoginProvider NVARCHAR(450),
    @ProviderKey NVARCHAR(450),
    @ProviderDisplayName NVARCHAR(MAX),
    @UserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AspNetUserLogins (LoginProvider, ProviderKey, ProviderDisplayName, UserId)
    VALUES (@LoginProvider, @ProviderKey, @ProviderDisplayName, @UserId);
END;
GO

IF OBJECT_ID('dbo.sp_GetAspNetUserLogins', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAspNetUserLogins;
GO
CREATE PROCEDURE dbo.sp_GetAspNetUserLogins
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.AspNetUserLogins;
END;
GO

-- =============================================
-- Stored Procedures for AspNetUserRoles
-- =============================================
IF OBJECT_ID('dbo.sp_InsertAspNetUserRole', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertAspNetUserRole;
GO
CREATE PROCEDURE dbo.sp_InsertAspNetUserRole
    @UserId NVARCHAR(450),
    @RoleId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
    VALUES (@UserId, @RoleId);
END;
GO

IF OBJECT_ID('dbo.sp_GetAspNetUserRoles', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAspNetUserRoles;
GO
CREATE PROCEDURE dbo.sp_GetAspNetUserRoles
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.AspNetUserRoles;
END;
GO

-- =============================================
-- Stored Procedures for AspNetUserTokens
-- =============================================
IF OBJECT_ID('dbo.sp_InsertAspNetUserToken', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertAspNetUserToken;
GO
CREATE PROCEDURE dbo.sp_InsertAspNetUserToken
    @UserId NVARCHAR(450),
    @LoginProvider NVARCHAR(450),
    @Name NVARCHAR(450),
    @Value NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AspNetUserTokens (UserId, LoginProvider, Name, Value)
    VALUES (@UserId, @LoginProvider, @Name, @Value);
END;
GO

IF OBJECT_ID('dbo.sp_GetAspNetUserTokens', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAspNetUserTokens;
GO
CREATE PROCEDURE dbo.sp_GetAspNetUserTokens
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.AspNetUserTokens;
END;
GO

-- =============================================
-- Stored Procedures for SecurityLogs
-- =============================================
IF OBJECT_ID('dbo.sp_InsertSecurityLog', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertSecurityLog;
GO
CREATE PROCEDURE dbo.sp_InsertSecurityLog
    @IpAddress NVARCHAR(20),
    @Email NVARCHAR(50),
    @Description NVARCHAR(200),
    @CreatedOn DATETIME,
    @Action NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.SecurityLogs (IpAddress, Email, Description, CreatedOn, Action)
    VALUES (@IpAddress, @Email, @Description, @CreatedOn, @Action);
END;
GO

IF OBJECT_ID('dbo.sp_GetSecurityLogs', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetSecurityLogs;
GO
CREATE PROCEDURE dbo.sp_GetSecurityLogs
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.SecurityLogs;
END;
GO
