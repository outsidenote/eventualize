Sure, here's the updated Markdown file that includes detailed explanations of `sqlcmd` and `bcp`:

```markdown
# Docker Compose Setup with SQL Server, Adminer, and SQL Tools

This guide provides a Docker Compose setup for SQL Server, Adminer, and command-line tools `sqlcmd` and `bcp`. These tools allow you to interact with SQL Server from the command line.

## Docker Compose File

Create a file named `docker-compose.yml` and add the following content:

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:latest
    container_name: sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrongPassword!
    ports:
      - "1433:1433"
    volumes:
      - sqlserverdata:/var/opt/mssql

  adminer:
    image: adminer
    container_name: adminer
    depends_on:
      - sqlserver
    ports:
      - "8080:8080"

  mssql-tools:
    image: mcr.microsoft.com/mssql-tools
    container_name: mssql-tools
    depends_on:
      - sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrongPassword!
    stdin_open: true
    tty: true

volumes:
  sqlserverdata:
    driver: local
```

### Explanation

1. **sqlserver**: Runs SQL Server.
2. **adminer**: Provides a web-based interface for SQL Server.
3. **mssql-tools**: Includes the `sqlcmd` and `bcp` command-line tools.

## sqlcmd and bcp

### sqlcmd

`sqlcmd` is a command-line tool used to run T-SQL commands and scripts against a SQL Server instance. It allows you to execute SQL queries, perform database management tasks, and run SQL scripts.

#### Common Uses of sqlcmd:
- **Running Queries**: Execute SQL queries directly from the command line.
- **Running Scripts**: Run SQL scripts saved in files.
- **Database Management**: Perform tasks such as creating databases, managing tables, and controlling users and permissions.
- **Automation**: Integrate with batch files and scripts for automation of routine database tasks.

#### Example Usage:
- **Run a Query:**
  ```sh
  sqlcmd -S 127.0.0.1 -U sa -P YourStrongPassword! -Q "SELECT * FROM YourDatabase.dbo.YourTable"
  ```
- **Run a Script:**
  ```sh
  sqlcmd -S 127.0.0.1 -U sa -P YourStrongPassword! -i path/to/your/script.sql
  ```

### bcp (Bulk Copy Program)

`bcp` is a command-line utility used for bulk data export and import between a SQL Server instance and a data file. It is useful for loading large amounts of data into SQL Server tables and exporting data out of SQL Server tables.

#### Common Uses of bcp:
- **Import Data**: Load data from a file into a SQL Server table.
- **Export Data**: Export data from a SQL Server table to a file.
- **Data Migration**: Migrate large volumes of data between different SQL Server instances or between SQL Server and other systems.

#### Example Usage:
- **Export Data to a File:**
  ```sh
  bcp YourDatabase.dbo.YourTable out path/to/outputfile.csv -c -t, -S 127.0.0.1 -U sa -P YourStrongPassword!
  ```
  - `-c`: Use character data type.
  - `-t,`: Use comma as the field terminator.

- **Import Data from a File:**
  ```sh
  bcp YourDatabase.dbo.YourTable in path/to/inputfile.csv -c -t, -S 127.0.0.1 -U sa -P YourStrongPassword!
  ```

## Using sqlcmd and bcp in Docker Container

1. **Run a Command with sqlcmd:**
   ```sh
   docker exec -it mssql-tools sqlcmd -S sqlserver -U sa -P YourStrongPassword! -Q "SELECT * FROM YourDatabase.dbo.YourTable"
   ```

2. **Run a Script with sqlcmd:**
   ```sh
   docker exec -it mssql-tools sqlcmd -S sqlserver -U sa -P YourStrongPassword! -i /path/to/your/script.sql
   ```

3. **Export Data with bcp:**
   ```sh
   docker exec -it mssql-tools bcp YourDatabase.dbo.YourTable out /path/to/outputfile.csv -c -t, -S sqlserver -U sa -P YourStrongPassword!
   ```

4. **Import Data with bcp:**
   ```sh
   docker exec -it mssql-tools bcp YourDatabase.dbo.YourTable in /path/to/inputfile.csv -c -t, -S sqlserver -U sa -P YourStrongPassword!
   ```

This setup allows you to manage and interact with your SQL Server instance using `sqlcmd` and `bcp` directly from the Docker container.
```
