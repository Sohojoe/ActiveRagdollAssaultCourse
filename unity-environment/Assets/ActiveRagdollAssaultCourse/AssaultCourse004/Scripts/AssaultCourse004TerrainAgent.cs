﻿using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class AssaultCourse004TerrainAgent : Agent {

    Terrain terrain;
	int lastSteps;
	AssaultCourse004Agent _assaultCourse004Agent;
	AssaultCourse004WalkerAgent _assaultCourse004WalkerAgent;


	int _posXInTerrain;
	int _posYInTerrain;
	float[,] _heights;

	int _heightIndex;
	float _curHeight;
	float _actionReward;

	internal const float _maxHeight = 10f;
	const float _midHeight = 5f;
	float _mapScaleY;

	public override void AgentReset()
	{
		// get start position
        if (this.terrain == null)
            this.terrain = Terrain.activeTerrain;
		if (this._assaultCourse004Agent == null)
			_assaultCourse004Agent = GetComponent<AssaultCourse004Agent>();
		if (this._assaultCourse004WalkerAgent == null)
			_assaultCourse004WalkerAgent = GetComponent<AssaultCourse004WalkerAgent>();
        //print($"HeightMap {this.terrain.terrainData.heightmapWidth}, {this.terrain.terrainData.heightmapHeight}. Scale {this.terrain.terrainData.heightmapScale}. Resolution {this.terrain.terrainData.heightmapResolution}");
        _mapScaleY = this.terrain.terrainData.heightmapScale.y;
		// get the normalized position of this game object relative to the terrain
        Vector3 tempCoord = (transform.position - terrain.gameObject.transform.position);
        Vector3 coord;
        coord.x = (tempCoord.x-1) / terrain.terrainData.size.x;
        coord.y = tempCoord.y / terrain.terrainData.size.y;
        coord.z = tempCoord.z / terrain.terrainData.size.z;
        // get the position of the terrain heightmap where this game object is
        _posXInTerrain = (int) (coord.x * terrain.terrainData.heightmapWidth); 
        _posYInTerrain = (int) (coord.z * terrain.terrainData.heightmapHeight);
        // we set an offset so that all the raising terrain is under this game object
        int offset = 0 / 2;
        // get the heights of the terrain under this game object
        _heights = terrain.terrainData.GetHeights(_posXInTerrain-offset,_posYInTerrain-offset, 100,1);
		_curHeight = _midHeight;
		_heightIndex = 0;
		_actionReward = 0f;

        
        // set the new height
        // terrain.terrainData.SetHeights(_posXInTerrain-offset,_posYInTerrain-offset,_heights);
        //_heights[0,0] = 0.1f/600f;
        //this.terrain.terrainData.SetHeights(_posXInTerrain, _posYInTerrain, _heights);
		ResetHeights();
		SetNextHeight(0);
		SetNextHeight(0);
		SetNextHeight(0);
		SetNextHeight(0);
		SetNextHeight(0);
		SetNextHeight(0);

		lastSteps = 0;
		//RequestDecision();
	}

	void ResetHeights()
	{
		for (int i = 0; i < _heights.Length; i++)
		{
			_heights[0,i] = _midHeight / _mapScaleY;
		}
        this.terrain.terrainData.SetHeights(_posXInTerrain, _posYInTerrain, _heights);
        this.terrain.terrainData.SetHeights(_posXInTerrain, _posYInTerrain+1, _heights);
	}

	void SetNextHeight(int action)
	{
		float actionSize = 0f;
		bool actionPos = (action-1) % 2 == 0;
		if (action != 0)
		{
			actionSize = ((float)((action+1)/2)) * 0.1f;
			_curHeight += actionPos ? actionSize : -actionSize;
			if (_curHeight < 0) {
				_curHeight = 0f;
				actionSize = 0;
				AddReward(-1f);
			}
			if (_curHeight > _maxHeight)
			{
				_curHeight = _maxHeight;
				actionSize = 0;
				AddReward(-1f);				
			}
		}

		_heights[0,_heightIndex] = _curHeight / _mapScaleY;
        this.terrain.terrainData.SetHeights(_posXInTerrain, _posYInTerrain, _heights);
        this.terrain.terrainData.SetHeights(_posXInTerrain, _posYInTerrain+1, _heights);

		_heightIndex++;
		_actionReward = actionSize;
		// if (_actionReward != 0 && actionPos)
		// 	_actionReward *= 2f; // boost reward for uphill
	}
	internal void OnNextMeter()
	{
		// AddReward(1);
		// AddReward(_actionReward);
		_actionReward = 0f;
		RequestDecision();
	}
	internal void Terminate(float cumulativeReward)
	{
		if (this.IsDone())
			return;
		var maxReward = 1000f;
		var agentReward = cumulativeReward;
		agentReward = Mathf.Clamp(agentReward, 0f, maxReward);
		var adverseralReward = maxReward - agentReward;
		AddReward(adverseralReward);
		Done();
	}

	public override void CollectObservations()
	{
		var height = _curHeight / _maxHeight;
		// add last agent distance
		
		int curSteps = 0;
		if (_assaultCourse004Agent != null)
			curSteps = _assaultCourse004Agent.GetStepCount();
		else if (_assaultCourse004WalkerAgent != null)
			curSteps = _assaultCourse004WalkerAgent.GetStepCount();
		var numberSinceLast = curSteps - lastSteps;
		numberSinceLast = 1 - (numberSinceLast/1000);
		lastSteps = curSteps;
        AddVectorObs(numberSinceLast);
        AddVectorObs(height);
        AddVectorObs(_actionReward);
	}
	public override void AgentAction(float[] vectorAction, string textAction)
	{
		// each action is a descreate for height change
		int action = (int)vectorAction[0];
		SetNextHeight(action);
	}

}
