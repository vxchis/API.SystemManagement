SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF DB_ID(N'SystemManagementDb_Attachments') IS NULL
    CREATE DATABASE [SystemManagementDb_Attachments];
GO

USE [SystemManagementDb_Attachments];
GO

IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NOT NULL DROP TABLE [dbo].[__EFMigrationsHistory];
IF OBJECT_ID(N'[dbo].[TaskFiles]', N'U') IS NOT NULL DROP TABLE [dbo].[TaskFiles];
IF OBJECT_ID(N'[dbo].[TaskProgressLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[TaskProgressLogs];
IF OBJECT_ID(N'[dbo].[TaskItems]', N'U') IS NOT NULL DROP TABLE [dbo].[TaskItems];
IF OBJECT_ID(N'[dbo].[Notifications]', N'U') IS NOT NULL DROP TABLE [dbo].[Notifications];
IF OBJECT_ID(N'[dbo].[Employees]', N'U') IS NOT NULL DROP TABLE [dbo].[Employees];
IF OBJECT_ID(N'[dbo].[UserRoles]', N'U') IS NOT NULL DROP TABLE [dbo].[UserRoles];
IF OBJECT_ID(N'[dbo].[DepartmentGroupDepartments]', N'U') IS NOT NULL DROP TABLE [dbo].[DepartmentGroupDepartments];
IF OBJECT_ID(N'[dbo].[DepartmentGroups]', N'U') IS NOT NULL DROP TABLE [dbo].[DepartmentGroups];
IF OBJECT_ID(N'[dbo].[Positions]', N'U') IS NOT NULL DROP TABLE [dbo].[Positions];
IF OBJECT_ID(N'[dbo].[Roles]', N'U') IS NOT NULL DROP TABLE [dbo].[Roles];
IF OBJECT_ID(N'[dbo].[Departments]', N'U') IS NOT NULL DROP TABLE [dbo].[Departments];
IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL DROP TABLE [dbo].[Users];
GO

CREATE TABLE [dbo].[Departments](
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
);
CREATE UNIQUE INDEX [IX_Departments_Code] ON [dbo].[Departments]([Code]);
GO

CREATE TABLE [dbo].[DepartmentGroups](
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    CONSTRAINT [PK_DepartmentGroups] PRIMARY KEY ([Id])
);
CREATE UNIQUE INDEX [IX_DepartmentGroups_Code] ON [dbo].[DepartmentGroups]([Code]);
GO

CREATE TABLE [dbo].[Positions](
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    CONSTRAINT [PK_Positions] PRIMARY KEY ([Id])
);
CREATE UNIQUE INDEX [IX_Positions_Code] ON [dbo].[Positions]([Code]);
GO

CREATE TABLE [dbo].[Roles](
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Level] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);
CREATE UNIQUE INDEX [IX_Roles_Code] ON [dbo].[Roles]([Code]);
GO

CREATE TABLE [dbo].[Users](
    [Id] uniqueidentifier NOT NULL,
    [Username] nvarchar(100) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [PasswordSalt] nvarchar(max) NOT NULL,
    [FullName] nvarchar(200) NOT NULL,
    [Email] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
CREATE UNIQUE INDEX [IX_Users_Username] ON [dbo].[Users]([Username]);
GO

CREATE TABLE [dbo].[DepartmentGroupDepartments](
    [DepartmentGroupId] uniqueidentifier NOT NULL,
    [DepartmentId] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_DepartmentGroupDepartments] PRIMARY KEY ([DepartmentGroupId],[DepartmentId]),
    CONSTRAINT [FK_DepartmentGroupDepartments_DepartmentGroups_DepartmentGroupId] FOREIGN KEY ([DepartmentGroupId]) REFERENCES [dbo].[DepartmentGroups]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DepartmentGroupDepartments_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments]([Id]) ON DELETE CASCADE
);
CREATE INDEX [IX_DepartmentGroupDepartments_DepartmentId] ON [dbo].[DepartmentGroupDepartments]([DepartmentId]);
GO

CREATE TABLE [dbo].[Employees](
    [Id] uniqueidentifier NOT NULL,
    [EmployeeCode] nvarchar(50) NOT NULL,
    [FullName] nvarchar(200) NOT NULL,
    [DepartmentId] uniqueidentifier NOT NULL,
    [PositionId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NULL,
    [Email] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [ManagerEmployeeId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Employees_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments]([Id]),
    CONSTRAINT [FK_Employees_Employees_ManagerEmployeeId] FOREIGN KEY ([ManagerEmployeeId]) REFERENCES [dbo].[Employees]([Id]),
    CONSTRAINT [FK_Employees_Positions_PositionId] FOREIGN KEY ([PositionId]) REFERENCES [dbo].[Positions]([Id]),
    CONSTRAINT [FK_Employees_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
);
CREATE UNIQUE INDEX [IX_Employees_EmployeeCode] ON [dbo].[Employees]([EmployeeCode]);
CREATE INDEX [IX_Employees_DepartmentId] ON [dbo].[Employees]([DepartmentId]);
CREATE INDEX [IX_Employees_PositionId] ON [dbo].[Employees]([PositionId]);
CREATE INDEX [IX_Employees_ManagerEmployeeId] ON [dbo].[Employees]([ManagerEmployeeId]);
CREATE UNIQUE INDEX [IX_Employees_UserId] ON [dbo].[Employees]([UserId]) WHERE [UserId] IS NOT NULL;
GO

CREATE TABLE [dbo].[UserRoles](
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId],[RoleId]),
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([Id]) ON DELETE CASCADE
);
CREATE INDEX [IX_UserRoles_RoleId] ON [dbo].[UserRoles]([RoleId]);
GO

CREATE TABLE [dbo].[Notifications](
    [Id] uniqueidentifier NOT NULL,
    [TargetUserId] uniqueidentifier NOT NULL,
    [Type] nvarchar(100) NOT NULL,
    [Title] nvarchar(300) NOT NULL,
    [Message] nvarchar(2000) NOT NULL,
    [RelatedEntityId] uniqueidentifier NULL,
    [RelatedEntityType] nvarchar(100) NULL,
    [IsRead] bit NOT NULL,
    [ReadAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Users_TargetUserId] FOREIGN KEY ([TargetUserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
);
CREATE INDEX [IX_Notifications_TargetUserId_IsRead_CreatedAt] ON [dbo].[Notifications]([TargetUserId],[IsRead],[CreatedAt]);
GO

CREATE TABLE [dbo].[TaskItems](
    [Id] uniqueidentifier NOT NULL,
    [TaskCode] nvarchar(50) NOT NULL,
    [Title] nvarchar(300) NOT NULL,
    [Description] nvarchar(max) NULL,
    [DepartmentId] uniqueidentifier NOT NULL,
    [AssignedByUserId] uniqueidentifier NOT NULL,
    [AssignedToUserId] uniqueidentifier NOT NULL,
    [DueDate] datetime2 NOT NULL,
    [Priority] int NOT NULL,
    [Status] int NOT NULL,
    [SourceType] int NOT NULL,
    [ProgressPercent] int NOT NULL,
    [ResultSummary] nvarchar(max) NULL,
    [AcceptedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    CONSTRAINT [PK_TaskItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TaskItems_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments]([Id]),
    CONSTRAINT [FK_TaskItems_Users_AssignedByUserId] FOREIGN KEY ([AssignedByUserId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_TaskItems_Users_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [dbo].[Users]([Id])
);
CREATE UNIQUE INDEX [IX_TaskItems_TaskCode] ON [dbo].[TaskItems]([TaskCode]);
CREATE INDEX [IX_TaskItems_DepartmentId] ON [dbo].[TaskItems]([DepartmentId]);
CREATE INDEX [IX_TaskItems_AssignedByUserId] ON [dbo].[TaskItems]([AssignedByUserId]);
CREATE INDEX [IX_TaskItems_AssignedToUserId] ON [dbo].[TaskItems]([AssignedToUserId]);
GO

CREATE TABLE [dbo].[TaskProgressLogs](
    [Id] uniqueidentifier NOT NULL,
    [TaskItemId] uniqueidentifier NOT NULL,
    [ProgressPercent] int NOT NULL,
    [Status] int NOT NULL,
    [Note] nvarchar(max) NULL,
    [ActionByUserId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    CONSTRAINT [PK_TaskProgressLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TaskProgressLogs_TaskItems_TaskItemId] FOREIGN KEY ([TaskItemId]) REFERENCES [dbo].[TaskItems]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TaskProgressLogs_Users_ActionByUserId] FOREIGN KEY ([ActionByUserId]) REFERENCES [dbo].[Users]([Id])
);
CREATE INDEX [IX_TaskProgressLogs_TaskItemId] ON [dbo].[TaskProgressLogs]([TaskItemId]);
CREATE INDEX [IX_TaskProgressLogs_ActionByUserId] ON [dbo].[TaskProgressLogs]([ActionByUserId]);
GO

CREATE TABLE [dbo].[TaskFiles](
    [Id] uniqueidentifier NOT NULL,
    [TaskItemId] uniqueidentifier NOT NULL,
    [TaskProgressLogId] uniqueidentifier NULL,
    [AttachmentType] int NOT NULL,
    [FileName] nvarchar(260) NOT NULL,
    [StoredFileName] nvarchar(260) NOT NULL,
    [RelativePath] nvarchar(500) NOT NULL,
    [ContentType] nvarchar(200) NOT NULL,
    [SizeBytes] bigint NOT NULL,
    [UploadedByUserId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    CONSTRAINT [PK_TaskFiles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TaskFiles_TaskItems_TaskItemId] FOREIGN KEY ([TaskItemId]) REFERENCES [dbo].[TaskItems]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TaskFiles_TaskProgressLogs_TaskProgressLogId] FOREIGN KEY ([TaskProgressLogId]) REFERENCES [dbo].[TaskProgressLogs]([Id]),
    CONSTRAINT [FK_TaskFiles_Users_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [dbo].[Users]([Id])
);
CREATE INDEX [IX_TaskFiles_TaskItemId_AttachmentType] ON [dbo].[TaskFiles]([TaskItemId],[AttachmentType]);
CREATE INDEX [IX_TaskFiles_TaskProgressLogId] ON [dbo].[TaskFiles]([TaskProgressLogId]);
CREATE INDEX [IX_TaskFiles_UploadedByUserId] ON [dbo].[TaskFiles]([UploadedByUserId]);
GO

CREATE TABLE [dbo].[__EFMigrationsHistory](
    [MigrationId] nvarchar(150) NOT NULL,
    [ProductVersion] nvarchar(32) NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
);
INSERT INTO [dbo].[__EFMigrationsHistory]([MigrationId], [ProductVersion]) VALUES (N'20260425000100_InitialCreate', N'10.0.0');
GO
