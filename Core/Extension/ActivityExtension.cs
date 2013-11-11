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
				.SetPositiveButton(Resource.String.Okay, (object sender, DialogClickEventArgs e) => { })
				.Show();
		}

	}
}

