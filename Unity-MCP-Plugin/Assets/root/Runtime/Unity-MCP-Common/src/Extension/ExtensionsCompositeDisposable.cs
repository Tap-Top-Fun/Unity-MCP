using System.Threading;
using R3;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public static class ExtensionsCompositeDisposable
    {
        public static CancellationToken ToCancellationToken(this CompositeDisposable disposables)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            disposables.Add(Disposable.Create(() => cancellationTokenSource.Cancel()));
            return cancellationTokenSource.Token;
        }
    }
}