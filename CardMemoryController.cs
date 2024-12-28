using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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

    // スコア表記用テキスト・ゲームオブジェクト
    [Header("スコア表記用テキスト（ゲームオブジェクト設置）"), SerializeField]
    private GameObject scoreTextObj;

    private TextMeshProUGUI scoreText;

    // Start is called before the first frame update
    void Start()
    {
        // スコアを初期化
        memoryGameScoreInt = 0;

        // score用表示のテキストコンポーネントを用意
        scoreText = scoreTextObj.GetComponent<TextMeshProUGUI>();

        // Resourcesのカードフォルダからカルタを読み込む
        cardResourceRandom = Resources.LoadAll("MemoryGameCard", typeof(GameObject)).Cast<GameObject>().ToArray();
        // Resourcesの意味カードフォルダからカルタを読み込む
        meanResourceRandom = Resources.LoadAll("MemoryGameCard", typeof(GameObject)).Cast<GameObject>().ToArray();
        
        // Shuffleメソッドからそれぞれ取得
        cardResourceRandom = Shuffle(cardResourceRandom);


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <Summary>=====================================
    /// リソースファイルにあるカードすべてをシャッフル
    /// シャッフルしたカードを出題用の８枚に設定する
    /// カルタのモデルと出題テキストは一緒になっているため、
    /// それぞれ出現させたら、非表示にさせる。
    /// </Summary>>=====================================

    private void Shuffle (GameObject[] gameObjectList) {
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
    }
}
