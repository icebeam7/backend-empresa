@description('Nombre base para los recursos de Azure')
param resourceBaseName string

@description('Ubicación de los recursos de Azure')
param location string = resourceGroup().location

@description('SKU del App Service Plan')
@allowed([
  'F1'
  'B1'
  'B2'
  'S1'
  'S2'
  'P1V2'
  'P2V2'
])
param appServicePlanSku string = 'F1'

@description('Ambiente de despliegue')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environment string = 'dev'

var appServicePlanName = '${resourceBaseName}-plan-${environment}'
var appServiceName = '${resourceBaseName}-app-${environment}'

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSku
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
  tags: {
    environment: environment
    project: 'empresa-api'
  }
}

// App Service
resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: appServiceName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      alwaysOn: appServicePlanSku != 'F1'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment == 'prod' ? 'Production' : 'Development'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
      cors: {
        allowedOrigins: [
          '*'
        ]
        supportCredentials: false
      }
    }
  }
  tags: {
    environment: environment
    project: 'empresa-api'
  }
}

@description('Nombre del App Service Plan creado')
output appServicePlanName string = appServicePlan.name

@description('Nombre del App Service creado')
output appServiceName string = appService.name

@description('URL del App Service')
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'

@description('ID del recurso del App Service')
output appServiceId string = appService.id
