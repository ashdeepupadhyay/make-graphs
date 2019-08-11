using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Graph : MonoBehaviour
{
    private RectTransform graphContainer;
    [SerializeField]
    private Sprite dot;

    private RectTransform labelTemplateX;
    private RectTransform lableTemplateY;

    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;

    private List<GameObject> gameObjectList;
    private void Awake()
    {
        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        lableTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("dashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("dashTemplateY").GetComponent<RectTransform>();

        gameObjectList = new List<GameObject>();
        //CreateCircle(new Vector2(200,200));
        List<int> valueList = new List<int>() { 5, 98, 56, 45, 30, 22, 17, 15, 13, 17, 25, 37, 40, 36, 33,60,54,22,67,98,34,25,2,7,87 };
        //List<int> valueList = new List<int>() { 5};

        ShowGraph(valueList,-1,(int _i)=>"Day"+(_i+1),(float _f)=>"$"+Mathf.RoundToInt(_f));
       // valueList[0] = 20;
       // ShowGraph(valueList, (int _i) => "Day" + (_i + 1), (float _f) => "$" + Mathf.RoundToInt(_f));

    }

    private GameObject CreateDot(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("dot", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = dot;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    private void ShowGraph(List<int> valueList, int maxVisibleValueAmount=-1,Func<int,string>getAxisLabelX=null, Func<float, string> getAxisLabelY=null)
    {
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

        BarChartVisual barChartVisual = new BarChartVisual(graphContainer,Color.green,0.8f);
        for (int i=Mathf.Max(valueList.Count-maxVisibleValueAmount,0); i < valueList.Count;++i)
        {
            float xposition = xSize+xIndex * xSize;
            //float yposition = (valueList[i] / yMaximum) * graphHeight;
            float yposition = ((valueList[i]-YMinimum) / (yMaximum-YMinimum)) * graphHeight;
            //GameObject barGameObject= CreateBar(new Vector2(xposition, yposition), xSize*0.9f);
            GameObject barGameObject = barChartVisual.AddGraphVisual(new Vector2(xposition, yposition), xSize);
            gameObjectList.Add(barGameObject);
            /*
            GameObject dotGameObject = CreateDot(new Vector2(xposition, yposition));

            gameObjectList.Add(dotGameObject);

            if (lastDotGameObject != null)
            {
                GameObject dotConnectionGameObject=CreateDotConnection(lastDotGameObject.GetComponent<RectTransform>().anchoredPosition, dotGameObject.GetComponent<RectTransform>().anchoredPosition);
                gameObjectList.Add(dotConnectionGameObject);
            }
            lastDotGameObject = dotGameObject;
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

    public GameObject CreateDotConnection(Vector2 dotPositionA,Vector2 dotPositionB)
    {
        GameObject gameObject = new GameObject("dot_connection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = dotPositionA+dir*distance*0.5f;
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

    private class BarChartVisual
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

        public GameObject AddGraphVisual(Vector2 graphPosition,float graphPositionWidth)
        {
            GameObject barGameObject = CreateBar(graphPosition, graphPositionWidth);
            return barGameObject;
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
}
