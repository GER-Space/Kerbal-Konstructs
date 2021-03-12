using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KodeUI;
using TMPro;

namespace KerbalKonstructs.UI
{
	public class VectorDisplay
	{
		Space space = Space.Self;
		Vector3d position;
		Planetarium.CelestialFrame frame;
		CelestialBody body;
		double scale = 30;

		public CelestialBody Body
		{
			get { return body; }
			set {
				body = value;
				Position = position;
				Frame = frame;
			}
		}
		public double Scale
		{
			get { return scale; }
			set {
				scale = Math.Max(1, value);
				upVR.Scale = scale;
				fwdVR.Scale = scale;
				rightVR.Scale = scale;
				backVR.Scale = scale;
				leftVR.Scale = scale;

				northVR.Scale = scale;
				eastVR.Scale = scale;
				southVR.Scale = scale;
				westVR.Scale = scale;
			}
		}
		public Space Space
		{
			get { return space; }
			set {
				space = value;
				if (space == Space.Self) {
					upVR.SetShow(true);
					fwdVR.SetShow(true);
					rightVR.SetShow(true);
					backVR.SetShow(true);
					leftVR.SetShow(true);

					northVR.SetShow(false);
					eastVR.SetShow(false);
					southVR.SetShow(false);
					westVR.SetShow(false);
				} else {
					fwdVR.SetShow(false);
					upVR.SetShow(false);
					rightVR.SetShow(false);
					backVR.SetShow(false);
					leftVR.SetShow(false);

					northVR.SetShow(true);
					eastVR.SetShow(true);
					southVR.SetShow(true);
					westVR.SetShow(true);
				}
			}
		}
		public Vector3d Position
		{
			get { return position; }
			set {
				position = value;
				Vector3d v = body.BodyFrame.LocalToWorld(value).xzy + body.position;
                fwdVR.Start = v;
                upVR.Start = v;
                rightVR.Start = v;
				backVR.Start = v;
				leftVR.Start = v;
                northVR.Start = v;
                eastVR.Start = v;
				southVR.Start = v;
				westVR.Start = v;
			}
		}
		public Planetarium.CelestialFrame Frame
		{
			get { return frame; }
			set {
				frame = value;
                fwdVR.Vector = body.BodyFrame.LocalToWorld(frame.Y).xzy;
                upVR.Vector = body.BodyFrame.LocalToWorld(frame.Z).xzy;
                rightVR.Vector = body.BodyFrame.LocalToWorld(frame.X).xzy;
				backVR.Vector = -fwdVR.Vector;
				leftVR.Vector = -rightVR.Vector;

                northVR.Vector = body.BodyFrame.LocalToWorld(frame.Y).xzy;
                eastVR.Vector = body.BodyFrame.LocalToWorld(frame.X).xzy;
				southVR.Vector = -northVR.Vector;
				westVR.Vector = -eastVR.Vector;
			}
		}

        VectorRenderer upVR = new VectorRenderer();
        VectorRenderer fwdVR = new VectorRenderer();
        VectorRenderer rightVR = new VectorRenderer();
		VectorRenderer backVR = new VectorRenderer();
		VectorRenderer leftVR = new VectorRenderer();

        VectorRenderer northVR = new VectorRenderer();
        VectorRenderer eastVR = new VectorRenderer();
		VectorRenderer southVR = new VectorRenderer();
		VectorRenderer westVR = new VectorRenderer();

        void SetupVectors()
        {
            // draw vectors
            fwdVR.Color = new Color(0, 0, 1);
            fwdVR.Vector = frame.Y.xzy;
            fwdVR.Scale = scale;
            fwdVR.Start = position;
            fwdVR.SetLabel("forward");
            fwdVR.Width = 0.01d;
            fwdVR.SetLayer(5);

			backVR.Color = new Color(0.972f, 1, 0.627f);
			backVR.Vector = -fwdVR.Vector;
			backVR.Scale = scale;
			backVR.Start = position;
			backVR.Width = 0.01d;
			backVR.SetLayer(5);

            upVR.Color = new Color(0, 1, 0);
            upVR.Vector = frame.Z.xzy;
            upVR.Scale = scale;
            upVR.Start = position;
            upVR.SetLabel("up");
            upVR.Width = 0.01d;

            rightVR.Color = new Color(1, 0, 0);
            rightVR.Vector = frame.X.xzy;
            rightVR.Scale = scale;
            rightVR.Start = position;
            rightVR.SetLabel("right");
            rightVR.Width = 0.01d;

			leftVR.Color = new Color(0.972f, 1, 0.627f);
			leftVR.Vector = -rightVR.Vector;
			leftVR.Scale = scale;
			leftVR.Start = position;
			leftVR.Width = 0.01d;

            northVR.Color = new Color(0.9f, 0.3f, 0.3f);
            northVR.Vector = frame.Y.xzy;
            northVR.Scale = scale;
            northVR.Start = position;
            northVR.SetLabel("north");
            northVR.Width = 0.01d;

			southVR.Color = new Color(0.972f, 1, 0.627f);
			southVR.Vector = -northVR.Vector;
			southVR.Scale = scale;
			southVR.Start = position;
			southVR.Width = 0.01d;

            eastVR.Color = new Color(0.3f, 0.3f, 0.9f);
            eastVR.Vector = frame.X.xzy;
            eastVR.Scale = scale;
            eastVR.Start = position;
            eastVR.SetLabel("east");
            eastVR.Width = 0.01d;

			westVR.Color = new Color(0.972f, 1, 0.627f);
			westVR.Vector = -eastVR.Vector;
			westVR.Scale = scale;
			westVR.Start = position;
			westVR.Width = 0.01d;
        }

		public VectorDisplay()
		{
			SetupVectors();
			CloseVectors();
		}

        public void CloseVectors()
        {
            northVR.SetShow(false);
            eastVR.SetShow(false);
			southVR.SetShow(false);
			westVR.SetShow(false);

            upVR.SetShow(false);
            fwdVR.SetShow(false);
            rightVR.SetShow(false);
			backVR.SetShow(false);
			leftVR.SetShow(false);
        }

		public void Draw()
		{
			if (space == Space.Self) {
				upVR.Draw();
				fwdVR.Draw();
				rightVR.Draw();
				backVR.Draw();
				leftVR.Draw();
			} else {
				northVR.Draw();
				eastVR.Draw();
				southVR.Draw();
				westVR.Draw();
			}
		}
	}
}
