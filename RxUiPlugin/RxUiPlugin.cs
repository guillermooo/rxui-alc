using PluginSdk;
using ReactiveUI;
using System.Reactive.Linq;

namespace RxUiPlugin
{
    public class RxUiPlugin : IPlugin
    {
        private readonly IDisposable _disposable;
        private readonly FooModel _model;

        public RxUiPlugin()
        {
            _model = new FooModel();

            _disposable = _model
                .WhenAnyValue(m => m.Value)
                .Select(x => x * 100)
                .Subscribe();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        public string GetMsg()
        {
            return $"This is a message from {nameof(RxUiPlugin)}.";
        }
    }
}