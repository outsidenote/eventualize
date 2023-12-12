---
layout: default
title: Code Contribution
nav_order: 1
parent: Contribution
---

# Code Contribution

## Quick Start

// TODO:

1. Install CLI
2. Create template
3. Spin Docker or having a connection string to cloud/on-prem DB

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=MasadNetunim12!@" -p 1433:1433 --name sql --hostname sql -d  mcr.microsoft.com/mssql/server:2022-latest
```

4. run tests
5. Run the sample code (F5)
6. Check the Telemetry
7. Manual deploy NuGet to nuget.org (first time)
8. CI/CD Set the permission & secret (will deploy NuGet on each push)