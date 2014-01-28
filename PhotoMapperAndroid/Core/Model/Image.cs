using System;
using Android.Gms.Maps.Model;

namespace PhotoMapperAndroid.Core.Model
{
	public class Image
	{
		public Android.Net.Uri ImageUri { get; set; }
		public string ImagePath { get; set; }
		public LatLng Location { get; set; }
	}
}

