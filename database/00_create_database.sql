-- =============================================================================
-- 00_create_database.sql
-- Creates the PlacementDb database if it does not exist yet.
-- Run while connected to master (or any database).
-- =============================================================================
IF DB_ID(N'PlacementDb') IS NULL
BEGIN
    CREATE DATABASE PlacementDb;
    PRINT 'PlacementDb created.';
END
ELSE
    PRINT 'PlacementDb already exists.';
GO
