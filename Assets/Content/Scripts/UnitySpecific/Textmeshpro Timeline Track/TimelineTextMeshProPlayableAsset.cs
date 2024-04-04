using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TimelineTextMeshProPlayableAsset : PlayableAsset, ITimelineClipAsset
{
    [SerializeField] private TimelineTextMeshProBehaviour template = new TimelineTextMeshProBehaviour();
    public                   ClipCaps                     clipCaps                                           => ClipCaps.None;
    public override          Playable                     CreatePlayable(PlayableGraph graph, GameObject go) { return ScriptPlayable <TimelineTextMeshProBehaviour>.Create(graph, template); }
}