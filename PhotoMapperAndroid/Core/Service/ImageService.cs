using System;
using System.IO;
using Android.App;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Database;
using Android.Media;
using Android.Util;
using PhotoMapperAndroid.Core.Extension;

namespace PhotoMapperAndroid.Core.Service
{
	public class ImageService : IImageService
	{
		private const string _logTag = "PhotoMapper.Core.Service.ImageService";

		public string GetImagePath(Android.Net.Uri uri, Activity activity)
		{
			if (uri == null)
				throw new ArgumentNullException("uri");

			string path = null;

			// The projection contains the columns we want to return in our query.
			string[] projection = new[] { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
			using (ICursor cursor = activity.ManagedQuery(uri, projection, null, null, null))
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
			catch (IOException exc)
			{
				Log.Error(_logTag, exc.Message);
			}

			return location;
		}

		public bool SetImageLocation(string imagePath, LatLng location)
		{
			if (string.IsNullOrWhiteSpace(imagePath))
				throw new ArgumentNullException("imagePath");
			if (location == null)
				throw new ArgumentNullException("location");

			bool success = true;

			try
			{
				var exif = new ExifInterface(imagePath);

				exif.SetAttribute(ExifInterface.TagGpsLatitude, location.Latitude.ToDMS());
				exif.SetAttribute(ExifInterface.TagGpsLatitudeRef, location.Latitude.ToLatitudeReference());
				exif.SetAttribute(ExifInterface.TagGpsLongitude, location.Longitude.ToDMS());
				exif.SetAttribute(ExifInterface.TagGpsLongitudeRef, location.Longitude.ToLongitudeReference());

				exif.SaveAttributes();
			}
			catch (IOException exc)
			{
				Log.Error(_logTag, exc.Message);
				success = false;
			}

			return success;
		}

		public Bitmap GetThumbnail(string imagePath, int thumbnailSize)
		{
			Bitmap thumbnail = null;

			try
			{
				BitmapFactory.Options bounds = new BitmapFactory.Options();
				bounds.InJustDecodeBounds = true;

				BitmapFactory.DecodeFile(imagePath, bounds);

				if ((bounds.OutWidth == -1) || (bounds.OutHeight == -1))
					return null;

				int maxOriginalDimension = (bounds.OutHeight > bounds.OutWidth) ? bounds.OutHeight : bounds.OutWidth;

				BitmapFactory.Options options = new BitmapFactory.Options();
				options.InSampleSize = maxOriginalDimension / thumbnailSize;

				thumbnail = BitmapFactory.DecodeFile(imagePath, options);
			}
			catch (Exception exc)
			{
				Log.Error(_logTag, exc.Message);
			}

			return thumbnail;
		}
	}
}

