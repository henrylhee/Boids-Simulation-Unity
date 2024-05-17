using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Timer
{
    public bool isFinished { get; private set; }
    private float duration;
    private float timeLeft;


    public void Initialize(float duration)
    {
        this.duration = duration;
        isFinished = false;
    }

    public void Update(float deltaTime)
    {
        timeLeft -= deltaTime;
        if(timeLeft <= 0) 
        {
            isFinished = true;
        }
    }

    public void Restart()
    {
        timeLeft = duration;
        isFinished = false;
    }
}
