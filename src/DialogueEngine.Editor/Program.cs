using Avalonia;
using DialogueEngine.Editor;

AppBuilder.Configure<App>()
          .UsePlatformDetect()
          .WithInterFont()
          .LogToTrace()
          .StartWithClassicDesktopLifetime(args);
