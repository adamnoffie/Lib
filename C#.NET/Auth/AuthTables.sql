<Query Kind="SQL">
  <Connection>
    <ID>47b33bf4-c011-4c05-a5b8-517fa3f8e9d1</ID>
    <Server>BSOFT-ADAM\SQLEXPRESS</Server>
    <Persist>true</Persist>
    <Database>Admin</Database>
    <ShowServer>true</ShowServer>
  </Connection>
</Query>

/* 
Create all the tables and relationships for Auth
@author: Adam Nofsinger
*/
BEGIN TRANSACTION

-- Users
CREATE TABLE Users
(
	UserID int NOT NULL IDENTITY (1, 1),
	Username nvarchar(50) NOT NULL,
	PasswordHash char(60) NOT NULL,
	EmailAddress nvarchar(50) NOT NULL,
	DateLastLogin datetime NULL,
	DateCreated datetime NOT NULL DEFAULT (getdate()),
	IsEnabled bit NOT NULL DEFAULT ((1)),
)
ALTER TABLE Users ADD CONSTRAINT 
PK_Users PRIMARY KEY CLUSTERED (UserID)

GO

INSERT INTO Users (Username, PasswordHash, EmailAddress )
	VALUES ('Admin', 
		'$2a$10$PbReRukDHessdg8ed2Iul.dGC1TPGt.t3vCfPNvV71zUUmCsA75hS', -- 'admin01'
		'admin@website.com');

-- UserRoles
CREATE TABLE UserRoles
(
	RoleID smallint NOT NULL IDENTITY (1, 1),
	RoleName nvarchar(20) NOT NULL
)
ALTER TABLE UserRoles ADD CONSTRAINT
PK_UserRoles PRIMARY KEY CLUSTERED (RoleID)

GO

INSERT INTO UserRoles (RoleName) VALUES ('Admin')
GO

-- UserRoleMappings
CREATE TABLE UserRoleMappings
(
	UserID int NOT NULL,
	UserRoleID smallint NOT NULL
)
ALTER TABLE UserRoleMappings ADD CONSTRAINT
PK_UserRoleMappings PRIMARY KEY CLUSTERED (UserID, UserRoleID)
GO

INSERT INTO UserRoleMappings (UserID, UserRoleID)
SELECT	usr.UserID, r.RoleID
FROM	Users usr, UserRoles r
WHERE	usr.Username = 'Admin' AND r.RoleName = 'Admin'

ALTER TABLE UserRoleMappings ADD CONSTRAINT
FK_UserRoleMappings_Users FOREIGN KEY (UserID) 
REFERENCES Users (UserID)	

ALTER TABLE UserRoleMappings ADD CONSTRAINT
FK_UserRoleMappings_UserRoles FOREIGN KEY (UserRoleID) 
REFERENCES UserRoles (RoleID)


COMMIT

SELECT * FROM Users
SELECT * FROM UserRoles
SELECT * FROM UserRoleMappings
