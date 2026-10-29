$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$framework = (Get-Content -Path framework.txt)
$version = (Get-Content -Path version.txt)
$csproj = ".\wsl-ssh-pageant-monitor.csproj"

Write-Host "Configuring framework ${framework} and version ${version}"

[xml]$xml = (Get-Content -Path $csproj)

if ($null -eq $xml) {
    throw "Failed to read csproj"
}

$frameworkNode = $xml.SelectSingleNode("Project/PropertyGroup/TargetFramework")

if ($null -eq $frameworkNode) {
    throw "TargetFramework node not found"
} else {
    $frameworkNode.'#text' = $framework
}

$versionNode = $xml.SelectSingleNode('Project/PropertyGroup/Version')

if ($null -eq $versionNode) {
    throw "Version node not found"
} else {
    $versionNode.'#text' = $version
}

$xml.Save($csproj)

Write-Host "Publishing..."
dotnet publish --configuration Release

Write-Host "Compressing..."
Compress-Archive -Path ".\bin\Release\${framework}\win-x64\publish\wsl-ssh-pageant-monitor.exe" -DestinationPath ".\bin\Release\${framework}\win-x64\publish\wsl-ssh-pageant-monitor-${version}.zip" -CompressionLevel Optimal

Write-Host "Success"
