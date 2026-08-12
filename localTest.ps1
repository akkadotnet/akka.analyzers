# Store the original working directory
$originalDir = Get-Location

try {
    dotnet pack -c Release -o ./bin/nuget

    cd ./test/Akka.Analyzers.NetFxInstallCanary

    # Install Akka.Analyzers package
    # package source mapping should force it to happen using local dir
    dotnet add package Akka.Analyzers

    # Restore and build the project
    dotnet restore
    dotnet build
}
catch {
    Write-Error "An error occurred: $_"
}
finally {
    # Return to the original directory
    Set-Location $originalDir
}
