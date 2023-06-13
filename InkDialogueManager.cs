using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.UI;
using TMPro;

using System.IO;

public class InkDialogueManager : MonoBehaviour
{
    public static event Action<Story> OnCreateStory;

    //Ink file assest
    [SerializeField]
    [Header("把Ink Json文件放在这里 Put the Json File here")]
    private TextAsset inkJSONAsset = null;
    // The story class generated from the json file
    public Story story;
    [SerializeField]
    public List<AllExpressions> allExpressions;
    //The canvas that display all the story text
    private Transform canvas = null;
    //Children of the ui component
    private Transform dialogueBox;
    private Transform dialogueBoxText;
    private Transform characterNameBox;
    private Transform characterNameText;
    private Transform choiceBox;
    private Transform choiceButton;
    private Transform characterBox;
    private Transform LogBox;
    private Transform LogText;
    private List<Transform> characterImages=new List<Transform>();
    private List<string> charactersPos=new List<string>();

    private string savePath = Application.dataPath + "/StreamingAssets/" ;



    void Awake()
    {
        //Find the components
        canvas = transform.Find("UI Canvas");
        dialogueBox = canvas.Find("Dialogue Box");
        dialogueBoxText=dialogueBox.Find("Text");
        characterNameBox = dialogueBox.Find("Character Name Box");
        characterNameText=characterNameBox.Find("Text");
        choiceBox = canvas.Find("Choice Box");  
        choiceButton=choiceBox.Find("Choice Button");
        characterBox = canvas.Find("Character Box");
        LogBox = canvas.Find("Log Box");
        LogText=LogBox.Find("Viewport/Content/Text");
        
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        for (int i = 0; i < characterBox.childCount;i++)
        {
            characterImages.Add(characterBox.GetChild(i));
            charactersPos.Add("");
        }
        StartStory();
    }

    // Creates a new Story object with the compiled story which we can then play!
    void StartStory()
    {
        story = new Story(inkJSONAsset.text);
        if (OnCreateStory != null) OnCreateStory(story);
        ContinueStory();
    }

    public void ContinueStory()
    {
        
        if (story.canContinue)
        {

            string text = story.Continue();
            //删除收尾的空格
            // This removes any white space from the text.
            text = text.Trim();
            //找到这一行的tags
            //Find the tags of this line
            List<string> tags = story.currentTags;
            // 将文字打印出来
            // Display the text on screen!
            PrintContent(text,tags);
        }
        else if ((story.currentChoices.Count > 0))
        {
            choiceBox.gameObject.SetActive(true) ;
            if (story.currentChoices.Count > choiceBox.childCount)
            {
                Transform newButton = Instantiate(choiceButton,choiceBox);
            }
            for (int i = 0; i < choiceBox.childCount; i++)
            {
                if (i < (story.currentChoices.Count))
                {
                    Choice choice = story.currentChoices[i];
                    choiceBox.GetChild(i).gameObject.SetActive(true);
                    choiceBox.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
                    choiceBox.GetChild(i).GetComponent<Button>().onClick.AddListener(delegate { OnClickChoiceButton(choice); });
                    choiceBox.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = choice.text.Trim();

                }
                else {
                    choiceBox.GetChild(i).gameObject.SetActive(false);
                }
                
            }
        }
        else
        {
            Debug.Log("story ends");
        }
    }

    public void PrintContent(string content, List<string> tags)
    {
        /*重要的规则 Important rules
         * 1. #CHAR:A:Happy 
         * 角色A 开心的立绘
         * Show Character A happy image
         * 2.#POS:0
         * 0-left,1-centre,2-right
         */
        string[] tagSplit;
        Sprite characterImage=null;
        string characterName="";
        int characterPos=2;
        for (int i = 0; i < tags.Count; i++)
        {
            if (!tags[i].Contains(":"))
            {
                continue;
            }

            tagSplit = tags[i].Trim().Split(":");
            //Character tag
            if (tagSplit[0] == "CHAR")
            {
                characterName= tagSplit[1];
                Debug.Log(tagSplit[2]);
                for (int j = 0;j<allExpressions.Count;j++)
                {
                    if (allExpressions[j].characterName == characterName)
                    {
                        for(int k = 0; k < allExpressions[j].expressions.Length; k++)
                        {
                            if (allExpressions[j].expressions[k].expression == tagSplit[2])
                            {
                                characterImage = allExpressions[j].expressions[k].img;
                            }

                        }
                    }
                }
            }
            else if (tagSplit[0] == "POS") {
                characterPos = Int32.Parse(tagSplit[1]);
            }
        }
        //改变角色立绘
        if (characterImage != null)
        {
            if (charactersPos.Contains(characterName))
            {
                characterImages[charactersPos.IndexOf(characterName)].gameObject.SetActive(false);
                charactersPos[charactersPos.IndexOf(characterName)] = "";
            }
            characterImages[characterPos].gameObject.SetActive(true);
            characterImages[characterPos].GetComponent<Image>().sprite = characterImage;
            charactersPos[characterPos]=characterName;
        }
        if (characterName != "")
        {
            characterNameBox.gameObject.SetActive(true);
            characterNameText.GetComponent<TMP_Text>().text = characterName;
        }
        else
        {
            characterNameBox.gameObject.SetActive(false);
        }

        //改变对话框内文字 Change text
        dialogueBoxText.GetComponent<TMP_Text>().text = content;
        LogText.GetComponent<TMP_Text>().text+= "<color=red>"+characterName + "</color=red>"+"    " + content+ " <br>";
    }
   

    //按choice后
    // after press choice
    void OnClickChoiceButton(Choice choice)
    {
        story.ChooseChoiceIndex(choice.index);
        choiceBox.gameObject.SetActive(false);
        ContinueStory();
    }
    [Serializable]
    public class AllExpressions
    {
        //角色名字 character Name
        public string characterName;
        public Expression[] expressions;

    }
    [Serializable]
    public class Expression
    {
        //表情 expression
        public string expression;
        //图片 sprite
        public Sprite img;

    }
# region 存档 SL
    public string SaveInk()
    {
        string savedJson = story.state.ToJson();
        return savedJson;
    }


    public void LoadInk(string savedJson)
    {
        story.state.LoadJson(savedJson);
    }

    public void OnSave()
    {
        string json =SaveInk();
        File.WriteAllText(savePath + "01.json", json);


    }
    public void OnLoad()
    {
        string json = File.ReadAllText(savePath + "01.json");
        LoadInk(json);
        ContinueStory() ;
    }
    public void OnLog()
    {
        LogBox.gameObject.SetActive(true);

    }
    #endregion
    #region RPG游戏专用
    public void OnCheck()
    {

    }
    public void SetCurrentIndex(string ChoiceText)
    {
        bool isFind = false;
        for (int i = 0; i < story.currentChoices.Count; i++)
        {
            if (story.currentChoices[i].text == ChoiceText)
            {
                if (isFind)
                {
                    Debug.Log("BUG：不能有重复的选项——" + ChoiceText);
                    break;

                }
                story.ChooseChoiceIndex(story.currentChoices[i].index);
                isFind = true;
            }
        }

        if (!isFind)
        {
            Debug.Log("Warning：选项不存在" + ChoiceText);
        }
        else
        {
            ContinueStory();
        }

    }
    #endregion

}


