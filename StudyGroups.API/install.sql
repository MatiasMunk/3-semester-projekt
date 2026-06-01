USE master;
GO

/* =========================================================
   DROP DATABASE (SAFE RESET)
   ========================================================= */
IF DB_ID('StudyGroupsDb') IS NOT NULL
BEGIN
    ALTER DATABASE StudyGroupsDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE StudyGroupsDb;
END
GO

/* =========================================================
   CREATE DATABASE
   ========================================================= */
CREATE DATABASE StudyGroupsDb;
GO

USE StudyGroupsDb;
GO

/* =========================================================
   USERS
   ========================================================= */
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,

    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255),

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
	
	Role NVARCHAR(255) NOT NULL DEFAULT 1
);
	
/* =========================================================
   TOPICS
   ========================================================= */
CREATE TABLE Topics (
    Id INT PRIMARY KEY IDENTITY,

    Name NVARCHAR(100) NOT NULL,
    Slug NVARCHAR(100) NOT NULL UNIQUE,

    Icon NVARCHAR(10),
    Color NVARCHAR(20),

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

/* =========================================================
   STUDY SESSIONS
   ========================================================= */
CREATE TABLE StudySessions (
    Id INT PRIMARY KEY IDENTITY,

    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),

    TopicId INT NOT NULL,
    Location NVARCHAR(255),

    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NULL,

    MaxParticipants INT NOT NULL,
    CurrentParticipants INT NOT NULL DEFAULT 0,

    CreatedByUserId INT NOT NULL,

	OwnerId INT NOT NULL,
	
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
	
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    FOREIGN KEY (TopicId) REFERENCES Topics(Id),
    FOREIGN KEY (OwnerId) REFERENCES Users(Id),

    CONSTRAINT StudySessions_MaxParticipants_Positive CHECK (MaxParticipants > 0),
    CONSTRAINT StudySessions_CurrentParticipants_Valid CHECK (CurrentParticipants >= 0 AND CurrentParticipants <= MaxParticipants)
);

/* =========================================================
   SESSION PARTICIPANTS
   ========================================================= */
CREATE TABLE SessionParticipants (
    Id INT PRIMARY KEY IDENTITY,

    SessionId INT NOT NULL,
    UserId INT NOT NULL,

    JoinedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    FOREIGN KEY (SessionId) REFERENCES StudySessions(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id),

    CONSTRAINT SessionUser UNIQUE (SessionId, UserId)
);

CREATE INDEX SessionParticipants_SessionId
ON SessionParticipants(SessionId);

/* =========================================================
   PRIVATE MESSAGES (persistent per study room)
   ========================================================= */
CREATE TABLE PrivateMessages (
    Id INT PRIMARY KEY IDENTITY,

    SessionId INT NOT NULL,
    SenderId INT NOT NULL,
    ReceiverId INT NOT NULL,

    Message NVARCHAR(MAX) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    FOREIGN KEY (SessionId) REFERENCES StudySessions(Id) ON DELETE CASCADE,
    FOREIGN KEY (SenderId) REFERENCES Users(Id),
    FOREIGN KEY (ReceiverId) REFERENCES Users(Id),

    CONSTRAINT PrivateMessages_NotSelf CHECK (SenderId <> ReceiverId)
);

CREATE INDEX PrivateMessages_Conversation
ON PrivateMessages(SessionId, SenderId, ReceiverId, CreatedAt);
	
/* =========================================================
   FRIEND REQUESTS
   ========================================================= */
CREATE TABLE FriendRequests (
    Id INT PRIMARY KEY IDENTITY,

    SenderId INT NOT NULL,
    ReceiverId INT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT N'pending',

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    RespondedAt DATETIME2 NULL,

    FOREIGN KEY (SenderId) REFERENCES Users(Id),
    FOREIGN KEY (ReceiverId) REFERENCES Users(Id),

    CONSTRAINT FriendRequests_NotSelf CHECK (SenderId <> ReceiverId),
    CONSTRAINT FriendRequests_Status CHECK (Status IN (N'pending', N'accepted', N'declined')),
    CONSTRAINT FriendRequests_Pair UNIQUE (SenderId, ReceiverId)
);

CREATE TABLE Friendships (
    Id INT PRIMARY KEY IDENTITY,

    UserId INT NOT NULL,
    FriendUserId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (FriendUserId) REFERENCES Users(Id),

    CONSTRAINT Friendships_NotSelf CHECK (UserId <> FriendUserId),
    CONSTRAINT Friendships_Pair UNIQUE (UserId, FriendUserId)
);

/* =========================================================
   SEED: TOPICS (MATCH YOUR UI)
   ========================================================= */
INSERT INTO Topics (Name, Slug, Icon, Color) VALUES
(N'Programming', N'programming', N'💻', N'#6366f1'),
(N'Mathematics', N'math', N'📐', N'#f59e0b'),
(N'Exam Preparation', N'exam', N'📝', N'#ef4444'),
(N'Science', N'science', N'🔬', N'#10b981'),
(N'Esotericism', N'esotericism', N'🔮', N'#10b981'),
(N'Languages', N'language', N'🌍', N'#3b82f6'),
(N'Business', N'business', N'📊', N'#8b5cf6'),
(N'Design', N'design', N'🎨', N'#ec4899'),
(N'Group Study', N'group', N'👥', N'#14b8a6');

GO

/* =========================================================
   SEED: USERS
   Required by sample StudySessions.CreatedByUserId FK.
   Dev login: admin / password
   ========================================================= */
INSERT INTO Users (Username, PasswordHash, Email)
VALUES
(N'MatiasMunk', N'$2a$12$vP8qx/zEKD7blEJSLlVgp.HypgBi0mlEFJ.Ex0jCYaCoim2dqHhBi', N'matiaspersson95@gmail.com'),
(N'testtest', N'$2a$12$5EOCsC7L/xZOU.Fs.yuPrefS7E7hdMTuokQqgXv3Ghhy6haeUgkYG', N'matiaspersson95@gmail.com');

GO

/* =========================================================
   SEED: SAMPLE SESSIONS
   ========================================================= */
INSERT INTO StudySessions
(Title, Description, TopicId, Location, StartTime, MaxParticipants, CreatedByUserId)
VALUES
(N'Calculus Exam Prep', N'Prepare for derivatives & integrals', 2, N'Room A1', DATEADD(hour, 2, GETDATE()), 10, (SELECT Id FROM Users WHERE Username = N'MatiasMunk')),
(N'C# Coding Session', N'Build APIs with .NET', 1, N'Room B2', DATEADD(hour, 4, GETDATE()), 8, (SELECT Id FROM Users WHERE Username = N'MatiasMunk')),
(N'Biology Group Study', N'Cell structures & DNA', 4, N'Room C3', DATEADD(day, 1, GETDATE()), 6, (SELECT Id FROM Users WHERE Username = N'MatiasMunk'));

GO