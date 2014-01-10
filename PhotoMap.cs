using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.Media;
using Xamarin.Geolocation;
using Xamarin.Media;
using PhotoMapper.Core.Service;
using PhotoMapper.Core.Extension;
using PhotoMapper.Core.Model;

namespace PhotoMapper
{
	[Activity(Label = "PhotoMap")]			
	public class PhotoMap : Activity
	{
		private const int ZoomLevel = 15;
		private const int SelectImageCode = 1000;
		private Dictionary<string, Image> _imageMarkers = new Dictionary<string, Image>();

		#region Services

		private IGeoLocationService _geoLocationService = null;
		public IGeoLocationService GeoLocationService
		{
			get { return _geoLocationService ?? (_geoLocationService = new GeoLocationService(this)); }
			set { _geoLocationService = value; }
		}

		private IImageService _imageService = null;
		public IImageService ImageService
		{
			get { return _imageService ?? (_imageService = new ImageService()); }
			set { _imageService = value; }
		}

		#endregion

		#region Overrides

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.PhotoMap);

			// Configure the map.
			GoogleMap map = GetMapFromFragment(Resource.Id.FragmentPhotoMap);
			if (map == null)
				throw new ApplicationException("Map handle not available.");
			
			map.MapType = GoogleMap.MapTypeNormal;
			map.UiSettings.CompassEnabled = true;
			map.UiSettings.RotateGesturesEnabled = true;
			map.UiSettings.ScrollGesturesEnabled = true;
			map.UiSettings.TiltGesturesEnabled = true;
			map.UiSettings.ZoomControlsEnabled = true;
			map.UiSettings.ZoomGesturesEnabled = true;
			map.UiSettings.MyLocationButtonEnabled = true;
			map.InfoWindowClick += HandleInfoWindowClick;
			map.MyLocationEnabled = true;
			map.SetInfoWindowAdapter(new PhotoInfoWindowAdapter(this, _imageMarkers));

			// Attach event handlers to controls.
//			Button buttonGoToCurrentLocation = FindViewById<Button>(Resource.Id.ButtonGoToCurrentLocation);
//			buttonGoToCurrentLocation.Click += (object sender, EventArgs e) =>
//			{
//				HandleGoToCurrentLocation();
//			};

			Button buttonGoToAddress = FindViewById<Button>(Resource.Id.ButtonGoToAddress);
			buttonGoToAddress.Click += (object sender, EventArgs e) =>
			{
				HandleGoToAddress();
			};
			
			Button buttonMapImage = FindViewById<Button>(Resource.Id.ButtonMapImage);
			buttonMapImage.Click += (object sender, EventArgs e) =>
			{
				HandleMapImage();
			};
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
		{
			base.OnActivityResult(requestCode, resultCode, intent);
			if (resultCode == Result.Canceled || intent == null)
				return;

			GoogleMap map = GetMapFromFragment(Resource.Id.FragmentPhotoMap);
			if (map == null)
				throw new ApplicationException("Map handle not available.");

			switch (requestCode)
			{
				case SelectImageCode:
					MapImage(map, intent.Data, ZoomLevel);
					break;
				default:
					break;
			}
		}

		#endregion

		#region Button Handlers

		private async void HandleGoToCurrentLocation()
		{
			GoogleMap map = GetMapFromFragment(Resource.Id.FragmentPhotoMap);
			if (map == null)
				throw new ApplicationException("Map handle not available.");

			if (GeoLocationService.IsGeolocationEnabled)
			{
				LatLng currentLocation = await GeoLocationService.GetCurrentLocationAsync();
				if (currentLocation != null)
				{
					map.ZoomToLocation(currentLocation, ZoomLevel);
					map.SetMarker(currentLocation, "Current Location");
				}
				else
				{
					this.DisplayMessage(Resource.String.TitleNoCurrentLocation, Resource.String.MessageNoCurrentLocation);
				}
			}
			else
			{
				this.DisplayMessage(Resource.String.TitleNoGeoEnabled, Resource.String.MessageNoGeoEnabled);
			}
		}

		private void HandleGoToAddress()
		{
			GoogleMap map = GetMapFromFragment(Resource.Id.FragmentPhotoMap);
			if (map == null)
				throw new ApplicationException("Map handle not available.");

			var inputControl = new EditText(this);

			new AlertDialog.Builder(this)
				.SetTitle(Resource.String.TitleAddressSearch)
				.SetMessage(Resource.String.MessageAddressSearch)
				.SetView(inputControl)
				.SetPositiveButton(Resource.String.ButtonOkay, (object sender, DialogClickEventArgs e) =>
				{
					if (!string.IsNullOrWhiteSpace(inputControl.Text))
						ZoomToAddress(map, inputControl.Text, ZoomLevel);
				})
				.SetNegativeButton(Resource.String.ButtonCancel, (object sender, DialogClickEventArgs e) =>
				{
				})
				.Show();
		}

		private void HandleMapImage()
		{
			var picker = new MediaPicker(this);
			if (!picker.PhotosSupported)
			{
				this.DisplayMessage(Resource.String.TitleNoDeviceImageSupport, Resource.String.MessageNoDeviceImageSupport);
				return;
			}

			var imageIntent = new Intent();
			imageIntent.SetType("image/*");
			imageIntent.SetAction(Intent.ActionGetContent);
			StartActivityForResult(Intent.CreateChooser(imageIntent, "Select Image"), SelectImageCode);
		}

		#endregion

		#region Helpers

		private GoogleMap GetMapFromFragment(int mapFragmentId)
		{
			MapFragment mapFragment = (MapFragment)FragmentManager.FindFragmentById(mapFragmentId);
			return mapFragment.Map;
		}

		private async void ZoomToAddress(GoogleMap map, string searchAddress, float zoom)
		{
			if (map == null)
				throw new ArgumentNullException("map");
			if (string.IsNullOrWhiteSpace(searchAddress))
				throw new ArgumentNullException("searchAddress");

			Address address = await GeoLocationService.GeoSearchAsync(searchAddress);
			if (address != null)
			{
				var location = new LatLng(address.Latitude, address.Longitude);
				map.ZoomToLocation(location, zoom);
				map.SetMarker(location, address.GetAddressLine(0) + " " + address.GetAddressLine(1));
			}
			else // Location not found...
			{
				this.DisplayMessage(Resource.String.TitleAddressNotFound, Resource.String.MessageAddressNotFound);
			}
		}

		private void MapImage(GoogleMap map, Android.Net.Uri imageUri, float zoom)
		{
			if (map == null)
				throw new ArgumentNullException("map");
			if (imageUri == null)
				throw new ArgumentNullException("imageUri");

			string imagePath = ImageService.GetImagePath(imageUri, this);

			if (_imageMarkers.Any(kvp => kvp.Value.ImagePath == imagePath)) // Image already mapped.
			{
				this.DisplayMessage(Resource.String.TitleImageAlreadyMapped, Resource.String.MessageImageAlreadyMapped);
			}
			else // Map the image with a marker.
			{
				LatLng location = ImageService.GetImageLocation(imagePath);
				if (location != null) // The image contains EXIF geo data.
				{
					map.ZoomToLocation(location, zoom);
					Marker marker = map.SetMarker(location, Path.GetFileName(imagePath), imagePath, draggable : true);
					if (marker != null)
						_imageMarkers.Add(marker.Id, new Image { ImageUri = imageUri, ImagePath = imagePath, Location = location });
				}
				else // No EXIF geo data present in image.
				{
					new AlertDialog.Builder(this)
						.SetTitle(Resource.String.TitleNoExifGeoDataInImage)
						.SetMessage(Resource.String.PromptNoExifGeoDataInImage)
						.SetPositiveButton(Resource.String.ButtonOkay, (object sender, DialogClickEventArgs e) =>
						{
							PlaceMovableImageMarker(map, imageUri, imagePath, zoom);
						})
							.SetNegativeButton(Resource.String.ButtonCancel, (object sender, DialogClickEventArgs e) =>
						{
						})
						.Show();
				}
			}
		}

		private async void PlaceMovableImageMarker(GoogleMap map, Android.Net.Uri imageUri, string imagePath, float zoom)
		{
			if (map == null)
				throw new ArgumentNullException("map");
			if (imageUri == null)
				throw new ArgumentNullException("imageUri");
			if (string.IsNullOrWhiteSpace(imagePath))
				throw new ArgumentNullException("imagePath");

			if (GeoLocationService.IsGeolocationEnabled)
			{
				LatLng currentLocation = await GeoLocationService.GetCurrentLocationAsync();
				if (currentLocation != null)
				{
					map.ZoomToLocation(currentLocation, zoom);
					Marker marker = map.SetMarker(currentLocation, Path.GetFileName(imagePath), imagePath, draggable : true);
					if (marker != null)
						_imageMarkers.Add(marker.Id, new Image { ImageUri = imageUri, ImagePath = imagePath, Location = currentLocation });
				}
				else
				{
					this.DisplayMessage(Resource.String.TitleNoCurrentLocation, Resource.String.MessageNoCurrentLocation);
				}
			}
			else
			{
				this.DisplayMessage(Resource.String.TitleNoGeoEnabled, Resource.String.MessageNoGeoEnabled);
			}
		}
		
		void HandleInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
		{
			var marker = e.P0;
			if (marker != null && _imageMarkers.ContainsKey(marker.Id))
			{
				var image = _imageMarkers[marker.Id];
				if (image != null && image.Location != marker.Position)
				{
					new AlertDialog.Builder(this)
						.SetTitle(Resource.String.TitleSetImageLocation)
						.SetMessage(Resource.String.PromptSetImageLocation)
						.SetPositiveButton(Resource.String.ButtonOkay, (object eventSender, DialogClickEventArgs eventArgs) =>
						{
							if (!ImageService.SetImageLocation(image.ImagePath, marker.Position))
								this.DisplayMessage(Resource.String.TitleSetImageLocationFailed, Resource.String.MessageSetImageLocationFailed);
						})
						.SetNegativeButton(Resource.String.ButtonCancel, (object eventSender, DialogClickEventArgs eventArgs) =>
						{
						})
						.Show();
				}
			}
		}

		#endregion
	}
}

