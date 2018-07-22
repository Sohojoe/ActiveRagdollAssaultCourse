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

        BodyParts["pelvis"] = GetComponentsInChildren<Rigidbody>().FirstOrDefault(x=>x.name=="torso");
        BodyParts["foot"] = GetComponentsInChildren<Rigidbody>().FirstOrDefault(x=>x.name=="foot");

        if (_assaultCourse004TerrainAgent == null)
            _assaultCourse004TerrainAgent = GetComponent<AssaultCourse004TerrainAgent>();
        _lastXPosInMeters = (int) (int) BodyParts["foot"].transform.position.x;
        _assaultCourse004TerrainAgent.Terminate(false);

        // set to true this to show monitor while training
        Monitor.SetActive(true);

        StepRewardFunction = StepRewardHopper101;
        TerminateFunction = LocalTerminateOnNonFootHitTerrain;
        ObservationsFunction = ObservationsDefault;
        // OnTerminateRewardValue = -100f;

        base.SetupBodyParts();
    }

    bool LocalTerminateOnNonFootHitTerrain()
    {
        int newXPosInMeters = (int) BodyParts["foot"].transform.position.x;
        if (newXPosInMeters > _lastXPosInMeters) {
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
        float ypos = foot.transform.position.y;
        List<Ray> rays = Enumerable.Range(0, 5*5).Select(x => new Ray(new Vector3(xpos+(x*.2f), AssaultCourse004TerrainAgent._maxHeight, 0f), Vector3.down)).ToList();
        List<float> distances = rays.Select
            ( x=>
                ypos - (AssaultCourse004TerrainAgent._maxHeight - 
                Physics.RaycastAll(x)
                .OrderBy(y=>y.distance)
                .FirstOrDefault()
                .distance)
            ).ToList();
        if (Application.isEditor && ShowMonitor)
        {
            var view = distances.Skip(10).Take(20).Select(x=>x).ToList();
            Monitor.Log("distances", view.ToArray());
            var time = Time.deltaTime;
            time *= agentParameters.numberOfActionsBetweenDecisions;
            for (int i = 0; i < rays.Count; i++)
            {
                var distance = distances[i];
                var origin = new Vector3(rays[i].origin.x, ypos,0f);
                var direction = distance > 0 ? Vector3.down : Vector3.up;
                var color = distance > 0 ? Color.yellow : Color.red;
                Debug.DrawRay(origin, direction*Mathf.Abs(distance), color, time, false);
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
