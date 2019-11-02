using KerbalKonstructs.Core;
using System.Linq;
using UnityEngine;

namespace KerbalKonstructs
{
    public class GUIOnClick : StaticModule
    {
        public string collider;
        public string FacilityType;
        private bool bObjectClicked;
        //public string animationName;
        //optional
        public bool HighlightOnHover = true;
        //public float animationSpeed = 1f;

        //private bool animationPlaying = false;
        //private bool playAnimationForward = true;
        //private Animation animationComponent;

        void Start()
        {
            //if (animationComponent == null)
            //{
            /* animationComponent = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                                  where animationList != null
                                  from AnimationState animationState in animationList
                                  where animationState.name == animationName
                                  select animationList).FirstOrDefault(); */

            GameObject obj = (from t in gameObject.GetComponentsInChildren<Transform>()
                              where t.gameObject != null && t.gameObject.name == collider
                              select t.gameObject).FirstOrDefault();

            GUIOnClick colliderObject = obj.AddComponent<GUIOnClick>();
            colliderObject.collider = collider;
            //colliderObject.animationName = animationName;
            colliderObject.HighlightOnHover = HighlightOnHover;
            colliderObject.FacilityType = FacilityType;
            //colliderObject.animationSpeed = animationSpeed;
            //colliderObject.animationComponent = animationComponent;
            Destroy(this);
            //}
        }

        void OnMouseDown()
        {
            if (!bObjectClicked)
            {
                bObjectClicked = true;
            }
            else
            {
                bObjectClicked = false;
            }
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

        /* IEnumerator playAnimation()
		{
			animationPlaying = true;
			animationComponent[animationName].speed = playAnimationForward ? animationSpeed : -animationSpeed;
			animationComponent[animationName].normalizedTime = playAnimationForward ? 0 : 1;
			animationComponent.Play(animationName);
			yield return new WaitForSeconds(animationComponent[animationName].length / animationSpeed);
			playAnimationForward = !playAnimationForward;
			animationPlaying = false;
		} */
    }
}
