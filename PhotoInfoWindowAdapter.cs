using System;
using System.Collections.Generic;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.App;
using PhotoMapper.Core.Model;
using Android.Graphics;
using Java.IO;

namespace PhotoMapper
{
	public class PhotoInfoWindowAdapter : Java.Lang.Object, GoogleMap.IInfoWindowAdapter
	{
		private const int ThumbnailSize = 200;

		private Activity _activity;
		private Dictionary<string, Image> _imageMarkers;

		public PhotoInfoWindowAdapter(Activity activity, Dictionary<string, Image> imageMarkers)
		{
			_activity = activity;
			_imageMarkers = imageMarkers;
		}

		#region IInfoWindowAdapter implementation

		public View GetInfoContents(Marker p0)
		{
			View view = _activity.LayoutInflater.Inflate(Resource.Layout.PhotoInfoWindow, null);

			if (_imageMarkers.ContainsKey(p0.Id))
			{
				var image = _imageMarkers[p0.Id];
				if (image != null)
				{
					Bitmap thumbnail = GetThumbnail(image.ImagePath);
					if (thumbnail != null)
					{
						ImageView imageView = (ImageView)view.FindViewById(Resource.Id.MappedImage);
						imageView.SetImageBitmap(thumbnail);

						TextView textView = (TextView)view.FindViewById(Resource.Id.MappedImageName);
						textView.Text = System.IO.Path.GetFileName(image.ImagePath);
					}
				}
			}

			return view;
		}

		public View GetInfoWindow(Marker p0)
		{
			return null;
		}

		private Bitmap GetThumbnail(string imagePath)
		{
			BitmapFactory.Options bounds = new BitmapFactory.Options();
			bounds.InJustDecodeBounds = true;

			BitmapFactory.DecodeFile(imagePath, bounds);

			if ((bounds.OutWidth == -1) || (bounds.OutHeight == -1))
				return null;

			int originalSize = (bounds.OutHeight > bounds.OutWidth) ? bounds.OutHeight : bounds.OutWidth;

			BitmapFactory.Options options = new BitmapFactory.Options();
			options.InSampleSize = originalSize / ThumbnailSize;

			return BitmapFactory.DecodeFile(imagePath, options);
		}

		#endregion
	}
}

