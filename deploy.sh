#!/bin/bash

# Script para desplegar la infraestructura en Azure usando Bicep

# Variables
RESOURCE_GROUP="rg-empresaapi-dev"
LOCATION="eastus"
TEMPLATE_FILE="infrastructure.bicep"
PARAMETERS_FILE="infrastructure.parameters.json"

# Colores para output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== Desplegando infraestructura en Azure ===${NC}"

# Verificar si el usuario está autenticado en Azure
echo -e "\n${BLUE}Verificando autenticación en Azure...${NC}"
az account show > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo -e "${RED}No estás autenticado en Azure. Ejecuta 'az login' primero.${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Autenticado en Azure${NC}"

# Crear grupo de recursos si no existe
echo -e "\n${BLUE}Verificando grupo de recursos...${NC}"
az group show --name $RESOURCE_GROUP > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo -e "${BLUE}Creando grupo de recursos: $RESOURCE_GROUP${NC}"
    az group create --name $RESOURCE_GROUP --location $LOCATION
    echo -e "${GREEN}✓ Grupo de recursos creado${NC}"
else
    echo -e "${GREEN}✓ Grupo de recursos ya existe${NC}"
fi

# Validar el template de Bicep
echo -e "\n${BLUE}Validando template de Bicep...${NC}"
az deployment group validate \
    --resource-group $RESOURCE_GROUP \
    --template-file $TEMPLATE_FILE \
    --parameters @$PARAMETERS_FILE

if [ $? -ne 0 ]; then
    echo -e "${RED}✗ La validación del template falló${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Template validado correctamente${NC}"

# Desplegar la infraestructura
echo -e "\n${BLUE}Desplegando infraestructura...${NC}"
az deployment group create \
    --resource-group $RESOURCE_GROUP \
    --template-file $TEMPLATE_FILE \
    --parameters @$PARAMETERS_FILE \
    --name "deployment-$(date +%Y%m%d-%H%M%S)"

if [ $? -eq 0 ]; then
    echo -e "\n${GREEN}✓ Infraestructura desplegada exitosamente${NC}"
    
    # Obtener la URL del App Service
    echo -e "\n${BLUE}Obteniendo información del despliegue...${NC}"
    APP_SERVICE_URL=$(az deployment group show \
        --resource-group $RESOURCE_GROUP \
        --name $(az deployment group list --resource-group $RESOURCE_GROUP --query "[0].name" -o tsv) \
        --query properties.outputs.appServiceUrl.value -o tsv)
    
    echo -e "${GREEN}URL del App Service: $APP_SERVICE_URL${NC}"
else
    echo -e "${RED}✗ El despliegue falló${NC}"
    exit 1
fi

echo -e "\n${BLUE}=== Despliegue completado ===${NC}"
