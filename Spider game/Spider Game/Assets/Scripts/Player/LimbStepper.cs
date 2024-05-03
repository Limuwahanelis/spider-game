using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

public class LimbStepper : MonoBehaviour
{
    struct MyRay
    {
        public MyRay(Ray ray , float length=1)
        {
            this.ray = ray;
            this.length = length;
        }
        public Ray ray;
        public float length;
    }

    public Vector3 LimbNormal => _limbNormal;
    public float Distance=>_distance;
    public bool IsLerping=> _isLimbLerping;
    public bool ShouldLerp => _shouldLerp;
    [SerializeField] Transform testc;
    [SerializeField] LayerMask _climbingMask;
    [SerializeField] float _checkDistance;
    [SerializeField] Transform _limbTarget;
    [SerializeField] Transform _limbTip;
    [SerializeField] Transform _limbAnchor;
    [SerializeField] Transform _mainBody;
    [SerializeField] private float _lerpSpeed;

    private Vector3 _limbNormal;
    private Vector3 _futureNormal;
    private float _distance;
    private bool _shouldLerp;
    [Header("Wall checking"),Space]
    
    [SerializeField] Transform _wallTran;
    [SerializeField] float _wallRayLength;
    private Vector3 _startingLerpPosition;
    private Vector3 _endLerpPosition;
    private bool _isLimbLerping;
    private bool _lerp;
    private float _lerpValue;
    private bool _isFirstTimeLerping;
    private Vector3 _oldTargetPosition;
    private void Awake()
    {
        Vector3 rayDirection = _limbAnchor.position - _limbTip.position;
        Vector3 hit = Vector3.one;
        MyRay ray = new MyRay(new Ray(_limbTip.position, rayDirection), _checkDistance);
        CheckLimbRay(ray, ref hit, out _, out _limbNormal, out _);
        _limbTarget.position = hit;
    }
    // Start is called before the first frame update
    void Start()
    {

    }
    public void SetUp(LayerMask climbingMask,float lerpSpeed)
    {
        _climbingMask = climbingMask;
        _lerpSpeed = lerpSpeed;
    }
    // Update is called once per frame
    void Update()
    {
        if(_lerp) 
        {
            _lerp = false;
            MoveLimb();
        }
        //_distance = Vector3.Distance(_limbAnchor.position, _limbTarget.position);
        //if (_isLimbLerping) return;
        //if (Vector3.Distance(_limbStart.position, _limbTarget.position) > _maxForwardSafeDistanceFromLimbStart) _shouldLerp = true;
        //if (Vector3.Distance(_limbStart.position, _limbTarget.position) < _minForwardSafeDistanceFromLimbStart)
        //{
        //    _shouldLerp = true;
        //    //_distance = 20;
        //}
        //if (Vector3.Distance(_limbTarget.position,_limbAnchor.position) > _limbAnchorSafeDistance) _shouldLerp= true;
    }
    public void lerp()
    {
        _lerp = true;
    }
    public void MoveLimb()
    {
            if (!_isLimbLerping)
            {
            Debug.Log("ler");
                LerpLimb();
            }

    }
    private bool CastRayForWall(out Vector3 hitpoint)
    {
        MyRay ray = new MyRay(new Ray(_wallTran.position,_wallTran.forward),_wallRayLength);
        hitpoint = Vector3.zero;
        
        return CheckLimbRay(ray, ref hitpoint, out _, out _limbNormal, out _);
    }
    private Vector3 CastRaysForAnchor()
    {
        Vector3 startRay = _limbAnchor.position;
        MyRay[] rays = new MyRay[] {new MyRay( new Ray(_oldTargetPosition, (_limbAnchor.position - _oldTargetPosition)),8),new MyRay(new Ray(_limbTip.position,_limbAnchor.position-_limbTip.position),_checkDistance),new MyRay( new Ray(startRay, _limbAnchor.right))
            , new MyRay( new Ray(startRay, -_limbAnchor.right)), new MyRay (new Ray(startRay, _limbAnchor.forward)), new MyRay (new Ray(startRay, -_limbAnchor.forward))
            ,new MyRay(new Ray(startRay+_limbAnchor.up*4,startRay-startRay+_limbAnchor.up*4),12)};
        Vector3 placeCandidate = _limbTarget.position;
        Vector3 hitPoint = _limbTarget.position;
        float closestDistance = 100;
        for (int i = 0; i < rays.Length; i++) 
        {
            //Debug.DrawRay(startRay, rays[i]);
            if (CheckLimbRay(rays[i], ref placeCandidate, out _, out Vector3 nomralCandidate, out _))
            {
                float distance = Vector3.Distance(placeCandidate, _limbAnchor.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    hitPoint = placeCandidate;
                    _futureNormal = nomralCandidate;
                }
            }
        }
        return hitPoint;
    }
    private bool CheckLimbRay(MyRay ray, ref Vector3 hitpoint, out float currentHitDistance, out Vector3 hitNormal, out bool gotCastHit)
    {
        hitNormal = Vector3.zero;
        gotCastHit = false;
        hitpoint = ray.ray.origin;
        currentHitDistance = 0;
        Ray limbRay = ray.ray;
        Debug.DrawRay(limbRay.origin, limbRay.direction*ray.length,Color.green);
        if (Physics.Raycast(limbRay, out RaycastHit hit, ray.length, _climbingMask))
        {
            hitNormal = hit.normal;
            hitpoint = hit.point;
            currentHitDistance = hit.distance;
            gotCastHit = true;
            return true;
        }
        return false;
    }
    private void LerpLimb()
    {
        _isLimbLerping = true;
        _isFirstTimeLerping = true;
        _startingLerpPosition = _limbTarget.position;
        Vector3 endLerpPos = _limbTarget.position + (_limbAnchor.position - _limbTarget.position) / 2;
        endLerpPos += _mainBody.up * 1f;
        _endLerpPosition = endLerpPos;
        _shouldLerp = false;
        _oldTargetPosition = _limbTarget.position;
        StartCoroutine(LerpCor());

    }
    private IEnumerator LerpCor()
    {
        _lerpValue += Time.deltaTime;
        float totalT = Vector3.Distance(_startingLerpPosition, _endLerpPosition) / _lerpSpeed;
        float t = _lerpValue / totalT;
        
        while (_isLimbLerping == true)
        {
            _lerpValue += Time.deltaTime;
            t = _lerpValue / totalT;
            _limbTarget.position = Vector3.Lerp(_startingLerpPosition, _endLerpPosition, t);

            if (_isFirstTimeLerping)
            {
                if (t >= 1)
                {
                    _isFirstTimeLerping = false;
                    _startingLerpPosition = _endLerpPosition;
                    _endLerpPosition = CastRaysForAnchor();
                    testc.position = _endLerpPosition;
                    if (_wallTran !=null)
                    {
                        if(CastRayForWall(out Vector3 hitpoint)) _endLerpPosition=hitpoint;
                    }
                    _lerpValue = 0;
                }
            }
            else
            {
                if (t >= 1)
                {
                    _limbTarget.position = _endLerpPosition;
                    _isLimbLerping = false;
                    _limbNormal=_futureNormal;
                    _lerpValue = 0;
                    Debug.Log("end lerp all");
                }
            }
            yield return null;
        }
    }
    private void OnDrawGizmosSelected()
    {
        if(_wallTran !=null) Gizmos.DrawLine(_wallTran.position, _wallTran.position+ _wallTran.forward*_wallRayLength);
    }
}
