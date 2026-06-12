#!/bin/bash

migratorLoc="/home/parroter/Repos/MushroomMapApp/src/MushroomMapApp.Migrator"
dbconn="Host=127.0.0.1;Database=MushroomMapApp;Username=root;Password=Password-1"

clear

echo "Choose option to run:"
echo "1) Migration"
echo "2) Rollback number of migrations"
echo "3) Rollback all migrations"

read -p "Choose option (1-3): " choice

cd "$migratorLoc" || exit 1

case "$choice" in
    1)
        dotnet run -- "$dbconn" "migrate"
        ;;
    2)
        read -p "Set quantity of migrations to rollback: " rollback_quantity
        if [ -z "$rollback_quantity" ]; then
            echo "Error"
            exit 1
        fi
        dotnet run -- "$dbconn" "rollback" "$rollback_quantity"
        ;;
    3)
        dotnet run -- "$dbconn" "rollbackall"
        ;;
esac


read -p "Press Enter to exit..."
exit 1