using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace AquaPP.Controls
{
    public partial class ToastNotification : UserControl
    {
        // Toast properties
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<ToastNotification, string>(nameof(Title));

        public static readonly StyledProperty<string> MessageProperty =
            AvaloniaProperty.Register<ToastNotification, string>(nameof(Message));

        public static readonly StyledProperty<string> IconProperty =
            AvaloniaProperty.Register<ToastNotification, string>(nameof(Icon), "ℹ️");

        public new static readonly StyledProperty<IBrush> BackgroundProperty =
            AvaloniaProperty.Register<ToastNotification, IBrush>(nameof(Background), Brushes.LightGray);

        public new static readonly StyledProperty<IBrush> BorderBrushProperty =
            AvaloniaProperty.Register<ToastNotification, IBrush>(nameof(BorderBrush), Brushes.Gray);

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Message
        {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public string Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public new IBrush Background
        {
            get => GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public new IBrush BorderBrush
        {
            get => GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public ToastNotification()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}