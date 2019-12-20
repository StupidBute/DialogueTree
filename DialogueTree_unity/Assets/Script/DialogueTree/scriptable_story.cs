using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(fileName = "new story", menuName = "Story")]
public class scriptable_story : ScriptableObject {
	public List<Character> lst_chars = new List<Character> ();
	public List<Node> lst_Node = new List<Node> ();
}
