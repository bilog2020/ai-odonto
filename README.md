# Biblioteca Digital Odontológica

> AI-powered dental reference library for FOUBA students.

**Repositorio:** https://github.com/bilog2020/ai-odonto

## Stack

- **Backend:** ASP.NET Core / .NET 10, Entity Framework Core, SQL Server
- **AI:** Claude (Anthropic) via Semantic Kernel, Voyage AI embeddings (RAG)
- **Auth:** ASP.NET Identity + JWT
- **Mobile:** React Native (Expo), expo-router
- **Infra:** Docker, GitHub Actions

## Inicio rápido

### Backend

```bash
# 1. Crear appsettings.Development.json con las API keys (nunca se sube a git)
# src/AiOdonto.Api/appsettings.Development.json
{
  "Anthropic": { "ApiKey": "sk-ant-..." },
  "VoyageAi": { "ApiKey": "pa-..." }
}

# 2. Crear la base de datos
cd src/AiOdonto.Api
dotnet ef database update

# 3. Arrancar la API
dotnet run
```

### Con Docker

```bash
ANTHROPIC_API_KEY=sk-... VOYAGE_API_KEY=pa-... docker compose up
```

### Mobile

```bash
cd mobile
npm install
npx expo start
```

### Tests

```bash
dotnet test AiOdonto.slnx
```
