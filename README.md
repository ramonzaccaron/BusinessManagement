# BusinessManagement


Modelo de Telemetria atual

![Arquitetura do projeto e Observabilidade](https://github.com/user-attachments/assets/07170221-ff61-437a-8907-0022b4b66a9e)

## Resumo das Portas por Serviço

| Serviço | Porta | Protocolo | Função | 
|-------------|-----------|---------------|------------| 
| OpenTelemetry Collector | 4317 | gRPC | Recebe telemetria das apps | 
| OpenTelemetry Collector | 4318 | HTTP | Recebe telemetria das apps | 
| OpenTelemetry Collector | 8889 | HTTP | Expõe métricas Prometheus | 
| Grafana Tempo | 3200 | HTTP | API HTTP para consultas | 
| Grafana Tempo | 9095 | gRPC | Comunicação interna | 
| Grafana Tempo | 4317 | gRPC | Recebe traces do Collector | 
| Grafana Tempo | 4318 | HTTP | Recebe traces do Collector | 
| Jaeger | 4317 | gRPC | Recebe traces do Collector | 
| Jaeger | 16686 | HTTP | Interface web | 
| Prometheus | 9090 | HTTP | Interface web e API | 
| Loki | 3100 | HTTP | API para logs |
