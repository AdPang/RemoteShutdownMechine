using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MyTestMAUI.IService;

namespace MyTestMAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseMauiCommunityToolkit();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

#if ANDROID
            builder.Services.AddSingleton<IToastService, MyApp.Platforms.Android.ToastService>();
#endif
            builder.Services.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}
