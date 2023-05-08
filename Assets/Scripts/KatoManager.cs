using System;
using System.Collections;
using System.Collections.Generic;
using FSG.MeshAnimator;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class KatoManager : SerializedMonoBehaviour
{
    [ReadOnly] public Dictionary<string, float> KatoStats = new Dictionary<string, float>();
    [SerializeField] private NavMeshAgent _NavMeshAgent;
    [SerializeField] private Renderer _Renderer;
    [SerializeField] private Collider _Collider;
    [SerializeField] private Rigidbody _Rigidbody;
    [SerializeField] private MeshAnimatorBase _MeshAnimator;
    [SerializeField] private ECdestroyMe _ExplosionVFXPrefab;

    private List<GameObject[]> BirthCertificates = new List<GameObject[]>();
    private KatoSpawner _Spawner;
    private Coroutine _StateProcess = null;
    private BehaviourState _CurrentState;

    public bool IsBreedingCooldown { get; set; }
    public BehaviourState CurrentState => _CurrentState;

    private void Start()
    {
        _MeshAnimator.OnAnimationFinished += OnAnimationFinished;
    }

    private void OnDestroy()
    {
        _MeshAnimator.OnAnimationFinished -= OnAnimationFinished;
    }

    //Sets current animation
    private void SetAnimationState(BehaviourState state)
    {
        if (state == _CurrentState)
        {
            return;
        }

        _CurrentState = state;

        switch (state)
        {
            case BehaviourState.Spawn:
                _MeshAnimator.Play("Spawn");
                break;
            case BehaviourState.Idle:
                _MeshAnimator.PlayRandom("Idle1", "Idle2");
                break;
            case BehaviourState.Walk:
                _MeshAnimator.Play("Walk");
                if (_StateProcess != null)
                {
                    StopCoroutine(_StateProcess);
                }

                _StateProcess = StartCoroutine(MoveToRandomSpot());
                break;
            case BehaviourState.KnockedOut:
                _MeshAnimator.Play("Damage_Down");
                if (_StateProcess != null)
                {
                    StopCoroutine(_StateProcess);
                }

                _StateProcess = StartCoroutine(ExplodeCoroutine());
                break;
        }
    }

    //On Animation finished
    private void OnAnimationFinished(string name)
    {
        switch (name)
        {
            case "Spawn":
                SetAnimationState(BehaviourState.Idle);
                break;
            case "Idle1":
            case "Idle2":
                SetAnimationState(BehaviourState.Walk);
                break;
        }
    }

    
    public void Initialize(Dictionary<string, float> stats, KatoSpawner spawner)
    {
        _Spawner = spawner;
        KatoStats = stats;

        IsBreedingCooldown = false;

        //Set color
        if (KatoStats.ContainsKey("Hue"))
        {
            var propertyBlock = new MaterialPropertyBlock();
            //set the color property
            propertyBlock.SetFloat("_HueOffset", KatoStats["Hue"]);
            //apply propertyBlock to renderer
            _Renderer.SetPropertyBlock(propertyBlock);
        }

        _NavMeshAgent.autoTraverseOffMeshLink = true;
        //_NavMeshAgent.autoRepath = true;

        //Set size
        if (KatoStats.TryGetValue("Size", out var size))
        {
            transform.localScale = _Spawner.KatoPrefab.transform.localScale * size;
            if (_Rigidbody != null)
            {
                if (_Spawner.KatoPrefab._Rigidbody != null)
                {
                    _Rigidbody.mass = _Spawner.KatoPrefab._Rigidbody.mass * size;
                }
            }

            //Set speed
            var speed = 1f / size;
            _MeshAnimator.speed = speed;
            _NavMeshAgent.speed = speed * 10f;
            _NavMeshAgent.angularSpeed = speed * 120f;
        }

        gameObject.SetActive(true);
        _CurrentState = BehaviourState.Idle;
        SetAnimationState(BehaviourState.Spawn);
    }

    //Finds a random position on the nav mesh to navigate to
    public IEnumerator MoveToRandomSpot()
    {
        yield return new WaitUntil(() => _NavMeshAgent.isOnNavMesh);
        //Find random position
        Vector3 randomDirection = Random.insideUnitSphere * KatoStats["PerceptionRadius"];
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, KatoStats["PerceptionRadius"], 1);
        //Get closest on navmesh
        var currentTarget = hit.position;
        _NavMeshAgent.SetDestination(currentTarget);
        
        //Wait till navigation complete or stuck
        var timer = 0f;
        var velocity = _NavMeshAgent.velocity.sqrMagnitude;
        while (true)
        {
            if (!_NavMeshAgent.pathPending && (_NavMeshAgent.isOnNavMesh &&
                                               _NavMeshAgent.remainingDistance <= _NavMeshAgent.stoppingDistance) &&
                (!_NavMeshAgent.hasPath || _NavMeshAgent.velocity.sqrMagnitude == 0f))
            {
                break;
            }

            if (Math.Abs(velocity - _NavMeshAgent.velocity.sqrMagnitude) < 0.05f)
            {
                timer += Time.deltaTime;
                if (timer >= 7f)
                {
                    break;
                }
            }
            else
            {
                timer = 0;
            }

            yield return null;
        }

        if (Random.Range(0, 2) > 0)
        {
            SetAnimationState(BehaviourState.Idle);
        }
        else
        {
            _CurrentState = BehaviourState.Idle;
            SetAnimationState(BehaviourState.Walk);
        }
    }

    //Initiates explosion
    public void Explode(Vector3 center)
    {
        if (_Rigidbody != null)
        {
            _CurrentState = BehaviourState.Idle;
            SetAnimationState(BehaviourState.KnockedOut);
            _NavMeshAgent.enabled = false;
            _Collider.isTrigger = false;
            _Rigidbody.isKinematic = false;
            _Rigidbody.AddExplosionForce(99999f, center, 50f, 20f);
        }
    }

    private IEnumerator ExplodeCoroutine()
    {
        if (_Rigidbody != null)
        {
            yield return new WaitForSeconds(2f);
            var timer = 0f;
            while (true)
            {
                if (transform.position.y < -200f || _Rigidbody.velocity.magnitude < 0.05f || timer >= 20f)
                {
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            if (transform.position.y < -200f || timer >= 20f)
            {
                Death();
            }
            else
            {
                //Recovery
                _NavMeshAgent.enabled = true;
                _Collider.isTrigger = true;
                _Rigidbody.isKinematic = true;
                if (_NavMeshAgent.isOnNavMesh)
                {
                    SetAnimationState(BehaviourState.Idle);
                }
                else
                {
                    Death();
                }
            }
        }
    }

    //Disable and return to pool
    private void Death()
    {
        _Collider.isTrigger = true;
        _Rigidbody.isKinematic = true;
        _NavMeshAgent.enabled = true;
        gameObject.SetActive(false);
        UniversalObjectPool.instance.ReturnToPool(this);
    }

    //initiate birth 
    public void Breed(Dictionary<string, float> stats, KatoManager otherParent)
    {
        if (CurrentState != BehaviourState.Idle && CurrentState != BehaviourState.Walk)
        {
            return;
        }

        StartCoroutine(BreedCoroutine(stats, otherParent));
    }

    private IEnumerator BreedCoroutine(Dictionary<string, float> stats, KatoManager otherParent)
    {
        //Check if child was already born
        for (int i = 0; i < BirthCertificates.Count; i++)
        {
            var certificate = BirthCertificates[i];
            if ((certificate[0] == gameObject && certificate[1] == otherParent.gameObject) ||
                (certificate[1] == gameObject && certificate[0] == otherParent.gameObject))
            {
                BirthCertificates.Remove(certificate);
                yield break;
            }
        }

        //Birth newborn
        IsBreedingCooldown = true;
        otherParent.IsBreedingCooldown = true;

        BirthCertificates.Add(new GameObject[2]
        {
            gameObject,
            otherParent.gameObject
        });

        var kato = UniversalObjectPool.instance.GetObject(_Spawner.KatoPrefab,
            otherParent._Collider.ClosestPoint(transform.position), Quaternion.identity);
        kato.Initialize(stats, _Spawner);

        yield return new WaitForSeconds(60f);
        IsBreedingCooldown = false;
        otherParent.IsBreedingCooldown = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent<KatoManager>(out var otherKato))
        {
            if (!IsBreedingCooldown && !otherKato.IsBreedingCooldown &&
                (CurrentState == BehaviourState.Idle || CurrentState == BehaviourState.Walk) &&
                otherKato.KatoStats["Type"] == KatoStats["Type"])
            {
                var childStats = new Dictionary<string, float>();
                //create child stats by averaging parents
                foreach (var stat in KatoStats)
                {
                    childStats.Add(stat.Key, (stat.Value + otherKato.KatoStats[stat.Key]) / 2f);
                }
                
                Breed(childStats, otherKato);
            }
            else if (_CurrentState != BehaviourState.KnockedOut &&
                     otherKato.CurrentState != BehaviourState.KnockedOut &&
                     otherKato.KatoStats["Type"] != KatoStats["Type"])
            {
                //explosion
                var center = other.ClosestPoint(transform.position);
                UniversalObjectPool.instance.GetObject<ECdestroyMe>(_ExplosionVFXPrefab, center, Quaternion.identity);
                Collider[] hitColliders = Physics.OverlapSphere(other.ClosestPoint(transform.position), 50f);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.attachedRigidbody != null &&
                        hitCollider.attachedRigidbody.TryGetComponent<KatoManager>(out var katoInRange))
                    {
                        //Initiate explosion
                        katoInRange.Explode(center);
                    }
                }
            }
        }
    }
}

public enum BehaviourState
{
    Spawn,
    Idle,
    Walk,
    KnockedOut
}