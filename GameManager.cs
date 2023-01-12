using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("-------------[ Ad ]")]
    public AdManager admManager;
    public int adCount;

    [Header("-------------[ Core ]")]
    public bool isOver;
    public bool isStart;
    public int score;
    public int maxLevel;
    public int dongleMaxLevel;
    public int MaxiumLevel;
    

    [Header("-------------[ Object Pooling ]")]
    public GameObject donglePrefeb;
    public GameObject itemPrefeb;
    public Transform dongleGroup;
    public Transform itemGroup;
    public List<Dongle> donglePool;
    public List<Item> itemPool;
    public GameObject effectPrefeb;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;
    [Range(1, 30)]
    public int poolSize;
    [Range(1, 30)]
    public int itemPoolSize;
    public int poolCursor;
    public int itemPoolCursor;
    public Dongle lastDongle;
    public Item lastItem;
    public int count;

    [Header("-------------[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx { LevelUp, Next, Attach, Button, Over };
    int sfxCursor;

    [Header("-------------[ UI ]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public GameObject helpGroup;
    public GameObject helpGroup2;
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;
    public Text foodExText;
    public Text timeText;
    public float _sec;
    public float _min;
    public int foodLevel;
    public Image[] foodImage;

    [Header("-------------[ ETC ]")]
    public GameObject line;
    public GameObject bottom;
    public SpriteRenderer backSprite;
    public float bottomTime;
    public int bottomMaxCount;

    void Start()
    {
        if(!PlayerPrefs.HasKey("AdCount")){
            PlayerPrefs.SetInt("AdCount", adCount);
        } else {
            adCount = 1 + PlayerPrefs.GetInt("AdCount");
            PlayerPrefs.SetInt("AdCount", adCount);
        }
        if(!PlayerPrefs.HasKey("FoodLevel")){
            PlayerPrefs.SetInt("FoodLevel", 2);
            foodLevel = 2;
        } else {
            foodLevel = PlayerPrefs.GetInt("FoodLevel");
        }
    }
    void Awake()
    {
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        itemPool = new List<Item>();
        for(int index = 0; index<poolSize; index++){
            MakeDongle();
        }
        for(int index = 0; index<itemPoolSize; index++){
            MakeItem();
        }

        if(!PlayerPrefs.HasKey("MaxScore")){
            PlayerPrefs.SetInt("MaxScore", 0);
        }

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }

    public void GameStart()
    {
        isStart = true;
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        timeText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        bgmPlayer.Play();
        SfxPlay(Sfx.Button);

        Invoke("NextDongle", 1.5f);
    }
    Item MakeItem()
    {
        // 이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefeb, effectGroup);
        instantEffectObj.name = "Effect" + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // 아이템 생성
        GameObject instantItemObj = Instantiate(itemPrefeb, itemGroup);
        instantItemObj.name = "Item" + itemPool.Count;
        Item instantItem = instantItemObj.GetComponent<Item>();
        instantItem.manager = this;
        instantItem.effect = instantEffect;
        itemPool.Add(instantItem);
        return instantItem;
    }
    Dongle MakeDongle()
    {
        // 이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefeb, effectGroup);
        instantEffectObj.name = "Effect" + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // 동글 생성
        GameObject instantDongleObj = Instantiate(donglePrefeb, dongleGroup);
        instantDongleObj.name = "Dongle" + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.manager = this;
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);
        return instantDongle;
    }
    Dongle GetDongle()
    {
        for(int index=0; index<donglePool.Count; index++){
            poolCursor = (poolCursor + 1) % donglePool.Count;
            if(!donglePool[poolCursor].gameObject.activeSelf){
                return donglePool[poolCursor];
            }
        }
        return MakeDongle();
    }
    Item GetItem()
    {
        for(int index=0; index<itemPool.Count; index++){
            itemPoolCursor = (itemPoolCursor + 1) % itemPool.Count;
            if(!itemPool[itemPoolCursor].gameObject.activeSelf){
                return itemPool[itemPoolCursor];
            }
        }
        return MakeItem();
    }
    void NextDongle()
    {
        if(isOver){
            return;
        }

        count++;
        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine(WaitNext());
    }
    void NextItem()
    {
        if(isOver){
            return;
        }

        lastItem = GetItem();
        lastItem.gameObject.SetActive(true);
        int ran = Random.Range(0, 4);
        if(ran == 0){
            lastItem.type = "Mag";
            lastItem.spriteRenderer.color = new Color(0.1f, 0.2f, 0.8f);
        } else if(ran == 1){
            lastItem.type = "LevelUp";
            lastItem.spriteRenderer.color = new Color(1, 0.6f, 0);
        } else if(ran == 2){
            lastItem.type = "LevelDown";
            lastItem.spriteRenderer.color = new Color(1,0, 0);
        } else if(ran == 3){
            lastItem.type = "Anchor";
            lastItem.spriteRenderer.color = Color.black;
            lastItem.rigid.mass = 1000;
        }
        lastItem.spriteRenderer.sprite = lastItem.sprites[ran];

        SfxPlay(Sfx.Next);
        StartCoroutine(WaitNext());
    }
    IEnumerator WaitNext()
    {
        while(lastDongle != null || lastItem != null){
            yield return null;
        }

        yield return new WaitForSeconds(2.5f);

        if(count % 10 == 0){
            NextItem();
            count = 0;
        } else {
            NextDongle();
        }
        
    }
    public void TouchDown()
    {
        if(count != 0){
            if(lastDongle == null)
            return;
        
            lastDongle.Drag();
        } else {
            if(lastItem == null)
            return;
        
            lastItem.Drag();
        }
        
    }
    public void TouchUp()
    {
        if(count != 0){
            if(lastDongle == null)
                return;

            lastDongle.Drop();
            lastDongle = null;
        } else {
            if(lastItem == null)
                return;

            lastItem.Drop();
            lastItem = null;
        }
    }
    public void GameOver()
    {
        if(isOver){
            return;
        }

        isOver = true;
        isStart = false;
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

        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        subScoreText.text = "Score : " + scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }

    public void Reset()
    {
        if(adCount >= 3){
            // 전면광고 보이기
            admManager.ShowAd();
            PlayerPrefs.SetInt("AdCount", 0);
            SfxPlay(Sfx.Button);
        } else {
            Restart(false);
        }
    }
    public void Restart(bool ad)
    {
        if(ad){
            SceneManager.LoadScene(0);
        } else {
            StartCoroutine(ResetCoroutine());
        }
    }
    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(0);
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
    void Update()
    {
        if(Input.GetButtonDown("Cancel")){
            Application.Quit();
        }
        if(!isStart){
            return;
        }
        _sec += Time.deltaTime;
        timeText.text = string.Format("{0:D2}:{1:D2}", (int)_min, (int)_sec);
        if((int)_sec>59){
            _sec = 0;
            _min++;
        }
        BottomUp();
    }
    void BottomUp()
    {
        if(bottomMaxCount == 8)
            return;
        
        bottomTime += Time.deltaTime;
        // 5분마다 바닥이 올라온다. 최고 y축 8 만큼 올라올 수 있음! 1.5 씩 8번!
        if(bottomTime >= 300){
            bottomTime = 0;
            bottomMaxCount++;
            StartCoroutine(BottomUpRoutine());
            // 레벨 증가
            
            if(dongleMaxLevel < MaxiumLevel){
                dongleMaxLevel++;
            }
        }
    }
    IEnumerator BottomUpRoutine()
    {
        int bottomFrameCount = 0;
        Vector3 nextPos = new Vector3(bottom.transform.position.x, bottom.transform.position.y + 1, 0);

        while(bottomFrameCount < 20){
            bottomFrameCount++;
            bottom.transform.position = Vector3.Lerp(bottom.transform.position, nextPos, 0.05f);

            yield return null;
        }
    }
    void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
    public void HelpButton()
    {
        SfxPlay(Sfx.Button);
        helpGroup.SetActive(true);
    }
    public void CloseButton()
    {
        if(helpGroup2.activeSelf){
            helpGroup2.SetActive(false);
        } else {
            helpGroup.SetActive(false);
        }
    }
    public void HelpNext()
    {
        SfxPlay(Sfx.Button);
        helpGroup2.SetActive(true);
        for(int i = 0; i<foodLevel;i++){
            foodImage[i].color = Color.white;
        }
    }
    public void FoodTouch(int num)
    {
        switch(num){
            case 0 :
                foodExText.text = "Level 1: Sushi";
            break;
            case 1 :
                foodExText.text = "Level 2: Muffins";
            break;
            case 2 :
                foodExText.text = "Level 3: Rice Ball";
            break;
            case 3 :
                foodExText.text = "Level 4: Gimbap";
            break;
            case 4 :
                foodExText.text = "Level 5: Donut";
            break;
            case 5 :
                foodExText.text = "Level 6: Steak";
            break;
            case 6 :
                foodExText.text = "Level 7: Hamburger";
            break;
            case 7 :
                foodExText.text = "Level 8: Snack";
            break;
            case 8 :
                foodExText.text = "Level 9: Water Melon";
            break;
            case 9 :
                foodExText.text = "Level 10: Tomato";
            break;
            case 10 :
                foodExText.text = "Level 11: Pine Apple";
            break;
        }
    }
}
