using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{

    public Transform target;
    public Tilemap tilemap;
    public Vector2 offset;
    public Camera followCamera;

    public float smoothing;

    Vector2 viewportHalfSize;

    float leftBoundary;
    float rightBoundary;
    float bottomBoundary;
    private Vector3 shakeOffset;

    // Start is called before the first frame update
    void Start()
    {
        tilemap.CompressBounds();
        CalculateBounds();
    }

    private void CalculateBounds()
    {
        viewportHalfSize = new Vector2(followCamera.aspect * followCamera.orthographicSize, followCamera.orthographicSize);

        leftBoundary = tilemap.transform.position.x + tilemap.cellBounds.min.x + viewportHalfSize.x;
        rightBoundary = tilemap.transform.position.x + tilemap.cellBounds.max.x - viewportHalfSize.x; 
        bottomBoundary = tilemap.transform.position.y + tilemap.cellBounds.min.y + viewportHalfSize.y;
    }

    private void LateUpdate()
    {
        Vector3 desiredPosition = target.position + new Vector3(offset.x, offset.y, transform.position.z) + shakeOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, 1 - Mathf.Exp(-smoothing * Time.deltaTime));

        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, leftBoundary, rightBoundary);
        smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, bottomBoundary, smoothedPosition.y);

        transform.position = smoothedPosition;
    }

    public void Shake(float intensity, float duration)
    {
        StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            //shake the character while in this coroutine 
            shakeOffset = Random.insideUnitCircle * intensity;
            elapsed += Time.deltaTime;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
