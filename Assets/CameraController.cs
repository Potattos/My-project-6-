using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject _camera;

    [Space]
    [Tooltip("드래그하여 이동할 거리 비율")]
    [SerializeField] private float _moveRate = 1f;

    [Space]
    [Tooltip("카메라 가속 배율")]
    [SerializeField] private float _accelerationRate = 1f;

    [Tooltip("카메라 감속 배율")]
    [SerializeField] private float _decelerationRate = 0.5f;

    [Tooltip("Y축 고정 값")]
    [SerializeField] private float _fixedY = 10f;

    [Tooltip("X축 이동 범위")]
    [SerializeField] private float _minX = -25f;
    [SerializeField] private float _maxX = 25f;

    private Vector3 _tmpClickPos;
    private Vector3 _tmpCameraPos;
    private Vector3 _velocity;
    private Vector3 _lastMousePosition;

    private bool _isDragging = false;

    private void Update()
    {
#if UNITY_EDITOR
        MouseMovement();
#elif UNITY_ANDROID || UNITY_IOS
        TouchMovement();
#endif
        ApplyDeceleration();
    }

    private void MouseMovement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 드래그 시작
            _tmpClickPos = Input.mousePosition;
            _tmpCameraPos = _camera.transform.position;
            _lastMousePosition = Input.mousePosition;
            _isDragging = true;
        }
        else if (Input.GetMouseButton(0))
        {
            // 드래그 중
            Vector3 movePos = Camera.main.ScreenToViewportPoint(_lastMousePosition - Input.mousePosition);
            _lastMousePosition = Input.mousePosition;

            Vector3 newPosition = _camera.transform.position + movePos * _moveRate;
            newPosition.x = Mathf.Clamp(newPosition.x, _minX, _maxX);
            newPosition.y = _fixedY;

            _camera.transform.position = newPosition;

            // 가속 계산
            _velocity = movePos / Time.deltaTime * _accelerationRate;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // 드래그 종료
            _isDragging = false;
        }
    }

    private void TouchMovement()
    {
        if (Input.touchCount != 1)
        {
            _isDragging = false;
            return;
        }

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            _tmpClickPos = touch.position;
            _tmpCameraPos = _camera.transform.position;
            _lastMousePosition = touch.position;
            _isDragging = true;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            Vector3 movePos = Camera.main.ScreenToViewportPoint(_lastMousePosition - (Vector3)touch.position);
            _lastMousePosition = touch.position;

            Vector3 newPosition = _camera.transform.position + movePos * _moveRate;
            newPosition.x = Mathf.Clamp(newPosition.x, _minX, _maxX);
            newPosition.y = _fixedY;

            _camera.transform.position = newPosition;

            // 가속 계산
            _velocity = movePos / Time.deltaTime * _accelerationRate;
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            _isDragging = false;
        }
    }

    private void ApplyDeceleration()
    {
        if (_isDragging || _velocity.sqrMagnitude == 0)
            return;

        // 감속 처리
        Vector3 deceleration = _velocity * (Time.deltaTime * _decelerationRate);
        _velocity -= deceleration;

        // 감속 후 속도가 거의 0이 되면 정지
        if (_velocity.sqrMagnitude < 0.01f)
        {
            _velocity = Vector3.zero;
        }

        Vector3 newPosition = _camera.transform.position + _velocity * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, _minX, _maxX);
        newPosition.y = _fixedY;

        _camera.transform.position = newPosition;
    }
}
