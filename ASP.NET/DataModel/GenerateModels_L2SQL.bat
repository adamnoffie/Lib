@ECHO OFF
REM GenerateModels_L2SQL.bat
REM Adam Nofsinger

REM Modify these to fit your application/model generation needs
SET server=192.168.200.151
SET user=username
SET pass=password
SET db_name=database_name
SET context_name=DefaultDataContext
SET namespace=Data

@ECHO ON
"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin\SqlMetal.exe" /server:%server% /user:%user% /password:%pass% /database:%db_name% /timeout:0 /code:"%~dp0Models_L2SQL.cs" /context:%context_name% /namespace:%namespace% /language:csharp /views /functions /sprocs /pluralize