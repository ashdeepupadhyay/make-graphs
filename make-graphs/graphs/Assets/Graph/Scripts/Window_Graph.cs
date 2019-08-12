using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class ListItem
{
    public Values[] Values;
}
[Serializable]
public class Values
{
    public string Day;
    public int value;
}

public class Window_Graph : MonoBehaviour
{
    private const float API_CHECK_MAXTIME = 1 * 60.0f; //1 minutes
    private float apiCheckCountdown = API_CHECK_MAXTIME;
 
    private static Window_Graph instance;

    [SerializeField]
    private Sprite dot;

    [SerializeField]
    private string URL;

    private RectTransform graphContainer;

    private RectTransform labelTemplateX;
    private RectTransform lableTemplateY;

    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;

    private GameObject tooltipGameObject;
    private RectTransform dashContainer;

    private List<GameObject> gameObjectList;
    private List<IGraphVisualObject> graphVisualObjectList;
    private List<RectTransform> yLabelList;

    // Cached values
    private List<int> valueList;
    private IGraphVisual graphVisual;
    private int maxVisibleValueAmount;
    private Func<int, string> getAxisLabelX = null;
    private Func<float, string> getAxisLabelY = null;

    private float xSize;

    private void Awake()
    {
        instance = this;
        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        lableTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("dashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("dashTemplateY").GetComponent<RectTransform>();
        tooltipGameObject = graphContainer.Find("ToolTip").gameObject;

        gameObjectList = new List<GameObject>();
        yLabelList = new List<RectTransform>();
        graphVisualObjectList = new List<IGraphVisualObject>();

        //List<int> valueList = new List<int>() { 5, 98, 56, 45, 30, 22, 17, 15, 13, 17, 25, 37, 40, 36, 33,60,54,22,67,98,34,25,2,7,87 };

        IGraphVisual lineGraphVisual= new LineGraphVisual(graphContainer, dot, Color.green, Color.white);
        IGraphVisual barGraphVisual=new BarChartVisual(graphContainer, Color.green, 0.8f);

        transform.Find("barGraphButton").GetComponent<Button>().onClick.AddListener(()=>SetGraphVisual(barGraphVisual));
        transform.Find("LineGraphButton").GetComponent<Button>().onClick.AddListener(() => SetGraphVisual(lineGraphVisual));
        transform.Find("increaseVisibleButton").GetComponent<Button>().onClick.AddListener(() => IncreaseGraphVisual());
        transform.Find("decreaseVisibleButton").GetComponent<Button>().onClick.AddListener(() => DecreaseGraphVisual());
        transform.Find("DollarYlabelButton").GetComponent<Button>().onClick.AddListener(() => SetGetAxisLabelY((float _f) => "$" + Mathf.RoundToInt(_f)));
        transform.Find("EuroYlabelButton").GetComponent<Button>().onClick.AddListener(() => SetGetAxisLabelY((float _f) => "€" + Mathf.RoundToInt(_f/1.18f)));

        HideToolTip();
        //ShowGraph(valueList, barGraphVisual, -1, (int _i) => "Day" + (_i + 1), (float _f) => "$" + Mathf.RoundToInt(_f));           
    }
    private void Start()
    {
        StartCoroutine(GetJson());
    }
    void Update()
    {
        apiCheckCountdown -= Time.deltaTime;
        if (apiCheckCountdown <= 0)
        {
            StartCoroutine(GetJson());
            apiCheckCountdown = API_CHECK_MAXTIME;
        }
    }

    IEnumerator GetJson()
    {
        UnityWebRequest www = UnityWebRequest.Get(URL);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);
            ProcessJson(www.downloadHandler.text);
            // Or retrieve results as binary data
            //byte[] results = www.downloadHandler.data;
        }
    }
    private void ProcessJson(string json)
    {
        List<int> valueList = new List<int>();
        ListItem items = JsonUtility.FromJson<ListItem>(json);
        Debug.Log(items.Values.Length);
        for (int i = 0; i < items.Values.Length; i++)
        {
            Debug.Log("***"+items.Values[i].Day+"****"+ items.Values[i].value);
            valueList.Add(items.Values[i].value);
        }
        IGraphVisual barGraphVisual = new BarChartVisual(graphContainer, Color.green, 0.8f);
        ShowGraph(valueList, barGraphVisual, -1, (int _i) => "Day" + (_i + 1), (float _f) => "$" + Mathf.RoundToInt(_f));           
    }
    private void ShowTooltip(string toolTipText, Vector2 anchoredPosition)
    {
        tooltipGameObject.SetActive(true);
        tooltipGameObject.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
        Text tooltipUIText = tooltipGameObject.transform.Find("Text").GetComponent<Text>();
        tooltipUIText.text = toolTipText;
        float textPadding = 4f;
        Vector2 backgroundSize = new Vector2(
            tooltipUIText.preferredWidth + textPadding * 2f,
            tooltipUIText.preferredHeight + textPadding * 2f);
        tooltipGameObject.transform.Find("background").GetComponent<RectTransform>().sizeDelta = backgroundSize;
        tooltipGameObject.transform.Find("background").GetComponent<RectTransform>().anchoredPosition = new Vector2(backgroundSize.x / 2, backgroundSize.y / 2);
        tooltipGameObject.transform.SetAsLastSibling();
    }
    public static void ShowTooltip_Static(string toolTipText, Vector2 anchoredPosition)
    {
        instance.ShowTooltip(toolTipText, anchoredPosition);
    }
    public static void HideTooltip_Static()
    {
        instance.HideToolTip();
    }
    private void HideToolTip()
    {
        tooltipGameObject.SetActive(false);
    }
    private void SetGetAxisLabelX(Func<int,string> getAxisLabelX)
    {
        ShowGraph(this.valueList, this.graphVisual, this.maxVisibleValueAmount, getAxisLabelX, this.getAxisLabelY);
    }
    private void SetGetAxisLabelY(Func<float, string> getAxisLabelY)
    {
        ShowGraph(this.valueList, this.graphVisual, this.maxVisibleValueAmount, this.getAxisLabelX, getAxisLabelY);
    }
    private void SetGraphVisual(IGraphVisual graphVisual)
    {
        ShowGraph(this.valueList, graphVisual,this.maxVisibleValueAmount,this.getAxisLabelX,this.getAxisLabelY);
    }

    private void IncreaseGraphVisual()
    {
        ShowGraph(this.valueList, graphVisual, this.maxVisibleValueAmount+1, this.getAxisLabelX, this.getAxisLabelY);
    }

    private void DecreaseGraphVisual()
    {
        ShowGraph(this.valueList, graphVisual, this.maxVisibleValueAmount - 1, this.getAxisLabelX, this.getAxisLabelY);
    }
    private void CalculateYScale(out float yMinimum, out float yMaximum)
    {
        // Identify y Min and Max values
        yMaximum = valueList[0];
        yMinimum = valueList[0];

        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
        {
            int value = valueList[i];
            if (value > yMaximum)
            {
                yMaximum = value;
            }
            if (value < yMinimum)
            {
                yMinimum = value;
            }
        }

        float yDifference = yMaximum - yMinimum;
        if (yDifference <= 0)
        {
            yDifference = 5f;
        }
        yMaximum = yMaximum + (yDifference * 0.2f);
        yMinimum = yMinimum - (yDifference * 0.2f);     
    }
    private void ShowGraph(List<int> valueList,IGraphVisual graphVisual, int maxVisibleValueAmount=-1,Func<int,string>getAxisLabelX=null, Func<float, string> getAxisLabelY=null)
    {
        this.valueList = valueList;
        this.graphVisual = graphVisual;
        this.getAxisLabelX = getAxisLabelX;
        this.getAxisLabelY = getAxisLabelY;
        
        if (maxVisibleValueAmount <= 0)
        {
            maxVisibleValueAmount = valueList.Count;
        }
        if (maxVisibleValueAmount > valueList.Count)
        {
            maxVisibleValueAmount = valueList.Count;
        }
        this.maxVisibleValueAmount = maxVisibleValueAmount;

        if (getAxisLabelX == null)
        {
            getAxisLabelX = delegate (int _i) { return _i.ToString(); };
        }
        if (getAxisLabelY == null)
        {
            getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        }

        foreach (GameObject gameObject in gameObjectList)
        {
            Destroy(gameObject);
        }
        gameObjectList.Clear();
        yLabelList.Clear();

        foreach (IGraphVisualObject graphVisualObject in graphVisualObjectList)
        {
            graphVisualObject.CleanUp();
        }
        graphVisualObjectList.Clear();
        graphVisual.CleanUp();

        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        float yMinimum, yMaximum;
        CalculateYScale(out yMinimum, out yMaximum);
        xSize = graphWidth / (maxVisibleValueAmount + 1);
        
        int xIndex = 0;

        for (int i=Mathf.Max(valueList.Count-maxVisibleValueAmount,0); i < valueList.Count;++i)
        {
            float xposition = xSize+xIndex * xSize;
            float yposition = ((valueList[i]- yMinimum) / (yMaximum- yMinimum)) * graphHeight;
            
            string toolTipText = getAxisLabelY(valueList[i]);
            IGraphVisualObject graphVisualObject = graphVisual.CreateGraphVisualObject(new Vector2(xposition, yposition), xSize, toolTipText);
            graphVisualObjectList.Add(graphVisualObject);

            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainer);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(xposition, -20f);
            labelX.GetComponent<Text>().text = getAxisLabelX(i);
            gameObjectList.Add(labelX.gameObject);

            RectTransform dashX = Instantiate(dashTemplateX);
            dashX.SetParent(graphContainer);
            dashX.gameObject.SetActive(true);
            dashX.anchoredPosition = new Vector2(xposition, 0f);
            gameObjectList.Add(dashX.gameObject);
            xIndex++;
        }

        int sepraterCount = 10;
        for(int i = 0; i <= sepraterCount; i++)
        {
            RectTransform labelY = Instantiate(lableTemplateY);
            labelY.SetParent(graphContainer,false);
            labelY.gameObject.SetActive(true);
            float normalisedValue = i * 1f / sepraterCount;
            labelY.anchoredPosition = new Vector2(-80f, normalisedValue*graphHeight);
            labelY.GetComponent<Text>().text = getAxisLabelY(yMinimum + (normalisedValue * (yMaximum- yMinimum)));
            yLabelList.Add(labelY);
            gameObjectList.Add(labelY.gameObject);

            RectTransform dashY = Instantiate(dashTemplateY);
            dashY.SetParent(dashContainer, false);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(0f, normalisedValue * graphHeight);
            gameObjectList.Add(dashY.gameObject);

        }
    }

    private void UpdateValue(int index,int value)
    {
        float yMinimumBefore, yMaximumBefore;
        CalculateYScale(out yMinimumBefore, out yMaximumBefore);
        valueList[index] = value;

        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;

        float yMinimum, yMaximum;
        CalculateYScale(out yMinimum, out yMaximum);

        bool yScaleChanged = yMinimumBefore != yMinimum || yMaximumBefore != yMaximum;
        if (!yScaleChanged)
        {
            // Y Scale did not change, update only this value
            float xPosition = xSize + index * xSize;
            float yPosition = ((value - yMinimum) / (yMaximum - yMinimum)) * graphHeight;

            // Add data point visual
            string tooltipText = getAxisLabelY(value);
            graphVisualObjectList[index].SetGraphVisualObjectInfo(new Vector2(xPosition, yPosition), xSize, tooltipText);
        }
        else
        {
            // Y scale changed, update whole graph and y axis labels
            // Cycle through all visible data points
            int xIndex = 0;
            for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
            {
                float xPosition = xSize + xIndex * xSize;
                float yPosition = ((valueList[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;

                // Add data point visual
                string tooltipText = getAxisLabelY(valueList[i]);
                graphVisualObjectList[xIndex].SetGraphVisualObjectInfo(new Vector2(xPosition, yPosition), xSize, tooltipText);

                xIndex++;
            }
            for (int i = 0; i < yLabelList.Count; i++)
            {
                float normalizedValue = i * 1f / yLabelList.Count;
                yLabelList[i].GetComponent<Text>().text = getAxisLabelY(yMinimum + (normalizedValue * (yMaximum - yMinimum)));
            }
        }
    }
    private interface IGraphVisual
    {
        IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth,string toolTipText);
        void CleanUp();
    }

    private interface IGraphVisualObject
    {
        void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string toolTipText);
        void CleanUp();
    }
    
    

    private class BarChartVisual: IGraphVisual
    {
        private RectTransform graphContainer;
        private Color barColor;
        private float barWidthMultiplier;
        public BarChartVisual(RectTransform graphContainer,Color barColor,float barWidthMultiplier)
        {
            this.graphContainer = graphContainer;
            this.barColor = barColor;
            this.barWidthMultiplier = barWidthMultiplier;
        }
        public void CleanUp()
        {

        }
        public IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition,float graphPositionWidth,string toolTipText)
        {
            GameObject barGameObject = CreateBar(graphPosition, graphPositionWidth);

            BarChartVisualObject barChartVisualObject = new BarChartVisualObject(barGameObject, barWidthMultiplier);
            barChartVisualObject.SetGraphVisualObjectInfo(graphPosition, graphPositionWidth, toolTipText);

            return barChartVisualObject;
        }
        private GameObject CreateBar(Vector2 graphPostion, float barWidth)
        {
            GameObject gameObject = new GameObject("bar", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().color = barColor;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(graphPostion.x, 0f);
            rectTransform.sizeDelta = new Vector2(barWidth*barWidthMultiplier, graphPostion.y);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(.5f, 0f);
            MouseHover barMouseHover = gameObject.AddComponent<MouseHover>();

            return gameObject;
        }
        public class BarChartVisualObject : IGraphVisualObject
        {
            private GameObject barGameObject;
            private float barWidthMultiplier;
            public BarChartVisualObject(GameObject barGameObject,float barWidthMultiplier)
            {
                this.barGameObject = barGameObject;
                this.barWidthMultiplier = barWidthMultiplier;

            }
            public void CleanUp()
            {
                Destroy(barGameObject);
            }
            public void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string toolTipText)
            {
                RectTransform rectTransform = barGameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(graphPosition.x, 0f);
                rectTransform.sizeDelta = new Vector2(graphPositionWidth * barWidthMultiplier, graphPosition.y);
                MouseHover barMouseHover = barGameObject.GetComponent<MouseHover>();

                barMouseHover.MouseOverOnceFunc = () =>
                {
                    ShowTooltip_Static(toolTipText, graphPosition);
                };
                barMouseHover.MouseOutOnceFunc = () =>
                {
                    HideTooltip_Static();
                };
            }
        }
    }

    private class LineGraphVisual: IGraphVisual
    {
        private RectTransform graphContainer;
        private Sprite dotSprite;
        private LineGraphVisualObject lastLineGraphVisualObject;
        private Color dotColor;
        private Color dotConnectionColor;

        public LineGraphVisual(RectTransform graphContainer,Sprite dotSprite,Color dotColor,Color dotConnectionColor)
        {
            this.graphContainer = graphContainer;
            this.dotSprite = dotSprite;
            this.lastLineGraphVisualObject = null;
            this.dotColor = dotColor;
            this.dotConnectionColor = dotConnectionColor;
        }

        public void CleanUp()
        {
            lastLineGraphVisualObject = null;
        }

        public IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string toolTipText)
        {           
            GameObject dotGameObject = CreateDot(graphPosition);
 
            GameObject dotConnectionGameObject=null;
            if (lastLineGraphVisualObject != null)
            {
                dotConnectionGameObject = CreateDotConnection(lastLineGraphVisualObject.GetGraphPosition(), dotGameObject.GetComponent<RectTransform>().anchoredPosition);
            }

            LineGraphVisualObject lineGraphVisualObject = new LineGraphVisualObject(dotGameObject, dotConnectionGameObject, lastLineGraphVisualObject);
            lineGraphVisualObject.SetGraphVisualObjectInfo(graphPosition, graphPositionWidth, toolTipText);
            lastLineGraphVisualObject = lineGraphVisualObject;

            return lineGraphVisualObject;
        }
        private GameObject CreateDot(Vector2 anchoredPosition)
        {
            GameObject gameObject = new GameObject("dot", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().sprite = dotSprite;
            gameObject.GetComponent<Image>().color = dotColor;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(11, 11);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            MouseHover dotMouseHover = gameObject.AddComponent<MouseHover>();
            
            return gameObject;
        }

        public GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
        {
            GameObject gameObject = new GameObject("dot_connection", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().color = dotConnectionColor;
            gameObject.GetComponent<Image>().raycastTarget = false;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            Vector2 dir = (dotPositionB - dotPositionA).normalized;
            float distance = Vector2.Distance(dotPositionA, dotPositionB);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(distance, 3f);
            rectTransform.anchoredPosition = dotPositionA + dir * distance * 0.5f;
            rectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVector(dir));

            return gameObject;
        }
        public class LineGraphVisualObject : IGraphVisualObject
        {
            public event EventHandler OnChangeGraphVisualObjectInfo;
            private GameObject dotGameObject;
            private GameObject dotConnectionGameObject;
            private LineGraphVisualObject lastVisualObject;

            public void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string toolTipText)
            {
                RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition =graphPosition;
                UpdateDotConnection();
                MouseHover dotMouseHover = dotGameObject.GetComponent<MouseHover>();

                dotMouseHover.MouseOverOnceFunc = () =>
                {
                    ShowTooltip_Static(toolTipText, graphPosition);
                };
                dotMouseHover.MouseOutOnceFunc = () =>
                {
                    HideTooltip_Static();
                };
                if (OnChangeGraphVisualObjectInfo != null) OnChangeGraphVisualObjectInfo(this, EventArgs.Empty);

            }
            private void UpdateDotConnection()
            {
                if (dotConnectionGameObject != null)
                {
                    RectTransform dotConnectionRectTransform = dotConnectionGameObject.GetComponent<RectTransform>();
                    Vector2 dir = (lastVisualObject.GetGraphPosition() - GetGraphPosition()).normalized;
                    float distance = Vector2.Distance(GetGraphPosition(), lastVisualObject.GetGraphPosition());
                    dotConnectionRectTransform.anchorMin = new Vector2(0, 0);
                    dotConnectionRectTransform.anchorMax = new Vector2(0, 0);
                    dotConnectionRectTransform.sizeDelta = new Vector2(distance, 3f);
                    dotConnectionRectTransform.anchoredPosition = GetGraphPosition() + dir * distance * 0.5f;
                    dotConnectionRectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVector(dir));
                }
            }
            public void CleanUp()
            {
                Destroy(dotGameObject);
                Destroy(dotConnectionGameObject);
            }
            public Vector2 GetGraphPosition()
            {
                RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                return rectTransform.anchoredPosition;
            }
            public LineGraphVisualObject(GameObject dotGameObject,GameObject dotConnectionGameObject,LineGraphVisualObject lastVisualObject)
            {
                this.dotGameObject = dotGameObject;
                this.dotConnectionGameObject = dotConnectionGameObject;
                this.lastVisualObject = lastVisualObject;

                if(lastVisualObject!=null)
                {
                    lastVisualObject.OnChangeGraphVisualObjectInfo += LastVisualObject_OnChangeGraphVisualObjectInfo;
                }
            }
            private void LastVisualObject_OnChangeGraphVisualObjectInfo(object sender,EventArgs e)
            {
                UpdateDotConnection();
            }
            public int GetAngleFromVector(Vector3 dir)
            {
                dir = dir.normalized;
                float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                if (n < 0) n += 360;
                int angle = Mathf.RoundToInt(n);
                return angle;
            }
        }
        public int GetAngleFromVector(Vector3 dir)
        {
            dir = dir.normalized;
            float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (n < 0) n += 360;
            int angle = Mathf.RoundToInt(n);
            return angle;
        }
    }
}
