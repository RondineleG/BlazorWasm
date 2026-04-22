# ============================================
# MrBur Terminal - Deployment Guide
# ============================================

## Estrutura

O projeto consiste em:
- **BlazorTerminal.Api** - Backend com Minimal APIs, WebSocket, SSH.NET
- **BlazorWasm** - Frontend Blazor (já deployado no GitHub Pages)

## Deploy na VPS

### 1. Conectar na VPS

```bash
ssh -i ~/.ssh/mrbur_deploy root@172.245.152.43
```

### 2. Parar containers existentes

```bash
docker stop mrbur-api mrbur-worker mrbur-web mrbur-redis 2>/dev/null
docker rm mrbur-api mrbur-worker mrbur-web mrbur-redis 2>/dev/null
docker rmi $(docker images --filter "reference=ghcr.io/rondineleg/*" -q) 2>/dev/null
```

### 3. Rodar o novo container

```bash
docker run -d \
  --name mrbur-terminal \
  -p 8080:8080 \
  -e Jwt__Secret="MrBurTerminalSecretKey2026!@#$%" \
  -e Jwt__Issuer="MrBurTerminal" \
  -e Jwt__ExpiryHours="24" \
  -e Ssh__Host="172.245.152.43" \
  -e Ssh__Username="root" \
  -e Ssh__KeyPath="/root/.ssh/mrbur_deploy" \
  -e Ssh__Port="22" \
  -v ~/.ssh/mrbur_deploy:/root/.ssh/mrbur_deploy:ro \
  ghcr.io/rondineleg/mrbur-terminal:latest
```

### 4. Verificar

```bash
docker ps
curl http://localhost:8080/health
```

## Endpoints da API

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | /api/auth/register | Criar usuário |
| POST | /api/auth/login | Login (retorna JWT) |
| GET | /api/auth/me | Usuário atual |
| WS | /ws/terminal | Terminal SSH |
| GET | /health | Health check |

## Frontend (BlazorWasm)

O frontend está em `BlazorWasm/` e faz deploy automático para GitHub Pages.

Após login, o usuário acessa `/terminal` e conecta via WebSocket ao backend.

## Fluxo de Autenticação

1. Usuário acessa `/login`
2. Envia username + password
3. API retorna JWT (24h de expiração)
4. Frontend armazena no localStorage
5. WebSocket conecta com ?token=JWT