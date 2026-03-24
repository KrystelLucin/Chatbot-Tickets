# Bot Conversacional en C# (.NET)

Este proyecto implementa un motor conversacional para un bot de soporte, desarrollado en C# con .NET, siguiendo una arquitectura por capas y cumpliendo los requerimientos de manejo de estado, flujos guiados e integración con servicios externos protegidos mediante OAuth.

---

## Funcionalidades

### 1. Creación de tickets (flujo guiado)

El bot permite crear un ticket mediante un flujo paso a paso:

1. Solicita nombre  
2. Solicita email (con validación de formato)  
3. Solicita descripción del problema  
4. Muestra resumen  
5. Solicita confirmación  
6. Crea el ticket llamando a un servicio externo  

---

### 2. Cancelación de flujo

En cualquier momento, el usuario puede escribir:

cancelar

El bot:
- detiene el flujo actual  
- limpia el estado de la conversación  

---

### 3. Consulta de estado de ticket

Ejemplo:

ver estado del ticket TCK-123

El bot:
- consulta el servicio externo  
- devuelve el estado del ticket  

---

### 4. Manejo de estado conversacional

- El estado se almacena en memoria por conversationId  
- Se controla mediante una máquina de estados simple  
- No se utiliza base de datos (según requerimiento)  

---

## Arquitectura

BotConversacional.sln  
├── BotConversacional.Gateway  
├── BotConversacional.Api  
├── BotConversacional.Application  
├── BotConversacional.Domain  
├── BotConversacional.Infrastructure  
└── BotConversacional.MockApi  

---

### Descripción de capas

- Gateway: Punto de entrada (Ocelot), actualmente como placeholder  
- Api: Exposición HTTP (POST /messages)  
- Application: Lógica conversacional y casos de uso  
- Domain: Modelos del dominio (estado conversacional, enums)  
- Infrastructure: Implementaciones técnicas (HTTP clients, OAuth, almacenamiento en memoria)  
- MockApi: Simulación del servicio externo (OAuth + tickets)  

---

## Cómo ejecutar el proyecto

### 1. Ejecutar MockApi

dotnet run --project BotConversacional.MockApi

---

### 2. Ejecutar Api

dotnet run --project BotConversacional.Api

---

### 3. Probar endpoint

POST /messages

{
  "conversationId": "test-1",
  "message": "quiero crear un ticket"
}

---

## Decisiones de diseño

- Se implementó un motor conversacional basado en estados para tener control explícito del flujo.  
- Se separó la lógica de negocio (Application) de la infraestructura (HTTP, OAuth).  
- Se utilizó almacenamiento en memoria conforme al requerimiento.  
- Se encapsularon las integraciones externas mediante interfaces para facilitar extensibilidad.  
