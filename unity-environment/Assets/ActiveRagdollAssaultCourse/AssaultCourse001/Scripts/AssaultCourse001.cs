using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MujocoUnity;
using UnityEngine;
using MLAgents;

public class AssaultCourse001Agent : MujocoAgent {

    public float TargetVelocityX;
    public float CurrentVelocityX;
    public int StepsUntilNextTarget;

    public override void AgentReset()
    {
        base.AgentReset();

        // set to true this to show monitor while training
        Monitor.SetActive(true);

        StepRewardFunction = StepRewardAssaultCourse001;
        TerminateFunction = TerminateOnNonFootHitTerrain;
        ObservationsFunction = ObservationsDefault;
        // OnTerminateRewardValue = -100f;

        // TerminateFunction = TerminateOnNonFootHitTerrain;
        // ObservationsFunction = ObservationsDefault;
        // OnEpisodeCompleteGetRewardFunction = GetRewardOnEpisodeComplete;


        BodyParts["pelvis"] = GetComponentsInChildren<Rigidbody>().FirstOrDefault(x=>x.name=="torso");
        BodyParts["foot"] = GetComponentsInChildren<Rigidbody>().FirstOrDefault(x=>x.name=="foot");
        base.SetupBodyParts();
        ManageTargetStep(true);

    }

    void ManageTargetStep(bool reset = false)
    {
        StepsUntilNextTarget--;
        if (StepsUntilNextTarget <= 0 || reset){
            StepsUntilNextTarget = 400;
            //TargetVelocityX = UnityEngine.Random.value *2f - 1f;
            if (TargetVelocityX == 0f)
                TargetVelocityX = (UnityEngine.Random.value >= .5f) ? 1f : -1f;
            else
                TargetVelocityX = TargetVelocityX == 1f ? -1f : 1f;
            // var rnd = UnityEngine.Random.value;
            //if (rnd>=.5f)
            //    TargetVelocityX = 1f;
            //else
            //    TargetVelocityX = -1f;

        }
    }


    public override void AgentOnDone()
    {

    }
    void ObservationsDefault()
    {
        if (ShowMonitor) {
        }
        var pelvis = BodyParts["pelvis"];
        AddVectorObs(pelvis.velocity);
        AddVectorObs(pelvis.transform.forward); // gyroscope 
        AddVectorObs(pelvis.transform.up);
        
        AddVectorObs(SensorIsInTouch);
        JointRotations.ForEach(x=>AddVectorObs(x));
        AddVectorObs(JointVelocity);
        var foot = BodyParts["foot"];
        AddVectorObs(foot.transform.position.y);

        AddVectorObs(TargetVelocityX);
        AddVectorObs(CurrentVelocityX);
    }

    float GetRewardOnEpisodeComplete()
    {
        return FocalPoint.transform.position.x;
    }

    float StepRewardAssaultCourse001()
    {
        // float heightPenality = GetHeightPenality(0.5f);
        float uprightBonus = GetForwardBonus("pelvis");
        CurrentVelocityX = GetAverageVelocity("pelvis");
        float velocityReward = 1f - (Mathf.Abs(TargetVelocityX - CurrentVelocityX) * 1.2f);
        velocityReward = Mathf.Clamp(velocityReward, -1f, 1f);
        float effort = GetEffort();
        // var effortPenality = 1e-2f * (float)effort;
        var effortPenality = 3e-1f * (float)effort;
        var jointsAtLimitPenality = GetJointsAtLimitPenality() * 4;

        var reward = velocityReward
            +uprightBonus
            // -heightPenality
            -effortPenality
            -jointsAtLimitPenality;
        if (ShowMonitor) {
            var hist = new []{reward, velocityReward, uprightBonus, -effortPenality, -jointsAtLimitPenality}.ToList();
            Monitor.Log("rewardHist", hist.ToArray());
        }
        ManageTargetStep();
        return reward;
    }
}
