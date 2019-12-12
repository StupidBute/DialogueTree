using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_importTest : MonoBehaviour {
	[SerializeField]
	string filename;
	TextAsset textAsset;
	// Use this for initialization
	void Start () {
		textAsset = Resources.Load<TextAsset> (filename);
		foreach (char c in textAsset.text) {
			if (c == '\r')
				print ("\\r");
			else if (c == '\n')
				print ("\\n");
			else if (c == '\t')
				print ("\\t");
			else if (c == ' ')
				print ("space");
			else
				print (c);
		}
			
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
