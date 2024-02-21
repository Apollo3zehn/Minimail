using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Minimail.Core;
using System.Text.Json;

namespace Minimail.Components.Pages;

public partial class Home
{
    [Inject]
    public IOptions<PathsOptions> PathsOptions { get; set; } = default!;

    private void Save()
    {
        var options = new JsonSerializerOptions() { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(State.Whitelist, options);

        var directoryName = Path.GetDirectoryName(PathsOptions.Value.Whitelist);

        if (directoryName is not null) {
            Directory.CreateDirectory(directoryName);
        }

        File.WriteAllText(PathsOptions.Value.Whitelist, jsonString);
    }
}