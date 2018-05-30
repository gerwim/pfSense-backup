FROM microsoft/dotnet:2.0-sdk

RUN mkdir /app
WORKDIR /app

COPY pfSenseBackup/ .
RUN dotnet clean
RUN dotnet restore
RUN dotnet publish -c Release -o out

ENTRYPOINT ["dotnet", "out/pfSenseBackup.dll"]
