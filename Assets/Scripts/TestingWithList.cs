using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class TestingWithList : MonoBehaviour
{
	[SerializeField] private bool useJobs;
	[SerializeField] private int totalUnitsToTest;
	
	[SerializeField] private int spawnWidth;
	[SerializeField] private int spawnLength;
	
	//create a group of bugs
	[SerializeField] private Transform pfBug;
	private List<Bug> BugList;
	
	public class Bug 
	{
		public Transform transform;
		public float moveX;
		public float moveZ;
	}


	private void Start()
	{
		
		//create our bug list with random spawn location (modified with spawn width/length)
		BugList = new List<Bug>();
		for (int i = 0; i < totalUnitsToTest; i++) {
			Transform bugTransform = Instantiate(pfBug, 
				new Vector3(UnityEngine.Random.Range(-spawnWidth,spawnWidth),
				0,
				UnityEngine.Random.Range(-spawnLength,spawnLength)),
				quaternion.identity);
				
			//add new instance to list	
			BugList.Add(new Bug 
			{
				transform = bugTransform,
				moveX = UnityEngine.Random.Range(1f,2f),
				moveZ = UnityEngine.Random.Range(1f,2f)
			});
		}
	}

	private void Update()
	{
		//float startTime = Time.realtimeSinceStartup;
		
		if(useJobs)
		{
			//create our arrays to use
			NativeArray<float> moveXArray = new NativeArray<float>(BugList.Count,Allocator.TempJob);
			NativeArray<float> moveZArray = new NativeArray<float>(BugList.Count,Allocator.TempJob);
			TransformAccessArray transformAccessArray = new TransformAccessArray(BugList.Count);
			
			//fill up our arrays with the current data
			for (int i = 0; i < BugList.Count; i++) {
				moveXArray[i] = BugList[i].moveX;
				moveZArray[i] = BugList[i].moveZ;
				//for transform
				transformAccessArray.Add(BugList[i].transform);
			}
	
			//create transform job
			BugParallelJobTransform bugParallelJobTransform = new BugParallelJobTransform 
			{
				deltaTime = Time.deltaTime,
				playerPosition = GameObject.FindGameObjectWithTag("ThePlayer").transform.position,
				moveXArray = moveXArray,
				moveZArray = moveZArray,
			};
			
			
			//schedule the transform job
			JobHandle transformHandle = bugParallelJobTransform.Schedule(transformAccessArray);
			
			//run the job
			transformHandle.Complete();

			//update initial values
			for (int i = 0; i < BugList.Count; i++) {
				//BugList[i].transform.position = positionArray[i];
				BugList[i].moveX = moveXArray[i];
				BugList[i].moveZ = moveZArray[i];
			}
			
			//dispose of arrays
			moveXArray.Dispose();
			moveZArray.Dispose();
			transformAccessArray.Dispose();
		}else
		{
			//test without jobs
			foreach(Bug bug in BugList)
			{
				bug.transform.position += new Vector3(bug.moveX*Time.deltaTime,0,bug.moveZ*Time.deltaTime);
			
				if(bug.transform.position.x > 5f)
				{
					bug.moveX = -math.abs(bug.moveX);
				}
				if(bug.transform.position.z > 5f)
				{
					bug.moveZ = +math.abs(bug.moveZ);
				}
				//Simulating an complex task
				for (int i = 0; i < totalUnitsToTest; i++) {
					ReallyToughTask();
				}
			}
		}
	}
	
	
	public void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(transform.position,new Vector3(spawnWidth,2,spawnLength));
	}
	private void ReallyToughTask()
	{
		//this simulates something tough like pathfinding
		float value = 0.0f;
		for (int i = 0; i < 50000; i++)
		{
			value = math.exp10(math.sqrt(value));
		}
	}
}
[BurstCompile]
public struct BugParallelJobTransform : IJobParallelForTransform
{
	//Code Monkey stuff

	public NativeArray<float> moveXArray;
	public NativeArray<float> moveZArray;
	public float deltaTime;
	public Vector3 playerPosition;
	
	
	public void Execute(int index, TransformAccess transform)
	{
		Vector3 direction = playerPosition - transform.position;
		float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
		
		Debug.DrawRay(transform.position,direction,Color.green);

		Quaternion angleAxis = Quaternion.AngleAxis(angle, Vector3.up);
		transform.rotation = Quaternion.Slerp(transform.rotation, angleAxis, deltaTime * 10);
		
		
		
		
		//Move towards player :):):):):)
		float speed = 0.02f;
		transform.position = Vector3.Lerp(transform.position,playerPosition,speed * deltaTime);

		/////////////////////////////////////////////////////
		//transform.position += new Vector3(moveXArray[index]*deltaTime,0,moveZArray[index]*deltaTime);
		//transform.
		//if(transform.position.x > 5f)
		//{
		//	moveXArray[index] = -math.abs(moveXArray[index]);
		//}
		//if(transform.position.z > 5f)
		//{
		//	moveZArray[index] = +math.abs(moveZArray[index]);
		//}
		///////////////////////////////////////////////////////
		
		//mimic an tough task 
		float value = 0.0f;
		for (int i = 0; i < 10000; i++)
		{
			value = math.exp10(math.sqrt(value));
		}
	}
}
