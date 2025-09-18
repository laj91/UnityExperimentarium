using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class RopeVerlet : MonoBehaviour
{
    [Header("Rope")]
    [SerializeField] private int _numOfRopeSegments = 50;
    [SerializeField] private float _ropeSegmentLength = 0.225f;

    [Header("Physics")]
    [SerializeField] private Vector3 _gravityForce = new Vector3(0f, -2f, 0f);
    [SerializeField] private float _dampingFactor = 0.98f; // optional
    [SerializeField] private LayerMask _collisionMask;
    [SerializeField] private float _collisionRadius = 0.1f;
    [SerializeField] private float _bounceFactor = 0.1f;

    [Header("Constraints")]
    [SerializeField] private int _numOfConstraintRuns = 50;

    [Header("Optimizations")]
    [SerializeField] private int _collisionSegmentInterval = 2;
    [SerializeField] private Transform _ropeStartTransform;

    private LineRenderer _lineRenderer;
    private List<RopeSegment> _ropeSegments = new List<RopeSegment>();

    private Vector3 _ropeStartPoint;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = _numOfRopeSegments;

        _ropeStartPoint = _ropeStartTransform.position;

        for (int i = 0; i < _numOfRopeSegments; i++)
        {
            _ropeSegments.Add(new RopeSegment(_ropeStartPoint));
            _ropeStartPoint.y -= _ropeSegmentLength;
        }
    }

    private void Update()
    {
        DrawRope();
    }

    private void FixedUpdate()
    {
        Simulate();

        for (int i = 0; i < _numOfConstraintRuns; i++)
        {
            ApplyConstraints();

            if (i % _collisionSegmentInterval == 0)
            {
                HandleCollisions();
            }
        }
    }

    private void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[_numOfRopeSegments];
        for (int i = 0; i < _ropeSegments.Count; i++)
        {
            ropePositions[i] = _ropeSegments[i].CurrentPosition;
        }

        _lineRenderer.SetPositions(ropePositions);
    }

    private void Simulate()
    {
        for (int i = 0; i < _ropeSegments.Count; i++)
        {
            RopeSegment segment = _ropeSegments[i];
            Vector3 velocity = (segment.CurrentPosition - segment.OldPosition) * _dampingFactor;

            segment.OldPosition = segment.CurrentPosition;
            segment.CurrentPosition += velocity;
            segment.CurrentPosition += _gravityForce * Time.fixedDeltaTime;
            _ropeSegments[i] = segment;
        }
    }

    private void ApplyConstraints()
    {
        // Keep first point attached to the start transform
        RopeSegment firstSegment = _ropeSegments[0];
        firstSegment.CurrentPosition = _ropeStartTransform.position;
        _ropeSegments[0] = firstSegment;

        for (int i = 0; i < _numOfRopeSegments - 1; i++)
        {
            RopeSegment currentSeg = _ropeSegments[i];
            RopeSegment nextSeg = _ropeSegments[i + 1];

            float dist = (currentSeg.CurrentPosition - nextSeg.CurrentPosition).magnitude;
            float difference = (dist - _ropeSegmentLength);

            Vector3 changeDir = (currentSeg.CurrentPosition - nextSeg.CurrentPosition).normalized;
            Vector3 changeVector = changeDir * difference;

            if (i != 0)
            {
                currentSeg.CurrentPosition -= (changeVector * 0.5f);
                nextSeg.CurrentPosition += (changeVector * 0.5f);
            }
            else
            {
                nextSeg.CurrentPosition += changeVector;
            }

            _ropeSegments[i] = currentSeg;
            _ropeSegments[i + 1] = nextSeg;
        }
    }

    private void HandleCollisions()
    {
        for (int i = 1; i < _ropeSegments.Count; i++)
        {
            RopeSegment segment = _ropeSegments[i];
            Vector3 velocity = segment.CurrentPosition - segment.OldPosition;
            Collider[] colliders = Physics.OverlapSphere(segment.CurrentPosition, _collisionRadius, _collisionMask);

            foreach (Collider collider in colliders)
            {
                Vector3 closestPoint = collider.ClosestPoint(segment.CurrentPosition);
                float distance = Vector3.Distance(segment.CurrentPosition, closestPoint);

                // If within the collision radius, resolve
                if (distance < _collisionRadius)
                {
                    Vector3 normal = (segment.CurrentPosition - closestPoint).normalized;
                    if (normal == Vector3.zero)
                    {
                        // Fallback method
                        normal = (segment.CurrentPosition - collider.transform.position).normalized;
                    }

                    float depth = _collisionRadius - distance;
                    segment.CurrentPosition += normal * depth;
                    velocity = Vector3.Reflect(velocity, normal) * _bounceFactor;
                }
            }

            segment.OldPosition = segment.CurrentPosition - velocity;
            _ropeSegments[i] = segment;
        }
    }

    public struct RopeSegment
    {
        public Vector3 CurrentPosition;
        public Vector3 OldPosition;

        public RopeSegment(Vector3 pos)
        {
            CurrentPosition = pos;
            OldPosition = pos;
        }
    }
}
