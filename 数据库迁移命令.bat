@echo off
set /p MigrationName="Enter the name of the migration: "
PowerShell -Command "cd Infrastructure; dotnet ef migrations add %MigrationName%" || pause