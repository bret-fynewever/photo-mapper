using System;
using System.Collections.Generic;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.App;
using PhotoMapper.Core.Model;
using PhotoMapper.Core.Service;

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

		#region Services

		private IImageService _imageService = null;
		public IImageService ImageService
		{
			get { return _imageService ?? (_imageService = new ImageService()); }
			set { _imageService = value; }
		}

		#endregion

		#region IInfoWindowAdapter implementation

		public View GetInfoContents(Marker p0)
		{
			View view = _activity.LayoutInflater.Inflate(Resource.Layout.PhotoInfoWindow, null);

			if (_imageMarkers.ContainsKey(p0.Id))
			{
				var image = _imageMarkers[p0.Id];
				if (image != null)
				{
					Bitmap thumbnail = ImageService.GetThumbnail(image.ImagePath, ThumbnailSize);
					if (thumbnail != null)
					{
						ImageView imageView = (ImageView)view.FindViewById(Resource.Id.ImageViewMappedImage);
						imageView.SetImageBitmap(thumbnail);

						TextView textView = (TextView)view.FindViewById(Resource.Id.TextViewMappedImageName);
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

		#endregion
	}
}

