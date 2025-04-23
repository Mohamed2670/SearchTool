using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SearchTool_ServerSide.Dtos.ScritpsDto;
using YourProjectNamespace.Services;

namespace SearchTool_ServerSide.Helper
{
    public static class SharePoint
    {
        /// <summary>
        /// Retrieves list items from SharePoint and returns a collection of DTOs.
        /// </summary>
        public static async Task<ICollection<ScriptAddDto>> GetScriptsFromSharePointAsync(string listName)
        {
            HttpClient client = await SharePointConnection.GetAuthenticatedHttpClientAsync();

            // Optionally check or create the list if it does not exist
            await SharePointConnection.CheckOrCreateListAsync(client, listName);

            // Retrieve list items from SharePoint
            List<JsonElement> spItems = await SharePointConnection.RetrieveListItemsAsync(client, listName);

            // Map SharePoint items to the DTO structure
            List<ScriptAddDto> dtos = new List<ScriptAddDto>();
            foreach (var item in spItems)
            {
                dtos.Add(SharePointConnection.MapToScriptAddDto(item));
            }
            return dtos;
        }
    }
}
