using System.Collections;
using System.Collections.Generic;
using InventoryAndEquipment;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class TutorialScript : MonoBehaviour
{
    [SerializeField] private CharacterController m_characterController;
    [SerializeField] private TextMeshProUGUI     m_TooltipText;
    [SerializeField] private PlayableDirector    m_TimeLine;
    private 
    // Start is called before the first frame update
    IEnumerable Start()
    {
        //m_TooltipText.text = "Press \"WASD\" to  Move";

        while (m_TimeLine.state == PlayState.Playing)
            yield return null;

        while (m_characterController.Character.State!=Character.StateTypes.Moving) 
            yield return null; 
        
        m_TimeLine.Resume();
    }


    public void CheckPointSignalReached() => m_TimeLine.Pause();

}
