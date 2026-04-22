using Avalonia;
using Sample2;

AppBuilder.Configure<App>()
          .UsePlatformDetect()
          .WithInterFont()
          .LogToTrace()
          .StartWithClassicDesktopLifetime(args);
