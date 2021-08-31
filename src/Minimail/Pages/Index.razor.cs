using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Minimail.Core;
using System.IO;
using System.Text.Json;

namespace Minimail.Pages
{
    public partial class Index
    {
        [Inject]
        public IOptions<PathsOptions> PathsOptions { get; set; }

        private void Save()
        {
            var options = new JsonSerializerOptions() { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(Program._whitelist, options);

            File.WriteAllText(this.PathsOptions.Value.Whitelist, jsonString);
        }
    }
}
