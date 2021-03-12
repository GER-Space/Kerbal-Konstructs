using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using KodeUI;

namespace KerbalKonstructs.UI {

	public class FieldToggle : ToggleText
	{
		FieldInfo field;
		object obj;

		public override void CreateUI()
		{
			base.CreateUI();

			OnValueChanged(onValueChanged);
			interactable = false;
		}

		void onValueChanged(bool on)
		{
			field.SetValue(obj, on);
		}

		public FieldToggle Field(object obj, string fieldName)
		{
			this.obj = obj;
			if (obj != null) {
				var type = obj.GetType();
				field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (field.FieldType != typeof(bool)) {
					throw new ArgumentException($"{fieldName} is not a bool");
				}
				SetIsOnWithoutNotify ((bool) field.GetValue(obj));
				interactable = true;
			} else {
				field = null;
				interactable = false;
			}
			return this;
		}
	}
}
