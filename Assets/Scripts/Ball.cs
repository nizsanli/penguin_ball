using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ball : MonoBehaviour {

	public CircleCollider2D ballColliderPrefab;
	private CircleCollider2D ballCollider;

	Vector3 moveVec = new Vector3(0f, 0f, 0f);
	float rot = 0f;

	float radius = 16f;

	// Use this for initialization
	void Start () {
		ballCollider = (CircleCollider2D) Instantiate(ballColliderPrefab, Vector3.zero, Quaternion.identity);
		ballCollider.transform.parent = transform;
		ballCollider.radius = radius;
	}

	// Update is called once per frame
	void Update () {
	
	}

	public void reset()
	{
		moveVec = new Vector3(0f, 0f, 0f);
		rot = 0f;

		transform.localRotation = Quaternion.identity;
	}

	public CircleCollider2D BallCollider
	{
		get {return ballCollider;}
	}

	public void applyHit(Vector3 normToCent)
	{
		float pwr = 215f;
		Vector3 pwrVec = normToCent * pwr;

		moveVec = pwrVec;
	}

	public void applyRotation(float z)
	{
		// if z negative, rotate cw
		// if z positive, rotate ccw
		float adder = 180f;
		rot = adder*z;
	}

	public void applyGravity()
	{
		float grav = 16f;
		Vector3 gravVec = new Vector3(0f, -grav, 0f);
		moveVec = moveVec + gravVec;
	}

	public void move()
	{
		transform.Translate(moveVec * Time.deltaTime, Space.World);
		transform.Rotate(0f, 0f, rot * Time.deltaTime, Space.Self);
	}

	public float Radius {
		get {return radius;}
	}
}
