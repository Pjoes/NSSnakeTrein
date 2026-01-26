using UnityEngine;

public enum SoundType
{
    TRAINMOVE,
    TRAINCRASH,
    ITEMPICKUP,
    PASSENGERPICKUP,
    ENEMYATTACK1,
    ENEMYATTACK2,
    GAMEOVER,
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundList;
    private static SoundManager instance;
    private AudioSource _audioSource;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1f)
    {
        instance._audioSource.PlayOneShot(instance.soundList[(int)sound], volume);
    }
}
