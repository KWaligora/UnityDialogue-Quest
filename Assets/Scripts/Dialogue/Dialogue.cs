using UnityEngine;
using System.Collections.Generic;
using System;

namespace RPG.Dialogue
{
    [CreateAssetMenu(fileName = "New Dialgoue", menuName = "Dialogue", order = 0)]
    public class Dialogue : ScriptableObject
    {
        [SerializeField]
        List<DialogueNode> nodes = new List<DialogueNode>();

        Dictionary<string, DialogueNode> nodeLookup = new Dictionary<string, DialogueNode>();

#if UNITY_EDITOR
        private void Awake() {
            if(nodes.Count == 0)
            {
                nodes.Add(new DialogueNode());
            } 
            OnValidate();          
        }
#endif
        private void OnValidate() 
        {
            nodeLookup.Clear();
            foreach(DialogueNode node in GetAllNodes())
            {
                nodeLookup[node.uniqueID] = node;
            }
        }

        public IEnumerable<DialogueNode> GetAllNodes()
        {
            return nodes;
        }

        public DialogueNode GetRootNode()
        {
            return nodes[0];
        }

        public IEnumerable<DialogueNode> GetAllChildern(DialogueNode parentNode)
        {
            foreach(string childID in parentNode.children)
            {
                if(nodeLookup.ContainsKey(childID))
                {
                    yield return nodeLookup[childID];
                }                
            }
        }
    }
}
