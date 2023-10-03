using ReactiveUI;

namespace RxUiPlugin
{
    public class FooModel : ReactiveObject
    {
        private int _value;
        public int Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }
    }
}