using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class Player : MonoBehaviour {

	public Ball ballPrefab;

	public Transform skyBack;
	public Transform blackTrans;

	private int picCount;

	public ParticleSystem snow;

	private int hits = 0;

	float camSpeed;

	Color backgroundCol = new Color(37f/255f, 37f/255f, 37f/255f, 1f);

	public Animator animator;

	private float elapsed = 0;
	float logoTime = 0f;

	public ScoreKeeper scoreKeeper;

	public Transform mountains;
	public Transform world;
	public Transform thumb;
	public Transform pengJump;

	float timeLost = 0f;

	// y offset of camera
	private float camOffset = 40f;

	// penguin animator from inspector
	public Ball activeBall;

	// container for the sprites
	public Transform allSprites;

	// reusable random object
	System.Random rand;

	// sprite to render logo
	public SpriteRenderer logo;

	float step = 0f;

	private enum STATE {
		LOGO,
		MAIN,
		GAME,
		LOSE
	}

	private STATE currentState;

	private int best;
	private TextAsset bestTxt;

	float margin;
	float topLev;
	float botLev;
	
	FileStream file;
	StreamReader reader;
	StreamWriter writer;

	// Use this for initialization
	void Start () {
		Screen.orientation = ScreenOrientation.Portrait;
		Camera.main.orthographicSize = 120f;
		Camera.main.transform.position = new Vector3(0f, camOffset, -10f);

		rand = new System.Random();

		currentState = STATE.LOGO;

		margin = 1f;
		topLev = activeBall.Radius * (1f + margin);
		botLev = activeBall.Radius * (1f - margin);

		hits = 0;

		best = loadBest();

		SetupSkyBack();

		picCount = 0;
	}

	private void SetupSkyBack()
	{
		float sizeSky = 500f;

		skyBack.gameObject.SetActive(false);
		skyBack.localScale = new Vector3(Camera.main.orthographicSize * 2f * Camera.main.aspect, sizeSky, 1f);
		skyBack.position = new Vector3(0f, skyBack.localScale.y * 0.5f - Camera.main.orthographicSize, 0f);

		blackTrans.gameObject.SetActive(false);
		blackTrans.localScale = new Vector3(skyBack.localScale.x, 1f, 1f);
		blackTrans.position = new Vector3(0f, skyBack.position.y + skyBack.localScale.y * 0.5f + blackTrans.GetComponent<SpriteRenderer>().sprite.textureRect.height * 0.5f,0f);
	}

	private void CheckSnow()
	{
		float endHeight = skyBack.transform.position.y + skyBack.transform.localScale.y * 0.5f * 0.25f;

		if (Camera.main.transform.position.y >= endHeight)
		{
			snow.emissionRate = 150f - (Camera.main.transform.position.y - endHeight) * 2f;
		}
	}

	private int loadBest()
	{
		file = new FileStream(Application.persistentDataPath + Path.DirectorySeparatorChar + "best.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
		reader = new StreamReader(file, System.Text.Encoding.UTF8);

		String line = reader.ReadLine();

		file.Close();
		reader.Close();

		int bestNum = 0;
		if (line != null)
		{
			bestNum = int.Parse(line);
		}

		return bestNum;
	}

	private void saveBest(int score)
	{
		file = new FileStream(Application.persistentDataPath + Path.DirectorySeparatorChar + "best.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
		file.SetLength(0);
		file.Flush();

		writer = new StreamWriter(file, System.Text.Encoding.UTF8);
		writer.WriteLine(score.ToString());
		writer.Flush();

		file.Close();
		writer.Close();
	}

	public void stateToGame()
	{
		scoreKeeper.hideResults();

		activeBall.transform.position = Vector3.zero;
		Camera.main.transform.position = new Vector3(0f, camOffset, -10f);
		mountains.transform.position = new Vector3(15f, -30f, 0f);
		world.transform.position = new Vector3(0f, -40f, 0f);
		
		activeBall.reset();
		scoreKeeper.moveScore();
		
		currentState = STATE.MAIN;
		hits = 0;
	}

	void OnGUI()
	{
		if (currentState == STATE.LOSE && timeLost > 0.5f)
		{
			if (hits > best)
			{
				saveBest(hits);
				best = hits;
			}

			scoreKeeper.showResults(hits);
		}
	}

	void FixedUpdate()
	{
		if (currentState == STATE.GAME)
		{
			activeBall.applyGravity();

			camSpeed = 0.1f;
			Camera.main.transform.Translate(new Vector3(0f, camSpeed, 0f));

			float parallax = 0.5f;
			float parallax2 = 0.85f;
			mountains.transform.Translate(new Vector3(0f, camSpeed * parallax, 0f));
			world.transform.Translate(new Vector3(0f, camSpeed * parallax2, 0f));

			scoreKeeper.moveScore();
			scoreKeeper.drawScore(hits);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.A))
		{
			ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "/screen" + picCount.ToString() + ".png", 3);
			picCount++;
		}

		if (currentState == STATE.GAME)
		{
			float time = 1f;
			if (rand.Next(1001) < 200 && elapsed >= time)
			{
				animator.SetTrigger("blink");
				elapsed = 0f;
			}
			elapsed += Time.deltaTime;

			if (Input.GetMouseButtonDown(0))
			{	
				Vector3 mousePosScreen = Input.mousePosition;
				Vector3 mousePosWorld = Camera.main.ScreenToWorldPoint(mousePosScreen);
				mousePosWorld.z = 0f;
				
				Vector3 ballPos = activeBall.BallCollider.transform.position;
				ballPos.z = 0f;

				float dist = Vector3.Distance(ballPos, mousePosWorld);
				if (dist < topLev && dist > botLev)
				{
					Vector3 vecBetweenNorm = (ballPos - mousePosWorld) / dist;
					Vector3 vecDown = new Vector3(0f, -1f, 0f);

					if (Vector3.Dot(vecBetweenNorm, vecDown) <= 0)
					{
						Vector3 cross = Vector3.Cross(vecBetweenNorm, vecDown);
						
						activeBall.applyRotation(cross.z);
						activeBall.applyHit(vecBetweenNorm);
						
						hits++;
					}
				}
			}

			activeBall.move();

			//CheckSnow();

			if (activeBall.transform.position.y < (Camera.main.transform.position.y - Camera.main.orthographicSize - camOffset) ||
			    activeBall.transform.position.y < -10f)
			{
				currentState = STATE.LOSE;
			}
		}
		else if (currentState == STATE.MAIN)
		{
			scoreKeeper.drawScore(hits);

			skyBack.gameObject.SetActive(true);
			blackTrans.gameObject.SetActive(true);

			Camera.main.backgroundColor = backgroundCol;
			thumb.GetComponent<SpriteRenderer>().enabled = true;
			pengJump.GetComponent<SpriteRenderer>().enabled = true;

			float time = 1f; // in seconds
			if (rand.Next(1001) < 25 && elapsed >= time)
			{
				animator.SetTrigger("blink");
				elapsed = 0f;
			}
			elapsed += Time.deltaTime;

			hits = 0;

			timeLost = 0f;

			// move thumb
			float flat = 0.06f;
			float xscale = (Mathf.Sin(step) + 1f) * flat;
			float yscale = (Mathf.Sin(step) + 1f) * flat;
			Vector3 scale = new Vector3(1f + xscale, 1f + yscale, 1f);
			thumb.localScale = scale;

			float movscale = 8f;
			Vector3 newPos = new Vector3(xscale * movscale + 1.5f, -25f - (yscale * movscale), 0f);
			thumb.position = newPos;

			// move peng jump
			movscale *= 10f;
			newPos = new Vector3(0f, 10f + (yscale * movscale), 0f);
			pengJump.position = newPos;

			float speeder = 9f;
			step += Time.deltaTime * speeder;

			if (Input.GetMouseButtonDown(0))
			{
				Vector3 mousePosScreen = new Vector3();
				Vector3 mousePosWorld = new Vector3();
				
				mousePosScreen = Input.mousePosition;
				mousePosWorld = Camera.main.ScreenToWorldPoint(mousePosScreen);
				mousePosWorld.z = 0f;
				
				Vector3 ballPos = activeBall.BallCollider.transform.position;
				ballPos.z = 0f;

				float dist = Vector3.Distance(ballPos, mousePosWorld);
				if (dist < topLev && dist > botLev)
				{
					Vector3 vecBetweenNorm = (ballPos - mousePosWorld) / dist;
					Vector3 vecDown = new Vector3(0f, -1f, 0f);
					
					if (Vector3.Dot(vecBetweenNorm, vecDown) <= 0)
					{
						Vector3 cross = Vector3.Cross(vecBetweenNorm, vecDown);
						
						activeBall.applyRotation(cross.z);
						activeBall.applyHit(vecBetweenNorm);
						
						hits = 1;

						currentState = STATE.GAME;
						thumb.GetComponent<SpriteRenderer>().enabled = false;
						pengJump.GetComponent<SpriteRenderer>().enabled = false;
					}
				}
			}
		}
		else if (currentState == STATE.LOSE)
		{
			timeLost += Time.deltaTime;

			float time = 1f; // in seconds
			if (rand.Next(1001) < 200 && elapsed >= time)
			{
				animator.SetTrigger("blink");
				elapsed = 0f;
			}
			elapsed += Time.deltaTime;
		}
		else if (currentState == STATE.LOGO)
		{
			Camera.main.backgroundColor = Color.white;

			logoTime += Time.deltaTime;
			if (logoTime > 3f)
			{
				currentState = STATE.MAIN;
				logoTime = 0f;

				logo.enabled = false;
				Destroy(logo.gameObject);

				foreach (Transform trans in allSprites)
				{
					trans.GetComponent<SpriteRenderer>().enabled = true;
				}

				Camera.main.orthographicSize = 90f;
			}
		}
	}

	public int Best
	{
		get {return best;}
	}

	public float CamOffset
	{
		get {return camOffset;}
	}
}
