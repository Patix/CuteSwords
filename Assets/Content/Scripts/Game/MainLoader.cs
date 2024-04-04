using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Content.Scripts.Game
{
    public class MainLoader: MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI    text;
        [SerializeField] private float          duration;
        [SerializeField] private AnimationCurve animationCurve;
        
        IEnumerator Start()
        {
            yield return AnimateLoadingText();
            SceneManager.LoadScene("Prototype Map");
        }

        private IEnumerator AnimateLoadingText()
        {
            var (r, g, b) = (text.color.r,text.color.g,text.color.b);
          
            while (Time.realtimeSinceStartup <duration)
            {
                var alpha = animationCurve.Evaluate(Time.realtimeSinceStartup);
                text.color = new Color(r, g, b, alpha);
                yield return null;
            }
        }
    }
}