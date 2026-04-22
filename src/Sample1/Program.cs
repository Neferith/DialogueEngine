using Avalonia;
using Sample1;

AppBuilder.Configure<App>()
          .UsePlatformDetect()
          .WithInterFont()
          .LogToTrace()
          .StartWithClassicDesktopLifetime(args);
