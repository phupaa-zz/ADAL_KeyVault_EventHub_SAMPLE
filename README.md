This is a simple Windows Form .NET project to demonstrate:
1. How to login to Azure AD and read secret from Azure Key Vault
2. Use the secret you got from #1 to create connection to Azure Event Hubs and send the message to Azure Event Hubs. 

#Before run the code, you need to:
1. Register app on your Azure AD.
2. Setup Azure KayVault and add secret. 
3. Setup Azure Event Hubs.
4. Open the sourcecode and update the variables below. 

        public static string ClientID = "REPLACE THIS WITH YOUR CLIENT/APP ID HERE";
        public static string ClientSecret = "REPLACE THIS WITH YOUR CLIENT/APP PASSWORD";
        public static string SecretName= "REPLACE THIS WITH YOUR SECRET NAME IN KEY VAULT";
        public static string AzureVaultURI = "REPLACE THIS WITH YOUR KEY VAULT URI";                
        public const string EhEntityPath = "REPLACE THIS WITH EVENT HUBS ENTITY NAME";
5. Done. 
This sample code was built by Visual Studio 2017. 
