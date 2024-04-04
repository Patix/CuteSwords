using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Serialization;

// A behaviour that is attached to a playable
[Serializable]
public class TimelineTextMeshProBehaviour : PlayableBehaviour
{
   [SerializeField] private string m_Text;
   
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        (playerData as TextMeshProUGUI).text = m_Text;
    }
}