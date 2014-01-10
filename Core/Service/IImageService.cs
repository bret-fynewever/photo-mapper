using System;
using Android.App;
using Android.Gms.Maps.Model;
using Android.Graphics;

namespace PhotoMapper.Core.Service
{
	public interface IImageService
	{
		string GetImagePath(Android.Net.Uri uri, Activity activity);
		LatLng GetImageLocation(string imagePath);
		bool SetImageLocation(string imagePath, LatLng location);
		Bitmap GetThumbnail(string imagePath, int thumbnailSize);
	}
}

