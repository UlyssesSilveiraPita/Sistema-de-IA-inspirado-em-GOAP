using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace up.AI.Waypoints.Tool
{
    public class WaypointCreator : EditorWindow
    {
        public GameObject wpNodePrefab;
        public WaypointsGroup waypointGroup;
        private bool _isEditMode;
        private bool _isSubNode;

        [MenuItem("up/tools/waypoint creator")]
        private static void Init()
        {
            WaypointCreator window = GetWindow<WaypointCreator>(false, "Waypoint Creator");
            window.Show();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui -= OnSeceneGUI;
            SceneView.duringSceneGui += OnSeceneGUI;
        }
        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSeceneGUI;
        }

        private void OnGUI()
        {
            _isEditMode = GUILayout.Toggle(_isEditMode,"Modo de Edicao","Button", GUILayout.Height(30));
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Waypoint Group", GUILayout.Width(100));
            waypointGroup = EditorGUILayout.ObjectField(waypointGroup, typeof(WaypointsGroup), true) as WaypointsGroup;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Node Prefab", GUILayout.Width(100));
            wpNodePrefab = EditorGUILayout.ObjectField(wpNodePrefab, typeof(GameObject), true) as GameObject;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            _isSubNode = GUILayout.Toggle(_isSubNode, "SubNode", "Button", GUILayout.Height(30));


            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox("Em modo de Edicao, clique na Cena para add WP Nodes", MessageType.Info);
        }

        private void OnSeceneGUI(SceneView sceneView)
        {
            Event e = Event.current;

            if (_isEditMode == false) return;

            if(e.type == EventType.MouseDown && e.button == 0 && e.alt == false)
            {
                if (wpNodePrefab == null)
                {
                    Debug.LogWarning("[Waypoint Creator] - Node Prefab null");
                    return;
                }

                if(waypointGroup == null)
                {
                    Debug.LogWarning("[Wapoint Creator] - Waypoint Group null");
                    return;
                }

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                RaycastHit hitInfo;

                if(Physics.Raycast(ray, out hitInfo, 1000f))
                {
                    GameObject nodeInstance = (GameObject)PrefabUtility.InstantiatePrefab(wpNodePrefab);
                    nodeInstance.transform.position = hitInfo.point;
                    nodeInstance.transform.SetParent(waypointGroup.transform);

                    WaypointNode node = nodeInstance.GetComponent<WaypointNode>();
                    node.ownerGroup = waypointGroup;
                    node.isSubNode = _isSubNode;

                    SerializedObject so = new SerializedObject(waypointGroup);
                    SerializedProperty points = so.FindProperty("_points");
                    
                    if(points != null)
                    {
                        int newIndex = points.arraySize;
                        points.InsertArrayElementAtIndex(newIndex);
                        points.GetArrayElementAtIndex(newIndex).objectReferenceValue = node;

                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(waypointGroup);
                    }


                    e.Use();
                }



            }

            sceneView.Repaint();


        }
    }

}

