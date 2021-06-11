@echo off
start cmd.exe @cmd /k "dotnet run --project DisbotNext.Api"
cd DisbotNext.Web
start cmd.exe @cmd /k "npm i && npm run serve"
cd ..