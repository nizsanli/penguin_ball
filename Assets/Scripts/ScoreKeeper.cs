using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreKeeper : MonoBehaviour {

	public Sprite zero;
	public Sprite one;
	public Sprite two;
	public Sprite three;
	public Sprite four;
	public Sprite five;
	public Sprite six;
	public Sprite seven;
	public Sprite eight;
	public Sprite nine;

	List<Transform> activeNums;
	Sprite[] spriteLookUp;

	private int lastScore;

	public Transform numberPrefab;

	public SpriteRenderer frame;
	public SpriteRenderer scoreTxt;
	public SpriteRenderer bestTxt;

	List<Transform> resultNums;

	public Player playerRef;

	public Canvas canvas;

	// Use this for initialization
	void Start () {
		activeNums = new List<Transform>();
		resultNums = new List<Transform>();

		spriteLookUp = new Sprite[10] {
			zero,
			one,
			two,
			three,
			four,
			five,
			six,
			seven,
			eight,
			nine
		};

		lastScore = 1;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void showResults(int score)
	{
		if (!frame.enabled)
		{
			float offset = playerRef.CamOffset;
			float fixedY = Camera.main.transform.position.y - offset;

			frame.transform.position = new Vector3(0f, fixedY + 61f, 0f);
			scoreTxt.transform.position = new Vector3(0f, fixedY + 85f, 0f);
			bestTxt.transform.position = new Vector3(0f, fixedY + 55f, 0f);
			canvas.transform.position = new Vector3(0f, fixedY, 0f);

			frame.enabled = true;
			scoreTxt.enabled = true;
			bestTxt.enabled = true;
			canvas.enabled = true;

			foreach (Transform transform in resultNums)
			{
				Destroy(transform.gameObject);
			}
			resultNums.Clear();

			Stack<int> nums = new Stack<int>();
			int scoreCopy = score;
			if (scoreCopy == 0)
			{
				nums.Push(0);
			}
			else
			{
				while (scoreCopy > 0)
				{
					nums.Push(scoreCopy % 10);
					scoreCopy /= 10;
				}
			}

			Vector3 spawn = new Vector3(6f, fixedY + 71f, 0f);
			spawn.z = 0f;

			spawn.x -= nums.Count * 6f;
			
			float textWidth = 12f;
			foreach (int num in nums)
			{
				Sprite spriteRef = spriteLookUp[num];
				
				Transform number = (Transform) Instantiate(numberPrefab, spawn, Quaternion.identity);
				number.GetComponent<SpriteRenderer>().sprite = spriteRef;
				number.GetComponent<SpriteRenderer>().sortingOrder = 2;
				
				spawn += (new Vector3(textWidth, 0f, 0f));
				
				resultNums.Add(number);
				
				number.parent = transform;
			}

			nums.Clear();
			scoreCopy = playerRef.Best;
			if (scoreCopy == 0)
			{
				nums.Push(0);
			}
			else
			{
				while (scoreCopy > 0)
				{
					nums.Push(scoreCopy % 10);
					scoreCopy /= 10;
				}
			}

			spawn = new Vector3(6f, fixedY + 41f, 0f);
			spawn.x -= nums.Count * 6f;

			foreach (int num in nums)
			{
				Sprite spriteRef = spriteLookUp[num];
				
				Transform number = (Transform) Instantiate(numberPrefab, spawn, Quaternion.identity);
				number.GetComponent<SpriteRenderer>().sprite = spriteRef;
				number.GetComponent<SpriteRenderer>().sortingOrder = 2;
				
				spawn += (new Vector3(textWidth, 0f, 0f));
				
				resultNums.Add(number);
				
				number.parent = transform;
			}
		}
	}

	public void hideResults()
	{
		if (frame.enabled)
		{
			frame.enabled = false;
			scoreTxt.enabled = false;
			bestTxt.enabled = false;
			canvas.enabled = false;
			
			for (int i = 0; i < resultNums.Count; i++)
			{
				Destroy(resultNums[i].gameObject);
			}
			
			resultNums.Clear();
		}
	}

	public void moveScore()
	{
		foreach (Transform number in activeNums)
		{
			float newY = Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)).y;
			float padding = 10f;
			newY -= padding;

			number.transform.position = new Vector3(number.position.x, newY, 0f);
		}
	}

	public void drawScore(int score)
	{
		if (score != lastScore)
		{
			foreach (Transform number in activeNums)
			{
				Destroy(number.gameObject);
			}
			activeNums.Clear();
			
			Stack<int> nums = new Stack<int>();
			int scoreCopy = score;
			if (scoreCopy == 0)
			{
				nums.Push(0);
			}
			else
			{
				while (scoreCopy > 0)
				{
					nums.Push(scoreCopy % 10);
					scoreCopy /= 10;
				}
			}

			lastScore = score;

			Vector3 spawn = Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f));
			float padding = 10f;
			spawn.x += padding;
			spawn.y -= padding;
			spawn.z = 0f;

			float textWidth = 12f;
			foreach (int num in nums)
			{
				Sprite spriteRef = spriteLookUp[num];

				Transform number = (Transform) Instantiate(numberPrefab, spawn, Quaternion.identity);
				number.GetComponent<SpriteRenderer>().sprite = spriteRef;

				spawn += (new Vector3(textWidth, 0f, 0f));

				activeNums.Add(number);

				number.parent = transform;
			}
		}
	}
}
