using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using EventManagement;
using Interaction;
using InventoryAndEquipment;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class TutorialScript : MonoBehaviour
{
    [SerializeField] private CharacterController m_characterController;
    [SerializeField] private TextMeshProUGUI     m_TooltipText;
    [SerializeField] private PlayableDirector    m_TimeLine;

    [SerializeField] private BoxCollider2D m_Character,
                                           m_Goblin,
                                           m_Knight,
                                           m_Bridge,
                                           m_ShopEntrance;
    [SerializeField] private EquipmentWindowUIController m_equipmentWindowUIController;
    
    private int       doneCheckPoints =0;
    private int       currentCheckPoint     = 0;
    private IMarker[] markers;

    // Start is called before the first frame update
    IEnumerator Start()
    {

        m_TimeLine.Play();
        
        //------------------------- Move
        yield return ResumeOn(() => m_characterController.Character.State == Character.StateTypes.Moving);
      
        //------------------------- Move To the Bridge
        yield return ResumeOn(() => m_Character.IsTouching(m_Bridge) || !m_Bridge.gameObject.activeInHierarchy);
      
        //------------------------- Kill Goblin
        yield return ResumeOn(() => !m_Goblin.isActiveAndEnabled);
       
        //------------------------- Speak With Knight
        yield return ResumeOn(() => m_Knight.bounds.Intersects(m_ShopEntrance.bounds));
      
        //------------------------- Go To Shop
        yield return ResumeOn(() => m_Character.IsTouching(m_ShopEntrance));
     
        //------------------------- Click Shop
        yield return ResumeOn(() => ShopController.WindowIsActive);
     
        //------------------------- Buy Item Shop
        yield return WaitForGameEventToResume(GameEvents.Shop_Item_Purchased);
       
        //------------------------- Sell Items
        yield return WaitForGameEventToResume(GameEvents.Shop_Item_Sold);
      
        //------------------------- Buy One More Item Shop
        yield return WaitForGameEventToResume(GameEvents.Shop_Item_Purchased);
     
        //------------------------- Close Shop
        yield return ResumeOn(() => !ShopController.WindowIsActive);
       
        //------------------------- Open Armory
        yield return ResumeOn(() => m_equipmentWindowUIController.gameObject.activeInHierarchy);
        
        //------------------------- Equip item
        yield return WaitForGameEventToResume(GameEvents.Equipment_Updated);
        
        //------------------------- Close Armory
        yield return ResumeOn(() => !m_equipmentWindowUIController.gameObject.activeInHierarchy);


        //House Clicked
        while (!m_Goblin.gameObject.activeInHierarchy) yield return null;
        
        Destroy(gameObject);
    }

    IEnumerator ResumeOn(Func <bool> condition)
    {
        while (!condition())
        {
            yield return null;
        }

        doneCheckPoints++;
       
        if (m_TimeLine.state == PlayState.Paused)
        {
            m_TimeLine.Resume();
        }
    }
 
  

    public void CheckPointSignalReached()
    {
        currentCheckPoint++;
        if(currentCheckPoint > doneCheckPoints  && m_TimeLine.state== PlayState.Playing) m_TimeLine.Pause();
    }

    private IEnumerator WaitForGameEventToResume(GameEvents GameEvent)
    {
        bool isDone = false;
        GameEvent.Subscribe(()=>isDone = true);
        yield return ResumeOn(() => isDone);
        GameEvent.Unsubscribe(()=>isDone = true);
    }
    
   
    private void LateUpdate()
    {
        // Fast Forward Tutorial checkpoints if Things are already done 
        FastForwardAlreadyDoneThingsInTimeline();
      
        
    }

    private void FastForwardAlreadyDoneThingsInTimeline()
    {
        if(doneCheckPoints == 0) return;
        
        if (markers == null)
        {
            markers ??= ((TimelineAsset)m_TimeLine.playableAsset).GetOutputTracks().First(x => x.name == "Signal Track").GetMarkers().ToArray();
            Array.Sort(markers, (marker, marker1) => marker.time.CompareTo(marker1.time) );
        }
        
        if (m_TimeLine.time < markers[doneCheckPoints -1].time)
        {
            m_TimeLine.time = markers[doneCheckPoints -1].time;
            if(m_TimeLine.state == PlayState.Paused) m_TimeLine.Resume();
        }
    }

    
}
