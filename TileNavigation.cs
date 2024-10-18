using System.Collections.Generic;
using UnityEngine;
using static ItemInfoUI.Equipment;

[System.Serializable]
public class Node
{
    public Node(bool _isWall, int _x, int _y) { isWall = _isWall; x = _x; y = _y; }

    public bool isWall;
    public Node ParentNode;

    // G : 시작으로부터 이동했던 거리, H : |가로|+|세로| 장애물 무시하여 목표까지의 거리, F : G + H
    public int x, y, G, H;
    public int F { get { return G + H; } }
}


public class TileNavigation : MonoBehaviour
{
    #region 싱글톤
    public static TileNavigation Inst;
    void Awake() => Inst = this;
    #endregion

    [Header("Component")]
    [SerializeField] SpreadTilemap spreadTilemap;
    [SerializeField] DivideSpace divideSpace;
    public GameObject markObject;

    [Header("캔버스그룹")]
    [SerializeField] CanvasGroup playerGroup;

    [HideInInspector] public List<LineRenderer> lineRenderers;
    [HideInInspector] public List<SpriteRenderer> placeSR;

    int sizeX, sizeY;

    Node[,] NodeArray;
    Node StartNode, TargetNode, CurNode;

    Vector2Int bottomLeft;
    Vector2Int topRight;
    Vector2Int startPos;
    Vector2Int targetPos;

    List<Node> OpenList, ClosedList;
    List<Node> FinalNodeList = new List<Node>();

    int selectedID = -1;

    public void JustSetStartPos(PlayableInfo info, Vector2Int pos)
    {
        if (PlayableMenu.Inst.selectObject != null)
        {
            SetEndPos(pos);
        }
        else if (selectedID == info.playableID)
        {
            Clear();
        }
        else if (selectedID != info.playableID)
        {
            Clear();

            markObject.SetActive(true);
            markObject.transform.position = new Vector3(pos.x, pos.y, 0)  + new Vector3(0, 0.6f, 0);

            selectedID = info.playableID;
            startPos = pos;

            BuffPooling.inst.Call(info.buffs);
            int nowIndex = playerGroup.NowActiveCanvasIndex();
            if (nowIndex == 0)
                PlayableMenu.Inst.LoadSkillInfo(info);
            else if (nowIndex == 1)
                PlayableMenu.Inst.LoadEquipInfo(info);

            return;
        }
    }

    public void ViewTile(PlayableInfo info, Skill_Frame skill, Vector2 pos)
    {
        spreadTilemap.ClearView();
        List<Vector2Int> tilePos = SkillRangeTile(skill, skill.GetRange(0), new Vector2Int((int)pos.x, (int)pos.y));

        HashSet<Vector2Int> possible = new HashSet<Vector2Int>();
        HashSet<Vector2Int> impossible = new HashSet<Vector2Int>();

        for (int i = 0; i < tilePos.Count; i++)
        {
            if (skill.GetTarget(tilePos[i], "Mob"))
                possible.Add(tilePos[i]);
            else
                impossible.Add(tilePos[i]);
        }

        spreadTilemap.SpreadSignTilemap(skill.skillType, possible, impossible);
    }

    public List<Vector2Int> SkillRangeTile(Skill_Frame skill, int range, Vector2Int pos)
    {
        List<Vector2Int> targetPos = new List<Vector2Int>();

        for (int x = -range; x <= range; x++)
        {
            int temp = range - Mathf.Abs(x);
            for (int y = -temp; y <= temp; y++)
            {
                Vector2Int point = new Vector2Int(x, y) + pos;

                if (SpreadTilemap.Inst.TilemapScanFloor((Vector3Int)point))
                    targetPos.Add(point);
            }
        }
        return targetPos;
    }

    public void Clear()
    {
        BuffPooling.inst.Clear();
        spreadTilemap.ClearView();
        markObject.SetActive(false);

        int nowIndex = playerGroup.NowActiveCanvasIndex();
        
        if (nowIndex <= 0)
            PlayableMenu.Inst.SkillClear(0);
        else if (nowIndex == 1)
            PlayableMenu.Inst.EquipClear(false);
        

        selectedID = -1;
        startPos = new Vector2Int();
        targetPos = new Vector2Int();
    }

    //리셋 버튼
    public void AllClear()
    {
        for (int i = 0; i < lineRenderers.Count; i++)
        {
            lineRenderers[i].positionCount = 0;

            PlayableObjs.Inst.units[i].GetComponent<ObjectPlan>().ClearSkill();
            placeSR[i].gameObject.SetActive(false);
            placeSR[i].transform.position = new Vector3Int();
        }
        Clear();
    }

    //이동
    public void SetEndPos(Vector2Int pos2)
    {
        //사거리 지우기
        if (!spreadTilemap.signPossible.HasTile((Vector3Int)pos2)) return;
        else spreadTilemap.ClearView();

        targetPos = pos2;

        Skill_Frame skill = PlayableMenu.Inst.selectObject;
        PlayableObjs.Inst.units[selectedID].GetComponent<ObjectPlan>().skill = skill;
        List<Node> FinalNodeList = new List<Node>();
        if (skill.skillType == Skill_Frame.SkillType.공격)
            FinalNodeList = FindingWay(startPos, targetPos, false);
        else
            FinalNodeList = FindingWay(startPos, targetPos, true);

        placeSR[selectedID].gameObject.SetActive(true);
        placeSR[selectedID].transform.position = ((Vector3Int)targetPos);
        Drawing(FinalNodeList, lineRenderers[selectedID]);
        Clear();
    }

    //최적루트 구하기
    public List<Node> FindingWay(Vector2Int startPos, Vector2Int targetPos, bool wall = true)
    {
        bottomLeft = new Vector2Int();
        topRight = new Vector2Int(divideSpace.totalWidth, divideSpace.totalHeight);

        // NodeArray의 크기 정해주고, isWall, x, y 대입
        sizeX = topRight.x - bottomLeft.x + 2;
        sizeY = topRight.y - bottomLeft.y + 2;
        NodeArray = new Node[sizeX, sizeY];

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                bool isWall = false;

                if(wall) isWall = !spreadTilemap.TilemapScanFloor(new Vector3Int(i, j));
                NodeArray[i, j] = new Node(isWall, i + bottomLeft.x, j + bottomLeft.y);
            }
        }

        // 시작과 끝 노드, 열린리스트와 닫힌리스트, 마지막리스트 초기화
        StartNode = NodeArray[startPos.x - bottomLeft.x, startPos.y - bottomLeft.y];
        TargetNode = NodeArray[targetPos.x - bottomLeft.x, targetPos.y - bottomLeft.y];

        OpenList = new List<Node>() { StartNode };
        ClosedList = new List<Node>();
        FinalNodeList = new List<Node>();


        while (OpenList.Count > 0)
        {
            // 열린리스트 중 가장 F가 작고 F가 같다면 H가 작은 걸 현재노드로 하고 열린리스트에서 닫힌리스트로 옮기기
            CurNode = OpenList[0];
            for (int i = 1; i < OpenList.Count; i++)
                if (OpenList[i].F <= CurNode.F && OpenList[i].H < CurNode.H) CurNode = OpenList[i];

            OpenList.Remove(CurNode);
            ClosedList.Add(CurNode);


            // 마지막
            if (CurNode == TargetNode)
            {
                Node TargetCurNode = TargetNode;
                while (TargetCurNode != StartNode)
                {
                    FinalNodeList.Add(TargetCurNode);
                    TargetCurNode = TargetCurNode.ParentNode;
                }
                FinalNodeList.Add(StartNode);
                FinalNodeList.Reverse();

                return FinalNodeList;
            }

            // ↑ → ↓ ←
            OpenListAdd(CurNode.x, CurNode.y + 1);
            OpenListAdd(CurNode.x + 1, CurNode.y);
            OpenListAdd(CurNode.x, CurNode.y - 1);
            OpenListAdd(CurNode.x - 1, CurNode.y);
        }
        return null;
    }

    //목적지 지우기
    void NodeClear(int id, Vector2Int pos)
    {
        if (TurnManager.Inst.turn != TurnManager.Turn.Main) return;

        if (selectedID == -1 && id + 1 < placeSR.Count) 
            NodeClear(id + 1, pos);
        else if (selectedID != -1) 
            id = selectedID;

        if (Vector2Int.RoundToInt(placeSR[id].transform.position) == pos)
        {
            FinalNodeList.Clear();
            targetPos = new Vector2Int();
            placeSR[id].gameObject.SetActive(false);
            placeSR[id].transform.position = new Vector3Int();
            lineRenderers[id].positionCount = 0;
            Clear();
            return;
        }
    }

    //목적지 그리기
    void Drawing(List<Node> FinalNodeList, LineRenderer line)
    {
        line.positionCount = FinalNodeList.Count;

        Color color;
        switch (PlayableMenu.Inst.selectObject.skillType)
        {
            case Skill_Frame.SkillType.위치:
                color = new Color(255 / 255f, 186 / 255f, 106 / 255f);
                break;
            case Skill_Frame.SkillType.버프:
                color = new Color(95 / 255f, 140 / 255f, 255 / 255f);
                break;
            default:
                color = Color.red;
                break;
        }
        placeSR[selectedID].color = color;
        line.startColor = color;
        line.endColor = color;


        if (FinalNodeList.Count != 0)
        {
            for (int i = 0; i < FinalNodeList.Count; i++)
                line.SetPosition(i, new Vector3(FinalNodeList[i].x, FinalNodeList[i].y));
        }
    }

    void OpenListAdd(int checkX, int checkY)
    {
        // 상하좌우 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면
        if (checkX >= bottomLeft.x && checkX < topRight.x + 1 && checkY >= bottomLeft.y && checkY < topRight.y + 1 && !NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y].isWall && !ClosedList.Contains(NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y]))
        {
            // 이웃노드에 넣고, 직선은 10, 대각선은 14비용
            Node NeighborNode = NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y];
            int MoveCost = CurNode.G + (CurNode.x - checkX == 0 || CurNode.y - checkY == 0 ? 10 : 14);


            // 이동비용이 이웃노드G보다 작거나 또는 열린리스트에 이웃노드가 없다면 G, H, ParentNode를 설정 후 열린리스트에 추가
            if (MoveCost < NeighborNode.G || !OpenList.Contains(NeighborNode))
            {
                NeighborNode.G = MoveCost;
                NeighborNode.H = (Mathf.Abs(NeighborNode.x - TargetNode.x) + Mathf.Abs(NeighborNode.y - TargetNode.y)) * 10;
                NeighborNode.ParentNode = CurNode;

                OpenList.Add(NeighborNode);
            }
        }
    }
}
