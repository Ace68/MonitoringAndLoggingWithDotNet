# Creazione Risorse dalla CLI
Prima di poter pubblicare la nostra applicazione, dobbiamo preparare l'ambiente.  
MS Azure raggruppa tutte le risorse sotto un [Resource Group](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/manage-resource-groups-portal), perciò ne dobbiamo creare uno.  
Prima di tutto, eseguiamo il login, e aggiorniamo la CLI all'ultima versione  

```
az login
az upgrade
```

Installiamo, o aggiorniamo, l'estensione per gestire le Azure Container Apps  
```
az extension add --name containerapp --upgrade
```
A questo punto abbiamo bisogno di registrare due namespace, il primo dovuto alla migrazione di Azure Container Apps da Microsoft.Web a Microsoft.App. [Doc](https://github.com/microsoft/azure-container-apps/issues/109)
```
az provider register --namespace Microsoft.App
```
Il secondo per attivare il monitoring della nostra applicazione
```
az provider register --namespace Microsoft.OperationalInsights
```
Abbiamo così terminato la configurazione della CLI di Azure, e possiamo cominciare a settare la nostra applicazione.  

## Environment
Definiamo le variabili d'ambiente  
```
$LOCATION="italynorth"
$ENVIRONMENT="brewup-env-containerapps"
$REGISTRY="brewupregistry"
$API_NAME="brewupapi"
$RESOURCE_GROUP="brewup-resourcegroup"
```

## Resource Group
```
az group create --location <LOCATION> --resource-group <RESOURCE_GROUP>
```

## ENVIRONMENT
az containerapp env create \
  --name $ENVIRONMENT \
  --resource-group $RESOURCE_GROUP \
  --location "$LOCATION"

az containerapp env create --name $ENVIRONMENT --resource-group $RESOURCE_GROUP --location "$LOCATION"

## Build and Push Image
docker build -t brewupapi .

### Container Registry
```
az acr create --name <REGISTRY> --resource-group <RESOURCE_GROUP> --sku Standard
```
docker tag brewupapi brewupregistry.azurecr.io/brewupapi
az acr login --name <REGISTRY>
docker push brewupregistry.azurecr.io/brewupapi

### Container Apps
Siamo pronti a pubblicare l'app

### Deploy image to a container app
```
az containerapp up  
  --name <API_NAME>  
  --resource-group <RESOURCE_GROUP>  
  --environment <ENVIRONMENT>  
  --image brewupregistry.azurecr.io/brewupapi  
  --target-port 8080  
  --ingress external  
  ```
az containerapp create --name brewupapi --resource-group $RESOURCE_GROUP  --environment $ENVIRONMENT --image brewupregistry.azurecr.io/brewupapi --location $LOCATION  --target-port 8080  --ingress external  
Di tutti i parametri necessari al comando, fatte attenzione alla porta esposta, .NET di default espone la 8080, ed al parametro "ingress external" che espone la nostra API al mondo esterno.  
