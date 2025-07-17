using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    GameManager gameManager;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();   
    }

    public void TriggerActived(string trigger)
    {
        gameManager.AnimationTriggered(trigger);
    }
}
