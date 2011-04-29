@ECHO OFF
REM GenerateModels_L2SQL.bat
REM (The SQLExpress App_Data folder mdf variant)
REM Adam Nofsinger

REM Modify these to fit your application/model generation needs
SET mdf_file=Database.mdf
SET context_name=DefaultDataContext
SET namespace=Data
SET timeout=5000

@ECHO OFF
cd %~dp0
cd ..\..\App_Data

@ECHO ON
"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin\SqlMetal.exe" /timeout:%timeout% /code:"%~dp0Models_L2SQL.cs" /context:%context_name% /namespace:%namespace% /language:csharp /views /functions /sprocs /pluralize "%CD%\%mdf_file%"
cd %~dp0