using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dongle lastDongle;
    public GameObject donglePrefeb;
    public Transform dongleGroup;
    public GameObject effectPrefeb;
    public Transform effectGroup;

    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx { LevelUp, Next, Attach, Button, Over };
    int sfxCursor;

    public int score;
    public int maxLevel;
    public bool isOver;

    void Awake()
    {
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        bgmPlayer.Play();
        NextDongle();
    }
    Dongle GetDongle()
    {
        // 이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefeb, effectGroup);
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();

        // 동글 생성
        GameObject instantDongleObj = Instantiate(donglePrefeb, dongleGroup);
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.effect = instantEffect;
        return instantDongle;
    }
    void NextDongle()
    {
        if(isOver){
            return;
        }

        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.manager = this;
        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine(WaitNext());
    }
    IEnumerator WaitNext()
    {
        while(lastDongle != null){
            yield return null;
        }

        yield return new WaitForSeconds(2.5f);

        NextDongle();
    }
    public void TouchDown()
    {
        if(lastDongle == null)
            return;
        
        lastDongle.Drag();
    }
    public void TouchUp()
    {
        if(lastDongle == null)
            return;

        lastDongle.Drop();
        lastDongle = null;
    }
    public void GameOver()
    {
        if(isOver){
            return;
        }

        isOver = true;
        StartCoroutine(GameOverRoutine());
    }
    IEnumerator GameOverRoutine()
    {
        // 1. 장면 안에 활성화 되어잇는 모든 동글 가져오기
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        // 2. 지우기 전에 모든 동글의 물리효과 비활성화
        for(int index=0;index<dongles.Length;index++){
            dongles[index].rigid.simulated = false;
        }
        
        // 3. 1번의 목록을 하나씩 접근해서 지우기
        for(int index=0;index<dongles.Length;index++){
            dongles[index].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        SfxPlay(Sfx.Over);
    }

    public void SfxPlay(Sfx type)
    {
        switch(type){
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0,3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }
        sfxPlayer[sfxCursor].Play();
        // 0 1 2 반복
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }
}
