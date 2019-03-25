dotnet publish ConsoleHost\ConsoleHost.csproj -c release -r alpine-x64 /p:CrossGenDuringPublish=false /p:ShowLinkerSizeComparison=true

docker build -f ".\Dockerfile" -t application-insights-istio-mixer-adapter:dev --no-cache ".\ConsoleHost\bin\Release\netcoreapp2.1\alpine-x64\publish"