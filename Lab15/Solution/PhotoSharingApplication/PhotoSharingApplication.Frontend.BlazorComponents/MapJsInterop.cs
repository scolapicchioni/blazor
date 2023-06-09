using Microsoft.JSInterop;

namespace PhotoSharingApplication.Frontend.BlazorComponents;
public class MapJsInterop : IAsyncDisposable {
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public MapJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                      "import", "./_content/PhotoSharingApplication.Frontend.BlazorComponents/Components/MapComponent.razor.js").AsTask());
    }
    public async ValueTask ShowMap(double latitude, double longitude, string description) {
        IJSObjectReference module = await moduleTask.Value;
        await module.InvokeVoidAsync("showMap", latitude, longitude, description);
    }

    public async ValueTask DisposeAsync() {
        if(moduleTask.IsValueCreated) {
            IJSObjectReference module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
