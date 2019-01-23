FROM microsoft/dotnet:2.0-sdk as builder

RUN mkdir /app
WORKDIR /app

COPY pfSenseBackup/ .
RUN dotnet clean
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Actual deployment image
FROM microsoft/dotnet:2.0-runtime
RUN mkdir /app
WORKDIR /app
COPY --from=builder /app/out/ /app/
ENTRYPOINT ["dotnet", "pfSenseBackup.dll"]