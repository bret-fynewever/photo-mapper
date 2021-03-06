using System;
using Android.Content;
using Android.App;

namespace PhotoMapperAndroid.Core.Extension
{
	public static class ContextExtension
	{
		public static void DisplayMessage(this Context context, int title, int message)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			new AlertDialog.Builder(context)
				.SetTitle(title)
				.SetMessage(message)
				.SetPositiveButton(Resource.String.ButtonOkay, (object sender, DialogClickEventArgs e) => { })
				.Show();
		}

		public static void DisplayMessage(this Context context, string title, string message)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			new AlertDialog.Builder(context)
				.SetTitle(title)
				.SetMessage(message)
				.SetPositiveButton(Resource.String.ButtonOkay, (object sender, DialogClickEventArgs e) => { })
				.Show();
		}

		public static void DisplayMessage(this Context context, int title, string message)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			new AlertDialog.Builder(context)
				.SetTitle(title)
				.SetMessage(message)
				.SetPositiveButton(Resource.String.ButtonOkay, (object sender, DialogClickEventArgs e) => { })
				.Show();
		}

		public static void DisplayMessage(this Context context, string title, int message)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			new AlertDialog.Builder(context)
				.SetTitle(title)
				.SetMessage(message)
				.SetPositiveButton(Resource.String.ButtonOkay, (object sender, DialogClickEventArgs e) => { })
				.Show();
		}
			}
}

