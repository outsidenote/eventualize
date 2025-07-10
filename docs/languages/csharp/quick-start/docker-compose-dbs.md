---
layout: default
title: Set up local environment
nav_order: 6
parent: Quick Start
grand_parent: Languages
has_children: false
---

# Local Environment set up

In order to test the quick start on local machine using docker compose,
create a `compose.yml` file  
with the following content:

```yml
name: evdb-quick-start-databases

volumes:
  mssql:
  psql:
  mongodb_data:

services:
  sqlserver:
    container_name: sqlserver-event-source-quick-start
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "MasadNetunim12!@"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    restart: unless-stopped

  psql:
    container_name: psql-event-source-quick-start
    image: postgres:latest
    environment:
      POSTGRES_USER: test_user
      POSTGRES_PASSWORD: MasadNetunim12!@
      POSTGRES_DB: test_db
    volumes:
      - psql:/var/lib/postgresql/data
      - ./dev/docker_psql_init:/docker-entrypoint-initdb.d
    ports:
      - 5432:5432
    restart: unless-stopped

  mongodb:
    image: mongo:8
    container_name: mongodb-event-source-quick-start
    volumes:
      - mongodb_data:/data/db
    environment:
      MONGO_INITDB_DATABASE: evdb
    healthcheck:
      test: echo 'db.runCommand("ping").ok' | mongosh localhost:27017/test --quiet
      interval: 10s
      timeout: 10s
      retries: 5
      start_period: 40s
    ports:
      - "27017:27017"
    command: "--bind_ip_all --quiet --logpath /dev/null --replSet rs0"
    restart: unless-stopped
  mongo-init:
    image: mongo:8
    container_name: mongodb-event-source-init-quick-start
    depends_on:
      mongodb:
        condition: service_healthy
    command: >
      mongosh --host mongodb:27017 --eval
      '
      rs.initiate( {
         _id : "rs0",
         members: [
            { _id: 0, host: "localhost:27017" }
         ]
      })
      '
    restart: no
```

⚠ You need a docker environment on your machine in order to run it!

Run it using the following command:

```bash
docker compose up -d
```

to get it down use:

```bash
docker compose down
// or use `-v` to take the volume down as well
docker compose down -v
```
