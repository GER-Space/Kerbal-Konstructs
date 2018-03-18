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
        public string RotationTrasform;
        public string ElevationTransform;


        private Transform rotTransform;
        private Transform elTransform;

        private DishController.Dish dish;
        private DishController controller;

        public void Start()
        {
            Log.Normal("DishAnimate Awake start");
            rotTransform = gameObject.transform.FindRecursive(RotationTrasform);
            elTransform = gameObject.transform.FindRecursive(ElevationTransform);

            if (rotTransform == null)
            {
                Log.Normal("Rotations Trasform not found");
                Destroy(this);
            }

            if (elTransform == null)
            {
                Log.Normal("Elevation Trasform not found");
                Destroy(this);
            }

            dish = new DishController.Dish();

            dish.elevationTransform = elTransform;
            //dish.elevationInit = new Quaternion();
            dish.rotationTransform = rotTransform;
            //dish.rotationInit = new Quaternion();


            controller = gameObject.AddComponent<DishController>();
            controller.dishes = new DishController.Dish[] { dish };
            controller.enabled = true;

            controller.maxElevation = 90f;
            controller.minElevation = 0f;

            Log.Normal("DishAnimate Awake complete");

        }


    }
}
