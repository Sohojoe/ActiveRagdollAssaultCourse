using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MujocoUnity;
using UnityEngine;
using MLAgents;

public class AssaultCourse004Agent : MujocoAgent {

    AssaultCourse004TerrainAgent _assaultCourse004TerrainAgent;
    int _lastXPosInMeters;
    public override void AgentReset()
    {
        base.AgentReset();

        if (_assaultCourse004TerrainAgent == null)
            _assaultCourse004TerrainAgent = GetComponent<AssaultCourse004TerrainAgent>();
        _lastXPosInMeters = (int) FocalPoint.transform.position.x;
        _assaultCourse004TerrainAgent.Terminate(false);

        // set to true this to show monitor while training
        Monitor.SetActive(true);

        StepRewardFunction = StepRewardHopper101;
        TerminateFunction = LocalTerminateOnNonFootHitTerrain;
        ObservationsFunction = ObservationsDefault;
        // OnTerminateRewardValue = -100f;

        BodyParts["pelvis"] = GetComponentsInChildren<Rigidbody>().FirstOrDefault(x=>x.name=="torso");
        BodyParts["foot"] = GetComponentsInChildren<Rigidbody>().FirstOrDefault(x=>x.name=="foot");
        base.SetupBodyParts();
    }

    bool LocalTerminateOnNonFootHitTerrain()
    {
        int newXPosInMeters = (int) FocalPoint.transform.position.x;
        if (newXPosInMeters != _lastXPosInMeters) {
            _assaultCourse004TerrainAgent.OnNextMeter();
            _lastXPosInMeters = newXPosInMeters;
        }

        var terminate = TerminateOnNonFootHitTerrain();
        if (terminate)
            _assaultCourse004TerrainAgent.Terminate(true);

        return terminate;
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

        var xpos = foot.transform.position.x;
        xpos -= 2f;
        float fraction = (xpos - (Mathf.Floor(xpos*5)/5)) * 5;
        List<Ray> rays = Enumerable.Range(0, 5*5).Select(x => new Ray(new Vector3(xpos+(x*.2f), 5f, 0f), Vector3.down)).ToList();
        List<float> distances = rays.Select
            ( x=>
                5f -  
                Physics.RaycastAll(x,10f)
                .OrderBy(y=>y.distance)
                .First()
                .distance
            ).ToList();
        if (Application.isEditor && ShowMonitor)
        {
            var view = distances.Skip(10).Take(20).Select(x=>x).ToList();
            Monitor.Log("distances", view.ToArray());
            var time = Time.deltaTime;
            time *= agentParameters.numberOfActionsBetweenDecisions;
            for (int i = 0; i < rays.Count; i++)
            {
                Debug.DrawRay(rays[i].origin, rays[i].direction*(5f-distances[i]), Color.yellow, time, true);
            }
        }        
        AddVectorObs(distances);
        AddVectorObs(fraction);
    }

    float StepRewardHopper101()
    {
        // float heightPenality = GetHeightPenality(0.5f);
        float uprightBonus = GetForwardBonus("pelvis");
        float velocity = GetVelocity("pelvis");
        float effort = GetEffort();
        // var effortPenality = 1e-2f * (float)effort;
        var effortPenality = 3e-1f * (float)effort;
        var jointsAtLimitPenality = GetJointsAtLimitPenality() * 4;

        //var uprightScaler = Mathf.Clamp(velocity,0,1);
        //uprightBonus *= 0f;//uprightScaler;

        var reward = velocity
            +uprightBonus
            // -heightPenality
            -effortPenality
            -jointsAtLimitPenality;
        if (ShowMonitor) {
            // var hist = new []{reward,velocity,uprightBonus,-heightPenality,-effortPenality}.ToList();
            var hist = new []{reward, velocity, uprightBonus, -effortPenality, -jointsAtLimitPenality}.ToList();
            Monitor.Log("rewardHist", hist.ToArray());
        }

        return reward;
    }
}
