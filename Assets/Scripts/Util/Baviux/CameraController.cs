using UnityEngine;
using System.Collections;

/**

	IMPORTANT: In order to avoid "stutter" when following the player, if it's moved using a RigidBody,
	           you have to enable interpolation on it: https://docs.unity3d.com/ScriptReference/Rigidbody-interpolation.html
			   Also, enable interpolation for RigidBodies that move along the player (i.e.: enenimes) for avoid "stutter" effect.

**/

namespace Baviux {

[ExecuteInEditMode]
public class CameraController : MonoBehaviour {

	public static int AXIS_X = 0;
	public static int AXIS_Y = 1;

	public GameObject followingObject;
	// Camera's trap is the dead zone in which the player can move with the camera staying fixed. It should be set according to the screen size
	private Vector2 trapPos = new Vector2(0, 0);
	public Vector2 trapSize = new Vector2(1, 1);
	// Camera's min & max position
	public Vector2 min = new Vector2(0, 0);
	public bool ignoreMin = true;
	public Vector2 max = new Vector2(0, 0);
	public bool ignoreMax = true;
	// Camra position offset
	public Vector2 offset = new Vector2(0, 0);
	// Damping used to smooth camera movement
	public float dampingTime = 0.3f;
	// The lookahead always shifts the camera in walking position; you can set it to 0 to disable.
	public Vector2 lookAhead = new Vector2(0, 0); 

	public event System.Action<CameraController> OnPositionUpdated;

	private Vector2 currentLookAhead = new Vector2(0, 0);
	private Vector2 dampingVelocity = new Vector2(0, 0);

	private float shakeRemainingTime = 0;
	private float shakeAmount = 0;
	private float shakeDuration = 0;

	// Use this for initialization
	void Start () {
		if (Application.isPlaying && followingObject != null){
			Move(followingObject.transform.position, false);
		}
	}

	// Camera update should be done inside LateUpdate, not anywhere else!
	void LateUpdate () {
		if (Application.isPlaying){
			if (followingObject != null){
				Move (followingObject.transform.position, dampingTime > 0);
			}

			if (shakeRemainingTime > 0){
				Shake();
			}
		}

		if (OnPositionUpdated != null){
			OnPositionUpdated(this);
		}
	}
	
	void Move(Vector2 pos, bool useDamping){
		transform.position = new Vector3(
			Move (AXIS_X, pos.x, useDamping),
			Move (AXIS_Y, pos.y, useDamping),
			transform.position.z);
	}

	float Move(int axis, float pos, bool useDamping){
		float returnValue;

		if (pos < trapPos[axis] - trapSize[axis]/2f){
			trapPos[axis] = pos + trapSize[axis]/2f;
			currentLookAhead[axis] = lookAhead[axis];
		}
		else if (pos > trapPos[axis] + trapSize[axis]/2f){
			trapPos[axis] = pos - trapSize[axis]/2f;
			currentLookAhead[axis] = -lookAhead[axis];
		}

		float targetPos = trapPos[axis] - currentLookAhead[axis] + offset[axis];
		
		if (!useDamping){
			returnValue = targetPos;
			dampingVelocity[axis] = 0;
		}
		else{
			float tmpDampingVelocity = dampingVelocity[axis];
			returnValue = Mathf.SmoothDamp(transform.position[axis], targetPos, ref tmpDampingVelocity, dampingTime);
			dampingVelocity[axis] = tmpDampingVelocity;
		}

		return Mathf.Clamp(returnValue, ignoreMin ? float.MinValue : min[axis], ignoreMax ? float.MaxValue : max[axis]);
	}

	void Shake(){
		transform.position += Random.insideUnitSphere.Set3(z: 0) * shakeAmount * (shakeRemainingTime / shakeDuration);
		shakeRemainingTime -= Time.unscaledDeltaTime;
	}

	public void Shake(float amount, float duration){
		shakeAmount = amount;
		shakeDuration = shakeRemainingTime = duration;
	}

	void OnDrawGizmosSelected () {
		// Trap
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireCube(trapPos, trapSize);
		// Center
		Gizmos.color = Color.green;
		Gizmos.DrawCube(new Vector2(transform.position.x - offset.x, transform.position.y - offset.y), new Vector2(0.1f, 0.1f));
	}
}

}