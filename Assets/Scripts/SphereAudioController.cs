﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(SphereController)), RequireComponent(typeof(ChuckSubInstance)), RequireComponent(typeof(AudioSource))]
public class SphereAudioController : MonoBehaviour
{
    [SerializeField] private AudioClip[] collisionDrumHits = null;
    [SerializeField] private AudioClip textureClip = null;
    [SerializeField] private float baseRate = 1.0f;
    [SerializeField] private float rateRangeDown = 0.5f;
    [SerializeField] private float rateRangeUp = 1.0f;
    [SerializeField] private float audioFadeDuration = 3.0f;
    [SerializeField] private float maxAudioFilePos = 0.5f;
    [SerializeField] private float minAudioFileGain = 0.7f;
    [SerializeField] private float maxAudioFileGain = 1.0f;
    [SerializeField] private bool doLoopAudioFile = true;
    [SerializeField] private float minDrumVolume = 0.1f;
    [SerializeField] private float maxDrumVolume = 0.3f;

    private SphereController sc;
    private ChuckSubInstance chuck;
    private AudioSource source;
    private AudioSource collideSource;
    private float minVolume = 0f;
    private float maxVolume = 1f;


    void Awake()
    {
        Assert.IsNotNull(textureClip);
        sc = GetComponent<SphereController>();
        chuck = GetComponent<ChuckSubInstance>();
        source = GetComponent<AudioSource>();
        collideSource = transform.Find("CollisionAudioSource").GetComponent<AudioSource>();
    }

    public void PlayCollideSound()
    {
        var clip = collisionDrumHits[Random.Range(0, collisionDrumHits.Length)];
        var vol = Random.Range(minDrumVolume, maxDrumVolume);
        collideSource.PlayOneShot(clip, vol);
    }

    public void Play()
    {
        // rate of the audio file playback is driven by the speed at which the sphere rotates
        float rate = GameUtils.Map(sc.GetSphereRotateAroundSpeedNormalized(), 0f, 1f, baseRate - rateRangeDown, baseRate + rateRangeUp);

        // gain of the audio file playback is driven by the size of the sphere
        float gain = GameUtils.Map(sc.GetSphereSizeNormalized(), 0f, 1f, minAudioFileGain, maxAudioFileGain);

        chuck.SetRunning(true);
        chuck.RunCode(string.Format(@"
                SndBuf textureBuf => dac;
                me.dir() + ""{4}.wav"" => textureBuf.read;

                // loop the clip
                // based on casting doLoopAudioFile
                // {3} => textureBuf.loop;

                // start randomly offset into the clip
                textureBuf.samples() * {2} $ int => int offset;
                Math.random2(0, offset) => textureBuf.pos;

                // change playback speed
                {0} => textureBuf.rate;

                // change amplitude
                {1} => textureBuf.gain;

                // pass time so that the file plays
                textureBuf.length() / textureBuf.rate() => now;
            ", rate, gain, maxAudioFilePos, System.Convert.ToInt32(doLoopAudioFile), textureClip.name));
        StartCoroutine(FadeAudio(FadeDirection.Up, DoNothingOnComplete));
    }

    public void Stop()
    {
        StartCoroutine(FadeAudio(FadeDirection.Down, StopAudioOnComplete));
    }

    // callback action when fade coroutine is complete
    private void DoNothingOnComplete()
    {
        // do nothing
    }

    // callback action when fade coroutine is complete
    private void StopAudioOnComplete()
    {
        source.volume = 0f;
        chuck.SetRunning(false);
    }

    private IEnumerator FadeAudio(FadeDirection direction, System.Action onComplete)
    {
        const int numFadeSteps = 100;
        float stepAmount = 1f / numFadeSteps;

        float easeSpeed = audioFadeDuration / numFadeSteps;
        float easeStart = minVolume;
        float easeGoal = maxVolume;

        if (direction == FadeDirection.Down)
        {
            easeStart = maxVolume;
            easeGoal = minVolume;
        }

        for (float pct = 0; pct <= 1f; pct += stepAmount)
        {
            float volumeLevel;
            if (direction == FadeDirection.Down)
            {
                volumeLevel = Mathfx.Sinerp(easeStart, easeGoal, pct);
            }
            else
            {
                volumeLevel = Mathfx.Coserp(easeStart, easeGoal, pct);
            }
            source.volume = volumeLevel;

            yield return new WaitForSeconds(easeSpeed);
        }
        onComplete();
    }
}
