using Microsoft.JSInterop;

namespace PhotoSharingApplication.Frontend.BlazorComponents;

public class MapJsInterop : IAsyncDisposable {
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;
    private Action<double, double> callback;

    public MapJsInterop(IJSRuntime jsRuntime) {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
           "import", "./_content/PhotoSharingApplication.Frontend.BlazorComponents/map.js").AsTask());
    }

    public async ValueTask ShowMap(double latitude, double longitude, string popupText) {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("showMap", latitude, longitude, popupText);
    }

    public async ValueTask ExtractCoords(byte[]? photoFile, string mimeType, Action<double, double> callback) {
        this.callback = callback;

        string dataUri = photoFile is null ? "" : $"data:{mimeType};base64,{Convert.ToBase64String(photoFile)}";
        var module = await moduleTask.Value;

        DotNetObjectReference<MapJsInterop>? objRef = DotNetObjectReference.Create(this);

        await module.InvokeVoidAsync("extractCoords", objRef, new { src = dataUri });
    }

    [JSInvokable]
    public void GetLatitudeLongitude(double Latitude, double Longitude) => callback(Latitude, Longitude);

    public async ValueTask DisposeAsync() {
        if (moduleTask.IsValueCreated) {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
