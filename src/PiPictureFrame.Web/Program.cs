//
// PiPictureFrame - Digital Picture Frame built for the Raspberry Pi.
// Copyright (C) 2022 Seth Hendrick
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using Mono.Options;
using PiPictureFrame.Api;
using Prometheus;

bool showHelp = false;
bool showVersion = false;
bool showLicense = false;
bool showCredits = false;
bool showReadme = false;
string pictureDirectory = "";
string rpiBacklightPowerFile = "";
string rpiBacklightBrightnessFile = "";

var options = new OptionSet
{
    {
        "h|help",
        "Shows thie mesage and exits.",
        v => showHelp = ( v is not null )
    },
    {
        "version",
        "Shows the version and exits.",
        v => showVersion = ( v is not null )
    },
    {
        "print_license",
        "Prints the software license and exits.",
        v => showLicense = ( v is not null )
    },
    {
        "print_readme",
        "Prints Readme and exits.",
        v => showReadme = ( v is not null )
    },
    {
        "print_credits",
        "Prints the third-party notices and credits.",
        v => showCredits = ( v is not null )
    },
    {
        "picture_directory=",
        $"Sets the directory of where to find pictures to render.  If not specfied, defaulted to: '{PiPictureFrameApiConfig.DefaultPhotoDirectory.FullName}'",
        v => pictureDirectory = v
    },
    {
        "rpi_backlight_brightness_file=",
        $"Overrides the file that contains the brightness setting of the Raspberry Pi backlight touch screen.  " +
            $"Ignored if not using that screen.  Defaulted to {PiPictureFrameApiConfig.DefaultRpiBacklightBrightnessFile.FullName}",
        v => rpiBacklightBrightnessFile = v
    },
    {
        "rpi_backlight_power_file=",
        $"Overrides the file that contains the power setting of the Raspberry Pi backlight touch screen.  " +
            $"Ignored if not using that screen.  Defaulted to {PiPictureFrameApiConfig.DefaultRpiBacklightPowerFile.FullName}",
        v => rpiBacklightPowerFile = v
    }
};

try
{
    options.Parse( args );

    if( showHelp )
    {
        PrintHelp();
        return 0;
    }
    else if( showVersion )
    {
        PrintVersion();
        return 0;
    }
    else if( showLicense )
    {
        PrintLicense();
        return 0;
    }
    else if( showReadme )
    {
        PrintReadme();
        return 0;
    }
    else if( showCredits )
    {
        PrintCredits();
        return 0;
    }

    var builder = WebApplication.CreateBuilder( args );

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    ILogger apiLogger;
    using( var logFactory = LoggerFactory.Create( logBuilder => logBuilder.AddSimpleConsole() ) )
    {
        apiLogger = logFactory.CreateLogger( nameof( PiPictureFrameApi ) );
    }

    var apiConfig = new PiPictureFrameApiConfig();
    if( string.IsNullOrEmpty( pictureDirectory ) == false )
    {
        apiConfig = apiConfig with
        {
            PictureDirectory = new DirectoryInfo( pictureDirectory )
        };
    }

    if( string.IsNullOrEmpty( rpiBacklightPowerFile ) == false )
    {
        apiConfig = apiConfig with
        {
            RpiBacklightPowerFile = new FileInfo( rpiBacklightPowerFile )
        };
    }

    if( string.IsNullOrEmpty( rpiBacklightBrightnessFile ) == false )
    {
        apiConfig = apiConfig with
        {
            RpiBacklightBrightnessFile = new FileInfo( rpiBacklightBrightnessFile )
        };
    }

    using( var api = new PiPictureFrameApi( apiConfig, apiLogger ) )
    {
        builder.Services.AddSingleton<IPiPictureFrameApi>( api );

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if( !app.Environment.IsDevelopment() )
        {
            app.UseExceptionHandler( "/Home/Error" );
        }
        app.UseStaticFiles();

        app.UseRouting();
        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapMetrics(
                    "/Metrics"
                );
            }
        );

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}" );

        async void System_OnExitRequest()
        {
            // Wait 5 seconds for the message to be sent back to the user.
            await Task.Delay( TimeSpan.FromSeconds( 5 ) );
            await app.StopAsync();
        }

        try
        {
            api.System.OnExitRequest += System_OnExitRequest;
            api.Init();

            app.Run();
        }
        finally
        {
            api.System.OnExitRequest -= System_OnExitRequest;
        }
    }

    return 0;
}
catch( OptionException e )
{
    Console.WriteLine( e.Message );
    return 1;
}
catch( Exception e )
{
    Console.WriteLine( "FATAL: Unhandled Exception:" );
    Console.WriteLine( e.Message );
    return -1;
}

void PrintHelp()
{
    if( options is not null )
    {
        options.WriteOptionDescriptions( Console.Out );
    }
}

void PrintVersion()
{
    Console.WriteLine(
        typeof( Program ).Assembly.GetName().Version?.ToString( 3 ) ?? "Unknown Version"
    );
}

void PrintLicense()
{
    Console.WriteLine(
        PiPictureFrameApi.Resources.GetLicense()
    );
}

void PrintReadme()
{
    Console.WriteLine(
        PiPictureFrameApi.Resources.GetReadme()
    );
}

void PrintCredits()
{
    Console.WriteLine(
        PiPictureFrameApi.Resources.GetCredits()
    );
}
