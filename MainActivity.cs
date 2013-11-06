using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace PhotoMapper
{
	[Activity(Label = "PhotoMapper", MainLauncher = true)]
	public class MainActivity : Activity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource.
			SetContentView(Resource.Layout.Main);

			// Attach events to controls.
			Button showMapButton = FindViewById<Button>(Resource.Id.ShowMapButton);
			showMapButton.Click += delegate
			{
				StartActivity(typeof(PhotoMap));
			};
		}
	}
}


