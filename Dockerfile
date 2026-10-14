FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 10000
ENTRYPOINT ["dotnet", "recipe-api.dll"]