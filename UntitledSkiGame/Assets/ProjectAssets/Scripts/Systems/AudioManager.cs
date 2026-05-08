using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //information
    public float music_volume;
    public float sfx_volume;

    //audio source
    [Header("Audio Source")]
    [SerializeField] AudioSource sfx;
    [SerializeField] AudioSource music;

    #region audios
    [SerializeField] public AudioClip bossSong;
    [SerializeField] public AudioClip normalSong;

    //world events
    [SerializeField] public AudioClip spawn;
    [SerializeField] public AudioClip exit;
    
    //pickup
    [SerializeField] public AudioClip coin;
    [SerializeField] public AudioClip item;
    [SerializeField] public AudioClip item_machine;

    //gun
    [SerializeField] public AudioClip charging;
    [SerializeField] public AudioClip charged;
    [SerializeField] public AudioClip blast;
    [SerializeField] public AudioClip recharged;

    //enemies
    [SerializeField] public AudioClip e_damage;
    [SerializeField] public AudioClip fly_drop;
    [SerializeField] public AudioClip e_deathSound;

    //player
    [SerializeField] public AudioClip p_damage;

    //boss sounds
    [SerializeField] public AudioClip head_hit;
    [SerializeField] public AudioClip hand_death;
    [SerializeField] public AudioClip hand_smash;

    //UI
    [SerializeField] public AudioClip pause;
    [SerializeField] public AudioClip unpause;
    [SerializeField] public AudioClip buttonpress;

    #endregion audios

    //getting the audio instance
    public static AudioManager instance;

    private bool ignore_volume;

    private void Awake()
    {
        //Make sure there is only one instance at all times
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        //playing the normal first music
        music.clip = normalSong;
        ignore_volume = false;
        music.loop = true;
        music.Play();
    }

    public void PlaySingleSFX(AudioClip clip)
    {
        sfx.PlayOneShot(clip);
    }

    public void PlaySFXPitched(AudioClip clip, float pitch)
    {
        sfx.pitch = pitch;
        sfx.PlayOneShot(clip);
        sfx.pitch = 1f;
    }

    public void EndSong(AudioClip song)
    {
        StartCoroutine(FadeMusic(song));
    }

    public void PlaySong(AudioClip song)
    {
        music.clip = song;
        music.Play();
    }

    public void PlayMainSong()
    {
        if(!music.isPlaying)
        {
            music.clip = normalSong;
            music.Play();
        }
    }

    //music fadeout transition
    private IEnumerator FadeMusic(AudioClip song)
    {
        ignore_volume = true;

        while(music.volume != 0)
        {
            music.volume -= 0.05f;
            yield return new WaitForSeconds(0.1f);
        }

        music.Stop();
        ignore_volume = false;
    }

    private void Update()
    {
        if (!ignore_volume)
        {
            if(music.volume != music_volume)
            {
                music.volume = music_volume;
            }
            if(sfx.volume !=  sfx_volume)
            {
                sfx.volume = sfx_volume;
            }
        }
        
    }
}
