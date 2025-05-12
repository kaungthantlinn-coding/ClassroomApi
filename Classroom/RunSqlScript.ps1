# PowerShell script to run SQL script
Write-Host "Running SQL script to create SubmissionAttachments table..."

# Get the connection string from appsettings.json
$appSettings = Get-Content -Path "appsettings.json" -Raw | ConvertFrom-Json
$connectionString = $appSettings.ConnectionStrings.DefaultConnection

Write-Host "Using connection string: $connectionString"

# Extract server and database from connection string
$server = $connectionString -replace ".*Data Source=([^;]+).*", '$1'
$database = $connectionString -replace ".*Initial Catalog=([^;]+).*", '$1'

Write-Host "Server: $server, Database: $database"

# Run the SQL script using Invoke-Sqlcmd
try {
    Invoke-Sqlcmd -ServerInstance $server -Database $database -InputFile "CreateSubmissionAttachmentsTable.sql"
    Write-Host "SQL script executed successfully"
}
catch {
    Write-Host "Error executing SQL script: $_"
}

Write-Host "Done."
