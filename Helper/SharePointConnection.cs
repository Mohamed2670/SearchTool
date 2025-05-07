using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Identity.Client;
using SearchTool_ServerSide.Dtos.ScritpsDto;

namespace YourProjectNamespace.Services
{


    public static class SharePointConnection
    {
        // Configuration
        private static readonly string tenantId = "a4630faa-4577-4d15-9542-a42e6f9dd2ac";
        private static readonly string clientId = "02991d84-7900-4257-94a9-e8e112fc0752";
        // Update the certificate file path and password accordingly
        private static readonly string certificatePath = @"path\to\your\certificate.pfx";
        private static readonly string certificatePassword = "your_certificate_password";
        private static readonly string authority = $"https://login.microsoftonline.com/{tenantId}";
        private static readonly string[] scopes = new string[] { "https://calidermatologyinstitute.sharepoint.com/.default" };

        // SharePoint site URL (update as needed)
        private static readonly string siteUrl = "https://calidermatologyinstitute.sharepoint.com/sites/CDIOperations";

        /// <summary>
        /// Authenticates with Azure AD using the certificate and returns an HttpClient configured with the token.
        /// </summary>
        public static async Task<HttpClient> GetAuthenticatedHttpClientAsync()
        {
            var certificate = new X509Certificate2(certificatePath, certificatePassword, X509KeyStorageFlags.MachineKeySet);

            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(authority)
                .WithCertificate(certificate)
                .Build();

            var authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            string token = authResult.AccessToken;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
            return client;
        }

        /// <summary>
        /// Checks if the specified SharePoint list exists; if it does not, creates it.
        /// </summary>
        public static async Task CheckOrCreateListAsync(HttpClient client, string listName)
        {
            string listUrl = $"{siteUrl}/_api/web/lists/getbytitle('{listName}')";
            HttpResponseMessage response = await client.GetAsync(listUrl);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ List '{listName}' already exists.");
            }
            else
            {
                Console.WriteLine($"ℹ️ List '{listName}' not found. Creating list...");
                string createUrl = $"{siteUrl}/_api/web/lists";
                var payload = new
                {
                    __metadata = new { type = "SP.List" },
                    Title = listName,
                    BaseTemplate = 100, // Generic custom list template
                    Description = "List created programmatically."
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json;odata=verbose");
                HttpResponseMessage createResponse = await client.PostAsync(createUrl, content);
                createResponse.EnsureSuccessStatusCode();
                Console.WriteLine($"✅ List '{listName}' created successfully.");
            }
        }

        /// <summary>
        /// Retrieves all list items from the specified SharePoint list in batches.
        /// </summary>
        public static async Task<List<JsonElement>> RetrieveListItemsAsync(HttpClient client, string listName)
        {
            List<JsonElement> items = new List<JsonElement>();
            string listItemsUrl = $"{siteUrl}/_api/web/lists/getbytitle('{listName}')/items?$top=5000";
            string nextUrl = listItemsUrl;

            while (!string.IsNullOrEmpty(nextUrl))
            {
                HttpResponseMessage response = await client.GetAsync(nextUrl);
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                if (doc.RootElement.TryGetProperty("value", out JsonElement valueElement))
                {
                    foreach (JsonElement item in valueElement.EnumerateArray())
                    {
                        items.Add(item);
                    }
                    nextUrl = doc.RootElement.TryGetProperty("@odata.nextLink", out JsonElement nextLink)
                        ? nextLink.GetString()
                        : null;
                }
                else if (doc.RootElement.TryGetProperty("d", out JsonElement dElement) &&
                         dElement.TryGetProperty("results", out JsonElement results))
                {
                    foreach (JsonElement item in results.EnumerateArray())
                    {
                        items.Add(item);
                    }
                    nextUrl = dElement.TryGetProperty("__next", out JsonElement nextElement)
                        ? nextElement.GetString()
                        : null;
                }
                else
                {
                    Console.WriteLine("❌ Unexpected response structure for list items.");
                    break;
                }
                await Task.Delay(100);
            }

            Console.WriteLine($"Retrieved {items.Count} items from SharePoint list '{listName}'.");
            return items;
        }

        /// <summary>
        /// Maps a SharePoint list item (JsonElement) to the internal DTO.
        /// </summary>
        public static ScriptAddDto MapToScriptAddDto(JsonElement item)
        {
            string GetString(JsonElement elem, string propertyName) =>
                elem.TryGetProperty(propertyName, out JsonElement prop) ? prop.ToString() : "";

            double GetDouble(JsonElement elem, string propertyName)
            {
                if (elem.TryGetProperty(propertyName, out JsonElement prop) &&
                    double.TryParse(prop.ToString(), out double result))
                    return result;
                return 0;
            }

            return new ScriptAddDto
            {
                Date = GetString(item, "field_0"),
                Script = GetString(item, "Title"),
                RxGroup = GetString(item, "field_17"),
                Bin = GetString(item, "field_18"),
                PCN = GetString(item, "field_19"),
                RxNumber = GetString(item, "field_2"),
                User = GetString(item, "field_3"),
                DrugName = GetString(item, "field_5"),
                Insurance = GetString(item, "field_6"),
                PF = GetString(item, "field_8"),
                Prescriber = GetString(item, "field_9"),
                Quantity = GetString(item, "field_10"),
                AcquisitionCost = (decimal)GetDouble(item, "field_11"),
                Discount = (decimal)GetDouble(item, "field_12"),
                InsurancePayment = (decimal)GetDouble(item, "field_13"),
                PatientPayment = (decimal)GetDouble(item, "field_14"),
                Branch = GetString(item, "field_4"),
                NDCCode = GetString(item, "field_15")
            };
        }
    }
}
