using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using System.Reflection;
using UnityEngine.SceneManagement;
using DG.Tweening;


public class CardMemoryController : MonoBehaviour
{
    // カードの出現初期位置を設定（Deck）
    [Header("カードの出現初期位置を設定（Deck）"), SerializeField]
    private GameObject cardStartDeck;

    // カードフィールドを設定
    [Header("カードフィールドを設定(配置)"), SerializeField]
    private GameObject[] cardFieldsObj;

    // 意味カードフィールドを設定
    [Header("意味カードフィールドを設定(配置)"), SerializeField]
    private GameObject[] meanFieldsObj;

    // Resourcesのカードをすべて配列に保管
    private GameObject[] cardResourceRandom = new GameObject[9];

    // Resourcesの意味カードをすべて配列に保管
    private GameObject[] meanResourceRandom = new GameObject[9];

    // カードDoTween処理用・出現用配列
    private GameObject[] tweenInstantiateCard = new GameObject[8];

    // 意味カードDoTween処理用・出現用配列
    private GameObject[] tweenInstantiateMean = new GameObject[8];

    // スコア加点用変数
    [HideInInspector]
    public static int memoryGameScoreInt;

    /// <Summary>=====================================
    /// 制限時間の変数設定
    /// </Summary>=====================================
    /// 
    // 制限時間の設定
    [Header("制限時間の設定(分単位：int)"), SerializeField]
    private int timeLimitMinutes;

    [Header("制限時間の設定(秒単位：float)"), SerializeField]
    private float timeLimitSeconds = 0f;

    // タイマー用GameObject, Text
    [Header("制限時間のタイマー表記ゲームオブジェクト(TMP)"), SerializeField]
    private GameObject timerTextObj;

    private TextMeshProUGUI timerText;

    // 経過時間
    private float time;

    // 問題の出題数を記録
    private int questionNum = 0;

    /// <Summary>=====================================
    /// 説明文とスタートを表記するための変数
    /// </Summary>>===================================
    ///
    // 説明文のゲームオブジェクト
    [Header("説明文表記のゲームオブジェクトを設置"), SerializeField]
    private GameObject introductionTextObj;

    // スタート表記のゲームオブジェクト
    [Header("スタート表記のゲームオブジェクトを設置"), SerializeField]
    private GameObject startImageObj;
    // スコア表記用テキスト・ゲームオブジェクト
    [Header("スコア表記用テキスト（ゲームオブジェクト設置）"), SerializeField]
    private GameObject scoreTextObj;

    private TextMeshProUGUI scoreText;

    /// <Summary>=====================================
    /// 主にUpdateで使用するもの
    /// </Summary>=====================================
    /// 
    //クリックされたゲームオブジェクトを代入する変数
    private GameObject clickedGameObject;

    // Startするまでのストッパー
    private bool startStopper = true;
    // 表示が点滅しないように
    private bool startActiveBool = true;
    // それぞれのカードを２重にひっくり返さないように
    private bool tagBoolMeen = false;
    private bool tagBoolCard = false;

    string tag2Name = "";
    string tag3Name = "";
    int cardOpen = 0;



    // Start is called before the first frame update
    void Start()
    {
        // スコアを初期化
        memoryGameScoreInt = 0;

        // score用表示のテキストコンポーネントを用意
        scoreText = scoreTextObj.GetComponent<TextMeshProUGUI>();

        // Resourcesのカードフォルダからカルタを読み込む
        cardResourceRandom = Resources.LoadAll<GameObject>("MemoryGameCard").ToArray();
        // Resourcesの意味カードフォルダからカルタを読み込む
        meanResourceRandom = Resources.LoadAll<GameObject>("MemoryGameMean").ToArray();
        
        // Shuffleメソッドからそれぞれ取得
        cardResourceRandom = Shuffle(cardResourceRandom);
        meanResourceRandom = Shuffle(meanResourceRandom);

        // フィールドに置くカードを配列に入れる
        for (int i = 0; i < tweenInstantiateCard.Length; i++) {
            tweenInstantiateCard[i] = cardResourceRandom[i];
            tweenInstantiateMean[i] = meanResourceRandom[i];
        }

        // 説明文を表示
        StartCoroutine(IntroductionActive());
        
        /// <Summary>=====================================
        /// 最初のカード演出
        /// デッキにカードを配置（コルーチン）
        /// 左右にカードを所定位置に展開（コルーチン）
        /// おもてにひっくり返す（コルーチン）
        /// カードを裏返す（コルーチン）
        /// スタート表示
        /// </Summary>>===================================
        ///

        StartCoroutine(DeckStart());

        /// <Summary>
        /// 制限時間の設定
        /// 60をかけて分に直す
        /// textをgetcomponentして反映する。
        /// </Summary>>
        /// 
        
        timerText = timerTextObj.GetComponent<TextMeshProUGUI>();
        timeLimitSeconds = timeLimitSeconds + timeLimitMinutes * 60;
    }

    // Update is called once per frame
    void Update()
    {
        
        // スタート表記が終わるまでゲームを起動させない
        if (startStopper) {
            StartCoroutine(StartActive());
        }else {
            /// <Summary>=====================================
            /// 制限時間の始まり
            /// 時間の表記
            /// </Summary>>=====================================
            ///
            timeLimitSeconds -= Time.deltaTime;
            var span = new TimeSpan(0, 0, (int)timeLimitSeconds);
            timerText.text = span.ToString(@"mm\:ss");

            /// <Summary>
            /// 問題テキストがなくなるまで続ける
            /// questionNumが8を超えたら
            /// また、時間切れでも終了
            /// </Summary>>
            ///
            if (questionNum < 8 && timeLimitSeconds >= 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit = new RaycastHit();
                    if (Physics.Raycast(ray, out hit))
                    {
                        clickedGameObject = hit.collider.gameObject;
                        
                        string tagName = clickedGameObject.transform.GetChild(0).gameObject.tag;
                        //string tagName = clickedGameObject.gameObject.tag;
                        
                        
                        Debug.Log("tagName : " + tagName);
                        
                        
                        // 最初のタッチ・クリック（クリックした後、同じゾーンではタッチしても開かない。） 
                        if (tagName == "Mean" && !tagBoolMeen) {
                            // クリック・タッチすると表にひっくり返る
                            clickedGameObject.transform.DOLocalRotate(new Vector3(180, 90, 90), 1f).SetEase(Ease.Linear);
                            tag2Name = clickedGameObject.gameObject.tag;
                            Debug.Log("tag2Name : " + tag2Name);
                            cardOpen += 1;
                            tagBoolMeen = true;
                            
                        }else if (tagName == "Card" && !tagBoolCard) {
                            clickedGameObject.transform.DOLocalRotate(new Vector3(-180, 180, 90), 1f).SetEase(Ease.Linear);
                            tag3Name = clickedGameObject.gameObject.tag;
                            Debug.Log("tag3Name : " + tag3Name);
                            cardOpen += 1;
                            tagBoolCard = true;
                        } 

                        if(tag2Name == tag3Name && cardOpen >= 2) {
                            Debug.Log("sss");
                            Destroy(clickedGameObject);
                            GameObject destroyObj = GameObject.FindWithTag("tag3Name");
                            Destroy(destroyObj);
                        }
                    }
        
                }
            }
        }
    }

    /// <Summary>=====================================
    /// リソースファイルにあるカードすべてをシャッフル
    /// シャッフルしたカードを出題用の８枚に設定する
    /// カルタのモデルと出題テキストは一緒になっているため、
    /// それぞれ出現させたら、非表示にさせる。
    /// </Summary>>=====================================

    private GameObject[] Shuffle (GameObject[] gameObjectList) {
        // Resourcesから取得したカードをいったんシャッフルする。
        int n = meanResourceRandom.Length;

        // フィッシャー・イエーツのシャッフルアルゴリズムを実装
        for (int i = n - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            // 選択した要素と最後の要素を交換
            GameObject tmp = gameObjectList[i];
            gameObjectList[i] = gameObjectList[j];
            gameObjectList[j] = tmp;
        }

        return gameObjectList;
    }

    /// <Summary>=====================================
    /// デッキにカードを配置する（ゲーム上に表示）
    /// </Summary>>=====================================
    private void DeckPreparationCard(GameObject[] gameObjectList) {        
        int o = 0;
        foreach(GameObject obj in gameObjectList) {
            var card = Instantiate(obj, new Vector3(cardStartDeck.transform.position.x, cardStartDeck.transform.position.y, cardStartDeck.transform.position.z), Quaternion.Euler(-180, 180, -90));
            gameObjectList[o] = card;
            o++;
        }
    }

    /// <Summary>=====================================
    /// カードと意味カードをそれぞれ左右に展開するメソッド
    /// </Summary>>=====================================
    private void LeftandRightDeployment() {
        int o = 0;
        foreach(GameObject obj in tweenInstantiateCard) {
            obj.transform.DOMove(new Vector3(cardFieldsObj[o].transform.position.x, cardFieldsObj[o].transform.position.y+0.3f, cardFieldsObj[o].transform.position.z), 2f);
            o++;
        }

        int l = 0;
        foreach(GameObject obj in tweenInstantiateMean) {
            obj.transform.DOMove(new Vector3(meanFieldsObj[l].transform.position.x, meanFieldsObj[l].transform.position.y+0.3f, meanFieldsObj[l].transform.position.z), 2f);
            l++;
        }
    }

    /// <Summary>=====================================
    /// カードを表にひっくり返すメソッド
    /// </Summary>>=====================================
    private void OpenCardWait() {
        foreach(GameObject obj in tweenInstantiateCard) {
            obj.transform.DOLocalRotate(new Vector3(-180, 180, 90), 1f)
            .SetEase(Ease.Linear);
        }

        foreach(GameObject obj in tweenInstantiateMean) {
            obj.transform.DOLocalRotate(new Vector3(180, 90, 90), 1f)
            .SetEase(Ease.Linear);
        }
    }

    /// <Summary>=====================================
    /// カードを裏にひっくり返すメソッド
    /// </Summary>>=====================================
    private void CloseCardWait() {
        foreach(GameObject obj in tweenInstantiateCard) {
            obj.transform.DOLocalRotate(new Vector3(0, 0,90), 1f)
            .SetEase(Ease.Linear);
        }

        foreach(GameObject obj in tweenInstantiateMean) {
            obj.transform.DOLocalRotate(new Vector3(0, -90, 90), 1f)
            .SetEase(Ease.Linear);
        }
    }

    /// <summary>=====================================
    /// 説明文表示用コルーチン
    /// </summary>=====================================
    /// <returns></returns>
    IEnumerator IntroductionActive()
    {
        // 説明文を表示
        introductionTextObj.SetActive(true);
        // 4秒表示
        yield return new WaitForSeconds(8f);
        // 説明文は非表示、スタートは表示
        introductionTextObj.SetActive(false);
        //introductionStopper = false;
    }

    /// <summary>=====================================
    /// デッキ配置用コルーチン
    /// </summary>=====================================
    /// <returns></returns>
    IEnumerator DeckStart()
    {
        // 説明文の待機をまず先に行う
        yield return new WaitForSeconds(6.5f);
        // デッキに配置
        DeckPreparationCard(tweenInstantiateCard);
        DeckPreparationCard(tweenInstantiateMean);
        yield return new WaitForSeconds(4.0f);
        LeftandRightDeployment();
        yield return new WaitForSeconds(4.0f);
        OpenCardWait();
        yield return new WaitForSeconds(10.0f);
        CloseCardWait();
    }

    /// <summary>=====================================
    /// スタート表示用コルーチン
    /// </summary>=====================================
    /// <returns></returns>
    IEnumerator StartActive() {
        yield return new WaitForSeconds(29f);
        if (startActiveBool) {
            startImageObj.SetActive(true);
            startActiveBool = false;
        }   
        yield return new WaitForSeconds (2f);
        startImageObj.SetActive(false);
        startStopper = false;
    }
}
