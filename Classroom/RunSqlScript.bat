@echo off
echo Running SQL script to create SubmissionAttachments table...

REM Get the connection string from appsettings.json
for /f "tokens=*" %%a in ('findstr /C:"DefaultConnection" appsettings.json') do (
    set connectionLine=%%a
)

REM Extract the connection string
set connectionString=%connectionLine:*:=%
set connectionString=%connectionString:~2,-2%

echo Using connection string: %connectionString%

REM Run the SQL script using sqlcmd
sqlcmd -S localhost -d ClassroomDB -i CreateSubmissionAttachmentsTable.sql

echo Done.
