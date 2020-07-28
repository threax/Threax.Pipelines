# Threax.Provision.CheapAzure
An inexpensive setup for azure. This will use a single database and app service plan and pack all applications into it. Your apps must be able to handle being in the same db as other apps for this setup to work. Otherwise add more databases, which will increase the costs.

## Creating Service Principals for Azure DevOps
Use the following powershell commands to create a service principal for azdo.

First setup the new service principal. This should add them as a Contributor.
```
$result = New-AzADServicePrincipal -DisplayName azdo-service-connection -PasswordCredential $credentials -Role Contributor -Scope /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/myResourceGroup
```

Now in Azure Devops create a new Azure Resource Manager service connection with a manual service principal. Use `$result.ApplicationId` as the Service Principal Id and `ConvertFrom-SecureString $result.Secret -AsPlainText` for the Service principal key. Tenant id and sub id come from azure. This should verify.

When registering permissions on the key vaults use `$result.Id` for the guid. This can be set as the AzDoUser in the core config.