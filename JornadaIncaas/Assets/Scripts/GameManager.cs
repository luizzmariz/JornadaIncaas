using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Animator logoAnimator;

    LevelManager levelManager;

    void Awake()
    {
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
                levelManager.StartLevel();
                break;
        }
    }
}
