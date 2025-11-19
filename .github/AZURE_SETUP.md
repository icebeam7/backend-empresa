# Configuración de GitHub Actions para Azure

## Secretos Requeridos

Para que el workflow de GitHub Actions pueda desplegar en Azure, necesitas configurar los siguientes secretos en tu repositorio:

### 1. AZURE_CREDENTIALS

Este secreto contiene las credenciales de un Service Principal con permisos para crear recursos en Azure.

#### Pasos para crear el Service Principal:

```bash
# 1. Login a Azure
az login

# 2. Obtener tu Subscription ID
az account show --query id -o tsv

# 3. Crear Service Principal con rol Contributor
az ad sp create-for-rbac \
  --name "github-actions-empresaapi" \
  --role contributor \
  --scopes /subscriptions/{SUBSCRIPTION_ID} \
  --sdk-auth
```

El comando anterior generará un JSON similar a este:

```json
{
  "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "subscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

**Copia TODO el JSON completo y agrégalo como secreto en GitHub.**

### 2. AZURE_SUBSCRIPTION_ID (Opcional)

Tu Subscription ID de Azure (puedes obtenerlo del JSON anterior o con `az account show --query id -o tsv`)

## Configurar Secretos en GitHub

1. Ve a tu repositorio en GitHub
2. Navega a **Settings** → **Secrets and variables** → **Actions**
3. Click en **New repository secret**
4. Agrega los siguientes secretos:

| Nombre | Valor | Descripción |
|--------|-------|-------------|
| `AZURE_CREDENTIALS` | JSON completo del Service Principal | Credenciales para autenticación |
| `AZURE_SUBSCRIPTION_ID` | ID de tu suscripción | ID de suscripción de Azure |

## Configurar Environments (Opcional)

Para mayor seguridad y control, puedes configurar environments en GitHub:

1. Ve a **Settings** → **Environments**
2. Crea environments para: `dev`, `staging`, `prod`
3. Configura **protection rules** para cada environment:
   - Requiere aprobación manual para `prod`
   - Restringe a branches específicos

## Uso del Workflow

### Ejecutar manualmente:

1. Ve a la pestaña **Actions** en tu repositorio
2. Selecciona el workflow **"Deploy Azure Infrastructure"**
3. Click en **"Run workflow"**
4. Completa los parámetros requeridos:
   - **Nombre del grupo de recursos**: `rg-empresaapi-dev`
   - **Ubicación**: `eastus`
   - **Nombre base**: `empresaapi`
   - **SKU**: `F1` (Free tier)
   - **Ambiente**: `dev`
   - **Crear grupo de recursos**: `true`

5. Click en **"Run workflow"**

### Parámetros Disponibles

#### Requeridos:
- **resource_group_name**: Nombre del grupo de recursos donde se crearán los recursos
- **location**: Región de Azure (eastus, westus, etc.)
- **resource_base_name**: Nombre base que se usará para nombrar los recursos
- **app_service_plan_sku**: Tier del plan (F1, B1, B2, S1, S2, P1V2, P2V2)

#### Opcionales:
- **environment**: Ambiente de despliegue (dev, staging, prod) - Default: `dev`
- **create_resource_group**: Si debe crear el grupo de recursos - Default: `true`

## Verificación del Despliegue

Después de ejecutar el workflow:

1. Revisa el **Summary** del workflow para ver:
   - Recursos creados
   - URL del App Service
   - Detalles del despliegue

2. Verifica en Azure Portal:
   ```bash
   az resource list --resource-group <nombre-grupo-recursos> --output table
   ```

3. Prueba la URL del App Service generada

## Troubleshooting

### Error: "The subscription is not registered to use namespace 'Microsoft.Web'"

Solución:
```bash
az provider register --namespace Microsoft.Web
```

### Error: "Insufficient permissions"

Verifica que el Service Principal tenga el rol `Contributor` en la suscripción:
```bash
az role assignment list --assignee <clientId> --output table
```

### Error: "Resource group not found" durante validación

Esto es normal si el grupo de recursos aún no existe. El workflow lo creará automáticamente.

## Recursos Adicionales

- [Azure Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
- [GitHub Actions for Azure](https://github.com/Azure/actions)
- [Azure CLI Reference](https://learn.microsoft.com/cli/azure/)
