using Xunit;
using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace Microsoft.AspNetCore.SignalR.Tests
{
    public class DisposerFacts
    {
        [Fact]
        public void DisposeDisposes()
        {
            bool disposed = false;
            var disposable = new DisposableAction(() => { disposed = true; });

            var disposer = new Disposer();
            disposer.Set(disposable);

            Assert.False(disposed);

            disposer.Dispose();

            Assert.True(disposed);
        }

        [Fact]
        public void SetDisposesIfShouldDispose()
        {
            bool disposed = false;
            var disposable = new DisposableAction(() => { disposed = true; });
            var disposer = new Disposer();

            Assert.False(disposed);
            disposer.Dispose();
            
            disposer.Set(disposable);
            Assert.True(disposed);
        }

        [Fact]
        public void MultipleDisposes()
        {
            bool disposed = false;
            var disposable = new DisposableAction(() => { disposed = true; });
            var disposer = new Disposer();

            Assert.False(disposed);
            disposer.Dispose();
            disposer.Dispose();
            disposer.Dispose();
            disposer.Dispose();

            disposer.Set(disposable);
            Assert.True(disposed);
        }
    }
}
