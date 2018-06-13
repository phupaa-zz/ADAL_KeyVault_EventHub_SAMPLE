using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;

namespace EnterpriseLogAnalytic_EventHubs_KeyValut_Windows
{
    public partial class Main : Form
    {

        //
        // Sample code to demonstrate how to logon to Azure AD with AppID and access to Azure Key Vault 
        // to get secret (SAS TOKEN) to access and send data to Azure Event Hubs. 
        //

        delegate void StringArgReturningVoidDelegate(string text);
        // Please consider if you want to hardcode this ClientID and Secret?        
        public static string ClientID = "REPLACE THIS WITH YOUR CLIENT/APP ID HERE";
        public static string ClientSecret = "REPLACE THIS WITH YOUR CLIENT/APP PASSWORD";
        public static string SecretName= "REPLACE THIS WITH YOUR SECRET NAME IN KEY VAULT";
        public static string AzureVaultURI = "REPLACE THIS WITH YOUR KEY VAULT URI";
        public static EventHubClient eventHubClient;
        public static string EhConnectionString = "";
        public const string EhEntityPath = "REPLACE THIS WITH EVENT HUBS ENTITY NAME";


        //recommend reading: 
        // How to: Make Thread-Safe Calls to Windows Forms Controls
        // https://docs.microsoft.com/en-us/dotnet/framework/winforms/controls/how-to-make-thread-safe-calls-to-windows-forms-controls 

        private void WriteOutput(string text)
        {
            if (this.Output1.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(WriteOutput);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                Output1.Text = Output1.Text + text + "\n---------------\n";
                
                Output1.SelectionStart = Output1.Text.Length;
                Output1.ScrollToCaret();
            }
            
        }
        private async void authUsingADALCallbackAsync() 
        {
            WriteOutput("Authenticating to Key Vault using ADAL callback.");
            WriteOutput(AzureVaultURI);


            // User ClientID and Password to login to AzureAD
            // then create connection to KeyVault 
            KeyVaultClient kvClient = new KeyVaultClient(
                async (string authority, string resource, string scope) =>
                {
                    var authContext = new AuthenticationContext(authority);
                    ClientCredential clientCred = new ClientCredential(ClientID, ClientSecret);
                    AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);
                    if (result == null)
                    {
                        throw new InvalidOperationException("Failed to retrieve access token for Key Vault, perhaps you DO NOT have authorization to access.  ");

                    }
                    WriteOutput("Login succeed! I got the token from ADAL!"+ "\n");
                    //WriteOutput(result.AccessToken);
                    return result.AccessToken;
                }
            );

            // After get KeyVault Connection, get the secret. 
            SecretBundle s = await kvClient.GetSecretAsync(AzureVaultURI, SecretName);
            EhConnectionString = s.Value;            
            WriteOutput(SecretName + " is " + EhConnectionString);
        }
        
        public Main()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WriteOutput("Hello world of ADAL and KV!!");
            authUsingADALCallbackAsync();
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }
        private static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();

        }
        //private async Task SendMessagesToEventHub(int numMessagesToSend)
        private async void SendMessagesToEventHub(int numMessagesToSend)
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EhConnectionString)
            {
                EntityPath = EhEntityPath
            };
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
              
            
            Random rand = new Random();
            for (var i = 0; i < numMessagesToSend; i++)
            {
                try
                {

                    var telemetryDataPoint = new
                    {
                        deviceId = RandomString(3),
                        menuName = RandomString(1),
                        Message = RandomString(2),
                        Severity = rand.Next(1, 4)
                    };
                    var messageString = JsonConvert.SerializeObject(telemetryDataPoint);                    
                    var message = Encoding.UTF8.GetBytes(messageString);
                    WriteOutput($"Sending message #: {i}");
                    await eventHubClient.SendAsync(new EventData(message));
                }
                catch (Exception exception)
                {
                    WriteOutput($"{DateTime.Now} > Exception: {exception.Message}");
                }

                await Task.Delay(10);
            }

            WriteOutput($"{numMessagesToSend} messages sent.");

            await eventHubClient.CloseAsync();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WriteOutput("Hello world of Event Hubs!!");
            SendMessagesToEventHub(Convert.ToInt32(textBox1.Text));


        }
    }
}
