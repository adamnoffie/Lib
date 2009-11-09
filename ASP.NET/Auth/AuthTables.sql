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
	EmailAddress nvarchar(50) NOT NULL
)
ALTER TABLE Users ADD CONSTRAINT 
PK_Users PRIMARY KEY CLUSTERED (UserID)

GO

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


ALTER TABLE UserRoleMappings ADD CONSTRAINT
FK_UserRoleMappings_Users FOREIGN KEY (UserID) 
REFERENCES Users (UserID)	

ALTER TABLE UserRoleMappings ADD CONSTRAINT
FK_UserRoleMappings_UserRoles FOREIGN KEY (UserRoleID) 
REFERENCES UserRoles (RoleID)


COMMIT
