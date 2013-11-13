using System;
using Android.Gms.Maps.Model;

namespace PhotoMapper.Core.Service
{
	public interface IImageService
	{
		string GetImagePath(Android.Net.Uri uri);
		LatLng GetImageLocation(string imagePath);
		void SetImageLocation(string imagePath, LatLng location);
	}
}

