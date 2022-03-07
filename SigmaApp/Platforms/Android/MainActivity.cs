using Android.App;
using Android.Content.PM;
using Android.OS;


namespace SigmaApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle bundle)
    {
        //ServicePointManager.ServerCertificateValidationCallback =
        //    (message, certificate, chain, sslPolicyErrors) => true;

        base.OnCreate(bundle);
    }
}
