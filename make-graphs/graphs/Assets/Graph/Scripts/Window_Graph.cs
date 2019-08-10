﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Graph : MonoBehaviour
{
    private RectTransform graphContainer;
    [SerializeField]
    private Sprite circleSprite;

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
        List<int> valueList = new List<int>() { 5, 98, 56, 45, 30, 22, 17, 15, 13, 17, 25, 37, 40, 36, 33 };
        ShowGraph(valueList,(int _i)=>"Day"+(_i+1),(float _f)=>"$"+Mathf.RoundToInt(_f));

       
    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    private void ShowGraph(List<int> valueList,Func<int,string>getAxisLabelX=null, Func<float, string> getAxisLabelY=null)
    {
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


        float graphHeight = graphContainer.sizeDelta.y;
        float xSize = 50f;
        float yMaximum = valueList[0];
        float YMinimum = valueList[0];
        foreach(int value in valueList)
        {
            if (value > yMaximum)
            {
                yMaximum = value;
            }

            if (value < YMinimum)
            {
                YMinimum = value;
            }
        }
        //yMaximum = yMaximum * 1.2f;
        yMaximum = yMaximum + ((yMaximum - YMinimum) * 0.2f);
        YMinimum = YMinimum - ((yMaximum - YMinimum) * 0.2f);

        GameObject lastCircleGameObject = null;
        for (int i= 0; i < valueList.Count;++i)
        {
            float xposition = xSize+i * xSize;
            //float yposition = (valueList[i] / yMaximum) * graphHeight;
            float yposition = ((valueList[i]-YMinimum) / (yMaximum-YMinimum)) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xposition, yposition));

            gameObjectList.Add(circleGameObject);

            if (lastCircleGameObject != null)
            {
                GameObject dotConnectionGameObject=CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);
                gameObjectList.Add(dotConnectionGameObject);
            }
            lastCircleGameObject = circleGameObject;

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
}
