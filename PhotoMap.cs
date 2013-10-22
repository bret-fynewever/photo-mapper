using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using PhotoMapper.Core;
using Android.Locations;

namespace PhotoMapper
{
	[Activity(Label = "PhotoMap")]			
	public class PhotoMap : Activity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.PhotoMap);

			GoogleMap map = GetMapFromFragment(Resource.Id.PhotoMapFragment);
			if (map != null)
			{
				map.MapType = GoogleMap.MapTypeNormal;
				map.UiSettings.CompassEnabled = true;
				map.UiSettings.RotateGesturesEnabled = true;
				map.UiSettings.ScrollGesturesEnabled = true;
				map.UiSettings.TiltGesturesEnabled = true;
				map.UiSettings.ZoomControlsEnabled = true;
				map.UiSettings.ZoomGesturesEnabled = true;
			
				// Attach events to controls.
				Button goToMinneapolisButton = FindViewById<Button>(Resource.Id.GoToMinneapolisButton);
				goToMinneapolisButton.Click += (object sender, EventArgs e) =>
				{
					var location = new LatLng(44.9833, -93.2667);   // Minneapolis latitude / longitude
					ZoomToLocation(map, location, 15);
					SetMarker(map, location, "Downtown Minneapolis");
				};

				Button goToAddressButton = FindViewById<Button>(Resource.Id.GoToAddressButton);
				goToAddressButton.Click += (object sender, EventArgs e) =>
				{
					GoToAddress();
				};
			}
		}

		private GoogleMap GetMapFromFragment(int mapFragmentId)
		{
			MapFragment mapFragment = (MapFragment)FragmentManager.FindFragmentById(mapFragmentId);
			return mapFragment.Map;
		}

		private void ZoomToLocation(GoogleMap map, LatLng location, float zoom)
		{
			if (map != null)
			{
				CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
				builder.Target(location);
				builder.Zoom(zoom);

				CameraPosition position = builder.Build();
				CameraUpdate update = CameraUpdateFactory.NewCameraPosition(position);

				map.AnimateCamera(update);
			}
		}

		private void SetMarker(GoogleMap map, LatLng location, string title)
		{
			if (map != null)
			{
				MarkerOptions markerOptions = new MarkerOptions();
				markerOptions.SetPosition(location);
				markerOptions.SetTitle(title);
				map.AddMarker(markerOptions);
			}
		}

		private void GoToAddress()
		{
			var inputControl = new EditText(this);

			new AlertDialog.Builder(this)
				.SetTitle(Resource.String.AddressSearchTitle)
				.SetMessage(Resource.String.AddressSearchMessage)
				.SetView(inputControl)
				.SetPositiveButton(Resource.String.Okay, (object sender, DialogClickEventArgs e) =>
				{
					ZoomToAddress(GetMapFromFragment(Resource.Id.PhotoMapFragment), inputControl.Text, 15);
				})
				.SetNegativeButton(Resource.String.Cancel, (object sender, DialogClickEventArgs e) =>
				{
				})
				.Show();
		}

		private async void ZoomToAddress(GoogleMap map, string searchAddress, float zoom)
		{
			if (map != null && !string.IsNullOrWhiteSpace(searchAddress))
			{
				var locationService = new LocationService(this);
				Address address = await locationService.GeoSearchAsync(searchAddress);
				if (address != null)
				{
					var location = new LatLng(address.Latitude, address.Longitude);
					ZoomToLocation(map, location, zoom);
					SetMarker(map, location, address.GetAddressLine(0) + " " + address.GetAddressLine(1));
				}
				else // Location not found...
				{
					new AlertDialog.Builder(this)
						.SetTitle(Resource.String.AddressNotFoundTitle)
						.SetMessage(Resource.String.AddressNotFoundMessage)
						.SetPositiveButton(Resource.String.Okay, (object sender, DialogClickEventArgs e) => {})
						.Show();
				}
			}
		}
	}
}

