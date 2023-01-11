using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public GameManager manager;
    public ParticleSystem effect;
    public int level;
    public bool isDrag;
    public bool isMerge;
    public bool isAttach;
    public bool isMaxLevel;

    public Rigidbody2D rigid;
    Animator anim;
    CircleCollider2D circle;
    SpriteRenderer spriteRenderer;

    float deadTime;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        circle = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        anim.SetInteger("Level", level);
    }

    void OnDisable()
    {
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;
        isMaxLevel = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circle.enabled = true;
    }
    void Update()
    {
        if(isDrag){
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // X 축 경계 설정
            float leftBorder = -4.2f + transform.localScale.x / 2f;
            float rightBorder = 4.2f - transform.localScale.x / 2f;
            if(mousePos.x < leftBorder){
                mousePos.x = leftBorder;
            } else if(mousePos.x > rightBorder) {
                mousePos.x = rightBorder;
            }

            mousePos.y = 8;
            mousePos.z = 0;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f);
        }
        if(level == 7){
            if(!isMaxLevel){
                isMaxLevel = true;
                Invoke("EffectPlay", 0.8f);
                Invoke("ActiveOff", 1f);
            }
        }
    }

    public void Drag()
    {
        isDrag = true;
    }
    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
    }
    void OnCollisionEnter2D(Collision2D collision) {
        StartCoroutine(AttachRoutine());
        if(collision.gameObject.tag == "Dongle"){
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            if(level == other.level && !isMerge && !other.isMerge && level < 7){
                // 나와 상대편 위치 가져오기
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;
                // 1. 내가 아래에 있을 경우
                // 2. 동일한 높이일 경우, 내가 오른쪽에 있을 경우
                if(meY < otherY || (meY == otherY && meX > otherX)){
                    // 상대방은 숨기기
                    other.Hide(transform.position);
                    // 나는 레벨 업
                    LevelUp();
                }
            }
        }
    }
    IEnumerator AttachRoutine()
    {
        if(isAttach){
            yield break;
        }

        isAttach = true;
        manager.SfxPlay(GameManager.Sfx.Attach);

        yield return new WaitForSeconds(0.5f);

        isAttach = false;

    }
    void OnCollisionStay2D(Collision2D collision) {
        if(collision.gameObject.tag == "Dongle"){
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            if(level == other.level && !isMerge && !other.isMerge && level < 7){
                // 나와 상대편 위치 가져오기
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;
                // 1. 내가 아래에 있을 경우
                // 2. 동일한 높이일 경우, 내가 오른쪽에 있을 경우
                if(meY < otherY || (meY == otherY && meX > otherX)){
                    // 상대방은 숨기기
                    other.Hide(transform.position);
                    // 나는 레벨 업
                    LevelUp();
                }
            }
        }
    }
    public void Hide(Vector3 targetPos)
    {
        isMerge = true;

        rigid.simulated = false;
        circle.enabled = false;
        if(targetPos == Vector3.up * 100){
            EffectPlay();
        }

        StartCoroutine(HideRoutine(targetPos));
    }
    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount = 0;

        while(frameCount < 20){
            frameCount++;

            if(targetPos != Vector3.up * 100){
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            } else if(targetPos == Vector3.up * 100){
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }

            yield return null;
        }
        ActiveOff();
    }
    public void LevelUp()
    {
        isMerge = true;

        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());
    }
    public void LevelDown()
    {
        isMerge = true;

        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine(LevelDownRoutine());
    }
    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level + 1);
        EffectPlay();

        yield return new WaitForSeconds(0.3f);
        level++;

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        isMerge = false;
    }
    IEnumerator LevelDownRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level - 1);
        EffectPlay();

        yield return new WaitForSeconds(0.3f);
        level--;

        isMerge = false;
    }

    void OnTriggerStay2D(Collider2D collision) {
        if(collision.tag == "Finish"){
            deadTime += Time.deltaTime;

            if(deadTime > 2){
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }
            if(deadTime > 5){
                manager.GameOver();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if(collision.tag == "Finish"){
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }
    }

    void EffectPlay()
    {
        manager.SfxPlay(GameManager.Sfx.LevelUp);
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
        manager.backSprite.color = new Color(0.9f,0.8f,0.9f);
        StartCoroutine(EffectPointerRoutine(effect.gameObject));
    }
    IEnumerator EffectPointerRoutine(GameObject effect)
    {
        yield return null;
        PointEffector2D effectPoint = effect.GetComponent<PointEffector2D>();
        effectPoint.enabled = true;
        yield return new WaitForSeconds(0.1f);
        effectPoint.enabled = false;
        manager.backSprite.color = new Color(0.9f,0.8f,1);
    }
    void ActiveOff()
    {
        manager.score += (int)Mathf.Pow(2, level);

        isMerge = false;
        gameObject.SetActive(false);
    }
}