cd ..
echo 'Builing Release Mode...'
dotnet build ./src/HttpClientGenerator/HttpClientGenerator.csproj --configuration Release

echo 'Packing ...'
dotnet pack ./src/HttpClientGenerator/HttpClientGenerator.csproj --configuration Release

echo 'Publishing ...'
dotnet nuget push bin/*.nupkg --skip-duplicate