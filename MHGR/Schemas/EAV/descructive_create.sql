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
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_attribute_relationships_attributes2]') AND parent_object_id = OBJECT_ID(N'[dbo].[attribute_relationships]'))
ALTER TABLE [dbo].[attribute_relationships] DROP CONSTRAINT [FK_attribute_relationships_attributes2]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_attribute_relationships_attributes1]') AND parent_object_id = OBJECT_ID(N'[dbo].[attribute_relationships]'))
ALTER TABLE [dbo].[attribute_relationships] DROP CONSTRAINT [FK_attribute_relationships_attributes1]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_attribute_relationships_attributes]') AND parent_object_id = OBJECT_ID(N'[dbo].[attribute_relationships]'))
ALTER TABLE [dbo].[attribute_relationships] DROP CONSTRAINT [FK_attribute_relationships_attributes]
GO
/****** Object:  Index [IX_result_sources_name]    Script Date: 9/3/2015 8:40:05 AM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[result_sources]') AND name = N'IX_result_sources_name')
DROP INDEX [IX_result_sources_name] ON [dbo].[result_sources]
GO
/****** Object:  Index [IX_patients_external_source_external_id]    Script Date: 9/3/2015 8:40:05 AM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patients]') AND name = N'IX_patients_external_source_external_id')
DROP INDEX [IX_patients_external_source_external_id] ON [dbo].[patients]
GO
/****** Object:  Index [IX_attributes_name]    Script Date: 9/3/2015 8:40:05 AM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[attributes]') AND name = N'IX_attributes_name')
DROP INDEX [IX_attributes_name] ON [dbo].[attributes]
GO
/****** Object:  Index [IX_attributes_code_code_system]    Script Date: 9/3/2015 8:40:05 AM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[attributes]') AND name = N'IX_attributes_code_code_system')
DROP INDEX [IX_attributes_code_code_system] ON [dbo].[attributes]
GO
/****** Object:  Table [dbo].[result_sources]    Script Date: 9/3/2015 8:40:05 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[result_sources]') AND type in (N'U'))
DROP TABLE [dbo].[result_sources]
GO
/****** Object:  Table [dbo].[result_files]    Script Date: 9/3/2015 8:40:05 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[result_files]') AND type in (N'U'))
DROP TABLE [dbo].[result_files]
GO
/****** Object:  Table [dbo].[result_entities]    Script Date: 9/3/2015 8:40:05 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[result_entities]') AND type in (N'U'))
DROP TABLE [dbo].[result_entities]
GO
/****** Object:  Table [dbo].[patients]    Script Date: 9/3/2015 8:40:05 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patients]') AND type in (N'U'))
DROP TABLE [dbo].[patients]
GO
/****** Object:  Table [dbo].[attributes]    Script Date: 9/3/2015 8:40:05 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[attributes]') AND type in (N'U'))
DROP TABLE [dbo].[attributes]
GO
/****** Object:  Table [dbo].[attribute_relationships]    Script Date: 9/3/2015 8:40:05 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[attribute_relationships]') AND type in (N'U'))
DROP TABLE [dbo].[attribute_relationships]
GO
/****** Object:  Table [dbo].[attribute_relationships]    Script Date: 9/3/2015 8:40:05 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[attribute_relationships]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[attribute_relationships](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[attribute1_id] [int] NOT NULL,
	[attribute2_id] [int] NOT NULL,
	[relationship_id] [int] NOT NULL,
 CONSTRAINT [PK_attribute_relationships] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[attributes]    Script Date: 9/3/2015 8:40:06 AM ******/
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
	[code] [nvarchar](50) NULL,
	[code_system] [nvarchar](40) NULL,
 CONSTRAINT [PK_attributes] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[patients]    Script Date: 9/3/2015 8:40:06 AM ******/
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
/****** Object:  Table [dbo].[result_entities]    Script Date: 9/3/2015 8:40:06 AM ******/
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
	[value_date_time] [datetime2](0) NULL,
 CONSTRAINT [PK_result_entities] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[result_files]    Script Date: 9/3/2015 8:40:06 AM ******/
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
/****** Object:  Table [dbo].[result_sources]    Script Date: 9/3/2015 8:40:06 AM ******/
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
/****** Object:  Index [IX_attributes_code_code_system]    Script Date: 9/3/2015 8:40:06 AM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[attributes]') AND name = N'IX_attributes_code_code_system')
CREATE NONCLUSTERED INDEX [IX_attributes_code_code_system] ON [dbo].[attributes]
(
	[code] ASC,
	[code_system] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_attributes_name]    Script Date: 9/3/2015 8:40:06 AM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[attributes]') AND name = N'IX_attributes_name')
CREATE NONCLUSTERED INDEX [IX_attributes_name] ON [dbo].[attributes]
(
	[name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_patients_external_source_external_id]    Script Date: 9/3/2015 8:40:06 AM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patients]') AND name = N'IX_patients_external_source_external_id')
CREATE NONCLUSTERED INDEX [IX_patients_external_source_external_id] ON [dbo].[patients]
(
	[external_source] ASC,
	[external_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_result_sources_name]    Script Date: 9/3/2015 8:40:06 AM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[result_sources]') AND name = N'IX_result_sources_name')
CREATE NONCLUSTERED INDEX [IX_result_sources_name] ON [dbo].[result_sources]
(
	[name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_attribute_relationships_attributes]') AND parent_object_id = OBJECT_ID(N'[dbo].[attribute_relationships]'))
ALTER TABLE [dbo].[attribute_relationships]  WITH CHECK ADD  CONSTRAINT [FK_attribute_relationships_attributes] FOREIGN KEY([attribute1_id])
REFERENCES [dbo].[attributes] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_attribute_relationships_attributes]') AND parent_object_id = OBJECT_ID(N'[dbo].[attribute_relationships]'))
ALTER TABLE [dbo].[attribute_relationships] CHECK CONSTRAINT [FK_attribute_relationships_attributes]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_attribute_relationships_attributes1]') AND parent_object_id = OBJECT_ID(N'[dbo].[attribute_relationships]'))
ALTER TABLE [dbo].[attribute_relationships]  WITH CHECK ADD  CONSTRAINT [FK_attribute_relationships_attributes1] FOREIGN KEY([attribute2_id])
REFERENCES [dbo].[attributes] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_attribute_relationships_attributes1]') AND parent_object_id = OBJECT_ID(N'[dbo].[attribute_relationships]'))
ALTER TABLE [dbo].[attribute_relationships] CHECK CONSTRAINT [FK_attribute_relationships_attributes1]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_attribute_relationships_attributes2]') AND parent_object_id = OBJECT_ID(N'[dbo].[attribute_relationships]'))
ALTER TABLE [dbo].[attribute_relationships]  WITH CHECK ADD  CONSTRAINT [FK_attribute_relationships_attributes2] FOREIGN KEY([relationship_id])
REFERENCES [dbo].[attributes] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_attribute_relationships_attributes2]') AND parent_object_id = OBJECT_ID(N'[dbo].[attribute_relationships]'))
ALTER TABLE [dbo].[attribute_relationships] CHECK CONSTRAINT [FK_attribute_relationships_attributes2]
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


INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (1, 'Relationship', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (2, 'Is a', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (3, 'Influenced by gene', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (4, 'Influences phenotype', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (5, 'Variant of gene', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (6, 'Gene containing variant', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (7, 'Phenotype', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (8, 'Clopidogrel metabolism', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (9, 'Warfarin metabolism', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (10, 'Familial Thrombophilia', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (11, 'Hypertrophic Cardiomyopathy', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (12, 'Ultrarapid metabolizer', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (13, 'Extensive metabolizer', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (14, 'Intermediate metabolizer', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (15, 'Poor metabolizer', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (16, 'Normal', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (17, 'Decreased', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (18, 'Homozygous prothrombin G20210A mutation', 'binary', '289.81', 'ICD9-CM')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (19, 'Heterozygous prothrombin G20210A mutation', 'binary', '289.81', 'ICD9-CM')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (20, 'No genetic risk for prothrombin-related thrombophilia', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (21, 'Homozygous Factor V Leiden mutation', 'binary', '289.81', 'ICD9-CM')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (22, 'Heterozygous Factor V Leiden mutation', 'binary', '289.81', 'ICD9-CM')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (23, 'No genetic risk for thrombophilia, due to factor V Leiden', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (24, 'Double heterozygous for prothrombin G20210A mutation and Factor V Leiden mutation', 'binary', '289.81', 'ICD9-CM')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (25, 'Cardiomyopathy, Familial Hypertrophic, 1', 'binary', '425.1', 'ICD9-CM')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (26, 'Cardiomyopathy, Familial Hypertrophic, 2', 'binary', '425.1', 'ICD9-CM')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (27, 'Cardiomyopathy, Familial Hypertrophic, 3', 'binary', '425.1', 'ICD9-CM')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (28, 'Cardiomyopathy, Familial Hypertrophic, 4', 'binary', '425.1', 'ICD9-CM')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (29, 'No genetic risk found', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (30, 'Gene', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (31, 'CYP2C19', 'binary', '2621', 'HGNC')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (32, 'CYP2C9', 'binary', '2623', 'HGNC')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (33, 'VKORC1', 'binary', '23663', 'HGNC')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (34, 'F2', 'binary', '3535', 'HGNC')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (35, 'F5', 'binary', '3542', 'HGNC')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (36, 'MYH7', 'binary', '7577', 'HGNC')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (37, 'MYBPC3', 'binary', '7551', 'HGNC')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (38, 'TNNT2', 'binary', '11949', 'HGNC')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (39, 'TPM1', 'binary', '12010', 'HGNC')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (40, 'rs12248560', 'short_text', 'rs12248560', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (41, 'rs28399504', 'short_text', 'rs28399504', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (42, 'rs41291556', 'short_text', 'rs41291556', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (43, 'rs72558184', 'short_text', 'rs72558184', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (44, 'rs4986893', 'short_text', 'rs4986893', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (45, 'rs4244285', 'short_text', 'rs4244285', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (46, 'rs72558186', 'short_text', 'rs72558186', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (47, 'rs56337013', 'short_text', 'rs56337013', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (48, 'rs17884712', 'short_text', 'rs17884712', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (49, 'rs6413438', 'short_text', 'rs6413438', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (50, 'rs1057910', 'short_text', 'rs1057910', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (51, 'rs1799853', 'short_text', 'rs1799853', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (52, 'rs9923231', 'short_text', 'rs9923231', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (53, 'rs9934438', 'short_text', 'rs9934438', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (54, 'rs8050894', 'short_text', 'rs8050894', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (55, 'rs6025', 'short_text', 'rs6025', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (56, 'rs1799963', 'short_text', 'rs1799963', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (57, 'rs121913626', 'short_text', 'rs121913626', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (58, 'rs3218713', 'short_text', 'rs3218713', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (59, 'rs3218714', 'short_text', 'rs3218714', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (60, 'rs121964855', 'short_text', 'rs121964855', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (61, 'rs121964856', 'short_text', 'rs121964856', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (62, 'rs121964857', 'short_text', 'rs121964857', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (63, 'rs28934269', 'short_text', 'rs28934269', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (64, 'rs28934270', 'short_text', 'rs28934270', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (65, 'rs727504290', 'short_text', 'rs727504290', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (66, 'rs104894504', 'short_text', 'rs104894504', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (67, 'rs375882485', 'short_text', 'rs375882485', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (68, 'rs397516083', 'short_text', 'rs397516083', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (69, 'rs397515937', 'short_text', 'rs397515937', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (70, 'rs397516074', 'short_text', 'rs397516074', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (71, 'rs397515963', 'short_text', 'rs397515963', 'dbSNP')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (72, 'Resulted on', 'date_time', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (73, 'Allele', 'short_text', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (74, 'Genomic Variant Format result', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (75, 'GVF Version', 'short_text', 'gvf-version', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (76, 'GFF Version', 'short_text', 'gff-version', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (77, 'File Version', 'short_text', 'file-version', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (78, 'Source method', 'binary', 'source-method', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (79, 'Source', 'short_text', 'Source', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (80, 'Type', 'short_text', 'Type', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (81, 'Dbxref', 'short_text', 'Dbxref', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (82, 'Comment', 'text', 'Comment', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (83, 'Technology platform', 'binary', 'technology-platform', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (84, 'Platform class', 'short_text', 'Platform_class', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (85, 'Platform name', 'short_text', 'Platform_name', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (86, 'Read type', 'short_text', 'Read_type', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (87, 'Read length', 'int', 'Read_length', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (88, 'Read pair span', 'short_text', 'Read_pair_span', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (89, 'Average coverage', 'int', 'Average_coverage', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (90, 'Feature ontology', 'short_text', 'feature-ontology', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (91, 'Genome build', 'short_text', 'genome-build', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (92, 'Sequence region', 'binary', 'sequence-region', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (93, 'Chromosome', 'short_text', 'Chromosome', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (94, 'Start position', 'int', 'Start position', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (95, 'End position', 'int', 'End position', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (96, 'Genomic Variant Format feature', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (97, 'Score', 'short_text', 'Score', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (98, 'Strand', 'short_text', 'Strand', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (99, 'Phase', 'short_text', 'Phase', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (100, 'Reference sequence', 'short_text', 'Reference_seq', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (101, 'Variant reads', 'short_text', 'Variant_reads', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (102, 'Genotype', 'short_text', 'Genotype', 'GVF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (103, 'Variant Call Format result', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (104, 'File Format', 'short_text', 'fileFormat', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (105, 'Reference', 'short_text', 'reference', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (106, 'Phasing', 'short_text', 'phasing', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (107, 'Information', 'binary', 'INFO', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (108, 'Filter', 'binary', 'FILTER', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (109, 'Format', 'binary', 'FORMAT', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (110, 'ID', 'short_text', 'ID', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (111, 'Number', 'short_text', 'Number', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (112, 'Type', 'short_text', 'Type', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (113, 'Description', 'short_text', 'Description', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (114, 'Variant Call Format variant', 'binary', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (115, 'Reference base', 'short_text', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (116, 'Number of Samples With Data', 'int', 'INFO:NS', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (117, 'Total Depth', 'int', 'INFO:DP', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (118, 'Allele Frequency', 'float', 'INFO:AF', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (119, 'Ancestral Allele', 'short_text', 'INFO:AA', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (120, 'dbSNP membership, build 129', 'binary', 'INFO:DB', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (121, 'HapMap2 membership', 'binary', 'INFO:H2', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (122, 'Genotype Quality', 'int', 'FORMAT:GQ', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (123, 'Genotype', 'short_text', 'FORMAT:GT', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (124, 'Read Depth', 'int', 'FORMAT:DP', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (125, 'q10 Filter', 'binary', 'FILTER:q10', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (126, 's50 Filter', 'binary', 'FILTER:s50', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (127, 'Quality', 'float', 'Quality', 'VCF')
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (128, 'SNP allele', 'short_text', NULL, NULL)
INSERT INTO dbo.attributes (id, name, value_type, code, code_system) VALUES (129, 'Star allele', 'short_text', NULL, NULL)


INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (2, 1, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (3, 1, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (4, 1, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (5, 1, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (6, 1, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (7, 1, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (30, 1, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (8, 7, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (9, 7, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (10, 7, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (11, 7, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (12, 8, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (13, 8, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (14, 8, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (15, 8, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (16, 9, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (17, 9, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (18, 10, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (19, 10, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (20, 10, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (21, 10, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (22, 10, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (23, 10, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (24, 10, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (25, 11, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (26, 11, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (27, 11, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (28, 11, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (29, 11, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (31, 30, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (32, 30, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (33, 30, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (34, 30, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (35, 30, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (36, 30, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (37, 30, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (38, 30, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (39, 30, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (8, 31, 3)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (9, 32, 3)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (9, 33, 3)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (10, 34, 3)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (10, 35, 3)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (11, 36, 3)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (11, 37, 3)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (11, 38, 3)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (11, 39, 3)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (31, 8, 4)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (32, 9, 4)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (33, 9, 4)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (34, 10, 4)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (35, 10, 4)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (36, 11, 4)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (37, 11, 4)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (38, 11, 4)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (39, 11, 4)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (40, 31, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (41, 31, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (42, 31, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (43, 31, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (44, 31, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (45, 31, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (46, 31, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (47, 31, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (48, 31, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (49, 31, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (50, 32, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (51, 32, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (52, 33, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (53, 33, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (54, 33, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (55, 35, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (56, 34, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (57, 36, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (58, 36, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (59, 36, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (60, 38, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (61, 38, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (62, 38, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (63, 39, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (64, 39, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (65, 39, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (66, 39, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (67, 37, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (68, 37, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (69, 37, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (70, 37, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (71, 37, 5)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (31, 40, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (31, 41, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (31, 42, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (31, 43, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (31, 44, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (31, 45, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (31, 46, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (31, 47, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (31, 48, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (31, 49, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (32, 50, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (32, 51, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (33, 52, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (33, 53, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (33, 54, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (35, 55, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (34, 56, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (36, 57, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (36, 58, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (36, 59, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (38, 60, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (38, 61, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (38, 62, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (39, 63, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (39, 64, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (39, 65, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (39, 66, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (37, 67, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (37, 68, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (37, 69, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (37, 70, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (37, 71, 6)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (128, 73, 2)
INSERT INTO dbo.attribute_relationships (attribute1_id, attribute2_id, relationship_id) VALUES (129, 73, 2)
