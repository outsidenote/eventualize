```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=MasadNetunim12!@" -p 1433:1433 --name sql --hostname sql -d  mcr.microsoft.com/mssql/server:2022-latest
```