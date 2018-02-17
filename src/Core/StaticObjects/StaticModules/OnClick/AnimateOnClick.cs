using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using KerbalKonstructs.Core;

namespace KerbalKonstructs
{
	public class AnimateOnClick : StaticModule
	{
		public string collider;
		public string animationName;
		//optional
		public bool HighlightOnHover = true;
		public float animationSpeed = 1f;

		private bool animationPlaying = false;
		private bool playAnimationForward = true;
		private Animation animationComponent;

		void Start()
		{
			if (animationComponent == null)
			{
				animationComponent = (from animationList in gameObject.GetComponentsInChildren<Animation>()
									  where animationList != null
									  from AnimationState animationState in animationList
									  where animationState.name == animationName
									  select animationList).FirstOrDefault();

				GameObject obj = (from t in gameObject.GetComponentsInChildren<Transform>()
								  where t.gameObject != null && t.gameObject.name == collider
								  select t.gameObject).FirstOrDefault();

				AnimateOnClick colliderObject = obj.AddComponent<AnimateOnClick>();
				colliderObject.collider = collider;
				colliderObject.animationName = animationName;
				colliderObject.HighlightOnHover = HighlightOnHover;
				colliderObject.animationSpeed = animationSpeed;
				colliderObject.animationComponent = animationComponent;
				Destroy(this);
			}
		}

		void OnMouseDown()
		{
			if (!animationPlaying)
				StartCoroutine(playAnimation());
		}

		void OnMouseEnter()
		{
			if (HighlightOnHover)
			{
				gameObject.GetComponent<Renderer>().material.SetFloat("_RimFalloff", 2.5f);
				gameObject.GetComponent<Renderer>().material.SetColor("_RimColor", Color.green);
			}
		}

		void OnMouseExit()
		{
			if (HighlightOnHover)
			{
				gameObject.GetComponent<Renderer>().material.SetFloat("_RimFalloff", 2.5f);
				gameObject.GetComponent<Renderer>().material.SetColor("_RimColor", Color.clear);
			}
		}

		IEnumerator playAnimation()
		{
			animationPlaying = true;
			animationComponent[animationName].speed = playAnimationForward ? animationSpeed : -animationSpeed;
			animationComponent[animationName].normalizedTime = playAnimationForward ? 0 : 1;
			animationComponent.Play(animationName);
			yield return new WaitForSeconds(animationComponent[animationName].length / animationSpeed);
			playAnimationForward = !playAnimationForward;
			animationPlaying = false;
		}
	}
}
