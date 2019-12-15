using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(fileName = "new story", menuName = "Story")]
public class scriptable_story : ScriptableObject {
	public Dictionary<string, string> Plot = new Dictionary<string, string>();
	public Dictionary<string, DialogueSet> dc_dialogues = new Dictionary<string, DialogueSet> ();
	public Dictionary<string, Question> dc_questions = new Dictionary<string, Question> ();
	public Dictionary<string, List<DivergeUnit>> dc_diverges = new Dictionary<string, List<DivergeUnit>> ();
}
