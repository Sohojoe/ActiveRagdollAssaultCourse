using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MujocoUnity;
using UnityEngine;
using MLAgents;

public class AssaultCourse003Agent : MujocoAgent {

    Terrain terrain;
    public override void AgentReset()
    {
        base.AgentReset();

        // set to true this to show monitor while training
        Monitor.SetActive(true);

        StepRewardFunction = StepRewardHopper101;
        TerminateFunction = TerminateOnNonFootHitTerrain;
        ObservationsFunction = ObservationsDefault;
        // OnTerminateRewardValue = -100f;

        BodyParts["pelvis"] = GetComponentsInChildren<Rigidbody>().FirstOrDefault(x=>x.name=="torso");
        BodyParts["foot"] = GetComponentsInChildren<Rigidbody>().FirstOrDefault(x=>x.name=="foot");
        base.SetupBodyParts();
        if (this.terrain == null)
            this.terrain = Terrain.activeTerrain;
        print($"HeightMap {this.terrain.terrainData.heightmapWidth}, {this.terrain.terrainData.heightmapHeight}. Scale {this.terrain.terrainData.heightmapScale}. Resolution {this.terrain.terrainData.heightmapResolution}");

        // get the normalized position of this game object relative to the terrain
        Vector3 tempCoord = (transform.position - terrain.gameObject.transform.position);
        Vector3 coord;
        coord.x = tempCoord.x / terrain.terrainData.size.x;
        coord.y = tempCoord.y / terrain.terrainData.size.y;
        coord.z = tempCoord.z / terrain.terrainData.size.z;
        // get the position of the terrain heightmap where this game object is
        int posXInTerrain = (int) (coord.x * terrain.terrainData.heightmapWidth); 
        int posYInTerrain = (int) (coord.z * terrain.terrainData.heightmapHeight);
        // we set an offset so that all the raising terrain is under this game object
        int offset = 0 / 2;
        // get the heights of the terrain under this game object
        float[,] heights = terrain.terrainData.GetHeights(posXInTerrain-offset,posYInTerrain-offset, 100,1);
        
        // set the new height
        terrain.terrainData.SetHeights(posXInTerrain-offset,posYInTerrain-offset,heights);
        heights[0,0] = 0f;//.1f/600f;
        this.terrain.terrainData.SetHeights(posXInTerrain, posYInTerrain, heights);
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
        List<Ray> rays = Enumerable.Range(0, 50).Select(x => new Ray(new Vector3(xpos+(x*.2f), 5f, 0f), Vector3.down)).ToList();
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
                Debug.DrawRay(rays[i].origin, rays[i].direction*distances[i], Color.yellow, time, true);
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
