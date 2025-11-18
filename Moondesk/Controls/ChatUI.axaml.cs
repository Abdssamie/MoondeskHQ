using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Input;
using AquaPP.Models;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace AquaPP.Controls;

// ReSharper disable once InconsistentNaming
public partial class ChatUI : UserControl // Ensure partial keyword is present
{

    public ChatUI()
    {
        InitializeComponent();
    }

    private ScrollViewer? _chatScroll;


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Messages.CollectionChanged += ValueOnCollectionChanged;
        _chatScroll = e.NameScope.Get<ScrollViewer>("ChatScrollViewer");
    }

    public static readonly StyledProperty<ObservableCollection<ChatMessage>> MessagesProperty =
        AvaloniaProperty.Register<ChatUI, ObservableCollection<ChatMessage>>(nameof(Messages),
            defaultValue: new ObservableCollection<ChatMessage>());

    public ObservableCollection<ChatMessage> Messages
    {
        get => GetValue(MessagesProperty);
        set
        {
            {
                SetValue(MessagesProperty, value);
                value.CollectionChanged += ValueOnCollectionChanged;
            }
        }
    }


    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<ChatUI, string>(nameof(Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<IImage?> UserImageSourceProperty =
        AvaloniaProperty.Register<ChatUI, IImage?>(nameof(UserImageSource));

    public IImage? UserImageSource
    {
        get => GetValue(UserImageSourceProperty);
        set => SetValue(UserImageSourceProperty, value);
    }

    public static readonly StyledProperty<IImage?> FriendImageSourceProperty =
        AvaloniaProperty.Register<ChatUI, IImage?>(nameof(FriendImageSource));

    public IImage? FriendImageSource
    {
        get => GetValue(FriendImageSourceProperty);
        set => SetValue(FriendImageSourceProperty, value);
    }


    public static readonly StyledProperty<ICommand?> SendCommandProperty =
        AvaloniaProperty.Register<ChatUI, ICommand?>(nameof(SendCommand));

    public ICommand? SendCommand
    {
        get => GetValue(SendCommandProperty);
        set => SetValue(SendCommandProperty, value);
    }

    private CancellationTokenSource? _animationToken;

    private void ValueOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _animationToken?.Cancel();
        _animationToken = new CancellationTokenSource();
        try
        {
            new Animation
            {
                Duration = TimeSpan.FromMilliseconds(800),
                FillMode = FillMode.Forward,
                Easing = new CubicEaseInOut(),
                IterationCount = new IterationCount(1),
                PlaybackDirection = PlaybackDirection.Normal,
                Children =
                {
                    new KeyFrame()
                    {
                        Setters = { new Setter { Property = ScrollViewer.OffsetProperty, Value = _chatScroll!.Offset } },
                        KeyTime = TimeSpan.FromSeconds(0)
                    },
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = ScrollViewer.OffsetProperty,
                                Value = new Vector(_chatScroll!.Offset.X, _chatScroll!.Offset.Y + 500)
                            }
                        },
                        KeyTime = TimeSpan.FromMilliseconds(800)
                    }
                }
            }.RunAsync(_chatScroll, _animationToken.Token);

        }
        catch (NullReferenceException ex)
        {
            throw new Exception($"Error trying to run ValueOnCollectionChanged when message collection changed: {ex.Message}");
        }
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
