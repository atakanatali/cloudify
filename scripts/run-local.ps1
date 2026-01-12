$ErrorActionPreference = 'Stop'

$rootDir = Resolve-Path (Join-Path $PSScriptRoot '..')
$apiProject = Join-Path $rootDir 'Cloudify.Api/Cloudify.Api.csproj'
$uiProject = Join-Path $rootDir 'Cloudify.Ui/Cloudify.Ui.csproj'

$apiProcess = Start-Process dotnet -ArgumentList @('run', '--project', $apiProject, '--urls', 'https://localhost:5001') -PassThru

try {
    dotnet run --project $uiProject
}
finally {
    if (-not $apiProcess.HasExited) {
        Stop-Process -Id $apiProcess.Id
    }
}
