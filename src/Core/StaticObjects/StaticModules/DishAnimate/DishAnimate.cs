using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using UnityEngine;

namespace KerbalKonstructs
{
    public class DishAnimate : StaticModule
    {
        public string RotationTrasform; // legacy
        public string RotationTransform;
        public string ElevationTransform;
        public string MaxSpeed = "10";
        public string FakeTimeWarp = "1";
        public string MaxElevation = "90";
        public string MinElevation = "10";

        private float speedMax = 10f;
        private float timeWarpFake = 1f;
        private float maxElevation = 90f;
        private float minElevation = 10f;

        private Transform rotTransform;
        private Transform elTransform;

        private DishController.Dish dish;
        private DishController controller;

        public void Start()
        {
            if (RotationTrasform != null && RotationTransform == null)
            {
                RotationTransform = RotationTrasform;
            }
            rotTransform = gameObject.transform.FindRecursive(RotationTransform);
            elTransform = gameObject.transform.FindRecursive(ElevationTransform);

            if (rotTransform == null)
            {
                Log.Normal("DishAnimate: Rotations Transform not found");
                Destroy(this);
            }

            if (elTransform == null)
            {
                Log.Normal("DishAnimate: Elevation Transform not found");
                Destroy(this);
            }

            if (!float.TryParse(MaxSpeed, out speedMax))
            {
                Log.UserWarning("Cannot parse MaxSpeed: " + MaxSpeed);
            }

            if (!float.TryParse(FakeTimeWarp, out timeWarpFake))
            {
                Log.UserWarning("Cannot parse FakeTimeWarp: " + FakeTimeWarp);
            }

            if (!float.TryParse(MaxElevation, out maxElevation))
            {
                Log.UserWarning("Cannot parse MaxElevation: " + MaxElevation);
            }

            if (!float.TryParse(MinElevation, out minElevation))
            {
                Log.UserWarning("Cannot parse MinElevation: " + MinElevation);
            }


            dish = new DishController.Dish();

            dish.elevationTransform = elTransform;
            //dish.elevationInit = new Quaternion();
            dish.rotationTransform = rotTransform;
            //dish.rotationInit = new Quaternion();


            controller = gameObject.AddComponent<DishController>();
            controller.dishes = new DishController.Dish[] { dish };
            controller.enabled = true;

            controller.fakeTimeWarp = timeWarpFake;
            controller.maxSpeed = speedMax;

            controller.maxElevation = maxElevation;
            controller.minElevation = minElevation;

        }


    }
}
