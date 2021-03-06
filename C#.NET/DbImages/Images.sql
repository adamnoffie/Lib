/*
 * Create tables for Images to be stored in the database.
 * @author: Adam Nofsinger
 *
 */

BEGIN TRANSACTION

/****** Object:  Table [dbo].[ImageTypes]    Script Date: 03/24/2010 11:07:50 ******/
CREATE TABLE [dbo].[ImageTypes](
	[ImageTypeID] [int] NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[IsEnabled] [bit] NOT NULL,
	CONSTRAINT [PK_ImageTypes] PRIMARY KEY CLUSTERED ([ImageTypeID] ASC)
)

/****** Object:  Table [dbo].[Images]    Script Date: 03/24/2010 11:07:50 ******/
CREATE TABLE [dbo].[Images](
	[ImageID] [int] IDENTITY(1,1) NOT NULL,
	[ImageTypeID] [int] NOT NULL,
	[RelatedID] [int] NOT NULL,
	[ImageData] [varbinary](max) NOT NULL,
	[ContentType] [varchar](100) NOT NULL,
	CONSTRAINT [PK_Images] PRIMARY KEY CLUSTERED ([ImageID] ASC)
)

/****** Object:  Default [DF_ImageTypes_DateCreated]    Script Date: 03/24/2010 11:07:50 ******/
ALTER TABLE [dbo].[ImageTypes] ADD  
	CONSTRAINT [DF_ImageTypes_DateCreated]  DEFAULT (getdate()) FOR [DateCreated]

/****** Object:  Default [DF_ImageTypes_IsEnabled]    Script Date: 03/24/2010 11:07:50 ******/
ALTER TABLE [dbo].[ImageTypes] ADD  
	CONSTRAINT [DF_ImageTypes_IsEnabled]  DEFAULT ((1)) FOR [IsEnabled]

/****** Object:  ForeignKey [FK_Images_ImageTypes]    Script Date: 03/24/2010 11:07:50 ******/
ALTER TABLE [dbo].[Images]  WITH CHECK ADD  
	CONSTRAINT [FK_Images_ImageTypes] FOREIGN KEY([ImageTypeID])
		REFERENCES [dbo].[ImageTypes] ([ImageTypeID])
ALTER TABLE [dbo].[Images] CHECK CONSTRAINT [FK_Images_ImageTypes]

COMMIT