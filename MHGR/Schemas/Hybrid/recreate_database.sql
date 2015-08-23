USE [master]
GO

alter database [mhgr_hybrid] set single_user with rollback immediate

IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'mhgr_hybrid')
DROP DATABASE [mhgr_hybrid]
GO


USE [master]
GO

/****** Object:  Database [mhgr_hybrid]    Script Date: 8/17/2015 9:54:42 PM ******/
CREATE DATABASE [mhgr_hybrid]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'mhgr_hybrid', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.BASHERDB\MSSQL\DATA\mhgr_hybrid.mdf' , SIZE = 25600KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'mhgr_hybrid_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.BASHERDB\MSSQL\DATA\mhgr_hybrid_log.ldf' , SIZE = 10240KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO

ALTER DATABASE [mhgr_hybrid] SET COMPATIBILITY_LEVEL = 120
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [mhgr_hybrid].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [mhgr_hybrid] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET ARITHABORT OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [mhgr_hybrid] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [mhgr_hybrid] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET  DISABLE_BROKER 
GO

ALTER DATABASE [mhgr_hybrid] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [mhgr_hybrid] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET RECOVERY SIMPLE 
GO

ALTER DATABASE [mhgr_hybrid] SET  MULTI_USER 
GO

ALTER DATABASE [mhgr_hybrid] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [mhgr_hybrid] SET DB_CHAINING OFF 
GO

ALTER DATABASE [mhgr_hybrid] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [mhgr_hybrid] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO

ALTER DATABASE [mhgr_hybrid] SET DELAYED_DURABILITY = DISABLED 
GO

ALTER DATABASE [mhgr_hybrid] SET  READ_WRITE 
GO

