# Despliegue de Infraestructura con Bicep

Este proyecto incluye archivos de infraestructura como código (IaC) usando Azure Bicep para desplegar la aplicación en Azure App Service.

## Archivos

- `infrastructure.bicep`: Template principal que define la infraestructura
- `infrastructure.parameters.json`: Archivo de parámetros con valores configurables
- `deploy.sh`: Script de despliegue automatizado

## Recursos Creados

El template de Bicep crea los siguientes recursos:

1. **App Service Plan**: Plan de hospedaje para la aplicación
2. **App Service**: Servicio web donde se despliega la API

## Requisitos Previos

1. Azure CLI instalado
2. Autenticación en Azure: `az login`
3. Suscripción de Azure activa

## Parámetros Configurables

| Parámetro | Descripción | Valores Permitidos | Default |
|-----------|-------------|-------------------|---------|
| `resourceBaseName` | Nombre base para los recursos | string | - |
| `location` | Región de Azure | string | eastus |
| `appServicePlanSku` | SKU del plan de servicio | F1, B1, B2, S1, S2, P1V2, P2V2 | F1 |
| `environment` | Ambiente de despliegue | dev, staging, prod | dev |

## Despliegue Manual

### 1. Crear grupo de recursos

```bash
az group create --name rg-empresaapi-dev --location eastus
```

### 2. Validar el template

```bash
az deployment group validate \
    --resource-group rg-empresaapi-dev \
    --template-file infrastructure.bicep \
    --parameters @infrastructure.parameters.json
```

### 3. Desplegar

```bash
az deployment group create \
    --resource-group rg-empresaapi-dev \
    --template-file infrastructure.bicep \
    --parameters @infrastructure.parameters.json
```

## Despliegue Automatizado

El script `deploy.sh` automatiza todo el proceso:

```bash
chmod +x deploy.sh
./deploy.sh
```

## Despliegue de la Aplicación

Después de crear la infraestructura, despliega tu aplicación:

```bash
# Publicar la aplicación
dotnet publish -c Release -o ./publish

# Crear archivo zip
cd publish
zip -r ../app.zip .
cd ..

# Desplegar en Azure
az webapp deploy \
    --resource-group rg-empresaapi-dev \
    --name <nombre-app-service> \
    --src-path app.zip \
    --type zip
```

## Personalización

Modifica `infrastructure.parameters.json` para cambiar:
- Nombre de los recursos
- Región de despliegue
- SKU del App Service Plan
- Ambiente (dev/staging/prod)

## Limpieza de Recursos

Para eliminar todos los recursos creados:

```bash
az group delete --name rg-empresaapi-dev --yes --no-wait
```
