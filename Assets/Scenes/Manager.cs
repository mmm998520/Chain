using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MTest
{
	public class Manager : MonoBehaviour
	{

		public Transform left;
		public Transform right;
		public Detent detent;

		void Update()
		{
			detent.DrawLength(left.position, right.position);
		}
	}
}