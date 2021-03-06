-------------------------------------------------------------------------------
-- THIS IS A DESTRUCTIVE SCRIPT - it will drop all tables that it is going to
-- create each time it is run.
-------------------------------------------------------------------------------

USE [mhgr_hybrid]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_variants_genes]') AND parent_object_id = OBJECT_ID(N'[dbo].[variants]'))
ALTER TABLE [dbo].[variants] DROP CONSTRAINT [FK_variants_genes]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_files_result_sources]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_files]'))
ALTER TABLE [dbo].[result_files] DROP CONSTRAINT [FK_result_files_result_sources]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_variants_patients]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_variants]'))
ALTER TABLE [dbo].[patient_variants] DROP CONSTRAINT [FK_patient_variants_patients]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_variant_information_variant_information_types]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_variant_information]'))
ALTER TABLE [dbo].[patient_variant_information] DROP CONSTRAINT [FK_patient_variant_information_variant_information_types]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_result_members_patient_result_collections]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_result_members]'))
ALTER TABLE [dbo].[patient_result_members] DROP CONSTRAINT [FK_patient_result_members_patient_result_collections]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_result_collections_result_files]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_result_collections]'))
ALTER TABLE [dbo].[patient_result_collections] DROP CONSTRAINT [FK_patient_result_collections_result_files]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_result_collections_patients]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_result_collections]'))
ALTER TABLE [dbo].[patient_result_collections] DROP CONSTRAINT [FK_patient_result_collections_patients]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_phenotypes_phenotypes]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_phenotypes]'))
ALTER TABLE [dbo].[patient_phenotypes] DROP CONSTRAINT [FK_patient_phenotypes_phenotypes]
GO
/****** Object:  Index [IX_variants_gene_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[variants]') AND name = N'IX_variants_gene_id')
DROP INDEX [IX_variants_gene_id] ON [dbo].[variants]
GO
/****** Object:  Index [IX_variants_external_source_external_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[variants]') AND name = N'IX_variants_external_source_external_id')
DROP INDEX [IX_variants_external_source_external_id] ON [dbo].[variants]
GO
/****** Object:  Index [IX_variant_information_types_source_name]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[variant_information_types]') AND name = N'IX_variant_information_types_source_name')
DROP INDEX [IX_variant_information_types_source_name] ON [dbo].[variant_information_types]
GO
/****** Object:  Index [IX_result_sources_name]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[result_sources]') AND name = N'IX_result_sources_name')
DROP INDEX [IX_result_sources_name] ON [dbo].[result_sources]
GO
/****** Object:  Index [IX_phenotypes_name]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[phenotypes]') AND name = N'IX_phenotypes_name')
DROP INDEX [IX_phenotypes_name] ON [dbo].[phenotypes]
GO
/****** Object:  Index [IX_phenotypes_external_source_external_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[phenotypes]') AND name = N'IX_phenotypes_external_source_external_id')
DROP INDEX [IX_phenotypes_external_source_external_id] ON [dbo].[phenotypes]
GO
/****** Object:  Index [IX_patients_external_source_external_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patients]') AND name = N'IX_patients_external_source_external_id')
DROP INDEX [IX_patients_external_source_external_id] ON [dbo].[patients]
GO
/****** Object:  Index [IX_patient_variants_variant_type_reference_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_variants]') AND name = N'IX_patient_variants_variant_type_reference_id')
DROP INDEX [IX_patient_variants_variant_type_reference_id] ON [dbo].[patient_variants]
GO
/****** Object:  Index [IX_patient_variants_patient_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_variants]') AND name = N'IX_patient_variants_patient_id')
DROP INDEX [IX_patient_variants_patient_id] ON [dbo].[patient_variants]
GO
/****** Object:  Index [IX_patient_variant_information_type_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_variant_information]') AND name = N'IX_patient_variant_information_type_id')
DROP INDEX [IX_patient_variant_information_type_id] ON [dbo].[patient_variant_information]
GO
/****** Object:  Index [IX_patient_variant_information_item_type_item_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_variant_information]') AND name = N'IX_patient_variant_information_item_type_item_id')
DROP INDEX [IX_patient_variant_information_item_type_item_id] ON [dbo].[patient_variant_information]
GO
/****** Object:  Index [IX_patient_result_members_member_type_member_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_result_members]') AND name = N'IX_patient_result_members_member_type_member_id')
DROP INDEX [IX_patient_result_members_member_type_member_id] ON [dbo].[patient_result_members]
GO
/****** Object:  Index [IX_patient_result_members_collection_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_result_members]') AND name = N'IX_patient_result_members_collection_id')
DROP INDEX [IX_patient_result_members_collection_id] ON [dbo].[patient_result_members]
GO
/****** Object:  Index [IX_patient_result_collections_patients]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_result_collections]') AND name = N'IX_patient_result_collections_patients')
DROP INDEX [IX_patient_result_collections_patients] ON [dbo].[patient_result_collections]
GO
/****** Object:  Index [IX_patient_phenotypes_phenotype_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_phenotypes]') AND name = N'IX_patient_phenotypes_phenotype_id')
DROP INDEX [IX_patient_phenotypes_phenotype_id] ON [dbo].[patient_phenotypes]
GO
/****** Object:  Index [IX_genes_symbol]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[genes]') AND name = N'IX_genes_symbol')
DROP INDEX [IX_genes_symbol] ON [dbo].[genes]
GO
/****** Object:  Table [dbo].[variants]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[variants]') AND type in (N'U'))
DROP TABLE [dbo].[variants]
GO
/****** Object:  Table [dbo].[variant_information_types]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[variant_information_types]') AND type in (N'U'))
DROP TABLE [dbo].[variant_information_types]
GO
/****** Object:  Table [dbo].[result_sources]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[result_sources]') AND type in (N'U'))
DROP TABLE [dbo].[result_sources]
GO
/****** Object:  Table [dbo].[result_files]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[result_files]') AND type in (N'U'))
DROP TABLE [dbo].[result_files]
GO
/****** Object:  Table [dbo].[phenotypes]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[phenotypes]') AND type in (N'U'))
DROP TABLE [dbo].[phenotypes]
GO
/****** Object:  Table [dbo].[patients]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patients]') AND type in (N'U'))
DROP TABLE [dbo].[patients]
GO
/****** Object:  Table [dbo].[patient_variants]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patient_variants]') AND type in (N'U'))
DROP TABLE [dbo].[patient_variants]
GO
/****** Object:  Table [dbo].[patient_variant_information]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patient_variant_information]') AND type in (N'U'))
DROP TABLE [dbo].[patient_variant_information]
GO
/****** Object:  Table [dbo].[patient_result_members]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patient_result_members]') AND type in (N'U'))
DROP TABLE [dbo].[patient_result_members]
GO
/****** Object:  Table [dbo].[patient_result_collections]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patient_result_collections]') AND type in (N'U'))
DROP TABLE [dbo].[patient_result_collections]
GO
/****** Object:  Table [dbo].[patient_phenotypes]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patient_phenotypes]') AND type in (N'U'))
DROP TABLE [dbo].[patient_phenotypes]
GO
/****** Object:  Table [dbo].[genes]    Script Date: 8/22/2015 8:52:11 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[genes]') AND type in (N'U'))
DROP TABLE [dbo].[genes]
GO
/****** Object:  Table [dbo].[genes]    Script Date: 8/22/2015 8:52:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[genes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[genes](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[symbol] [nvarchar](50) NOT NULL,
	[external_id] [nvarchar](50) NULL,
	[external_source] [nvarchar](50) NULL,
	[chromosome] [nvarchar](5) NULL,
 CONSTRAINT [PK_genes] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[patient_phenotypes]    Script Date: 8/22/2015 8:52:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patient_phenotypes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[patient_phenotypes](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[phenotype_id] [int] NOT NULL,
	[resulted_on] [datetime2](0) NULL,
	[report] [text] NULL,
 CONSTRAINT [PK_patient_phenotypes] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[patient_result_collections]    Script Date: 8/22/2015 8:52:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patient_result_collections]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[patient_result_collections](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[patient_id] [int] NOT NULL,
	[received_on] [datetime2](0) NOT NULL,
	[result_file_id] [int] NULL,
 CONSTRAINT [PK_patient_result_collections] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[patient_result_members]    Script Date: 8/22/2015 8:52:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patient_result_members]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[patient_result_members](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[collection_id] [int] NOT NULL,
	[member_id] [int] NOT NULL,
	[member_type] [tinyint] NOT NULL,
 CONSTRAINT [PK_patient_result_members] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[patient_variant_information]    Script Date: 8/22/2015 8:52:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patient_variant_information]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[patient_variant_information](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[item_id] [int] NOT NULL,
	[item_type] [tinyint] NOT NULL,
	[type_id] [int] NOT NULL,
	[value] [nvarchar](255) NULL,
 CONSTRAINT [PK_patient_variant_information] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[patient_variants]    Script Date: 8/22/2015 8:52:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patient_variants]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[patient_variants](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[variant_type] [tinyint] NOT NULL,
	[reference_id] [int] NOT NULL,
	[patient_id] [int] NOT NULL,
	[value1] [nvarchar](50) NULL,
	[value2] [nvarchar](50) NULL,
	[resulted_on] [datetime2](0) NULL,
 CONSTRAINT [PK_patient_variants] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[patients]    Script Date: 8/22/2015 8:52:11 PM ******/
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
/****** Object:  Table [dbo].[phenotypes]    Script Date: 8/22/2015 8:52:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[phenotypes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[phenotypes](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[value] [nvarchar](255) NULL,
	[external_id] [nvarchar](50) NULL,
	[external_source] [nvarchar](50) NULL,
 CONSTRAINT [PK_phenotypes] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[result_files]    Script Date: 8/22/2015 8:52:11 PM ******/
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
/****** Object:  Table [dbo].[result_sources]    Script Date: 8/22/2015 8:52:11 PM ******/
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
/****** Object:  Table [dbo].[variant_information_types]    Script Date: 8/22/2015 8:52:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[variant_information_types]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[variant_information_types](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](50) NOT NULL,
	[description] [nvarchar](255) NULL,
	[source] [tinyint] NOT NULL,
 CONSTRAINT [PK_variant_information_types] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[variants]    Script Date: 8/22/2015 8:52:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[variants]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[variants](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[gene_id] [int] NULL,
	[external_id] [nvarchar](50) NULL,
	[external_source] [nvarchar](50) NULL,
	[chromosome] [varchar](5) NULL,
	[start_position] [int] NULL,
	[end_position] [int] NULL,
	[reference_genome] [nvarchar](50) NULL,
	[reference_bases] [nvarchar](255) NULL,
 CONSTRAINT [PK_variants] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_genes_symbol]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[genes]') AND name = N'IX_genes_symbol')
CREATE NONCLUSTERED INDEX [IX_genes_symbol] ON [dbo].[genes]
(
	[symbol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_patient_phenotypes_phenotype_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_phenotypes]') AND name = N'IX_patient_phenotypes_phenotype_id')
CREATE NONCLUSTERED INDEX [IX_patient_phenotypes_phenotype_id] ON [dbo].[patient_phenotypes]
(
	[phenotype_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_patient_result_collections_patients]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_result_collections]') AND name = N'IX_patient_result_collections_patients')
CREATE NONCLUSTERED INDEX [IX_patient_result_collections_patients] ON [dbo].[patient_result_collections]
(
	[patient_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_patient_result_members_collection_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_result_members]') AND name = N'IX_patient_result_members_collection_id')
CREATE NONCLUSTERED INDEX [IX_patient_result_members_collection_id] ON [dbo].[patient_result_members]
(
	[collection_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_patient_result_members_member_type_member_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_result_members]') AND name = N'IX_patient_result_members_member_type_member_id')
CREATE NONCLUSTERED INDEX [IX_patient_result_members_member_type_member_id] ON [dbo].[patient_result_members]
(
	[member_type] ASC,
	[member_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_patient_variant_information_item_type_item_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_variant_information]') AND name = N'IX_patient_variant_information_item_type_item_id')
CREATE NONCLUSTERED INDEX [IX_patient_variant_information_item_type_item_id] ON [dbo].[patient_variant_information]
(
	[item_type] ASC,
	[item_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_patient_variant_information_type_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_variant_information]') AND name = N'IX_patient_variant_information_type_id')
CREATE NONCLUSTERED INDEX [IX_patient_variant_information_type_id] ON [dbo].[patient_variant_information]
(
	[type_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_patient_variants_patient_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_variants]') AND name = N'IX_patient_variants_patient_id')
CREATE NONCLUSTERED INDEX [IX_patient_variants_patient_id] ON [dbo].[patient_variants]
(
	[patient_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_patient_variants_variant_type_reference_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patient_variants]') AND name = N'IX_patient_variants_variant_type_reference_id')
CREATE NONCLUSTERED INDEX [IX_patient_variants_variant_type_reference_id] ON [dbo].[patient_variants]
(
	[variant_type] ASC,
	[reference_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_patients_external_source_external_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[patients]') AND name = N'IX_patients_external_source_external_id')
CREATE NONCLUSTERED INDEX [IX_patients_external_source_external_id] ON [dbo].[patients]
(
	[external_source] ASC,
	[external_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_phenotypes_external_source_external_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[phenotypes]') AND name = N'IX_phenotypes_external_source_external_id')
CREATE NONCLUSTERED INDEX [IX_phenotypes_external_source_external_id] ON [dbo].[phenotypes]
(
	[external_source] ASC,
	[external_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_phenotypes_name]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[phenotypes]') AND name = N'IX_phenotypes_name')
CREATE NONCLUSTERED INDEX [IX_phenotypes_name] ON [dbo].[phenotypes]
(
	[name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_result_sources_name]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[result_sources]') AND name = N'IX_result_sources_name')
CREATE NONCLUSTERED INDEX [IX_result_sources_name] ON [dbo].[result_sources]
(
	[name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_variant_information_types_source_name]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[variant_information_types]') AND name = N'IX_variant_information_types_source_name')
CREATE NONCLUSTERED INDEX [IX_variant_information_types_source_name] ON [dbo].[variant_information_types]
(
	[source] ASC,
	[name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_variants_external_source_external_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[variants]') AND name = N'IX_variants_external_source_external_id')
CREATE NONCLUSTERED INDEX [IX_variants_external_source_external_id] ON [dbo].[variants]
(
	[external_source] ASC,
	[external_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_variants_gene_id]    Script Date: 8/22/2015 8:52:11 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[variants]') AND name = N'IX_variants_gene_id')
CREATE NONCLUSTERED INDEX [IX_variants_gene_id] ON [dbo].[variants]
(
	[gene_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_phenotypes_phenotypes]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_phenotypes]'))
ALTER TABLE [dbo].[patient_phenotypes]  WITH CHECK ADD  CONSTRAINT [FK_patient_phenotypes_phenotypes] FOREIGN KEY([phenotype_id])
REFERENCES [dbo].[phenotypes] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_phenotypes_phenotypes]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_phenotypes]'))
ALTER TABLE [dbo].[patient_phenotypes] CHECK CONSTRAINT [FK_patient_phenotypes_phenotypes]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_result_collections_patients]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_result_collections]'))
ALTER TABLE [dbo].[patient_result_collections]  WITH CHECK ADD  CONSTRAINT [FK_patient_result_collections_patients] FOREIGN KEY([patient_id])
REFERENCES [dbo].[patients] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_result_collections_patients]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_result_collections]'))
ALTER TABLE [dbo].[patient_result_collections] CHECK CONSTRAINT [FK_patient_result_collections_patients]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_result_collections_result_files]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_result_collections]'))
ALTER TABLE [dbo].[patient_result_collections]  WITH CHECK ADD  CONSTRAINT [FK_patient_result_collections_result_files] FOREIGN KEY([result_file_id])
REFERENCES [dbo].[result_files] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_result_collections_result_files]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_result_collections]'))
ALTER TABLE [dbo].[patient_result_collections] CHECK CONSTRAINT [FK_patient_result_collections_result_files]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_result_members_patient_result_collections]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_result_members]'))
ALTER TABLE [dbo].[patient_result_members]  WITH CHECK ADD  CONSTRAINT [FK_patient_result_members_patient_result_collections] FOREIGN KEY([collection_id])
REFERENCES [dbo].[patient_result_collections] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_result_members_patient_result_collections]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_result_members]'))
ALTER TABLE [dbo].[patient_result_members] CHECK CONSTRAINT [FK_patient_result_members_patient_result_collections]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_variant_information_variant_information_types]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_variant_information]'))
ALTER TABLE [dbo].[patient_variant_information]  WITH CHECK ADD  CONSTRAINT [FK_patient_variant_information_variant_information_types] FOREIGN KEY([type_id])
REFERENCES [dbo].[variant_information_types] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_variant_information_variant_information_types]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_variant_information]'))
ALTER TABLE [dbo].[patient_variant_information] CHECK CONSTRAINT [FK_patient_variant_information_variant_information_types]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_variants_patients]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_variants]'))
ALTER TABLE [dbo].[patient_variants]  WITH CHECK ADD  CONSTRAINT [FK_patient_variants_patients] FOREIGN KEY([patient_id])
REFERENCES [dbo].[patients] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patient_variants_patients]') AND parent_object_id = OBJECT_ID(N'[dbo].[patient_variants]'))
ALTER TABLE [dbo].[patient_variants] CHECK CONSTRAINT [FK_patient_variants_patients]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_files_result_sources]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_files]'))
ALTER TABLE [dbo].[result_files]  WITH CHECK ADD  CONSTRAINT [FK_result_files_result_sources] FOREIGN KEY([result_source_id])
REFERENCES [dbo].[result_sources] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_result_files_result_sources]') AND parent_object_id = OBJECT_ID(N'[dbo].[result_files]'))
ALTER TABLE [dbo].[result_files] CHECK CONSTRAINT [FK_result_files_result_sources]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_variants_genes]') AND parent_object_id = OBJECT_ID(N'[dbo].[variants]'))
ALTER TABLE [dbo].[variants]  WITH CHECK ADD  CONSTRAINT [FK_variants_genes] FOREIGN KEY([gene_id])
REFERENCES [dbo].[genes] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_variants_genes]') AND parent_object_id = OBJECT_ID(N'[dbo].[variants]'))
ALTER TABLE [dbo].[variants] CHECK CONSTRAINT [FK_variants_genes]
GO
