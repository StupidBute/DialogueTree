using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_StairsSwitch : MonoBehaviour {
	public enum Direction{Right, Left};
	public Direction SwitchDirection = Direction.Right;

	[System.Serializable]
	public struct RoadInfo{
		public float zValue;
		public int spriteSort;
		public RoadInfo(float _z, int _sort){
			zValue = _z;
			spriteSort = _sort;
		}
	}

	public RoadInfo[] MySwitchRoads = new RoadInfo[3];		//0:default		1:up		2:down
	RoadInfo OriginRoad = new RoadInfo(0, 20);

	public void DoStairsSwitch(sc_character _char, float _spd){
		if (_spd > 0.0001f && SwitchDirection == Direction.Right || _spd < 0.0001f && SwitchDirection == Direction.Left) {
			if (_char.Up && ! _char.Down)
				SwitchRoad (_char, MySwitchRoads [1]);
			else if (_char.Down && ! _char.Up)
				SwitchRoad (_char, MySwitchRoads [2]);
			else
				SwitchRoad (_char, MySwitchRoads [0]);
		} else
			SwitchRoad (_char, OriginRoad);
	}

	void SwitchRoad(sc_character _char, RoadInfo targetRoad){
		int originSort = _char.GetNowSortOrder ();
		Vector3 tmpVec;
		tmpVec = _char.transform.position;
		tmpVec.z = targetRoad.zValue;
		_char.transform.position = tmpVec;
		_char.SetSortingOrder (originSort % 10 + targetRoad.spriteSort, false);
	}
}
