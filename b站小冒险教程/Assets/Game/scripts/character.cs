using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class character : MonoBehaviour
{
    //player
    private CharacterController _cc;
    public float MoveSpeed = 5f;
    private Vector3 _movementVelosity;
    private playerinput _playerInput;
    private float _verticalVolsity;
    public float Gravity = -9.8f;
    private Animator _animator;
    public int Coin;

    //enemy
    public bool IsPlayer = true;
    private UnityEngine.AI.NavMeshAgent _naviMeshAgent;
    private Transform TargetPlayer;

    //health
    private Health _health;

    //DamageCster
    private DamageCaster _damageCaster;

    
    //player slides
    private float attackStartTime;
    public float AttackSlideDuration = 0.4f;
    public float AttackSlideSpeed = 0.06f;

    private Vector3 impactOnCharacter;
    public bool IsInvincible;
    public float invincibleDuration = 2f;
    private float attackAnimationDuration;
    public float SlideSpeed = 9f;

    //state machine
    public enum CharacterState{
        Normal,Attacking,Dead,BeingHit,Slide,Spawn
    }
    public CharacterState CurrentState;
    public float SpawnDuration = 2f;
    public float currentSpawnTime;

    //Material animation
    private MaterialPropertyBlock _materialPropertBlock;
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    public GameObject ItemToDrop;

    private void Awake() {
        _cc = GetComponent<CharacterController>();
        
        _animator = GetComponent<Animator>();

        _health = GetComponent<Health>();
        _damageCaster = GetComponentInChildren<DamageCaster>();

        _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _materialPropertBlock = new MaterialPropertyBlock();
        _skinnedMeshRenderer.GetPropertyBlock(_materialPropertBlock);

        if(!IsPlayer){
            _naviMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            TargetPlayer = GameObject.FindWithTag("Player").transform;
            _naviMeshAgent.speed = MoveSpeed;
            SwitchStateTo(CharacterState.Spawn);
        }
        else{
            _playerInput = GetComponent<playerinput>();
        }
    }


    private void CalculatePlayerMovement(){

        if(_playerInput.MouseButtonDown && _cc.isGrounded){
            SwitchStateTo(CharacterState.Attacking);
            return;
        }
        else if(_playerInput.SpaceKeyDown && _cc.isGrounded)
        {
            SwitchStateTo(CharacterState.Slide);
            return;
        }

        _movementVelosity.Set(_playerInput.HorizontalInput,0f,_playerInput.VerticalInput);
        _movementVelosity.Normalize();
        _movementVelosity = Quaternion.Euler(0,-45f,0) * _movementVelosity;

        _animator.SetFloat("speed",_movementVelosity.magnitude);

        _movementVelosity *= MoveSpeed * Time.deltaTime;
        

        if(_movementVelosity != Vector3.zero){
            transform.rotation = Quaternion.LookRotation(_movementVelosity);
        }

        _animator.SetBool("airborne",!_cc.isGrounded);
        
        
    }

    private void CalculateEnemyMovement(){
        if (Vector3.Distance(TargetPlayer.position, transform.position) >= _naviMeshAgent.stoppingDistance){
            _naviMeshAgent.SetDestination(TargetPlayer.position);
            _animator.SetFloat("speed",0.2f);
        }
        else{
            _naviMeshAgent.SetDestination(transform.position);
            _animator.SetFloat("speed",0f);


            SwitchStateTo(CharacterState.Attacking);
        }
    }

    private void FixedUpdate() {

        switch(CurrentState)
        {
            case CharacterState.Normal:
            if(IsPlayer)
            {
                CalculatePlayerMovement();
            }
            else
            {
                CalculateEnemyMovement();
            }
            break;
            case CharacterState.Attacking:
            if(IsPlayer)
            {
                
                if(Time.time < attackStartTime + AttackSlideDuration)
                {
                    float timePassed = Time.time - attackStartTime;
                    float lerpTime = timePassed / AttackSlideDuration;
                    _movementVelosity = Vector3.Lerp(transform.forward * AttackSlideSpeed, Vector3.zero,lerpTime);
                }
                if(_playerInput.MouseButtonDown && _cc.isGrounded)
                {
                    string currentClipName = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                    attackAnimationDuration = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

                    if(currentClipName !="LittleAdventurerAndie_ATTACK_03" && attackAnimationDuration>0.5f && attackAnimationDuration<0.7f)
                    {
                        _playerInput.MouseButtonDown = false;
                        SwitchStateTo(CharacterState.Attacking);
                        //CalculatePlayerMovement();
                    }
                }
            }
            break;
            case CharacterState.Dead:
            return;
            case CharacterState.BeingHit:
               
            break;

            case CharacterState.Slide:
                _movementVelosity = transform.forward * SlideSpeed * Time.deltaTime;
                break;

            case CharacterState.Spawn:
                currentSpawnTime -= Time.deltaTime;
                if(currentSpawnTime <= 0)
                {
                    SwitchStateTo(CharacterState.Normal);
                }
                break;
        }

        if(impactOnCharacter.magnitude >0.2f)
        {
            _movementVelosity = impactOnCharacter * Time.deltaTime;
        }
        impactOnCharacter = Vector3.Lerp(impactOnCharacter,Vector3.zero,Time.deltaTime * 5);
       
        
        if(IsPlayer)
        {
            if(_cc.isGrounded == false)
            {
                _verticalVolsity = Gravity;
            }
            
        else
            {
                _verticalVolsity = Gravity * 0.3f;
            }
 
            _movementVelosity += _verticalVolsity * Vector3.up * Time.deltaTime;
        

            _cc.Move(_movementVelosity);
            _movementVelosity = Vector3.zero;
        }
        else
        {
            if(CurrentState !=CharacterState.Normal)
            {
            _cc.Move(_movementVelosity);
            _movementVelosity = Vector3.zero;
            }
        }

        
    }


    public void SwitchStateTo(CharacterState newState)
    {

        if(IsPlayer)
        {
            _playerInput.ClearCache();
        }
        

        //exiting state
        switch(CurrentState)
        {
            case CharacterState.Normal:
                break;
            case CharacterState.Attacking:

                if(_damageCaster != null)
                {
                    _damageCaster.DisableDamageCaster();
                    

                }
                if(IsPlayer)
                {
                    GetComponent<PlayerVFXManager>().StopBlade();
                }
                
                break;
            case CharacterState.Dead:
                return;
            case CharacterState.BeingHit:
                break;
            case CharacterState.Slide:
                break;
            case CharacterState.Spawn:
                IsInvincible = false;
                break;
        }
        //entering state
        switch(newState)
        {
            case CharacterState.Normal:
            break;

            case CharacterState.Attacking:
            

                if(!IsPlayer)
                {
                    Quaternion newRotation = Quaternion.LookRotation(TargetPlayer.position - transform.position);
                    transform.rotation = newRotation;
                }
                _animator.SetTrigger("Attack");
                if(IsPlayer)
                {
                    attackStartTime = Time.time;
                    RotateToCursor();
                }
            break;
            
            case CharacterState.Dead:
                _cc.enabled = false;
                _animator.SetTrigger("Dead");
                StartCoroutine(MaterialDissolve());
                if(!IsPlayer)
                {
                    SkinnedMeshRenderer mesh = GetComponentInChildren<SkinnedMeshRenderer>();
                    mesh.gameObject.layer = 0;
                }
            break;
            case CharacterState.BeingHit:
                _animator.SetTrigger("BeingHit");
                if(IsPlayer)
                {
                    IsInvincible = true;
                    StartCoroutine(DelayCancelInvincible());
                }
            break;
            case CharacterState.Slide:
                _animator.SetTrigger("Slide");
            break;
            
            case CharacterState.Spawn:
                IsInvincible = true;
                currentSpawnTime = SpawnDuration;
                StartCoroutine(MaterialAppear());
            break;

            
        }
        CurrentState = newState;

        //Debug.Log("Switch to" + CurrentState);
    }

    public void SlideAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }

    public void AttackAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }

    public void BeingHitAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }

    public void ApplyDamage(int damage, Vector3 attackerPos = new Vector3())
    {
        if(IsInvincible)
        {
            return;
        }
        if(_health !=null){
            _health.ApplyDamage(damage);
        }

        if(!IsPlayer)
        {
            GetComponent<EnemyVFXManager>().PlayBeingHitVFX(attackerPos);
        }

        StartCoroutine(MaterialBlink());

        if(IsPlayer)
        {
            SwitchStateTo(CharacterState.BeingHit);
            AddImpact(attackerPos,8f);
        }
        else
        {
            AddImpact(attackerPos,2.5f);
        }
    }
    IEnumerator DelayCancelInvincible()
    {
        yield return new WaitForSeconds(invincibleDuration);
        IsInvincible = false;
    }

    private void AddImpact(Vector3 attackerPos,float force)
    {
        Vector3 impactDir = transform.position - attackerPos;
        impactDir.Normalize();
        impactDir.y = 0;
        impactOnCharacter = impactDir * force;
    }

    public void EnableDamageCaster()
    {
        _damageCaster.EnableDamageCaster();
    }
    public void DisableDamageCaster()
    {
        _damageCaster.DisableDamageCaster();
    }

    IEnumerator MaterialBlink()
    {
        _materialPropertBlock.SetFloat("_blink",0.4f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertBlock);
        yield return new WaitForSeconds(0.2f);

        _materialPropertBlock.SetFloat("_blink",0);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertBlock);
    }

    IEnumerator MaterialDissolve()
    {
        yield return new WaitForSeconds(2);
        float dissolveTimeDuration = 2f;
        float currentDissolveTime = 0;
        float dissolveHight_start = 20f;
        float dissolveHeight_target = -10f;
        float dissolveHeight;

        _materialPropertBlock.SetFloat("_enableDissolve",1f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertBlock);
        while (currentDissolveTime < dissolveTimeDuration)
        {
            currentDissolveTime += Time.deltaTime;
            dissolveHeight = Mathf.Lerp(dissolveHight_start,dissolveHeight_target,currentDissolveTime / dissolveTimeDuration);
            _materialPropertBlock.SetFloat("_dissolve_height",dissolveHeight);
            _skinnedMeshRenderer.SetPropertyBlock(_materialPropertBlock);
            yield return null;
        }
        DropItem();
    }

    public void DropItem()
    {
        Instantiate(ItemToDrop,transform.position,Quaternion.identity);
    }
    public void PickUpItem(PickUp item)
    {
        switch(item.Type)
        {
            case PickUp.PickUpType.Heal:
            AddHealth(item.Value);
            break;
            case PickUp.PickUpType.Coin:
            AddCoin(item.Value);
            break;
        }
    }
    private void AddHealth(int health)
    {
        _health.AddHealth(health);
        GetComponent<PlayerVFXManager>().PlayHealVFX();
    }
    private void AddCoin(int coin)
    {
        Coin += coin;
    }
    public void RotateToTarget()
    {
        if(CurrentState != CharacterState.Dead)
        {
            transform.LookAt(TargetPlayer,Vector3.up);
        }
    }
    IEnumerator MaterialAppear()
    {
        float dissolveTimeDuration = SpawnDuration;
        float currentDissolveTime = 0;
        float dissolveHight_start = -10f;
        float dissolveHeight_target = 20f;
        float dissolveHeight;

        _materialPropertBlock.SetFloat("_enableDissolve",1f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertBlock);

        while(currentDissolveTime < dissolveTimeDuration)
        {
            currentDissolveTime += Time.deltaTime;
            dissolveHeight = Mathf.Lerp(dissolveHight_start,dissolveHeight_target,currentDissolveTime/dissolveTimeDuration);
            _skinnedMeshRenderer.SetPropertyBlock(_materialPropertBlock);
            yield return null;
        }
        _materialPropertBlock.SetFloat("_enableDissolve",0f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertBlock);
    }
    private void OnDrawGizmos() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitResult;
        if(Physics.Raycast(ray,out hitResult,1000,1<<LayerMask.NameToLayer("CursorTest")))
        {
            Vector3 cursorPos = hitResult.point;
            Gizmos.DrawWireSphere(cursorPos,1);
        }
    }
    private void RotateToCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitResult;
        if(Physics.Raycast(ray,out hitResult,1000,1<<LayerMask.NameToLayer("CursorTest")))
        {
            Vector3 cursorPos = hitResult.point;
            transform.rotation = Quaternion.LookRotation(cursorPos - transform.position,Vector3.up);
        }
    }
}
