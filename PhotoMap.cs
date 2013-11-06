using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.Media;
using Xamarin.Media;
using PhotoMapper.Core;
using Android.Database;

namespace PhotoMapper
{
	[Activity(Label = "PhotoMap")]			
	public class PhotoMap : Activity
	{
		private const int ZoomLevel = 15;
		private const int SelectImageCode = 1000;

		private IGeoLocationService _geoLocationService;
		public IGeoLocationService GeoLocationService
		{
			get { return _geoLocationService ?? (_geoLocationService = new GeoLocationService(this)); }
			set { _geoLocationService = value; }
		}

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
					ZoomToLocation(map, location, ZoomLevel);
					SetMarker(map, location, "Downtown Minneapolis");
				};

				Button goToAddressButton = FindViewById<Button>(Resource.Id.GoToAddressButton);
				goToAddressButton.Click += (object sender, EventArgs e) =>
				{
					GoToAddress();
				};
				
				Button mapImageButton = FindViewById<Button>(Resource.Id.MapImageButton);
				mapImageButton.Click += delegate
				{
					var picker = new MediaPicker(this);
					if (!picker.PhotosSupported)
					{
						DisplayMessage(Resource.String.NoDeviceImageSupportTitle, Resource.String.NoDeviceImageSupportMessage);
						return;
					}

					var imageIntent = new Intent();
					imageIntent.SetType("image/*");
					imageIntent.SetAction(Intent.ActionGetContent);
					StartActivityForResult(Intent.CreateChooser(imageIntent, "Select Image"), SelectImageCode);
				};
			}
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
		{
			base.OnActivityResult(requestCode, resultCode, intent);

			if (resultCode == Result.Canceled || intent == null)
				return;

			GoogleMap map = GetMapFromFragment(Resource.Id.PhotoMapFragment);
			if (map == null)
				return;

			switch (requestCode)
			{
				case SelectImageCode:
					MapImage(map, GetPathToImage(intent.Data), ZoomLevel);
					break;
				default:
					break;
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
					ZoomToAddress(GetMapFromFragment(Resource.Id.PhotoMapFragment), inputControl.Text, ZoomLevel);
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
				Address address = await GeoLocationService.GeoSearchAsync(searchAddress);
				if (address != null)
				{
					var location = new LatLng(address.Latitude, address.Longitude);
					ZoomToLocation(map, location, zoom);
					SetMarker(map, location, address.GetAddressLine(0) + " " + address.GetAddressLine(1));
				}
				else // Location not found...
				{
					DisplayMessage(Resource.String.AddressNotFoundTitle, Resource.String.AddressNotFoundMessage);
				}
			}
		}

		private void MapImage(GoogleMap map, string path, float zoom)
		{
			try
			{
				LatLng location = GetImageLocation(path);
				if (location != null)
				{
					ZoomToLocation(map, location, zoom);
					SetMarker(map, location, Path.GetFileName(path));
				}
				else // No EXIF geo data present in image...
				{
					DisplayMessage(Resource.String.NoExifGeoDataInImageTitle, Resource.String.NoExifGeoDataInImageMessage);
				}
			}
			catch (IOException)
			{
				// TODO:  handle IO error.
			}
		}

		private string GetPathToImage(Android.Net.Uri uri)
		{
			string path = null;
			// The projection contains the columns we want to return in our query.
			string[] projection = new[] { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
			using (ICursor cursor = ManagedQuery(uri, projection, null, null, null))
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

		private LatLng GetImageLocation(string imagePath)
		{
			LatLng location = null;

			var exif = new ExifInterface(imagePath);
			float[] latLong = new float[2];
			if (exif.GetLatLong(latLong))
				location = new LatLng(latLong[0], latLong[1]);

			return location;
		}

		private void DisplayMessage(int title, int message)
		{
			new AlertDialog.Builder(this)
				.SetTitle(title)
				.SetMessage(message)
				.SetPositiveButton(Resource.String.Okay, (object sender, DialogClickEventArgs e) => { })
				.Show();
		}
	}
}

