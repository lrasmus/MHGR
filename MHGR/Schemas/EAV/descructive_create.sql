-------------------------------------------------------------------------------
-- THIS IS A DESTRUCTIVE SCRIPT - it will drop all tables that it is going to
-- create each time it is run.
-------------------------------------------------------------------------------

USE [mhgr_eav]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_files_result_sources]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_files]'))
ALTER TABLE [dbo].[result_files] DROP CONSTRAINT [FK_result_files_result_sources]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_entities_result_files]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_entities]'))
ALTER TABLE [dbo].[result_entities] DROP CONSTRAINT [FK_result_entities_result_files]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_entities_result_entities]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_entities]'))
ALTER TABLE [dbo].[result_entities] DROP CONSTRAINT [FK_result_entities_result_entities]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_entities_patients]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_entities]'))
ALTER TABLE [dbo].[result_entities] DROP CONSTRAINT [FK_result_entities_patients]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_entities_attributes]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_entities]'))
ALTER TABLE [dbo].[result_entities] DROP CONSTRAINT [FK_result_entities_attributes]
GO
/****** Object:  Index [IX_result_sources_name]    Script Date: 8/22/2015 8:53:41 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[result_sources]') AND name = N'IX_result_sources_name')
DROP INDEX [IX_result_sources_name] ON [dbo].[result_sources]
GO
/****** Object:  Index [IX_patients_external_source_external_id]    Script Date: 8/22/2015 8:53:41 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patients]') AND name = N'IX_patients_external_source_external_id')
DROP INDEX [IX_patients_external_source_external_id] ON [dbo].[patients]
GO
/****** Object:  Table [dbo].[result_sources]    Script Date: 8/22/2015 8:53:41 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[result_sources]') AND type in (N'U'))
DROP TABLE [dbo].[result_sources]
GO
/****** Object:  Table [dbo].[result_files]    Script Date: 8/22/2015 8:53:41 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[result_files]') AND type in (N'U'))
DROP TABLE [dbo].[result_files]
GO
/****** Object:  Table [dbo].[result_entities]    Script Date: 8/22/2015 8:53:41 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[result_entities]') AND type in (N'U'))
DROP TABLE [dbo].[result_entities]
GO
/****** Object:  Table [dbo].[patients]    Script Date: 8/22/2015 8:53:41 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patients]') AND type in (N'U'))
DROP TABLE [dbo].[patients]
GO
/****** Object:  Table [dbo].[attributes]    Script Date: 8/22/2015 8:53:41 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[attributes]') AND type in (N'U'))
DROP TABLE [dbo].[attributes]
GO
/****** Object:  Table [dbo].[attributes]    Script Date: 8/22/2015 8:53:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[attributes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[attributes](
	[id] [int] NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[description] [nvarchar](1000) NULL,
	[value_type] [nvarchar](15) NULL,
 CONSTRAINT [PK_attributes] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[patients]    Script Date: 8/22/2015 8:53:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patients]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[patients](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[external_id] [nvarchar](50) NOT NULL,
	[external_source] [nvarchar](50) NOT NULL,
	[first_name] [nvarchar](50) NULL,
	[last_name] [nvarchar](50) NULL,
	[date_of_birth] [date] NULL,
 CONSTRAINT [PK_patients] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[result_entities]    Script Date: 8/22/2015 8:53:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[result_entities]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[result_entities](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[patient_id] [int] NOT NULL,
	[result_file_id] [int] NULL,
	[attribute_id] [int] NOT NULL,
	[parent_id] [int] NULL,
	[value_float] [float] NULL,
	[value_int] [int] NULL,
	[value_short_text] [nvarchar](255) NULL,
	[value_text] [text] NULL,
	[value_date_time] [datetime] NULL,
 CONSTRAINT [PK_result_entities] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[result_files]    Script Date: 8/22/2015 8:53:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[result_files]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[result_files](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[md5] [nvarchar](255) NOT NULL,
	[received_on] [datetime2](7) NOT NULL,
	[result_source_id] [int] NULL,
 CONSTRAINT [PK_result_files] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[result_sources]    Script Date: 8/22/2015 8:53:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[result_sources]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[result_sources](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[description] [nvarchar](255) NULL,
 CONSTRAINT [PK_result_sources] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_patients_external_source_external_id]    Script Date: 8/22/2015 8:53:41 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patients]') AND name = N'IX_patients_external_source_external_id')
CREATE NONCLUSTERED INDEX [IX_patients_external_source_external_id] ON [dbo].[patients]
(
	[external_source] ASC,
	[external_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_result_sources_name]    Script Date: 8/22/2015 8:53:41 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[result_sources]') AND name = N'IX_result_sources_name')
CREATE NONCLUSTERED INDEX [IX_result_sources_name] ON [dbo].[result_sources]
(
	[name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_entities_attributes]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_entities]'))
ALTER TABLE [dbo].[result_entities]  WITH CHECK ADD  CONSTRAINT [FK_result_entities_attributes] FOREIGN KEY([attribute_id])
REFERENCES [dbo].[attributes] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_entities_attributes]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_entities]'))
ALTER TABLE [dbo].[result_entities] CHECK CONSTRAINT [FK_result_entities_attributes]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_entities_patients]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_entities]'))
ALTER TABLE [dbo].[result_entities]  WITH CHECK ADD  CONSTRAINT [FK_result_entities_patients] FOREIGN KEY([patient_id])
REFERENCES [dbo].[patients] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_entities_patients]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_entities]'))
ALTER TABLE [dbo].[result_entities] CHECK CONSTRAINT [FK_result_entities_patients]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_entities_result_entities]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_entities]'))
ALTER TABLE [dbo].[result_entities]  WITH CHECK ADD  CONSTRAINT [FK_result_entities_result_entities] FOREIGN KEY([parent_id])
REFERENCES [dbo].[result_entities] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_entities_result_entities]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_entities]'))
ALTER TABLE [dbo].[result_entities] CHECK CONSTRAINT [FK_result_entities_result_entities]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_entities_result_files]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_entities]'))
ALTER TABLE [dbo].[result_entities]  WITH CHECK ADD  CONSTRAINT [FK_result_entities_result_files] FOREIGN KEY([result_file_id])
REFERENCES [dbo].[result_files] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_entities_result_files]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_entities]'))
ALTER TABLE [dbo].[result_entities] CHECK CONSTRAINT [FK_result_entities_result_files]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_files_result_sources]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_files]'))
ALTER TABLE [dbo].[result_files]  WITH CHECK ADD  CONSTRAINT [FK_result_files_result_sources] FOREIGN KEY([result_source_id])
REFERENCES [dbo].[result_sources] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_files_result_sources]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_files]'))
ALTER TABLE [dbo].[result_files] CHECK CONSTRAINT [FK_result_files_result_sources]
GO
