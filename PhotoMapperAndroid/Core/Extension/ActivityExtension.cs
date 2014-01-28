using System;
using Android.App;
using Android.Content;

namespace PhotoMapper.Core.Extension
{
	public static class ActivityExtension
	{
		public static void DisplayMessage(this Activity activity, int title, int message)
		{
			if (activity == null)
				throw new ArgumentNullException("activity");

			new AlertDialog.Builder(activity)
				.SetTitle(title)
				.SetMessage(message)
				.SetPositiveButton(Resource.String.ButtonOkay, (object sender, DialogClickEventArgs e) => { })
				.Show();
		}

		public static void DisplayMessage(this Activity activity, string title, string message)
		{
			if (activity == null)
				throw new ArgumentNullException("activity");

			new AlertDialog.Builder(activity)
				.SetTitle(title)
				.SetMessage(message)
				.SetPositiveButton(Resource.String.ButtonOkay, (object sender, DialogClickEventArgs e) => { })
				.Show();
		}

		public static void DisplayMessage(this Activity activity, int title, string message)
		{
			if (activity == null)
				throw new ArgumentNullException("activity");

			new AlertDialog.Builder(activity)
				.SetTitle(title)
				.SetMessage(message)
				.SetPositiveButton(Resource.String.ButtonOkay, (object sender, DialogClickEventArgs e) => { })
				.Show();
		}

		public static void DisplayMessage(this Activity activity, string title, int message)
		{
			if (activity == null)
				throw new ArgumentNullException("activity");

			new AlertDialog.Builder(activity)
				.SetTitle(title)
				.SetMessage(message)
				.SetPositiveButton(Resource.String.ButtonOkay, (object sender, DialogClickEventArgs e) => { })
				.Show();
		}
	}
}

