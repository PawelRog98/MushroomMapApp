@ECHO OFF
cls
setlocal EnableDelayedExpansion

set migratorLoc=E:\Projekty\ChatShaker\ChatShaker.Migrator
set dbConn="Server=localhost;Database=ChatShaker;User Id=root;Password=Password-1;TrustServerCertificate=True"

echo Choose option to run:
echo 1) Migration
echo 2) Rollback number of migrations
echo 3) Rollback all migrations

set /p choice=Choose option (1-3): 

if "%choice%"=="1" set command=migrate
if "%choice%"=="2" (
	set /p rollbackQuantity=Set quantity of migrations to rollback: 
    	if "!rollbackQuantity!"=="" (
        echo Error
        pause
        exit /b
    )
    set command=rollback !rollbackQuantity!
)
if "%choice%"=="3" set command=rollbackall

if not defined command (
    echo Error
    pause
    exit /b
)

cd /d "%migratorLoc%"

echo %command%

dotnet run %dbConn% %command%

pause