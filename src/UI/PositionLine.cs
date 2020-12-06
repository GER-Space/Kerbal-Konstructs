using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KodeUI;
using TMPro;

namespace KerbalKonstructs.UI
{

	public class PositionLine : HorizontalLayout
	{
		UIText altitude;
		UIText latitude;
		UIText longitude;

		public override void CreateUI()
		{
			base.CreateUI();

			this.Add<FixedSpace>() .Size(5) .Finish()
				.Add<UIText>()
					.Text(KKLocalization.Alt)
					.Alignment(TextAlignmentOptions.Left)
					.Finish()
				.Add<FixedSpace>() .Size(5) .Finish()
				.Add<UIText>(out altitude)
					.Alignment(TextAlignmentOptions.Left)
					.Finish()

				.Add<FlexibleSpace>() .Finish()
				.Add<UIText>()
					.Text(KKLocalization.Lat)
					.Alignment(TextAlignmentOptions.Left)
					.Finish()
				.Add<FixedSpace>() .Size(5) .Finish()
				.Add<UIText>(out latitude)
					.Alignment(TextAlignmentOptions.Right)
					.Finish()

				.Add<FlexibleSpace>() .Finish()
				.Add<UIText>()
					.Text(KKLocalization.Lon)
					.Alignment(TextAlignmentOptions.Left)
					.Finish()
				.Add<FixedSpace>() .Size(5) .Finish()
				.Add<UIText>(out longitude)
					.Alignment(TextAlignmentOptions.Right)
					.Finish()
				.Add<FixedSpace>() .Size(5) .Finish()
				;
		}

		public override void Style()
		{
			base.Style();
		}

		public PositionLine Altitude(double altitude)
		{
			this.altitude.Text($"{altitude:F1}m");
			return this;
		}

		public PositionLine Latitude(double latitude)
		{
			this.latitude.Text($"{latitude:F3}°");
			return this;
		}

		public PositionLine Longitude(double longitude)
		{
			this.longitude.Text($"{longitude:F3}°");
			return this;
		}
	}
}
