using Microsoft.JSInterop;

namespace PhotoSharingApplication.Frontend.BlazorComponents; 
public class CoordinatesJsInterop : IAsyncDisposable {
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public CoordinatesJsInterop(IJSRuntime jsRuntime) {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                      "import", "./_content/PhotoSharingApplication.Frontend.BlazorComponents/Components/PhotoEditComponent.razor.js").AsTask());
    }

    private Action<double, double> callback;
    public async ValueTask ExtractCoords(byte[]? photoFile, string mimeType, Action<double, double> callback) {
        this.callback = callback;
        string dataUri = photoFile is null ? "" : $"data:{mimeType};base64,{Convert.ToBase64String(photoFile)}";
        var module = await moduleTask.Value;

        DotNetObjectReference<CoordinatesJsInterop>? objRef = DotNetObjectReference.Create(this);
        await module.InvokeVoidAsync("extractCoords", objRef, new { src = dataUri });
    }

    [JSInvokable]
    public void GetLatitudeLongitude(double Latitude, double Longitude) => callback(Latitude, Longitude);

    public async ValueTask DisposeAsync() {
        if (moduleTask.IsValueCreated) {
            IJSObjectReference module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
