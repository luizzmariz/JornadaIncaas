using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] Animator logoAnimator;
    [SerializeField] Animator manAnimator;
    [SerializeField] Animator tjrnAnimator;
    [SerializeField] AudioSource introAudio;

    LevelManager levelManager;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        levelManager = FindFirstObjectByType<LevelManager>();
    }

    void Start()
    {
        logoAnimator.SetTrigger("StartLogo1");
    }

    public void AnimationTriggered(string trigger)
    {
        switch (trigger)
        {
            case "StartLogo1":
                logoAnimator.SetTrigger("StartLogo2");
                break;

            case "StartLogo2":
                // introAudio.Play();
                manAnimator.SetTrigger("NextStep");
                tjrnAnimator.SetTrigger("NextStep");
                StartCoroutine(IDK());
                break;

            case "ManTransition":
                manAnimator.SetTrigger("NextStep");
                tjrnAnimator.SetTrigger("NextStep");
                levelManager.StartLevel();
                break;

            case "FinishLevel1":
                Debug.Log("g");
                break;
        }
    }

    public void FinishLevel(int level)
    {
        AnimationTriggered("FinishLevel" + level);
    }

    public IEnumerator IDK()
    {
        yield return new WaitForSeconds(2);

        AnimationTriggered("ManTransition");
    }

    public void MoveCamera()
    {
        
    }
}
