-- =============================================
-- Stored Procedures for User Management
-- =============================================

-- Check if user exists by email
IF OBJECT_ID('dbo.sp_GetUserByEmail', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserByEmail;
GO
CREATE PROCEDURE dbo.sp_GetUserByEmail
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.AspNetUsers WHERE Email = @Email;
END;
GO

-- Check if user exists by username
IF OBJECT_ID('dbo.sp_GetUserByUserName', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserByUserName;
GO
CREATE PROCEDURE dbo.sp_GetUserByUserName
    @UserName NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.AspNetUsers WHERE UserName = @UserName;
END;
GO

-- Get user by ID
IF OBJECT_ID('dbo.sp_GetUserById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserById;
GO
CREATE PROCEDURE dbo.sp_GetUserById
    @Id NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.AspNetUsers WHERE Id = @Id;
END;
GO

-- Create new user
IF OBJECT_ID('dbo.sp_CreateUser', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CreateUser;
GO
CREATE PROCEDURE dbo.sp_CreateUser
    @Id NVARCHAR(450),
    @UserName NVARCHAR(256),
    @NormalizedUserName NVARCHAR(256),
    @Email NVARCHAR(256),
    @NormalizedEmail NVARCHAR(256),
    @EmailConfirmed BIT,
    @PasswordHash NVARCHAR(MAX),
    @SecurityStamp NVARCHAR(MAX),
    @ConcurrencyStamp NVARCHAR(MAX),
    @PhoneNumber NVARCHAR(MAX) = NULL,
    @PhoneNumberConfirmed BIT = 0,
    @TwoFactorEnabled BIT = 1,
    @LockoutEnd DATETIMEOFFSET = NULL,
    @LockoutEnabled BIT = 1,
    @AccessFailedCount INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO dbo.AspNetUsers
        (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
         PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, 
         TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount)
        VALUES
        (@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed,
         @PasswordHash, @SecurityStamp, @ConcurrencyStamp, @PhoneNumber, @PhoneNumberConfirmed,
         @TwoFactorEnabled, @LockoutEnd, @LockoutEnabled, @AccessFailedCount);
        
        -- Return success
        SELECT 1 AS Result;
    END TRY
    BEGIN CATCH
        -- Return failure
        SELECT 0 AS Result;
    END CATCH
END;
GO

-- Update user
IF OBJECT_ID('dbo.sp_UpdateUser', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_UpdateUser;
GO
CREATE PROCEDURE dbo.sp_UpdateUser
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
    @LockoutEnd DATETIMEOFFSET,
    @LockoutEnabled BIT,
    @AccessFailedCount INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.AspNetUsers
    SET UserName = @UserName,
        NormalizedUserName = @NormalizedUserName,
        Email = @Email,
        NormalizedEmail = @NormalizedEmail,
        EmailConfirmed = @EmailConfirmed,
        PasswordHash = @PasswordHash,
        SecurityStamp = @SecurityStamp,
        ConcurrencyStamp = @ConcurrencyStamp,
        PhoneNumber = @PhoneNumber,
        PhoneNumberConfirmed = @PhoneNumberConfirmed,
        TwoFactorEnabled = @TwoFactorEnabled,
        LockoutEnd = @LockoutEnd,
        LockoutEnabled = @LockoutEnabled,
        AccessFailedCount = @AccessFailedCount
    WHERE Id = @Id;
    
    -- Return the number of rows affected
    SELECT @@ROWCOUNT AS Result;
END;
GO

-- Confirm email
IF OBJECT_ID('dbo.sp_ConfirmUserEmail', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ConfirmUserEmail;
GO
CREATE PROCEDURE dbo.sp_ConfirmUserEmail
    @Id NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.AspNetUsers
    SET EmailConfirmed = 1
    WHERE Id = @Id;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- Update password hash
IF OBJECT_ID('dbo.sp_UpdatePasswordHash', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_UpdatePasswordHash;
GO
CREATE PROCEDURE dbo.sp_UpdatePasswordHash
    @Id NVARCHAR(450),
    @PasswordHash NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.AspNetUsers
    SET PasswordHash = @PasswordHash
    WHERE Id = @Id;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- Increment failed access count
IF OBJECT_ID('dbo.sp_IncrementAccessFailedCount', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_IncrementAccessFailedCount;
GO
CREATE PROCEDURE dbo.sp_IncrementAccessFailedCount
    @Id NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.AspNetUsers
    SET AccessFailedCount = AccessFailedCount + 1
    WHERE Id = @Id;
    
    SELECT AccessFailedCount FROM dbo.AspNetUsers WHERE Id = @Id;
END;
GO

-- Reset access failed count
IF OBJECT_ID('dbo.sp_ResetAccessFailedCount', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ResetAccessFailedCount;
GO
CREATE PROCEDURE dbo.sp_ResetAccessFailedCount
    @Id NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.AspNetUsers
    SET AccessFailedCount = 0
    WHERE Id = @Id;
    
    SELECT @@ROWCOUNT AS Result;
END;
GO

-- Set lockout end date
IF OBJECT_ID('dbo.sp_SetLockoutEndDate', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_SetLockoutEndDate;
GO
CREATE PROCEDURE dbo.sp_SetLockoutEndDate
    @Id NVARCHAR(450),
    @LockoutEnd DATETIMEOFFSET
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.AspNetUsers
    SET LockoutEnd = @LockoutEnd
    WHERE Id = @Id;
    
    SELECT @@ROWCOUNT AS Result;
END;
GO

-- =============================================
-- Stored Procedures for Roles
-- =============================================

-- Get all roles
IF OBJECT_ID('dbo.sp_GetAllRoles', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAllRoles;
GO
CREATE PROCEDURE dbo.sp_GetAllRoles
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.AspNetRoles;
END;
GO

-- Get role by name
IF OBJECT_ID('dbo.sp_GetRoleByName', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetRoleByName;
GO
CREATE PROCEDURE dbo.sp_GetRoleByName
    @Name NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.AspNetRoles WHERE Name = @Name;
END;
GO

-- Create role
IF OBJECT_ID('dbo.sp_CreateRole', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CreateRole;
GO
CREATE PROCEDURE dbo.sp_CreateRole
    @Id NVARCHAR(450),
    @Name NVARCHAR(256),
    @NormalizedName NVARCHAR(256),
    @ConcurrencyStamp NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
        VALUES (@Id, @Name, @NormalizedName, @ConcurrencyStamp);
        
        SELECT 1 AS Result;
    END TRY
    BEGIN CATCH
        SELECT 0 AS Result;
    END CATCH
END;
GO

-- =============================================
-- Stored Procedures for User Roles
-- =============================================

-- Get user roles
IF OBJECT_ID('dbo.sp_GetUserRoles', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserRoles;
GO
CREATE PROCEDURE dbo.sp_GetUserRoles
    @UserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.* 
    FROM dbo.AspNetRoles r
    INNER JOIN dbo.AspNetUserRoles ur ON r.Id = ur.RoleId
    WHERE ur.UserId = @UserId;
END;
GO

-- Check if user is in role
IF OBJECT_ID('dbo.sp_IsUserInRole', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_IsUserInRole;
GO
CREATE PROCEDURE dbo.sp_IsUserInRole
    @UserId NVARCHAR(450),
    @RoleName NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) AS InRole
    FROM dbo.AspNetUserRoles ur
    INNER JOIN dbo.AspNetRoles r ON ur.RoleId = r.Id
    WHERE ur.UserId = @UserId AND r.Name = @RoleName;
END;
GO

-- Remove user from role
IF OBJECT_ID('dbo.sp_RemoveUserFromRole', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_RemoveUserFromRole;
GO
CREATE PROCEDURE dbo.sp_RemoveUserFromRole
    @UserId NVARCHAR(450),
    @RoleId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.AspNetUserRoles
    WHERE UserId = @UserId AND RoleId = @RoleId;
    
    SELECT @@ROWCOUNT AS Result;
END;
GO

-- =============================================
-- Stored Procedures for User Tokens
-- =============================================

-- Get authentication token
IF OBJECT_ID('dbo.sp_GetAuthenticationToken', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAuthenticationToken;
GO
CREATE PROCEDURE dbo.sp_GetAuthenticationToken
    @UserId NVARCHAR(450),
    @LoginProvider NVARCHAR(450),
    @Name NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Value 
    FROM dbo.AspNetUserTokens
    WHERE UserId = @UserId 
      AND LoginProvider = @LoginProvider 
      AND Name = @Name;
END;
GO

-- Set authentication token
IF OBJECT_ID('dbo.sp_GetAuthenticationToken', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAuthenticationToken;
GO
CREATE PROCEDURE dbo.sp_GetAuthenticationToken
    @UserId NVARCHAR(450),
    @LoginProvider NVARCHAR(450),
    @Name NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Value 
    FROM dbo.AspNetUserTokens
    WHERE UserId = @UserId 
      AND LoginProvider = @LoginProvider 
      AND Name = @Name;
END;
GO

-- Set authentication token
IF OBJECT_ID('dbo.sp_SetAuthenticationToken', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_SetAuthenticationToken;
GO
CREATE PROCEDURE dbo.sp_SetAuthenticationToken
    @UserId NVARCHAR(450),
    @LoginProvider NVARCHAR(450),
    @Name NVARCHAR(450),
    @Value NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.AspNetUserTokens 
               WHERE UserId = @UserId AND LoginProvider = @LoginProvider AND Name = @Name)
    BEGIN
        UPDATE dbo.AspNetUserTokens
        SET Value = @Value
        WHERE UserId = @UserId AND LoginProvider = @LoginProvider AND Name = @Name;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.AspNetUserTokens (UserId, LoginProvider, Name, Value)
        VALUES (@UserId, @LoginProvider, @Name, @Value);
    END
    
    SELECT 1 AS Result; -- Return success
END;
GO

-- Remove authentication token
IF OBJECT_ID('dbo.sp_RemoveAuthenticationToken', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_RemoveAuthenticationToken;
GO
CREATE PROCEDURE dbo.sp_RemoveAuthenticationToken
    @UserId NVARCHAR(450),
    @LoginProvider NVARCHAR(450),
    @Name NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.AspNetUserTokens
    WHERE UserId = @UserId AND LoginProvider = @LoginProvider AND Name = @Name;
    
    SELECT @@ROWCOUNT AS Result;
END;
GO

-- =============================================
-- Stored Procedures for Security Logs
-- =============================================

-- Insert security log
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
    
    SELECT SCOPE_IDENTITY() AS Result; -- Return the new ID
END;
GO

-- Get security logs
IF OBJECT_ID('dbo.sp_GetSecurityLogs', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetSecurityLogs;
GO
CREATE PROCEDURE dbo.sp_GetSecurityLogs
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @Email NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.SecurityLogs
    WHERE (@StartDate IS NULL OR CreatedOn >= @StartDate)
      AND (@EndDate IS NULL OR CreatedOn <= @EndDate)
      AND (@Email IS NULL OR Email = @Email)
    ORDER BY CreatedOn DESC;
END;
GO

-- =============================================
-- Stored Procedures for User Claims
-- =============================================

-- Get user claims
IF OBJECT_ID('dbo.sp_GetUserClaims', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserClaims;
GO
CREATE PROCEDURE dbo.sp_GetUserClaims
    @UserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.AspNetUserClaims WHERE UserId = @UserId;
END;
GO

-- Add user claim
IF OBJECT_ID('dbo.sp_AddUserClaim', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_AddUserClaim;
GO
CREATE PROCEDURE dbo.sp_AddUserClaim
    @UserId NVARCHAR(450),
    @ClaimType NVARCHAR(MAX),
    @ClaimValue NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AspNetUserClaims (UserId, ClaimType, ClaimValue)
    VALUES (@UserId, @ClaimType, @ClaimValue);
    
    SELECT SCOPE_IDENTITY() AS Id;
END;
GO

-- Remove user claim
IF OBJECT_ID('dbo.sp_RemoveUserClaim', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_RemoveUserClaim;
GO
CREATE PROCEDURE dbo.sp_RemoveUserClaim
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.AspNetUserClaims WHERE Id = @Id;
END;
GO

-- =============================================
-- Stored Procedures for User Logins
-- =============================================

-- Get user logins
IF OBJECT_ID('dbo.sp_GetUserLogins', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserLogins;
GO
CREATE PROCEDURE dbo.sp_GetUserLogins
    @UserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.AspNetUserLogins WHERE UserId = @UserId;
END;
GO

-- Add user login
IF OBJECT_ID('dbo.sp_AddUserLogin', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_AddUserLogin;
GO
CREATE PROCEDURE dbo.sp_AddUserLogin
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

-- Remove user login
IF OBJECT_ID('dbo.sp_RemoveUserLogin', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_RemoveUserLogin;
GO
CREATE PROCEDURE dbo.sp_RemoveUserLogin
    @LoginProvider NVARCHAR(450),
    @ProviderKey NVARCHAR(450),
    @UserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.AspNetUserLogins
    WHERE LoginProvider = @LoginProvider 
      AND ProviderKey = @ProviderKey 
      AND UserId = @UserId;
END;
GO

-- =============================================
-- Additional Helper Stored Procedures
-- =============================================

-- =============================================
-- Stored Procedures for Lockout Management
-- =============================================

-- Handle failed login attempt
IF OBJECT_ID('dbo.sp_HandleFailedLogin', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_HandleFailedLogin;
GO
CREATE PROCEDURE dbo.sp_HandleFailedLogin
    @Id NVARCHAR(450),
    @MaxFailedAttempts INT = 3,
    @LockoutMinutes INT = 5
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Increment failed count
    UPDATE dbo.AspNetUsers
    SET AccessFailedCount = AccessFailedCount + 1
    WHERE Id = @Id;

    -- Check if we need to lock the account
    IF EXISTS (
        SELECT 1 FROM dbo.AspNetUsers 
        WHERE Id = @Id AND AccessFailedCount >= @MaxFailedAttempts
    )
    BEGIN
        UPDATE dbo.AspNetUsers
        SET LockoutEnd = DATEADD(MINUTE, @LockoutMinutes, GETUTCDATE()),
            LockoutEnabled = 1
        WHERE Id = @Id;
    END

    -- Return the current values from the table (single SELECT)
    SELECT 
        AccessFailedCount AS FailedCount,
        CAST(CASE WHEN LockoutEnabled = 1 AND LockoutEnd > GETUTCDATE() THEN 1 ELSE 0 END AS BIT) AS IsLockedOut,
        LockoutEnd
    FROM dbo.AspNetUsers
    WHERE Id = @Id;
END;

GO

-- Handle successful login
IF OBJECT_ID('dbo.sp_HandleSuccessfulLogin', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_HandleSuccessfulLogin;
GO
CREATE PROCEDURE dbo.sp_HandleSuccessfulLogin
    @Id NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Reset failed count and clear any lockout
    UPDATE dbo.AspNetUsers
    SET AccessFailedCount = 0,
        LockoutEnd = NULL
    WHERE Id = @Id;
    
    SELECT 1 AS Result;
END;
GO

-- Check if user is locked out
IF OBJECT_ID('dbo.sp_CheckUserLockout', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CheckUserLockout;
GO
CREATE PROCEDURE dbo.sp_CheckUserLockout
    @Id NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @LockoutEnd DATETIMEOFFSET;
    DECLARE @IsLockedOut BIT = 0;
    DECLARE @RemainingMinutes INT = 0;
    
    SELECT 
        @LockoutEnd = LockoutEnd
    FROM dbo.AspNetUsers
    WHERE Id = @Id;
    
    -- Check if currently locked out
    IF @LockoutEnd IS NOT NULL AND @LockoutEnd > GETUTCDATE()
    BEGIN
        SET @IsLockedOut = 1;
        SET @RemainingMinutes = DATEDIFF(MINUTE, GETUTCDATE(), @LockoutEnd);
        
        -- Ensure we return at least 1 minute if there's any time remaining
        IF @RemainingMinutes < 1 AND @LockoutEnd > GETUTCDATE()
            SET @RemainingMinutes = 1;
    END
    ELSE IF @LockoutEnd IS NOT NULL AND @LockoutEnd <= GETUTCDATE()
    BEGIN
        -- Lockout has expired, clear it
        UPDATE dbo.AspNetUsers
        SET LockoutEnd = NULL,
            AccessFailedCount = 0
        WHERE Id = @Id;
    END
    
    SELECT 
        @IsLockedOut AS IsLockedOut,
        @LockoutEnd AS LockoutEnd,
        @RemainingMinutes AS RemainingMinutes;
END;
GO

-- Get user with lockout info
IF OBJECT_ID('dbo.sp_GetUserWithLockoutInfo', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserWithLockoutInfo;
GO
CREATE PROCEDURE dbo.sp_GetUserWithLockoutInfo
    @UserName NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.*,
        CASE 
            WHEN u.LockoutEnd IS NOT NULL AND u.LockoutEnd > GETUTCDATE() THEN 1
            ELSE 0
        END AS IsCurrentlyLockedOut,
        CASE 
            WHEN u.LockoutEnd IS NOT NULL AND u.LockoutEnd > GETUTCDATE() 
            THEN DATEDIFF(MINUTE, GETUTCDATE(), u.LockoutEnd)
            ELSE 0
        END AS LockoutRemainingMinutes
    FROM dbo.AspNetUsers u
    WHERE u.UserName = @UserName;
END;
GO

-- Clear expired lockouts (maintenance procedure)
IF OBJECT_ID('dbo.sp_ClearExpiredLockouts', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ClearExpiredLockouts;
GO
CREATE PROCEDURE dbo.sp_ClearExpiredLockouts
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.AspNetUsers
    SET LockoutEnd = NULL,
        AccessFailedCount = 0
    WHERE LockoutEnd IS NOT NULL 
      AND LockoutEnd <= GETUTCDATE();
    
    SELECT @@ROWCOUNT AS ClearedCount;
END;
GO


-- sp_AddUserToRole - Add user to a role
IF OBJECT_ID('dbo.sp_AddUserToRole', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_AddUserToRole;
GO
CREATE PROCEDURE dbo.sp_AddUserToRole
    @UserId NVARCHAR(450),
    @RoleId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if the user-role relationship already exists
    IF EXISTS (SELECT 1 FROM dbo.AspNetUserRoles 
               WHERE UserId = @UserId AND RoleId = @RoleId)
    BEGIN
        -- Already exists, return 0 (false)
        SELECT 0 AS Result;
        RETURN;
    END
    
    BEGIN TRY
        -- Insert the user-role relationship
        INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
        VALUES (@UserId, @RoleId);
        
        -- Return 1 (true) for success
        SELECT 1 AS Result;
    END TRY
    BEGIN CATCH
        -- Return 0 (false) for failure
        SELECT 0 AS Result;
    END CATCH
END;
GO