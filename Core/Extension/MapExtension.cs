using System;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;

namespace PhotoMapper.Core.Extension
{
	public static class MapExtension
	{
		public static void ZoomToLocation(this GoogleMap map, LatLng location, float zoom)
		{
			if (map == null)
				throw new ArgumentNullException("map");
			if (location == null)
				throw new ArgumentNullException("location");

			CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
			builder.Target(location);
			builder.Zoom(zoom);

			CameraPosition position = builder.Build();
			CameraUpdate update = CameraUpdateFactory.NewCameraPosition(position);

			map.AnimateCamera(update);
		}

		public static void SetMarker(this GoogleMap map, LatLng location, string title, string snippet = "", bool draggable = false)
		{
			if (map == null)
				throw new ArgumentNullException("map");
			if (location == null)
				throw new ArgumentNullException("location");

			MarkerOptions markerOptions = new MarkerOptions();
			markerOptions.SetPosition(location);
			markerOptions.SetTitle(title);
			markerOptions.SetSnippet(snippet);
			markerOptions.Draggable(draggable);
			map.AddMarker(markerOptions);
		}
	}
}

