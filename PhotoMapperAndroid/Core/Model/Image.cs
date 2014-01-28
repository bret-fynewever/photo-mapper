using System;
using Android.Gms.Maps.Model;

namespace PhotoMapper.Core.Model
{
	public class Image
	{
		public Android.Net.Uri ImageUri { get; set; }
		public string ImagePath { get; set; }
		public LatLng Location { get; set; }
	}
}

