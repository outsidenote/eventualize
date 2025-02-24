## Jaeger, Prometheus & Grafana

Project structure:

```txt
.
├── compose.yaml
├── redis
├── jaeger
├── grafana
│   └── datasource.yml
├── prometheus
│   └── prometheus.yml
└── README.md
```

[_compose.yaml_](compose.yaml)

```yml
services:
  redis:
    image: redislabs/rejson
    ...
    ports:
      - 6379:6379
  jaeger:
    image: jaegertracing/opentelemetry-all-in-one
    ...
    ports:
      - 16686:16686
      - 4318:4318
      - 4317:4317
prometheus:
    image: prom/prometheus
    ...
    ports:
      - 9090:9090
  grafana:
    image: grafana/grafana
    ...
    ports:
      - 3000:3000
```

The compose file defines a stack with two services `prometheus` and `grafana`.
When deploying the stack, docker compose maps port the default ports for each service to the equivalent ports on the host in order to inspect easier the web interface of each service.
Make sure the ports 9090 and 3000 on the host are not already in use.

## Deploy with docker compose

```bash
$ docker compose up -d
Creating network "event-sourcing-backbone_default" with the default driver
Creating volume "event-sourcing-backbone_prom_data" with default driver
...
Creating redis    ... done
Creating jaeger    ... done
Creating grafana    ... done
Creating prometheus ... done
Attaching to prometheus, grafana

```

## Expected result

Listing containers must show two containers running and the port mapping as below:

```bash
$ docker ps
CONTAINER ID        IMAGE                                     COMMAND                  CREATED             STATUS              PORTS                    NAMES
xsdec63w814f        redislabs/rejson                          "..."                    8 minutes ago       Up 8 minutes        0.0.0.0:6379->6379/tcp   redis
abdes63s814f        jaegertracing/opentelemetry-all-in-one    "..."                    8 minutes ago       Up 8 minutes        0.0.0.0:...              jaeger
dbdec637814f        prom/prometheus                           "/bin/prometheus --c…"   8 minutes ago       Up 8 minutes        0.0.0.0:9090->9090/tcp   prometheus                      
79f667cb7dc2        grafana/grafana                           "/run.sh"                8 minutes ago       Up 8 minutes        0.0.0.0:3000->3000/tcp   grafana
```

Navigate to [`http://127.0.0.1:3000`](http://127.0.0.1:3000) in your web browser and use the login credentials specified in the compose file to access Grafana. It is already configured with prometheus as the default datasource.



Navigate to [`http://127.0.0.1:9090`](http://127.0.0.1:9090) in your web browser to access directly the web interface of prometheus.
- Check prometheus targets [`http://127.0.0.1:9090/targets`](http://127.0.0.1:9090/targets)

Stop and remove the containers. Use `-v` to remove the volumes if looking to erase all data.

```bash
$ docker compose down -v
```
