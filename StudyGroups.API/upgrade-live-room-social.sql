USE StudyGroupsDb;
GO

IF OBJECT_ID(N'dbo.PrivateMessages', N'U') IS NULL
BEGIN
    CREATE TABLE PrivateMessages (
        Id INT PRIMARY KEY IDENTITY,

        SessionId INT NOT NULL,
        SenderUserId INT NOT NULL,
        ReceiverUserId INT NOT NULL,

        Message NVARCHAR(MAX) NOT NULL,
        IsRead BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

        FOREIGN KEY (SessionId) REFERENCES StudySessions(Id),
        FOREIGN KEY (SenderUserId) REFERENCES Users(Id),
        FOREIGN KEY (ReceiverUserId) REFERENCES Users(Id),

        CONSTRAINT CK_PrivateMessages_NotSelf CHECK (SenderUserId <> ReceiverUserId)
    );

    CREATE INDEX IX_PrivateMessages_Conversation
    ON PrivateMessages(SessionId, SenderUserId, ReceiverUserId, CreatedAt);
END
GO

IF OBJECT_ID(N'dbo.PrivateMessages', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.PrivateMessages', N'IsRead') IS NULL
BEGIN
    ALTER TABLE PrivateMessages
    ADD IsRead BIT NOT NULL CONSTRAINT DF_PrivateMessages_IsRead DEFAULT 0;
END
GO

IF OBJECT_ID(N'dbo.FriendRequests', N'U') IS NULL
BEGIN
    CREATE TABLE FriendRequests (
        Id INT PRIMARY KEY IDENTITY,

        RequesterUserId INT NOT NULL,
        ReceiverUserId INT NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT N'pending',

        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        RespondedAt DATETIME2 NULL,

        FOREIGN KEY (RequesterUserId) REFERENCES Users(Id),
        FOREIGN KEY (ReceiverUserId) REFERENCES Users(Id),

        CONSTRAINT CK_FriendRequests_NotSelf CHECK (RequesterUserId <> ReceiverUserId),
        CONSTRAINT CK_FriendRequests_Status CHECK (Status IN (N'pending', N'accepted', N'declined')),
        CONSTRAINT UQ_FriendRequests_Pair UNIQUE (RequesterUserId, ReceiverUserId)
    );
END
GO

IF OBJECT_ID(N'dbo.Friendships', N'U') IS NULL
BEGIN
    CREATE TABLE Friendships (
        Id INT PRIMARY KEY IDENTITY,

        UserId INT NOT NULL,
        FriendUserId INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

        FOREIGN KEY (UserId) REFERENCES Users(Id),
        FOREIGN KEY (FriendUserId) REFERENCES Users(Id),

        CONSTRAINT CK_Friendships_NotSelf CHECK (UserId <> FriendUserId),
        CONSTRAINT UQ_Friendships_Pair UNIQUE (UserId, FriendUserId)
    );
END
GO
