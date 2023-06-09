namespace PhotoSharingApplication.Frontend.Client.DuendeAuth; 
public class AntiforgeryHandler : DelegatingHandler {
    public AntiforgeryHandler() { }
    public AntiforgeryHandler(HttpClientHandler innerHandler) : base(innerHandler) { }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
        request.Headers.Add("X-CSRF", "1");
        return base.SendAsync(request, cancellationToken);
    }
}
