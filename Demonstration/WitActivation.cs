 using Oculus.Voice;
 using UnityEngine;

 public class WitActivation : MonoBehaviour
 {
     private AppVoiceExperience _voiceExperience;
     private void OnValidate()
     {
         if (!_voiceExperience) _voiceExperience = GetComponent<AppVoiceExperience>();
     }

     private void Start()
     {
         _voiceExperience = GetComponent<AppVoiceExperience>();
     }

     private void Update()
     {
        // Input Manager
        if (!InputManager.Instance.IsInputAllowed())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("*** Pressed M for Mic Recording ***");
            ActivateWit();
        }
     }

     public void ActivateWit()
     {
        Debug.Log("Mic Enabled");
         _voiceExperience.Activate();
     }
 }