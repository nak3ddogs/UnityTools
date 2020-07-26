using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public enum SwipeDir
{
	None,
	Up,
	Down,
	Left,
	Right,
	UpLeft,
	UpRight,
	DownLeft,
	DownRight
};

public class SwipeDetector : MonoBehaviour
{
	private static class CardinalDirection
	{
		public static readonly Vector2 Up = new Vector2(0, 1);
		public static readonly Vector2 Down = new Vector2(0, -1);
		public static readonly Vector2 Right = new Vector2(1, 0);
		public static readonly Vector2 Left = new Vector2(-1, 0);
		public static readonly Vector2 UpRight = new Vector2(1, 1);
		public static readonly Vector2 UpLeft = new Vector2(-1, 1);
		public static readonly Vector2 DownRight = new Vector2(1, -1);
		public static readonly Vector2 DownLeft = new Vector2(-1, -1);
	}

	public float minSwipeLength = 0.5f;
	public bool useEightDirections = false;
	public bool endTouchOnMinSwipeLength = false;

	const float _eightDirAngle = 0.906f;
	const float _fourDirAngle = 0.5f;
	const float _defaultDPI = 72f;
	const float _dpcmFactor = 2.54f;

	static Dictionary<SwipeDir, Vector2> cardinalDirections = new Dictionary<SwipeDir, Vector2>() {
		{ SwipeDir.Up,         CardinalDirection.Up         },
		{ SwipeDir.Down,         CardinalDirection.Down         },
		{ SwipeDir.Right,         CardinalDirection.Right     },
		{ SwipeDir.Left,         CardinalDirection.Left         },
		{ SwipeDir.UpRight,     CardinalDirection.UpRight     },
		{ SwipeDir.UpLeft,     CardinalDirection.UpLeft     },
		{ SwipeDir.DownRight,     CardinalDirection.DownRight },
		{ SwipeDir.DownLeft,     CardinalDirection.DownLeft     }
	};

	public delegate void OnSwipeDetectedHandler(SwipeDir swipeDirection);

	public OnSwipeDetectedHandler OnSwipeDetected;

	private float _dpcm;
	private SwipeDir _swipeDirection;
	private Vector2 _firstPressPos;
	private Vector2 _secondPressPos;


	void Awake()
	{
		float dpi = (Screen.dpi == 0) ? _defaultDPI : Screen.dpi;
		_dpcm = dpi / _dpcmFactor;
	}

	void Update()
	{
		DetectSwipe();
	}

	void DetectSwipe()
	{
		if (GetTouchInput() || GetMouseInput())
		{
			Vector2 currentSwipe = new Vector3(_secondPressPos.x - _firstPressPos.x, _secondPressPos.y - _firstPressPos.y);

			float swipeCm = currentSwipe.magnitude / _dpcm;

			// Make sure it was a legit swipe, not a tap
			if (swipeCm < minSwipeLength)
			{
				if (Application.isEditor)
				{
					Debug.Log("[SwipeManager] Swipe was not long enough.");
				}

				_swipeDirection = SwipeDir.None;
				return;
			}

			_swipeDirection = GetSwipeDirByTouch(currentSwipe);

			OnSwipeDetected?.Invoke(_swipeDirection);
			_firstPressPos = _secondPressPos;
		}
		else
		{
			_swipeDirection = SwipeDir.None;
		}
	}

	bool GetTouchInput()
	{
		if (Input.touches.Length > 0)
		{
			Touch t = Input.GetTouch(0);

			if (t.phase == TouchPhase.Began)
			{
				_firstPressPos = new Vector2(t.position.x, t.position.y);
			}


			if (endTouchOnMinSwipeLength)
			{

				_secondPressPos = new Vector2(t.position.x, t.position.y);
				Vector2 currentSwipe = new Vector3(_secondPressPos.x - _firstPressPos.x, _secondPressPos.y - _firstPressPos.y);
				float swipeCm = currentSwipe.magnitude / _dpcm;

				if (swipeCm > minSwipeLength)
				{
					return true;
				}
			}

			if (t.phase == TouchPhase.Ended)
			{
				_secondPressPos = new Vector2(t.position.x, t.position.y);
				return true;
			}
		}

		return false;
	}

	bool GetMouseInput()
	{
		if (Input.GetMouseButtonDown(0))
		{
			_firstPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		}

		if (endTouchOnMinSwipeLength && Input.GetMouseButton(0))
		{
			_secondPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

			Vector2 currentSwipe = new Vector3(_secondPressPos.x - _firstPressPos.x, _secondPressPos.y - _firstPressPos.y);
			float swipeCm = currentSwipe.magnitude / _dpcm;
			if (swipeCm > minSwipeLength)
			{
				return true;
			}
		}

		if (Input.GetMouseButtonUp(0))
		{
			_secondPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			return true;
		}

		return false;
	}

	bool IsDirection(Vector2 direction, Vector2 cardinalDirection)
	{
		var angle = useEightDirections ? _eightDirAngle : _fourDirAngle;
		return Vector2.Dot(direction, cardinalDirection) > angle;
	}

	SwipeDir GetSwipeDirByTouch(Vector2 currentSwipe)
	{
		currentSwipe.Normalize();
		var swipeDir = cardinalDirections.FirstOrDefault(dir => IsDirection(currentSwipe, dir.Value));
		return swipeDir.Key;
	}
}