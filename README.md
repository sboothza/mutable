# Mutable

# Build Instructions

Need to manually update package
run from project folder:


    dotnet build --configuration Release Mutable.csproj
    dotnet pack --configuration Release --no-build Mutable.csproj
    cp bin/Release/*.nupkg /D/src/nuget_repo
