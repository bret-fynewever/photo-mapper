using System;
using Android.App;
using Android.Gms.Maps.Model;
using Android.Database;
using Android.Media;
using System.IO;

namespace PhotoMapper.Core.Service
{
	public class ImageService : IImageService
	{
		private Activity _activity;

		public ImageService(Activity activity)
		{
			_activity = activity;
		}

		public string GetImagePath(Android.Net.Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException("uri");

			string path = null;

			// The projection contains the columns we want to return in our query.
			string[] projection = new[] { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
			using (ICursor cursor = _activity.ManagedQuery(uri, projection, null, null, null))
			{
				if (cursor != null)
				{
					int columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
					cursor.MoveToFirst();
					path = cursor.GetString(columnIndex);
				}
			}

			return path;
		}

		public LatLng GetImageLocation(string imagePath)
		{
			if (string.IsNullOrWhiteSpace(imagePath))
				throw new ArgumentNullException("imagePath");

			LatLng location = null;

			try
			{
				var exif = new ExifInterface(imagePath);
				float[] latLong = new float[2];
				if (exif.GetLatLong(latLong))
					location = new LatLng(latLong[0], latLong[1]);
			}
			catch (IOException)
			{
				// TODO:  handle IO error.
			}

			return location;
		}
	}
}

