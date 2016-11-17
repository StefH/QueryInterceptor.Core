dotnet restore
dotnet pack -c Release QueryInterceptor.Core\project.json
dotnet pack -c Release QueryInterceptor.EntityFramework\project.json
pause