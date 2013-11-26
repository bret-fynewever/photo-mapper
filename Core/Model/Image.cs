using System;
using Android.Gms.Maps.Model;

namespace PhotoMapper.Core.Model
{
	public class Image
	{
		public string ImagePath { get; set; }
		public LatLng Location { get; set; }
	}
}

