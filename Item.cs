using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public Sprite[] sprites;
    public GameManager manager;
    public PointEffector2D pointEffect;
    public ParticleSystem effect;
    public string type;
    public bool isDrag;
    public bool isMerge;
    public bool isAttach;
    public Rigidbody2D rigid;
    CircleCollider2D circle;
    public SpriteRenderer spriteRenderer;
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        circle = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        pointEffect = GetComponent<PointEffector2D>();
    }
    void OnDisable()
    {
        isDrag = false;
        isMerge = false;
        isAttach = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        rigid.mass = 1;
        circle.enabled = true;
        pointEffect.enabled = false;
        
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
    }
    public void Drag()
    {
        isDrag = true;
    }
    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
        manager.count++;
    }
    void OnCollisionEnter2D(Collision2D collision) {
        StartCoroutine(AttachRoutine());
        if(collision.gameObject.tag == "Dongle"){
            Dongle dongleLogic = collision.gameObject.GetComponent<Dongle>();
            // 아이템 발동
            switch(type){
                case "Mag":
                    StartCoroutine(StartMagRoutine());
                    break;
                case "LevelUp":
                    if(dongleLogic.level != manager.dongleMaxLevel){
                        Hide(collision.transform.position);
                        dongleLogic.LevelUp();
                    }
                    break;
                case "LevelDown":
                    if(dongleLogic.level != 0){
                        Hide(collision.transform.position);
                        dongleLogic.LevelDown();
                    }
                    break;
            }
        } else if(type == "Anchor" && collision.gameObject.tag == "Border"){
            EffectPlay();
            gameObject.SetActive(false);
        }
    }
    void OnCollisionStay2D(Collision2D collision) {
        if(collision.gameObject.tag == "Dongle"){
            Dongle dongleLogic = collision.gameObject.GetComponent<Dongle>();
            // 아이템 발동
            switch(type){
                case "Mag":
                    StartCoroutine(StartMagRoutine());
                    break;
                case "LevelUp":
                    if(dongleLogic.level != manager.dongleMaxLevel){
                        Hide(collision.transform.position);
                        dongleLogic.LevelUp();
                    }
                    break;
                case "LevelDown":
                    if(dongleLogic.level != 0){
                        Hide(collision.transform.position);
                        dongleLogic.LevelDown();
                    }
                    break;
            }
        } else if(type == "Anchor" && collision.gameObject.tag == "Border"){
            EffectPlay();
            gameObject.SetActive(false);
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
        gameObject.SetActive(false);
    }
    IEnumerator StartMagRoutine()
    {
        rigid.velocity = Vector2.zero;
        rigid.bodyType = RigidbodyType2D.Kinematic;
        pointEffect.enabled = true;
        manager.line.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        pointEffect.enabled = false;
        manager.line.gameObject.SetActive(true);
        EffectPlay();
        rigid.bodyType = RigidbodyType2D.Dynamic;
        gameObject.SetActive(false);
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
    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        manager.backSprite.color = new Color(0.9f,0.8f,0.9f);
        effect.Play();
        Invoke("BackGroundColorReturn", 0.1f);
    }
    void BackGroundColorReturn()
    {
        manager.backSprite.color = new Color(0.9f,0.8f,1);
    }
}
