using DeepLearning;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SIGGRAPH_2017 {
	public class BioAnimation_Simulation : MonoBehaviour {

		public bool Inspect = false;

		public float TargetBlending = 0.25f;
		public float GaitTransition = 0.25f;
		public float TrajectoryCorrection = 1f;

		public SimulationController controller;

		private Actor Actor;
		private PFNN NN;
		private Trajectory Trajectory;

		private Vector3 TargetDirection;
		private Vector3 TargetVelocity;

		//Rescaling for character (cm to m)
		private float UnitScale = 100f;

		//State
		private Vector3[] Positions = new Vector3[0];
		private Vector3[] Forwards = new Vector3[0];
		private Vector3[] Ups = new Vector3[0];
		private Vector3[] Velocities = new Vector3[0];

		//Trajectory for 60 Hz framerate
		private const int PointSamples = 12;
		private const int RootSampleIndex = 6;
		private const int RootPointIndex = 60;
		private const int FuturePoints = 5;
		private const int PreviousPoints = 6;
		private const int PointDensity = 10;

        public List<float[]> coordinates;
        public float[] coordsToSerialize;
        
        List<Transform> skeletonJoints;
        private float currentTimeStep;
        private int id = -1;
        private int indexHipJoint = 0; // J1 is the hip joint (root)

        private int index = 0;

        void Reset() {
			controller = new SimulationController();
		}

		void Awake() {
			Actor = GetComponent<Actor>();
			NN = GetComponent<PFNN>();
			TargetDirection = new Vector3(transform.forward.x, 0f, transform.forward.z);
			TargetVelocity = Vector3.zero;
			Positions = new Vector3[Actor.Bones.Length];
			Forwards = new Vector3[Actor.Bones.Length];
			Ups = new Vector3[Actor.Bones.Length];
			Velocities = new Vector3[Actor.Bones.Length];
			Trajectory = new Trajectory(111, controller.GetNames(), transform.position, TargetDirection);
			Trajectory.Postprocess();
			if(controller.Styles.Length > 0) {
				for(int i=0; i<Trajectory.Points.Length; i++) {
					Trajectory.Points[i].Styles[0] = 1f;
				}
			}
			for(int i=0; i<Actor.Bones.Length; i++) {
				Positions[i] = Actor.Bones[i].Transform.position;
				Forwards[i] = Actor.Bones[i].Transform.forward;
				Ups[i] = Actor.Bones[i].Transform.up;
				Velocities[i] = Vector3.zero;
			}

			if(NN.Parameters == null) {
				Debug.Log("No parameters saved.");
				return;
			}
			NN.LoadParameters();
            //Time.captureDeltaTime = 1.0f / 72.0f;
            controller.handler = GetComponent<AnimationInputHandlerFromSimulation>();
            controller.handler.allowed.Clear();
            controller.handler.allowed.Add(controller.Forward);
            controller.handler.allowed.Add(controller.Back);
            controller.handler.allowed.Add(controller.Left);
            controller.handler.allowed.Add(controller.Right);
            controller.handler.allowed.Add(controller.TurnLeft);
            controller.handler.allowed.Add(controller.TurnRight);
            
        }
        
		void Start() {
			Utility.SetFPS(60);
		}

        public void InitWithCSVData(int ID, float timestep)
        {
            id = ID;
            Transform[] children = transform.GetComponentsInChildren<Transform>();
            skeletonJoints = new List<Transform>(children);
            
            coordinates = new List<float[]>();
            coordsToSerialize = new float[GetComponent<AnimationInputHandlerFromSimulation>().timedPositions.Count * PositionSerializer.numberOfValuesPerFrame];

            currentTimeStep = timestep;
            
        }

        public float distanceToTarget(Vector2 target)
        {
            Vector3 hipPosition = skeletonJoints[indexHipJoint].transform.position;
            Vector2 hipPos = new Vector2(hipPosition.x, hipPosition.z);
            return (target - hipPos).magnitude;
        }

        public AnimationInputHandlerFromSimulation.TimedPosition GetRealTimedPosition()
        {
            Vector2 hip = new Vector2(skeletonJoints[indexHipJoint].transform.position.x, skeletonJoints[indexHipJoint].transform.position.z);
            List<AnimationInputHandlerFromSimulation.TimedPosition> timedPositionList = GetComponent<AnimationInputHandlerFromSimulation>().timedPositions;
            return timedPositionList.OrderBy(x => (hip - x.position).sqrMagnitude).First();
        }

        public Vector2 CalculateNewDirection(Vector2 target)
        {
            Vector2 hip = new Vector2(skeletonJoints[indexHipJoint].transform.position.x, skeletonJoints[indexHipJoint].transform.position.z);  
            return target - hip; //epsilon to check
        }

        public void FilterSerializedData()
        {
            // do a one to one comparison between data from timedPositions and serialized data
            AnimationInputHandlerFromSimulation component = GetComponent<AnimationInputHandlerFromSimulation>();
            
            int toIndex = 0;
            int initialIndexFromCoordinates = 0;
            int coordinateToSerializeIndex = 0;
            for (int i =0; i < component.timedPositions.Count(); ++i)
            {
                AnimationInputHandlerFromSimulation.TimedPosition fromCSV = component.timedPositions[i];
                float time = fromCSV.time;
                Vector2 pos = fromCSV.position;
                int fromIndex = -1;
                float distance = float.MaxValue;

                //

                /*if (coordinateToSerializeIndex != coordsToSerialize.Length)
                {
                    if(initialIndexFromCoordinates >= coordinates.Count)
                    {
                        initialIndexFromCoordinates = coordinates.Count - PositionSerializer.jointsNumbers;
                    }
                }*/

                for (int index= initialIndexFromCoordinates; index < coordinates.Count; index+= PositionSerializer.jointsNumbers) //the first is the hip (skeleton)
                {
                    Vector2 hipPos = new Vector2(coordinates[index][2], coordinates[index][4]);
                    if((pos-hipPos).sqrMagnitude < distance)
                    {
                        distance = (pos - hipPos).sqrMagnitude;
                        fromIndex = index;
                    }
                }
                    
                toIndex = fromIndex + PositionSerializer.jointsNumbers;
                    
                for (int j = fromIndex; j < toIndex && j >= 0; j++)
                {
                    //coordinates[j][1] = time;
                    
                    //Debug.Log(j.ToString());
                    coordsToSerialize[coordinateToSerializeIndex] = time;
                    coordsToSerialize[coordinateToSerializeIndex + 1] = id;
                    coordsToSerialize[coordinateToSerializeIndex + 2] = coordinates[j][2];
                    coordsToSerialize[coordinateToSerializeIndex + 3] = coordinates[j][3];
                    coordsToSerialize[coordinateToSerializeIndex + 4] = coordinates[j][4];
                    coordinateToSerializeIndex += (PositionSerializer.timeAndIndex + PositionSerializer.positionCoord);
                }
                initialIndexFromCoordinates = fromIndex; //conservative (toindex)
            }
        }

        public void Serialize()
        {
            // ask to AnimationInputHandlerFromSimulation the timedposition and consider the next one to do, 
            // grab the Vector and control the hip position and th next position, dynamic is always the same but if hip position and target are below a distance 
            // threshold the position is serialized and the time is the currentTimeStep.
            // if yes save the value, otherwise NaN
            int indexJoint = 0;
            foreach (Transform sj in skeletonJoints)
            {
                float[] values = new float[5];
                values[0] = index;
                values[1] = indexJoint; //set the sequence index, GetComponent<AnimationInputHandlerFromSimulation>().timedPositions[currentIndex].time
                values[2] = sj.position.x;
                values[3] = sj.position.y;
                values[4] = sj.position.z;
                coordinates.Add(values);
                indexJoint++;
            }
            index++;
        }

        void Update() {
            if (SimulationManager.status != SimulationManager.STATUS.RECORD) return;
            Time.captureDeltaTime = PositionSerializer.framerate;

            if (NN.Parameters == null) {
				return;
			}

            GetComponent<AnimationInputHandlerFromSimulation>().SetStartOn();
            if (GetComponent<AnimationInputHandlerFromSimulation>().isArrived())
            {
                this.gameObject.active = false;
                return;
            }

            //Update Target Direction / Velocity
            var turn = controller.QueryTurn();
            var move = controller.QueryMove();

            //TargetDirection = Vector3.Lerp(TargetDirection, Quaternion.AngleAxis(turn * 60f, Vector3.up) * Trajectory.Points[RootPointIndex].GetDirection(), TargetBlending);
            TargetDirection = Vector3.Lerp(TargetDirection, Quaternion.AngleAxis(turn, Vector3.up) * Trajectory.Points[RootPointIndex].GetDirection(), TargetBlending);
            TargetVelocity = Vector3.Lerp(TargetVelocity, (Quaternion.LookRotation(TargetDirection, Vector3.up) * move).normalized, TargetBlending);
            
            //Debug.Log(TargetVelocity.magnitude);

            //Update Gait
            for (int i=0; i<controller.Styles.Length; i++) {
				Trajectory.Points[RootPointIndex].Styles[i] = Utility.Interpolate(Trajectory.Points[RootPointIndex].Styles[i], controller.Styles[i].Query(controller.handler) ? 1f : 0f, GaitTransition);
			}
			//For Human Only
			//Trajectory.Points[RootPointIndex].Styles[0] = Utility.Interpolate(Trajectory.Points[RootPointIndex].Styles[0], 1.0f - Mathf.Clamp(Vector3.Magnitude(TargetVelocity) / 0.1f, 0.0f, 1.0f), GaitTransition);
			//Trajectory.Points[RootPointIndex].Styles[1] = Mathf.Max(Trajectory.Points[RootPointIndex].Styles[1] - Trajectory.Points[RootPointIndex].Styles[2], 0f);
			//

			/*
			//Blend Trajectory Offset
			Vector3 positionOffset = transform.position - Trajectory.Points[RootPointIndex].GetPosition();
			Quaternion rotationOffset = Quaternion.Inverse(Trajectory.Points[RootPointIndex].GetRotation()) * transform.rotation;
			Trajectory.Points[RootPointIndex].SetPosition(Trajectory.Points[RootPointIndex].GetPosition() + positionOffset);
			Trajectory.Points[RootPointIndex].SetDirection(rotationOffset * Trajectory.Points[RootPointIndex].GetDirection());

			for(int i=RootPointIndex; i<Trajectory.Points.Length; i++) {
				float factor = 1f - (i - RootPointIndex)/(RootPointIndex - 1f);
				Trajectory.Points[i].SetPosition(Trajectory.Points[i].GetPosition() + factor*positionOffset);
			}
			*/

			//Predict Future Trajectory
			Vector3[] trajectory_positions_blend = new Vector3[Trajectory.Points.Length];
			trajectory_positions_blend[RootPointIndex] = Trajectory.Points[RootPointIndex].GetPosition();

			for(int i=RootPointIndex+1; i<Trajectory.Points.Length; i++) {
				float bias_pos = 0.75f;
				float bias_dir = 1.25f;
				float scale_pos = (1.0f - Mathf.Pow(1.0f - ((float)(i - RootPointIndex) / (RootPointIndex)), bias_pos));
				float scale_dir = (1.0f - Mathf.Pow(1.0f - ((float)(i - RootPointIndex) / (RootPointIndex)), bias_dir));
				float vel_boost = PoolBias();

				float rescale = 1f / (Trajectory.Points.Length - (RootPointIndex + 1f));

				trajectory_positions_blend[i] = trajectory_positions_blend[i-1] + Vector3.Lerp(
					Trajectory.Points[i].GetPosition() - Trajectory.Points[i-1].GetPosition(), 
					vel_boost * rescale * TargetVelocity,
					scale_pos);

				Trajectory.Points[i].SetDirection(Vector3.Lerp(Trajectory.Points[i].GetDirection(), TargetDirection, scale_dir));

				for(int j=0; j<Trajectory.Points[i].Styles.Length; j++) {
					Trajectory.Points[i].Styles[j] = Trajectory.Points[RootPointIndex].Styles[j];
				}
			}
			
			for(int i=RootPointIndex+1; i<Trajectory.Points.Length; i++) {
				Trajectory.Points[i].SetPosition(trajectory_positions_blend[i]);
			}

			for(int i=RootPointIndex; i<Trajectory.Points.Length; i+=PointDensity) {
				Trajectory.Points[i].Postprocess();
			}

			for(int i=RootPointIndex+1; i<Trajectory.Points.Length; i++) {
				//ROOT	1		2		3		4		5
				//.x....x.......x.......x.......x.......x
				Trajectory.Point prev = GetPreviousSample(i);
				Trajectory.Point next = GetNextSample(i);
				float factor = (float)(i % PointDensity) / PointDensity;

				Trajectory.Points[i].SetPosition((1f-factor)*prev.GetPosition() + factor*next.GetPosition());
				Trajectory.Points[i].SetDirection((1f-factor)*prev.GetDirection() + factor*next.GetDirection());
				Trajectory.Points[i].SetLeftsample((1f-factor)*prev.GetLeftSample() + factor*next.GetLeftSample());
				Trajectory.Points[i].SetRightSample((1f-factor)*prev.GetRightSample() + factor*next.GetRightSample());
				Trajectory.Points[i].SetSlope((1f-factor)*prev.GetSlope() + factor*next.GetSlope());
			}

			//Avoid Collisions
			CollisionChecks(RootPointIndex+1);

			if(NN.Parameters != null) {
				//Calculate Root
				Matrix4x4 currentRoot = Trajectory.Points[RootPointIndex].GetTransformation();
				Matrix4x4 previousRoot = Trajectory.Points[RootPointIndex-1].GetTransformation();
					
				//Input Trajectory Positions / Directions
				for(int i=0; i<PointSamples; i++) {
					Vector3 pos = Trajectory.Points[i*PointDensity].GetPosition().GetRelativePositionTo(currentRoot);
					Vector3 dir = Trajectory.Points[i*PointDensity].GetDirection().GetRelativeDirectionTo(currentRoot);
					NN.SetInput(PointSamples*0 + i, UnitScale * pos.x);
					NN.SetInput(PointSamples*1 + i, UnitScale * pos.z);
					NN.SetInput(PointSamples*2 + i, dir.x);
					NN.SetInput(PointSamples*3 + i, dir.z);
				}

				//Input Trajectory Gaits
				for (int i=0; i<PointSamples; i++) {
					for(int j=0; j<Trajectory.Points[i*PointDensity].Styles.Length; j++) {
						NN.SetInput(PointSamples*(4+j) + i, Trajectory.Points[i*PointDensity].Styles[j]);
					}
					//FOR HUMAN ONLY
					NN.SetInput(PointSamples*8 + i, Trajectory.Points[i*PointDensity].GetSlope());
					//
				}

				//Input Previous Bone Positions / Velocities
				for(int i=0; i<Actor.Bones.Length; i++) {
					int o = 10*PointSamples;
					Vector3 pos = Positions[i].GetRelativePositionTo(previousRoot);
					Vector3 vel = Velocities[i].GetRelativeDirectionTo(previousRoot);
					NN.SetInput(o + Actor.Bones.Length*3*0 + i*3+0, UnitScale * pos.x);
					NN.SetInput(o + Actor.Bones.Length*3*0 + i*3+1, UnitScale * pos.y);
					NN.SetInput(o + Actor.Bones.Length*3*0 + i*3+2, UnitScale * pos.z);
					NN.SetInput(o + Actor.Bones.Length*3*1 + i*3+0, UnitScale * vel.x);
					NN.SetInput(o + Actor.Bones.Length*3*1 + i*3+1, UnitScale * vel.y);
					NN.SetInput(o + Actor.Bones.Length*3*1 + i*3+2, UnitScale * vel.z);
				}

				//Input Trajectory Heights
				for(int i=0; i<PointSamples; i++) {
					int o = 10*PointSamples + Actor.Bones.Length*3*2;
					NN.SetInput(o + PointSamples*0 + i, UnitScale * (Trajectory.Points[i*PointDensity].GetRightSample().y - currentRoot.GetPosition().y));
					NN.SetInput(o + PointSamples*1 + i, UnitScale * (Trajectory.Points[i*PointDensity].GetPosition().y - currentRoot.GetPosition().y));
					NN.SetInput(o + PointSamples*2 + i, UnitScale * (Trajectory.Points[i*PointDensity].GetLeftSample().y - currentRoot.GetPosition().y));
				}

				//Predict
				float rest = Mathf.Pow(1.0f-Trajectory.Points[RootPointIndex].Styles[0], 0.25f);
				NN.SetDamping(1f - (rest * 0.9f + 0.1f));
				NN.Predict();

				//Update Past Trajectory
				for(int i=0; i<RootPointIndex; i++) {
					Trajectory.Points[i].SetPosition(Trajectory.Points[i+1].GetPosition());
					Trajectory.Points[i].SetDirection(Trajectory.Points[i+1].GetDirection());
					Trajectory.Points[i].SetLeftsample(Trajectory.Points[i+1].GetLeftSample());
					Trajectory.Points[i].SetRightSample(Trajectory.Points[i+1].GetRightSample());
					Trajectory.Points[i].SetSlope(Trajectory.Points[i+1].GetSlope());
					for(int j=0; j<Trajectory.Points[i].Styles.Length; j++) {
						Trajectory.Points[i].Styles[j] = Trajectory.Points[i+1].Styles[j];
					}
				}

				//Update Current Trajectory
				Trajectory.Points[RootPointIndex].SetPosition((rest * new Vector3(NN.GetOutput(0) / UnitScale, 0f, NN.GetOutput(1) / UnitScale)).GetRelativePositionFrom(currentRoot));
				Trajectory.Points[RootPointIndex].SetDirection(Quaternion.AngleAxis(rest * Mathf.Rad2Deg * (-NN.GetOutput(2)), Vector3.up) * Trajectory.Points[RootPointIndex].GetDirection());
				Trajectory.Points[RootPointIndex].Postprocess();
				Matrix4x4 nextRoot = Trajectory.Points[RootPointIndex].GetTransformation();

				//Update Future Trajectory
				for(int i=RootPointIndex+1; i<Trajectory.Points.Length; i++) {
					Trajectory.Points[i].SetPosition(Trajectory.Points[i].GetPosition() + (rest * new Vector3(NN.GetOutput(0) / UnitScale, 0f, NN.GetOutput(1) / UnitScale)).GetRelativeDirectionFrom(nextRoot));
				}
				for(int i=RootPointIndex+1; i<Trajectory.Points.Length; i++) {
					int w = RootSampleIndex;
					float m = Mathf.Repeat(((float)i - (float)RootPointIndex) / (float)PointDensity, 1.0f);
					float posX = (1-m) * NN.GetOutput(8+(w*0)+(i/PointDensity)-w) + m * NN.GetOutput(8+(w*0)+(i/PointDensity)-w+1);
					float posZ = (1-m) * NN.GetOutput(8+(w*1)+(i/PointDensity)-w) + m * NN.GetOutput(8+(w*1)+(i/PointDensity)-w+1);
					float dirX = (1-m) * NN.GetOutput(8+(w*2)+(i/PointDensity)-w) + m * NN.GetOutput(8+(w*2)+(i/PointDensity)-w+1);
					float dirZ = (1-m) * NN.GetOutput(8+(w*3)+(i/PointDensity)-w) + m * NN.GetOutput(8+(w*3)+(i/PointDensity)-w+1);
					Trajectory.Points[i].SetPosition(
						Utility.Interpolate(
							Trajectory.Points[i].GetPosition(),
							new Vector3(posX / UnitScale, 0f, posZ / UnitScale).GetRelativePositionFrom(nextRoot),
							TrajectoryCorrection
							)
						);
					Trajectory.Points[i].SetDirection(
						Utility.Interpolate(
							Trajectory.Points[i].GetDirection(),
							new Vector3(dirX, 0f, dirZ).normalized.GetRelativeDirectionFrom(nextRoot),
							TrajectoryCorrection
							)
						);
				}

				for(int i=RootPointIndex+PointDensity; i<Trajectory.Points.Length; i+=PointDensity) {
					Trajectory.Points[i].Postprocess();
				}

				for(int i=RootPointIndex+1; i<Trajectory.Points.Length; i++) {
					//ROOT	1		2		3		4		5
					//.x....x.......x.......x.......x.......x
					Trajectory.Point prev = GetPreviousSample(i);
					Trajectory.Point next = GetNextSample(i);
					float factor = (float)(i % PointDensity) / PointDensity;

					Trajectory.Points[i].SetPosition((1f-factor)*prev.GetPosition() + factor*next.GetPosition());
					Trajectory.Points[i].SetDirection((1f-factor)*prev.GetDirection() + factor*next.GetDirection());
					Trajectory.Points[i].SetLeftsample((1f-factor)*prev.GetLeftSample() + factor*next.GetLeftSample());
					Trajectory.Points[i].SetRightSample((1f-factor)*prev.GetRightSample() + factor*next.GetRightSample());
					Trajectory.Points[i].SetSlope((1f-factor)*prev.GetSlope() + factor*next.GetSlope());
				}

				//Avoid Collisions
				CollisionChecks(RootPointIndex);
				
				//Compute Posture
				int opos = 8 + 4*RootSampleIndex + Actor.Bones.Length*3*0;
				int ovel = 8 + 4*RootSampleIndex + Actor.Bones.Length*3*1;
				//int orot = 8 + 4*RootSampleIndex + Actor.Bones.Length*3*2;
				for(int i=0; i<Actor.Bones.Length; i++) {			
					Vector3 position = new Vector3(NN.GetOutput(opos+i*3+0), NN.GetOutput(opos+i*3+1), NN.GetOutput(opos+i*3+2)) / UnitScale;
					Vector3 velocity = new Vector3(NN.GetOutput(ovel+i*3+0), NN.GetOutput(ovel+i*3+1), NN.GetOutput(ovel+i*3+2)) / UnitScale;
					//Quaternion rotation = new Quaternion(PFNN.GetOutput(orot+i*3+0), PFNN.GetOutput(orot+i*3+1), PFNN.GetOutput(orot+i*3+2), 0f).Exp();
					Positions[i] = Vector3.Lerp(Positions[i].GetRelativePositionTo(currentRoot) + velocity, position, 0.5f).GetRelativePositionFrom(currentRoot);
					Velocities[i] = velocity.GetRelativeDirectionFrom(currentRoot);
					//rotations[i] = rotation.GetRelativeRotationFrom(currentRoot);
				}
				
				//Update Posture
				transform.position = nextRoot.GetPosition();
				transform.rotation = nextRoot.GetRotation();
				for(int i=0; i<Actor.Bones.Length; i++) {
					Actor.Bones[i].Transform.position = Positions[i];
					Actor.Bones[i].Transform.rotation = Quaternion.LookRotation(Forwards[i], Ups[i]);
				}
			}
		}

		private float PoolBias() {
			float[] styles = Trajectory.Points[RootPointIndex].Styles;
			float bias = 0f;
			for(int i=0; i<styles.Length; i++) {
				float _bias = controller.Styles[i].Bias;
				float max = 0f;
				for(int j=0; j<controller.Styles[i].Multipliers.Length; j++) {
					if(Input.GetKey(controller.Styles[i].Multipliers[j].Key)) {
						max = Mathf.Max(max, controller.Styles[i].Bias * controller.Styles[i].Multipliers[j].Value);
					}
				}
				for(int j=0; j<controller.Styles[i].Multipliers.Length; j++) {
					if(Input.GetKey(controller.Styles[i].Multipliers[j].Key)) {
						_bias = Mathf.Min(max, _bias * controller.Styles[i].Multipliers[j].Value);
					}
				}
				bias += styles[i] * _bias;
			}
			return bias;
		}

		private Trajectory.Point GetSample(int index) {
			return Trajectory.Points[Mathf.Clamp(index*10, 0, Trajectory.Points.Length-1)];
		}

		private Trajectory.Point GetPreviousSample(int index) {
			return GetSample(index / 10);
		}

		private Trajectory.Point GetNextSample(int index) {
			if(index % 10 == 0) {
				return GetSample(index / 10);
			} else {
				return GetSample(index / 10 + 1);
			}
		}

		private void CollisionChecks(int start) {
			for(int i=start; i<Trajectory.Points.Length; i++) {
				float safety = 0.5f;
				Vector3 previousPos = Trajectory.Points[i-1].GetPosition();
				Vector3 currentPos = Trajectory.Points[i].GetPosition();
				Vector3 testPos = previousPos + safety*(currentPos-previousPos).normalized;
				Vector3 projectedPos = Utility.ProjectCollision(previousPos, testPos, LayerMask.GetMask("Obstacles"));
				if(testPos != projectedPos) {
					Vector3 correctedPos = testPos + safety * (previousPos-testPos).normalized;
					Trajectory.Points[i].SetPosition(correctedPos);
				}
			}
		}

		void OnGUI() {
			GUI.color = UltiDraw.Mustard;
			GUI.backgroundColor = UltiDraw.Black;
			float height = 0.05f;
			GUI.Box(Utility.GetGUIRect(0.025f, 0.05f, 0.3f, controller.Styles.Length*height), "");
			for(int i=0; i<controller.Styles.Length; i++) {
				GUI.Label(Utility.GetGUIRect(0.05f, 0.075f + i*0.05f, 0.025f, height), controller.Styles[i].Name);
				string keys = string.Empty;
				for(int j=0; j<controller.Styles[i].Keys.Length; j++) {
					keys += controller.Styles[i].Keys[j].ToString() + " ";
				}
				GUI.Label(Utility.GetGUIRect(0.075f, 0.075f + i*0.05f, 0.05f, height), keys);
				GUI.HorizontalSlider(Utility.GetGUIRect(0.125f, 0.075f + i*0.05f, 0.15f, height), Trajectory.Points[RootPointIndex].Styles[i], 0f, 1f);
			}
		}

		void OnRenderObject() {
			/*
			UltiDraw.Begin();
			UltiDraw.DrawGUICircle(new Vector2(0.5f, 0.85f), 0.075f, UltiDraw.Black.Transparent(0.5f));
			Quaternion rotation = Quaternion.AngleAxis(-360f * NN.GetPhase() / (2f * Mathf.PI), Vector3.forward);
			Vector2 a = rotation * new Vector2(-0.005f, 0f);
			Vector2 b = rotation *new Vector3(0.005f, 0f);
			Vector3 c = rotation * new Vector3(0f, 0.075f);
			UltiDraw.DrawGUITriangle(new Vector2(0.5f + b.x/Screen.width*Screen.height, 0.85f + b.y), new Vector2(0.5f + a.x/Screen.width*Screen.height, 0.85f + a.y), new Vector2(0.5f + c.x/Screen.width*Screen.height, 0.85f + c.y), UltiDraw.Cyan);
			UltiDraw.End();
			*/

			if(Application.isPlaying) {
				if(NN.Parameters == null) {
					return;
				}

				UltiDraw.Begin();
				UltiDraw.DrawLine(Trajectory.Points[RootPointIndex].GetPosition(), Trajectory.Points[RootPointIndex].GetPosition() + TargetDirection, 0.05f, 0f, UltiDraw.Red.Transparent(0.75f));
				UltiDraw.DrawLine(Trajectory.Points[RootPointIndex].GetPosition(), Trajectory.Points[RootPointIndex].GetPosition() + TargetVelocity, 0.05f, 0f, UltiDraw.Green.Transparent(0.75f));
				UltiDraw.End();
				Trajectory.Draw(10);
				
				UltiDraw.Begin();
				for(int i=0; i<Actor.Bones.Length; i++) {
					UltiDraw.DrawArrow(
						Actor.Bones[i].Transform.position,
						Actor.Bones[i].Transform.position + Velocities[i],
						0.75f,
						0.0075f,
						0.05f,
						UltiDraw.Purple.Transparent(0.5f)
					);
				}
				UltiDraw.End();
			}
		}

		void OnDrawGizmos() {
			if(!Application.isPlaying) {
				OnRenderObject();
			}
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(BioAnimation_Simulation))]
	public class BioAnimation_Simulation_Editor : Editor {

			public BioAnimation_Simulation Target;

			void Awake() {
				Target = (BioAnimation_Simulation)target;
			}

			public override void OnInspectorGUI() {
				Undo.RecordObject(Target, Target.name);

				Inspector();
				Target.controller.Inspector();

				if(GUI.changed) {
					EditorUtility.SetDirty(Target);
				}
			}

			private void Inspector() {
				Utility.SetGUIColor(UltiDraw.Grey);
				using(new EditorGUILayout.VerticalScope ("Box")) {
					Utility.ResetGUIColor();

					if(Utility.GUIButton("Animation", UltiDraw.DarkGrey, UltiDraw.White)) {
						Target.Inspect = !Target.Inspect;
					}
					
					if(Target.Inspect) {
						using(new EditorGUILayout.VerticalScope ("Box")) {
							Target.TargetBlending = EditorGUILayout.Slider("Target Blending", Target.TargetBlending, 0f, 1f);
							Target.GaitTransition = EditorGUILayout.Slider("Gait Transition", Target.GaitTransition, 0f, 1f);
							Target.TrajectoryCorrection = EditorGUILayout.Slider("Trajectory Correction", Target.TrajectoryCorrection, 0f, 1f);
						}
					}
					
				}
			}
	}
	#endif
}