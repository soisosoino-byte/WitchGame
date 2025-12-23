using UnityEngine;

[RequireComponent(typeof(AudioSource))] // ‚±‚ê‚ğ‚Â‚¯‚é‚Æ©“®‚ÅAudioSource‚ª’Ç‰Á‚³‚ê‚Ü‚·
public class ButtonSound : MonoBehaviour
{
    public AudioClip clickSE; // –Â‚ç‚µ‚½‚¢‰¹
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // Ÿè‚É–Â‚ç‚È‚¢‚æ‚¤‚É‚·‚é
    }

    // ƒ{ƒ^ƒ“‚©‚çŒÄ‚Ño‚·ŠÖ”
    public void PlaySE()
    {
        if (clickSE != null)
        {
            audioSource.PlayOneShot(clickSE);
        }
    }
}