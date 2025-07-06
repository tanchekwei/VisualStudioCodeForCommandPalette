study README.md in current directory to understand the project
study flow.md in current directory to understand the project
then list all files in Documentation folder and study Documentation/overview.md
you can read other files if you have any question about the extension framework or the helpers

task 1. my test solution is in WorkspaceLauncherForVSCode.Tests folder, help me ensure proper code coverage for the project
dotnet build both solution ensure no error

study WorkspaceLauncherForVSCode.Tests.csproj to understand the test library used and do not change it

never change WorkspaceLauncherForVSCode.csproj

ensure full code coverage and ensure all tests passed

ensure the test case is valid

below is the command to run test with code coverage
PS D:\Code\VSCodeExtension\MyExtension> cd WorkspaceLauncherForVSCode.Tests
PS D:\Code\VSCodeExtension\MyExtension\WorkspaceLauncherForVSCode.Tests> dotnet test --collect:"XPlat Code Coverage" --logger "trx;LogFileName=TestResults.trx" --verbosity minimal

only read WorkspaceLauncherForVSCode.Tests\TestResults\TestResults.trx to understand what is the error

do not read WorkspaceLauncherForVSCode.Tests\TestResults\c16c78a7-829f-4986-94f7-819fb77e1e2a\coverage.cobertura.xml

must use the options --logger "trx;LogFileName=TestResults.trx" --verbosity minimal to reduce the result size for you to read.

do not read WorkspaceLauncherForVSCode.Tests\TestResults\c16c78a7-829f-4986-94f7-819fb77e1e2a\coverage.cobertura.xml
only read WorkspaceLauncherForVSCode.Tests\TestResults\TestResults.trx to understand what is the error

to reduce warning message, run with dotnet build WorkspaceLauncherForVSCode.Tests/WorkspaceLauncherForVSCode.Tests.csproj -r win-x64--nologo -v q --property WarningLevel=0 /clp:ErrorsOnly

do not read the coverage report
do net read coverage.cobertura.xml