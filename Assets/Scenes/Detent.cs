using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detent : MonoBehaviour
{

	//单个锁片 信息记录
	class Point
	{
		public static Point left;
		public static Point right;

		public bool active { get { return root == null ? false : root.activeSelf; } }
		public GameObject root;
		public HingeJoint2D joint;
		public HingeJoint2D joint2;
		public Rigidbody2D rigidbody;

		public Point(GameObject obj)
		{

			root = obj;
			HingeJoint2D[] joints = obj.GetComponents<HingeJoint2D>();
			joint = joints[0];
			joint2 = joints[1];
			rigidbody = obj.GetComponent<Rigidbody2D>();
		}
		public void Init(string name, Vector3 pos, Quaternion qua)
		{

			root.name = name;
			root.transform.position = pos;
			root.transform.rotation = qua;
			rigidbody.velocity = Vector2.zero;

			joint2.enabled = false;
			Show();
		}
		public void Show(bool show = true)
		{
			if (show != root.activeSelf) root.SetActive(show);
		}
	}

	public Transform content;                       //片段存放目标
	private List<Point> mem = new List<Point>();    //片段内存池
	public SpriteRenderer[] prefabs;        //锁片 预制体
											//当考虑生成的初始锁链为曲线时可自行改动长度并重新设计输入参数形式，这里没实现只了预留了下
	public bool[] HangSegment;              //锁片两端 是否固定与空间中
	public bool useBendLimit = false;       //使用片段弯曲限制
	public int bendLimit = 45;              //片段弯曲限制角度
	public bool useBreakForce = false;      //使用片段断裂界限
	public float BreakForce = 100;          //片段断裂峰值 --力
	private Vector2 leftP;                  //左端点位置
	private Vector2 rightP;                 //右端点位置
	private Vector2 rightOff;               //右端点偏移
	[Range(-0.5f, 0.5f)] public float overlapFactor;        //重叠比例
	[Range(0, 0.5f)] public float minError = 0.25f;     //允许的最小误差
	public int maxLength = 50;                          //最大片段数

	float segmentHeight;        //片段长度
	float yScale;               //片段长度缩放

	private void Awake() { Init(); }

	//初始化
	public void Init()
	{
		yScale = prefabs[0].transform.localScale.y;
		segmentHeight = prefabs[0].bounds.size.y * (1 + overlapFactor);
	}

	//在两点之间生成片段
	public void DrawLength(Vector2 l, Vector2 r)
	{

		if (leftP != l || (rightP + rightOff) != r)
		{

			leftP = l;
			rightP = r;
			Debug.DrawLine(leftP, rightP);
		}
		else return;

		int i = 0;
		float distance = Vector2.Distance(rightP, leftP);
		int segmentCount = (int)(distance / segmentHeight);
		float error = distance - segmentCount * segmentHeight;
		bool fixError = error > minError * segmentHeight || segmentCount == 0;

		int length = segmentCount;
		if (maxLength <= segmentCount)
		{
			length = maxLength;
			fixError = false;
			rightOff = rightP - ((rightP - leftP).normalized * maxLength * segmentHeight + leftP);
			rightP -= rightOff;
		}
		else
		{
			rightOff = Vector2.zero;
		}

		for (; i < length; i++) AddPoint(i);

		if (fixError) AddPoint(i++, true);

		if (mem.Count > 0 && i > 0) Point.right = mem[i - 1];

		for (; i < mem.Count; i++) mem[i].Show(false);

		if (!HangSegment[0])
		{
			Point.left.joint.enabled = false;
		}
		if (HangSegment[1])
		{

			Point.right.joint2.autoConfigureConnectedAnchor = false;
			Point.right.joint2.connectedBody = null;
			if (fixError)
			{
				Point.right.joint2.anchor = Point.right.root.transform.InverseTransformPoint(rightP);
			}
			else
			{
				Point.right.joint2.anchor = new Vector2(0, segmentHeight / 2);
			}
			Point.right.joint2.connectedAnchor = rightP;
			Point.right.joint2.enabled = true;
		}
	}

	//清除不使用的片段
	public void Clear()
	{

		int i = 0, sum = mem.Count;
		for (; i < sum; i++) if (!mem[i].active) break;

		for (int j = i; j < sum; j++)
		{
			Destroy(mem[i].root);
			mem.RemoveAt(i);
		}
	}

	//添加片段 <i>片段顺序	<fix>是否消除误差
	private void AddPoint(int i, bool fix = false)
	{

		Point point;
		if (i < mem.Count)
		{
			point = mem[i];
		}
		else
		{
			GameObject obj = Instantiate(prefabs[i % 2], content).gameObject;
			point = new Point(obj);
			mem.Add(point);
		}

		float theta = Mathf.Atan2(rightP.y - leftP.y, rightP.x - leftP.x);

		float dtheta = theta * Mathf.Rad2Deg;
		if (dtheta > 180) dtheta -= 360;
		else if (dtheta < -180) dtheta += 360;


		float dx = segmentHeight * Mathf.Cos(theta);
		float dy = segmentHeight * Mathf.Sin(theta);
		float startX = leftP.x + dx / 2;
		float startY = leftP.y + dy / 2;

		point.Init(
			"Segment_" + i + (fix ? "_fix" : ""),
			new Vector3(startX + dx * i, startY + dy * i),
			Quaternion.Euler(0, 0, theta * Mathf.Rad2Deg - 90)
		);

		if (i == 0)
		{
			Point.left = point;
			point.joint.connectedAnchor = leftP;
			point.joint.anchor = new Vector2(0, -segmentHeight / 2);
		}
		else
		{
			AddJoint(dtheta, segmentHeight / yScale, i - 1, point);
		}
	}

	//为片段添加关节
	private void AddJoint(float dthetaT, float segmentHeightT, int index, Point point)
	{
		point.joint.connectedBody = mem[index].rigidbody;
		point.joint.anchor = new Vector2(0, -segmentHeightT / 2);
		point.joint.connectedAnchor = new Vector2(0, segmentHeightT / 2);
	}

	//使用片段角度限制与片段断裂峰值
	private void LimitAndBreak(float dthetaT, Point point)
	{

		if (useBendLimit)
		{
			point.joint.useLimits = true;
			point.joint.limits = new JointAngleLimits2D()
			{
				min = dthetaT - bendLimit,
				max = dthetaT + bendLimit
			};
		}
		if (useBreakForce) point.joint.breakForce = BreakForce;
	}
}