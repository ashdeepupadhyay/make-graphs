using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Graph : MonoBehaviour
{
    private static Window_Graph instance;
    private RectTransform graphContainer;
    [SerializeField]
    private Sprite dot;

    private RectTransform labelTemplateX;
    private RectTransform lableTemplateY;

    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;

    private GameObject tooltipGameObject;

    private List<GameObject> gameObjectList;

    private List<int> valueList;
    private IGraphVisual graphVisual;
    private int maxVisibleValueAmount;
    private Func<int, string> getAxisLabelX = null;
    private Func<float, string> getAxisLabelY = null;

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
        //CreateCircle(new Vector2(200,200));
        List<int> valueList = new List<int>() { 5, 98, 56, 45, 30, 22, 17, 15, 13, 17, 25, 37, 40, 36, 33,60,54,22,67,98,34,25,2,7,87 };
        //List<int> valueList = new List<int>() { 5};
        IGraphVisual lineGraphVisual= new LineGraphVisual(graphContainer, dot, Color.green, Color.white);
        IGraphVisual barGraphVisual=new BarChartVisual(graphContainer, Color.green, 0.8f);
        ShowGraph(valueList, barGraphVisual, -1,(int _i)=>"Day"+(_i+1),(float _f)=>"$"+Mathf.RoundToInt(_f));
        transform.Find("barGraphButton").GetComponent<Button>().onClick.AddListener(()=>SetGraphVisual(barGraphVisual));
        transform.Find("LineGraphButton").GetComponent<Button>().onClick.AddListener(() => SetGraphVisual(lineGraphVisual));
        transform.Find("increaseVisibleButton").GetComponent<Button>().onClick.AddListener(() => IncreaseGraphVisual());
        transform.Find("decreaseVisibleButton").GetComponent<Button>().onClick.AddListener(() => DecreaseGraphVisual());
        transform.Find("DollarYlabelButton").GetComponent<Button>().onClick.AddListener(() => SetGetAxisLabelY((float _f) => "$" + Mathf.RoundToInt(_f)));
        transform.Find("EuroYlabelButton").GetComponent<Button>().onClick.AddListener(() => SetGetAxisLabelY((float _f) => "€" + Mathf.RoundToInt(_f/1.18f)));
        //ShowToolTip("this is a tool tip", new Vector2(100, 100));


        // valueList[0] = 20;
        // ShowGraph(valueList, (int _i) => "Day" + (_i + 1), (float _f) => "$" + Mathf.RoundToInt(_f));

    }

    private void ShowToolTip(string toolTipText,Vector2 anchoredPosition)
    {
        tooltipGameObject.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
        tooltipGameObject.SetActive(true);
        Text tooltipUIText = tooltipGameObject.transform.Find("Text").GetComponent<Text>();
        tooltipUIText.text = toolTipText;
        float textPadding = 4f;
        Vector2 backgroundSize = new Vector2(
            tooltipUIText.preferredWidth+textPadding*2f, 
            tooltipUIText.preferredHeight+ textPadding * 2f);
        tooltipGameObject.transform.Find("background").GetComponent<RectTransform>().sizeDelta = backgroundSize;
        tooltipGameObject.transform.Find("background").GetComponent<RectTransform>().anchoredPosition = new Vector2(backgroundSize.x/2,backgroundSize.y/2);
        //tooltipGameObject.transform.Find("background").GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
        tooltipGameObject.transform.SetAsLastSibling();
    }
    public static void ShowTooltip_Static(string toolTipText, Vector2 anchoredPosition)
    {
        instance.ShowToolTip(toolTipText, anchoredPosition);
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

    private void ShowGraph(List<int> valueList,IGraphVisual graphVisual, int maxVisibleValueAmount=-1,Func<int,string>getAxisLabelX=null, Func<float, string> getAxisLabelY=null)
    {
        this.valueList = valueList;
        this.graphVisual = graphVisual;
        this.getAxisLabelX = getAxisLabelX;
        this.getAxisLabelY = getAxisLabelY;
        if (getAxisLabelX == null)
        {
            getAxisLabelX = delegate (int _i) { return _i.ToString(); };
        }
        if (getAxisLabelY == null)
        {
            getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        }
        if (maxVisibleValueAmount <= 0)
        {
            maxVisibleValueAmount = valueList.Count;
        }
        this.maxVisibleValueAmount = maxVisibleValueAmount;

        foreach (GameObject gameObject in gameObjectList)
        {
            Destroy(gameObject);
        }
        gameObjectList.Clear();


        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        float xSize = graphWidth/(maxVisibleValueAmount+1);//+1 to give buffer on right side

        float yMaximum = valueList[0];
        float YMinimum = valueList[0];
        for(int i=Mathf.Max( valueList.Count - maxVisibleValueAmount,0);i<valueList.Count;i++)
        {
            int value = valueList[i];
            if (value > yMaximum)
            {
                yMaximum = value;
            }

            if (value < YMinimum)
            {
                YMinimum = value;
            }
        }
        float yDifference = yMaximum - YMinimum;
        if (yDifference <= 0)
        {
            yDifference = 5f;
        }
        //yMaximum = yMaximum * 1.2f;
        yMaximum = yMaximum + ((yDifference) * 0.2f);
        YMinimum = YMinimum - ((yDifference) * 0.2f);

        //GameObject lastDotGameObject = null;
        int xIndex = 0;

        //BarChartVisual barChartVisual = new BarChartVisual(graphContainer,Color.green,0.8f);
        //LineGraphVisual lineGraphVisual = new LineGraphVisual(graphContainer, dot,Color.green,Color.white);
        for (int i=Mathf.Max(valueList.Count-maxVisibleValueAmount,0); i < valueList.Count;++i)
        {
            float xposition = xSize+xIndex * xSize;
            //float yposition = (valueList[i] / yMaximum) * graphHeight;
            float yposition = ((valueList[i]-YMinimum) / (yMaximum-YMinimum)) * graphHeight;
            //GameObject barGameObject= CreateBar(new Vector2(xposition, yposition), xSize*0.9f);
            //GameObject barGameObject = barChartVisual.AddGraphVisual(new Vector2(xposition, yposition), xSize);
            //gameObjectList.AddRange(barChartVisual.AddGraphVisual(new Vector2(xposition, yposition), xSize));
            //gameObjectList.AddRange(lineGraphVisual.AddGraphVisual(new Vector2(xposition, yposition), xSize));
            string toolTipText = getAxisLabelY(valueList[i]);
            gameObjectList.AddRange(graphVisual.AddGraphVisual(new Vector2(xposition, yposition), xSize, toolTipText));

            
            //gameObjectList.Add(barGameObject);
            /*
            
            */
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
            labelY.SetParent(graphContainer);
            labelY.gameObject.SetActive(true);
            float normalisedValue = i * 1f / sepraterCount;
            labelY.anchoredPosition = new Vector2(-80f, normalisedValue*graphHeight);
            //labelY.GetComponent<Text>().text = getAxisLabelY(normalisedValue * yMaximum);
            labelY.GetComponent<Text>().text = getAxisLabelY(YMinimum+ (normalisedValue * (yMaximum-YMinimum)));
            gameObjectList.Add(labelY.gameObject);

            RectTransform dashY = Instantiate(dashTemplateY);
            dashY.SetParent(graphContainer);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(0f, normalisedValue * graphHeight);
            gameObjectList.Add(dashY.gameObject);

        }
    }

    private interface IGraphVisual
    {
        List<GameObject> AddGraphVisual(Vector2 graphPosition, float graphPositionWidth,string toolTipText);
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

        public List<GameObject> AddGraphVisual(Vector2 graphPosition,float graphPositionWidth,string toolTipText)
        {
            GameObject barGameObject = CreateBar(graphPosition, graphPositionWidth);
            MouseHover barMouseHover = barGameObject.AddComponent<MouseHover>();
            barMouseHover.MouseOverOnceFunc += () =>
            {
                ShowTooltip_Static(toolTipText, graphPosition);
            };
            barMouseHover.MouseOutOnceFunc += () =>
            {
                HideTooltip_Static();
            };
            return new List<GameObject>() { barGameObject };
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
            return gameObject;
        }
    }

    private class LineGraphVisual: IGraphVisual
    {
        private RectTransform graphContainer;
        private Sprite dotSprite;
        private GameObject lastDotGameObject;
        private Color dotColor;
        private Color dotConnectionColor;
        public LineGraphVisual(RectTransform graphContainer,Sprite dotSprite,Color dotColor,Color dotConnectionColor)
        {
            this.graphContainer = graphContainer;
            this.dotSprite = dotSprite;
            this.lastDotGameObject = null;
            this.dotColor = dotColor;
            this.dotConnectionColor = dotConnectionColor;
        }
        public List<GameObject> AddGraphVisual(Vector2 graphPosition, float graphPositionWidth, string toolTipText)
        {
            List<GameObject> gameObjectList = new List<GameObject>();
            GameObject dotGameObject = CreateDot(graphPosition);
            MouseHover dotMouseHover = dotGameObject.AddComponent<MouseHover>();
            dotMouseHover.MouseOverOnceFunc += () =>
            {
                ShowTooltip_Static(toolTipText, graphPosition);
            };
            dotMouseHover.MouseOutOnceFunc += () =>
            {
                HideTooltip_Static();
            };
            gameObjectList.Add(dotGameObject);

            if (lastDotGameObject != null)
            {
                GameObject dotConnectionGameObject = CreateDotConnection(lastDotGameObject.GetComponent<RectTransform>().anchoredPosition, dotGameObject.GetComponent<RectTransform>().anchoredPosition);
                gameObjectList.Add(dotConnectionGameObject);
            }
            lastDotGameObject = dotGameObject;
            return gameObjectList;
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
            return gameObject;
        }

        public GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
        {
            GameObject gameObject = new GameObject("dot_connection", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().color = dotConnectionColor;
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
