# api-backend

## Production

```env
JWT_KEY=XXXXXXXXXXXXXXX(atleast 128 bits)
MOZART_HASKELL=HOST:PORT
MOZART_PYTHON=HOST:PORT
SEED_ADMIN="true"/"false"
# if seed admin then
ADMIN_MAIL="someEmail@email.dk"
ADMIN_PASS="YourPass!1231"
```

## Development

Compose env format:

```env
API_PORT=xx
POSTGRES_HOST=xx
POSTGRES_DB=xx
POSTGRES_USER=xx
POSTGRES_PASSWORD=xx
POSTGRES_PORT=xx
```

To access the most recent test-coverage report, go to: https://p7-projekt.github.io/api-backend/

[![Coverage Report](https://github.com/p7-projekt/api-backend/actions/workflows/pages/pages-build-deployment/badge.svg)](https://p7-projekt.github.io/api-backend/)
[![CI](https://github.com/p7-projekt/api-backend/actions/workflows/dotnet.yml/badge.svg)]()
