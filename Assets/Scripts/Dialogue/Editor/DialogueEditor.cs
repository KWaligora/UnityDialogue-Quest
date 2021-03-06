using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;

namespace RPG.Dialogue.Editor
{
    public class DialogueEditor : EditorWindow
    {
        Dialogue selectedDialogue = null;
        [NonSerialized]
        GUIStyle nodeStyle;
        [NonSerialized]
        DialogueNode draggingNode = null;
        [NonSerialized]
        Vector2 draggingOffset;
        [NonSerialized]
        DialogueNode creatingNode = null; //not null if need to create new node
        [NonSerialized]
        DialogueNode nodeToDelete = null;
        [NonSerialized]
        DialogueNode linkingParentNode = null;
        Vector2 scrollPosition;
        [NonSerialized]
        bool draggingCanvas = false;
        [NonSerialized]
        Vector2 draggingCanvasOffset;

        [MenuItem("Window/Dialogue Editor")]
        public static void ShowEditorWindow()
        {
            GetWindow(typeof(DialogueEditor), false, "Dialogue Editor");
        }

        [OnOpenAssetAttribute(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            Dialogue dialogue = EditorUtility.InstanceIDToObject(instanceID) as Dialogue;
            if(dialogue != null)
            {
                ShowEditorWindow();
                return true;
            }

            return false;
        }

       private void OnEnable() 
        {
            Selection.selectionChanged += OnSelectionChange;
            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
            nodeStyle.padding = new RectOffset(20, 20, 20, 20);
            nodeStyle.border = new RectOffset(12, 12, 12, 12);
        }

       private void OnSelectionChange()
       {
           Dialogue newDialogue = Selection.activeObject as Dialogue;
           if(newDialogue)
           {
               selectedDialogue = newDialogue;
               Repaint();
           }
       }

        private void OnGUI() 
        {
            if(selectedDialogue == null)
            {
                EditorGUILayout.LabelField("No Dialogue Selected");
            }
            else
            {       
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                GUILayoutUtility.GetRect(4000, 4000);

                ProcessEvents();
                foreach(DialogueNode node in selectedDialogue.GetAllNodes())
                {                
                    DrawConnections(node);
                }   
                foreach(DialogueNode node in selectedDialogue.GetAllNodes())
                {
                    DrawNode(node);                 
                }

                EditorGUILayout.EndScrollView();

                if(creatingNode != null)
                {
                    Undo.RecordObject(selectedDialogue, "Added Dialogue node");
                    selectedDialogue.CreateNode(creatingNode);
                    creatingNode = null;
                }
                if(nodeToDelete != null)
                {
                    Undo.RecordObject(selectedDialogue, "Remove node");
                    selectedDialogue.DeleteNode(nodeToDelete);
                    nodeToDelete = null;                    
                }     
            }
        }

        private void DrawConnections(DialogueNode node)
        {
            Vector3 startPosition = new Vector2(node.rect.xMax, node.rect.center.y);
            foreach(DialogueNode childNode  in selectedDialogue.GetAllChildern(node))
            {               
                Vector3 endPosition = new Vector2(childNode.rect.xMin, childNode.rect.center.y);
                Vector3 controlPointOffset = endPosition - startPosition;
                controlPointOffset.y = 0;

                Handles.DrawBezier(startPosition,
                    endPosition, startPosition + controlPointOffset,
                    endPosition - controlPointOffset,
                    Color.white, null, 4f);
            }
        }

        private void ProcessEvents()
        {
            if(Event.current.type == EventType.MouseDown && draggingNode == null)
            {
                draggingNode = GetNodeAtPoint(Event.current.mousePosition);
                if(draggingNode != null)
                {
                    draggingOffset = draggingNode.rect.position - Event.current.mousePosition;
                }
                else
                {
                    draggingCanvas = true;
                    draggingCanvasOffset = Event.current.mousePosition + scrollPosition;                    
                }
            }
            else if(Event.current.type == EventType.MouseDrag && draggingNode != null)
            {                
                Undo.RecordObject(selectedDialogue, "Move dialogue node");
                draggingNode.rect.position = Event.current.mousePosition + draggingOffset;
                GUI.changed = true;
            }            
            else if (Event.current.type == EventType.MouseDrag && draggingCanvas)
            {
                scrollPosition = draggingCanvasOffset - Event.current.mousePosition;
                GUI.changed = true;
            }
            else if(Event.current.type == EventType.MouseUp && draggingNode != null)
            {
                draggingNode = null;
            }
            else if (Event.current.type == EventType.MouseUp && draggingCanvas)
            {
                draggingCanvas = false;
            }
        }

        private void DrawNode(DialogueNode node)
        {
            GUILayout.BeginArea(new Rect(node.rect), nodeStyle);
            EditorGUI.BeginChangeCheck();

            string newText = EditorGUILayout.TextField(node.text);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(selectedDialogue, "Update Dialogue Text");
                node.text = newText;
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                creatingNode = node;
            }
            DrawLinkButtons(node);

            if (GUILayout.Button("x"))
            {
                nodeToDelete = node;
            }
            GUILayout.EndHorizontal();

            foreach (DialogueNode childNode in selectedDialogue.GetAllChildern(node))
            {
                EditorGUILayout.LabelField(childNode.text);
            }

            GUILayout.EndArea();
        }

        private void DrawLinkButtons(DialogueNode node)
        {
            if (linkingParentNode == null)
            {
                if (GUILayout.Button("link"))
                {
                    linkingParentNode = node;
                }
            }
            else if(linkingParentNode == node)
            {
                if (GUILayout.Button("cancel"))
                {
                    linkingParentNode = null;
                }
            }
            else if(linkingParentNode.children.Contains(node.uniqueID))
            {
                if (GUILayout.Button("Unlink"))
                {
                    Undo.RecordObject(selectedDialogue, "Unlink dialogue node");
                    linkingParentNode.children.Remove(node.uniqueID);
                    linkingParentNode = null;
                }
            }
            else
            {
                if (GUILayout.Button("child"))
                {
                    Undo.RecordObject(selectedDialogue, "Add dialogue link");
                    linkingParentNode.children.Add(node.uniqueID);
                    linkingParentNode = null;
                }
            }
        }

        private DialogueNode GetNodeAtPoint(Vector2 point)
        {
            DialogueNode foundNode = null;
            foreach(DialogueNode node in selectedDialogue.GetAllNodes())
            {
                if(node.rect.Contains(point))
                {
                    foundNode = node;
                }
            }
            return foundNode;
        }
    }
}