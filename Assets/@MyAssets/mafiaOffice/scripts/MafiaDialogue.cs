using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MafiaDialogue : MonoBehaviour
{
    public SubtitleManager subtitleManager;
    public AudioSource dialogueAudio;
    public AudioClip[] dialogueClips;

    private string[] subtitles = {
        "�Sabes por qu� est�s aqu�?",
        "Mataste a mi hija. Ese quir�fano era su �ltima oportunidad.",
        "Podr�a matarte ahora mismo. F�cil. R�pido.",
        "Pero eres m�s �til vivo que muerto.",
        "Trabajar�s para m�. Sin preguntas, sin fallos.",
        "Trabajar�s en una tienda que controlo. Parece un negocio leg�timo, pero es solo una fachada.",
        "Los clientes entran... y algunos no vuelven a salir. T� sabr�s cu�les. T� sabr�s qu� hacer.",
        "Y no olvides: partes del cuerpo. las necesito para mi otro negocio.",
        "Una sola falla... y terminar�s como mi hija."
    };
    public Animator mafiaAnimator;
    public string[] animationTriggers;
    private void Start()
    {
        StartCoroutine(PlayDialogue());
    }

    private IEnumerator PlayDialogue()
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < dialogueClips.Length; i++)
        {
            if (i < animationTriggers.Length && !string.IsNullOrEmpty(animationTriggers[i]))
            {
                mafiaAnimator.SetTrigger(animationTriggers[i]);
            }

            subtitleManager.DisplaySubtitle(subtitles[i], dialogueClips[i].length);

            dialogueAudio.clip = dialogueClips[i];
            dialogueAudio.Play();

            yield return new WaitForSeconds(dialogueClips[i].length + 0.5f);
        }
        SceneManager.LoadScene("tutorial");
    }
}
