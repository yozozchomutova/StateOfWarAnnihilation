using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TTNode : MonoBehaviour
{
    public RectTransform rt;

    [Header("Main properties")]
    public string nodeId;
    public string nodeName;

    [Header("Sub properties")]
    public bool prop_start;
    public bool prop_targetTeam;
    public bool prop_targetUnit;
    public bool prop_targetPosition;
    public bool prop_money;
    public bool prop_research;
    public bool prop_jets;
    public bool prop_destroyers;
    public bool prop_cyclones;
    public bool prop_carrybuses;
    public bool prop_debrises;

    [Header("UI Stuff")]
    public Text topBarLabel;
    public RawImage topBarRaw;

    public RectTransform[] inputBtns;
    public RectTransform[] outputBtns;

    public InputField[] data_inputFields;
    public Dropdown[] data_dropdowns;
    public Text[] data_targetPosition;

    private float nodeX;
    private float nodeY;

    public TTNode[] inputNodes; //Which nodes connect to
    public int[] inputNodeIDs; //Which input index connect to

    [HideInInspector] public Ser linkedSerClass;

    private void Start()
    {
        if (inputNodes != null && inputNodes.Length != outputBtns.Length)
        {
            inputNodes = new TTNode[outputBtns.Length];
            inputNodeIDs = new int[outputBtns.Length];
        }
    }

    public void OnOutputClick(int outputId) //=Transmit
    {
        if (BarTaskTriggers.linkNodeFirst == null)
        {
            BarTaskTriggers.linkNodeFirst = this;
            BarTaskTriggers.linkNodeFirstOutputId = outputId;
        } else
        {
            BarTaskTriggers.linkNodeFirst = null;
        }
    }

    public void OnInputClick(int inputId) //=Receive
    {
        if (BarTaskTriggers.linkNodeFirst != null)
        {
            //Make link
            TTNode fromNode = BarTaskTriggers.linkNodeFirst;
            fromNode.inputNodes[BarTaskTriggers.linkNodeFirstOutputId] = this;
            fromNode.inputNodeIDs[BarTaskTriggers.linkNodeFirstOutputId] = inputId;

            //Clear
            BarTaskTriggers.linkNodeFirst = null;
        }
    }

    public void OnSelectUnitClick()
    {

    }

    public Ser serializeNode()
    {
        List<byte> dataList = new List<byte>();
        switch (nodeId)
        {
            case BarTaskTriggers.NODE_TIME_COUNTDOWN:
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[0].text))); //Time - seconds
                break;
            case BarTaskTriggers.NODE_GIVE_RESOURCES:
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[0].text))); //Money
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[1].text))); //Research
                break;
            case BarTaskTriggers.NODE_GIVE_AIR_FORCES:
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[0].text))); //Jets
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[1].text))); //Destroyers
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[2].text))); //Cyclones
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[3].text))); //Carrybuses
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[4].text))); //Debrises
                break;
            case BarTaskTriggers.NODE_LAUNCH_AIR_FORCE:
                dataList.AddRange(BitConverter.GetBytes(data_dropdowns[0].value)); //Air force ID
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[0].text))); //X
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[1].text))); //Z
                break;
            case BarTaskTriggers.NODE_AUTOPATH_UNIT:
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[0].text))); //Unit ID
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[1].text))); //X
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[2].text))); //Z
                dataList.AddRange(BitConverter.GetBytes(double.Parse(data_inputFields[3].text))); //Max speed
                break;
            case BarTaskTriggers.NODE_UNIT_CHECK_DISTANCE:
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[0].text))); //Unit ID
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[1].text))); //X
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[2].text))); //Z
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[3].text))); //Range
                break;
            case BarTaskTriggers.NODE_IS_UNIT_DEAD:
                dataList.AddRange(BitConverter.GetBytes(int.Parse(data_inputFields[0].text))); //Unit ID
                break;
        }

        int[] generatedNodeIDs = new int[inputNodes.Length];
        for (int i = 0; i < generatedNodeIDs.Length; i++)
        {
            generatedNodeIDs[i] = BarTaskTriggers.visibleNodes.IndexOf(inputNodes[i]);
            print(nodeName + ": Found: " + generatedNodeIDs[i] + " Old Input : " + inputNodes[i]);
            if (generatedNodeIDs[i] != -1)
                generatedNodeIDs[i]++;
        }

        //print("Rt " + nodeId + " |Y: " + (rt == null));
        linkedSerClass = new Ser(nodeId, rt.anchoredPosition.x, rt.anchoredPosition.y, dataList.ToArray(), generatedNodeIDs, inputNodeIDs);
        return linkedSerClass;
    }

    public Ser deserializeNode(Ser s, Team team)
    {
        nodeId = s.nodeId;
        SetPosition(s.x, s.y);
        topBarLabel.text = nodeName;
        topBarLabel.color = team.id == 0 ? Color.black : Color.white;
        topBarRaw.color = team.minimapColor - new Color(0.2f, 0.2f, 0.2f, 0f);

        if (s.data != null)
        {
            switch (nodeId)
            {
                case BarTaskTriggers.NODE_TIME_COUNTDOWN:
                    data_inputFields[0].text = "" + BitConverter.ToInt32(s.data, 0); //Time - seconds
                    break;
                case BarTaskTriggers.NODE_GIVE_RESOURCES:
                    data_inputFields[0].text = "" + BitConverter.ToInt32(s.data, 0); //Money
                    data_inputFields[1].text = "" + BitConverter.ToInt32(s.data, sizeof(Int32)); //Research
                    break;
                case BarTaskTriggers.NODE_GIVE_AIR_FORCES:
                    data_inputFields[0].text = "" + BitConverter.ToInt32(s.data, 0); //Jets
                    data_inputFields[1].text = "" + BitConverter.ToInt32(s.data, sizeof(Int32)); //Destroyers
                    data_inputFields[2].text = "" + BitConverter.ToInt32(s.data, sizeof(Int32) * 2); //Cyclones
                    data_inputFields[3].text = "" + BitConverter.ToInt32(s.data, sizeof(Int32) * 3); //Carrybuses
                    data_inputFields[4].text = "" + BitConverter.ToInt32(s.data, sizeof(Int32) * 4); //Debrises
                    break;
                case BarTaskTriggers.NODE_LAUNCH_AIR_FORCE:
                    data_dropdowns[0].value = BitConverter.ToInt32(s.data, 0); //Air force ID
                    data_inputFields[0].text = "" + BitConverter.ToInt32(s.data, sizeof(Int32)); //X
                    data_inputFields[1].text = "" + BitConverter.ToInt32(s.data, sizeof(Int32) * 2); //Z
                    break;
                case BarTaskTriggers.NODE_AUTOPATH_UNIT:
                    data_inputFields[0].text = "" + BitConverter.ToInt32(s.data, 0); //Unit ID
                    data_inputFields[1].text = "" + BitConverter.ToInt32(s.data, sizeof(Int32)); //X
                    data_inputFields[2].text = "" + BitConverter.ToInt32(s.data, sizeof(Int32) * 2); //Z
                    data_inputFields[3].text = "" + BitConverter.ToDouble(s.data, sizeof(Int32) * 3); //Max speed
                    break;
                case BarTaskTriggers.NODE_UNIT_CHECK_DISTANCE:
                    data_inputFields[0].text = "" + BitConverter.ToInt32(s.data, 0); //Unit ID
                    data_inputFields[1].text = "" + BitConverter.ToInt32(s.data, sizeof(Int32)); //X
                    data_inputFields[2].text = "" + BitConverter.ToInt32(s.data, sizeof(Int32) * 2); //Z
                    data_inputFields[3].text = "" + BitConverter.ToInt32(s.data, sizeof(Int32) * 3); //Range
                    break;
                case BarTaskTriggers.NODE_IS_UNIT_DEAD:
                    data_inputFields[0].text = "" + BitConverter.ToInt32(s.data, 0); //Unit ID
                    break;
            }
        }

        linkedSerClass = s;
        return s;
    }

    public Ser deserializeNode2()
    {
        if (linkedSerClass.linkedInputNodeIDs != null)
        {
            int[] lind = linkedSerClass.getLINID();
            inputNodes = new TTNode[lind.Length];
            for (int i = 0; i < inputNodes.Length; i++)
            {
                if (lind[i]-1 > -1)
                    inputNodes[i] = BarTaskTriggers.visibleNodes[lind[i]-1];
            }
            inputNodeIDs = linkedSerClass.linkedInputPropertyIDs;
        }

        return linkedSerClass;
    }

    //Moving
    private float lastMouseX;
    private float lastMouseY;
    public void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(1) && nodeId != BarTaskTriggers.NODE_START_BASE) //Remove node
        {
            Destroy(gameObject);
        }

        lastMouseX = Input.mousePosition.x;
        lastMouseY = Input.mousePosition.y;
    }

    public void OnMouseDrag()
    {
        float curMouseX = Input.mousePosition.x;
        float curMouseY = Input.mousePosition.y;

        MovePosition(curMouseX - lastMouseX, curMouseY - lastMouseY);

        lastMouseX = curMouseX;
        lastMouseY = curMouseY;
    }

    public void MovePosition(float x, float y)
    {
        nodeX += x;
        nodeY += y;
        rt.anchoredPosition = new Vector2(nodeX, nodeY);
    }

    public void SetPosition(float x, float y)
    {
        rt.anchoredPosition = new Vector2(x, y);
        nodeX = x;
        nodeY = y;
    }

    //data requests
    public void DataRequest3DPosition()
    {

    }

    public void DataRequestUnit()
    {

    }

    //Ser class is used for saving / loading data
    [Serializable]
    public class Ser
    {
        public string nodeId;

        public float x;
        public float y;

        public int[] linkedInputNodeIDs; //Which node
        public int[] linkedInputPropertyIDs; //Which input property ID

        public byte[] data;

        public Ser(string nodeId, float x, float y, byte[] data, int[] linkedInputNodeIDs, int[] linkedInputPropertyIDs)
        {
            this.nodeId = nodeId;

            this.x = x;
            this.y = y;

            this.linkedInputNodeIDs = linkedInputNodeIDs;
            this.linkedInputPropertyIDs = linkedInputPropertyIDs;

            this.data = data;
        }

        public int[] getLINID()
        {
            return this.linkedInputNodeIDs;
        }
    }
}
