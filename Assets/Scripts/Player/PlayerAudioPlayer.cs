using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerAudioPlayer : MonoBehaviour
{ 
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;
    public AudioClip LandingAudioClip;

    public AudioClip[] JumpAudioClip;
    [Range(0, 2)] public float JumpAudioVolume = 0.5f;

    private CharacterController _controller;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    public void PlayFootstepSound()
    {
        if (FootstepAudioClips.Length > 0)
        {
            var index = Random.Range(0, FootstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }

    public void PlayJumpSound()
    {
        if (FootstepAudioClips.Length > 0)
        {
            var index = Random.Range(0, JumpAudioClip.Length);
            AudioSource.PlayClipAtPoint(JumpAudioClip[index], transform.TransformPoint(_controller.center), JumpAudioVolume);
        }
    }

    public void PlayLandSound()
    {
        AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
    }
}
