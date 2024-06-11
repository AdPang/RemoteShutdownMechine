using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Widget;
using MyTestMAUI.IService;
using Application = Android.App.Application;

[assembly: Dependency(typeof(MyApp.Platforms.Android.ToastService))]
namespace MyApp.Platforms.Android
{
    public class ToastService : IToastService
    {
        public void ShowToast(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
        }
    }
}