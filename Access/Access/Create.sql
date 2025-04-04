-- =============================================
-- Create table: AspNetRoles
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.objects 
    WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoles]') 
      AND type in (N'U')
)
BEGIN
    CREATE TABLE [dbo].[AspNetRoles](
        [Id]                nvarchar(450)   NOT NULL,
        [Name]              nvarchar(256)   NULL,
        [NormalizedName]    nvarchar(256)   NULL,
        [ConcurrencyStamp]  nvarchar(max)   NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- =============================================
-- Create table: AspNetUsers
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.objects 
    WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') 
      AND type in (N'U')
)
BEGIN
    CREATE TABLE [dbo].[AspNetUsers](
        [Id]                    nvarchar(450)   NOT NULL,
        [UserName]              nvarchar(256)   NULL,
        [NormalizedUserName]    nvarchar(256)   NULL,
        [Email]                 nvarchar(256)   NULL,
        [NormalizedEmail]       nvarchar(256)   NULL,
        [EmailConfirmed]        bit             NOT NULL,
        [PasswordHash]          nvarchar(max)   NULL,
        [SecurityStamp]         nvarchar(max)   NULL,
        [ConcurrencyStamp]      nvarchar(max)   NULL,
        [PhoneNumber]           nvarchar(max)   NULL,
        [PhoneNumberConfirmed]  bit             NOT NULL,
        [TwoFactorEnabled]      bit             NOT NULL,
        [LockoutEnd]            datetimeoffset  NULL,
        [LockoutEnabled]        bit             NOT NULL,
        [AccessFailedCount]     int             NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- =============================================
-- Create table: AspNetRoleClaims
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.objects 
    WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoleClaims]') 
      AND type in (N'U')
)
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims](
        [Id]         int            NOT NULL IDENTITY(1,1),
        [RoleId]     nvarchar(450)  NOT NULL,
        [ClaimType]  nvarchar(max)  NULL,
        [ClaimValue] nvarchar(max)  NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- =============================================
-- Create table: AspNetUserClaims
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.objects 
    WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]') 
      AND type in (N'U')
)
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims](
        [Id]         int            NOT NULL IDENTITY(1,1),
        [UserId]     nvarchar(450)  NOT NULL,
        [ClaimType]  nvarchar(max)  NULL,
        [ClaimValue] nvarchar(max)  NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- =============================================
-- Create table: AspNetUserLogins
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.objects 
    WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]') 
      AND type in (N'U')
)
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins](
        [LoginProvider]       nvarchar(450) NOT NULL,
        [ProviderKey]         nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId]              nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC)
    );
END
GO

-- =============================================
-- Create table: AspNetUserRoles
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.objects 
    WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') 
      AND type in (N'U')
)
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles](
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC)
    );
END
GO

-- =============================================
-- Create table: AspNetUserTokens
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.objects 
    WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserTokens]') 
      AND type in (N'U')
)
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens](
        [UserId]        nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name]          nvarchar(450) NOT NULL,
        [Value]         nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginProvider] ASC, [Name] ASC)
    );
END
GO

-- =============================================
-- Add Foreign Key Constraints
-- =============================================
ALTER TABLE [dbo].[AspNetRoleClaims] WITH NOCHECK
    ADD CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
    FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id])
    ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[AspNetUserClaims] WITH NOCHECK
    ADD CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
    ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[AspNetUserLogins] WITH NOCHECK
    ADD CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
    ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[AspNetUserRoles] WITH NOCHECK
    ADD CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
    FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id])
    ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[AspNetUserRoles] WITH NOCHECK
    ADD CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
    ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[AspNetUserTokens] WITH NOCHECK
    ADD CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
    ON DELETE CASCADE;
GO

-- =============================================
-- Create Indexes
-- =============================================
-- Index on AspNetRoleClaims: RoleId
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_AspNetRoleClaims_RoleId'
      AND object_id = OBJECT_ID(N'[dbo].[AspNetRoleClaims]')
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId]
    ON [dbo].[AspNetRoleClaims] ([RoleId] ASC);
END
GO

-- Unique Index on AspNetRoles: NormalizedName (filtered: NOT NULL)
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'RoleNameIndex'
      AND object_id = OBJECT_ID(N'[dbo].[AspNetRoles]')
)
BEGIN
    CREATE UNIQUE INDEX [RoleNameIndex]
    ON [dbo].[AspNetRoles] ([NormalizedName] ASC)
    WHERE [NormalizedName] IS NOT NULL;
END
GO

-- Index on AspNetUserClaims: UserId
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_AspNetUserClaims_UserId'
      AND object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]')
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId]
    ON [dbo].[AspNetUserClaims] ([UserId] ASC);
END
GO

-- Index on AspNetUserLogins: UserId
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_AspNetUserLogins_UserId'
      AND object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]')
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId]
    ON [dbo].[AspNetUserLogins] ([UserId] ASC);
END
GO

-- Index on AspNetUserRoles: RoleId
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_AspNetUserRoles_RoleId'
      AND object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]')
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId]
    ON [dbo].[AspNetUserRoles] ([RoleId] ASC);
END
GO

-- Index on AspNetUsers: NormalizedEmail
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'EmailIndex'
      AND object_id = OBJECT_ID(N'[dbo].[AspNetUsers]')
)
BEGIN
    CREATE INDEX [EmailIndex]
    ON [dbo].[AspNetUsers] ([NormalizedEmail] ASC);
END
GO

-- Unique Index on AspNetUsers: NormalizedUserName (filtered: NOT NULL)
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'UserNameIndex'
      AND object_id = OBJECT_ID(N'[dbo].[AspNetUsers]')
)
BEGIN
    CREATE UNIQUE INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers] ([NormalizedUserName] ASC)
    WHERE [NormalizedUserName] IS NOT NULL;
END
GO


IF NOT EXISTS (SELECT * FROM sys.objects 
               WHERE object_id = OBJECT_ID(N'[dbo].[SecurityLogs]') 
                 AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SecurityLogs](
        [Id] INT IDENTITY(1,1) NOT NULL,
        [IpAddress] NVARCHAR(20) NULL,
        [Email] NVARCHAR(50) NULL,
        [Description] NVARCHAR(200) NULL,
        [CreatedOn] DATETIME NULL,
        [Action] NVARCHAR(50) NULL,
        CONSTRAINT [PK_SecurityLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END






