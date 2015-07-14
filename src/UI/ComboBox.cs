/*
 * 
// Popup list created by Eric Haines
// ComboBox Extended by Hyungseok Seo.(Jerry) sdragoon@nate.com
// Refactored by zhujiangbo jumbozhu@gmail.com
// Slight edit for button to show the previously selected item AndyMartin458 www.clubconsortya.blogspot.com
// 
// -----------------------------------------------
// This code working like ComboBox Control.
// I just changed some part of code, 
// because I want to seperate ComboBox button and List.
// ( You can see the result of this code from Description's last picture )
// -----------------------------------------------
//
// === usage ======================================
using UnityEngine;
using System.Collections;
 
public class ComboBoxTest : MonoBehaviour
{
	GUIContent[] comboBoxList;
	private ComboBox comboBoxControl;// = new ComboBox();
	private GUIStyle listStyle = new GUIStyle();
 
	private void Start()
	{
		comboBoxList = new GUIContent[5];
		comboBoxList[0] = new GUIContent("Thing 1");
		comboBoxList[1] = new GUIContent("Thing 2");
		comboBoxList[2] = new GUIContent("Thing 3");
		comboBoxList[3] = new GUIContent("Thing 4");
		comboBoxList[4] = new GUIContent("Thing 5");
 
		listStyle.normal.textColor = Color.white; 
		listStyle.onHover.background =
		listStyle.hover.background = new Texture2D(2, 2);
		listStyle.padding.left =
		listStyle.padding.right =
		listStyle.padding.top =
		listStyle.padding.bottom = 4;
 
		comboBoxControl = new ComboBox(new Rect(50, 100, 100, 20), comboBoxList[0], comboBoxList, "button", "box", listStyle);
	}
 
	private void OnGUI () 
	{
		comboBoxControl.Show();
	}
}
 
*/


using System;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class ComboBox
	{
		private static bool forceToUnShow = false;
		private static int useControlID = -1;
		public bool isClickedComboButton = false;
		private int selectedItemIndex = 0;

		//private Rect rect;
		private GUIContent buttonContent;
		private GUIContent[] listContent;
		private string buttonStyle;
		private string boxStyle;
		private GUIStyle listStyle;

		private Action<int> selectionCallback;

		public ComboBox(GUIContent buttonContent, GUIContent[] listContent, Action<int> callback, GUIStyle listStyle)
		{
			//this.rect = rect;
			this.buttonContent = buttonContent;
			this.listContent = listContent;
			this.buttonStyle = "button";
			this.boxStyle = "box";
			this.listStyle = listStyle;
			this.selectionCallback = callback;
		}

		public ComboBox(GUIContent buttonContent, GUIContent[] listContent, string buttonStyle, string boxStyle, Action<int> callback, GUIStyle listStyle)
		{
			//this.rect = rect;
			this.buttonContent = buttonContent;
			this.listContent = listContent;
			this.buttonStyle = buttonStyle;
			this.boxStyle = boxStyle;
			this.listStyle = listStyle;
			this.selectionCallback = callback;
		}

		public int Show(Rect rect)
		{
			if (forceToUnShow)
			{
				forceToUnShow = false;
				isClickedComboButton = false;
			}

			bool done = false;
			int controlID = GUIUtility.GetControlID(FocusType.Passive);

			switch (Event.current.GetTypeForControl(controlID))
			{
				case EventType.mouseUp:
					{
						if (isClickedComboButton)
						{
							done = true;
						}
					}
					break;
			}

			if (GUI.Button(rect, buttonContent, buttonStyle))
			{
				if (useControlID == -1)
				{
					useControlID = controlID;
					isClickedComboButton = false;
				}

				if (useControlID != controlID)
				{
					forceToUnShow = true;
					useControlID = controlID;
				}
				isClickedComboButton = true;
			}

			if (isClickedComboButton)
			{
				Rect listRect = new Rect(rect.x, rect.y + listStyle.CalcHeight(listContent[0], 1.0f),
						  rect.width, listStyle.CalcHeight(listContent[0], 1.0f) * listContent.Length);

				GUI.Box(listRect, "", boxStyle);
				int newSelectedItemIndex = GUI.SelectionGrid(listRect, selectedItemIndex, listContent, 1, listStyle);
				if (newSelectedItemIndex != selectedItemIndex)
				{
					selectedItemIndex = newSelectedItemIndex;
					buttonContent = listContent[selectedItemIndex];
					if(selectionCallback != null)
						selectionCallback(selectedItemIndex);
				}
			}

			if (done)
				isClickedComboButton = false;

			return selectedItemIndex;
		}

		public int SelectedItemIndex
		{
			get
			{
				return selectedItemIndex;
			}
			set
			{
				selectedItemIndex = value;
			}
		}
	}
}