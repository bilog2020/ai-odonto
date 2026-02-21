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
cd src/AiOdonto.Api
# Configurar appsettings.json con las API keys
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
