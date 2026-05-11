using UnityEditor;
using up.AI.Actions;

namespace up.AI.Utils
{
    [CustomEditor(typeof(EnemyAI))]
    public class EnemyAIEditor : Editor
    {
        private Editor _configEditor;
        private Editor _actionEditor;

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI();

            EnemyAI ai = (EnemyAI)target;
            EnemyConfig config = ai.config;
            ActionProfile action = config.actionProfile;

            if (config != null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Config Data", EditorStyles.boldLabel);

                if(_configEditor == null || _configEditor.target != config)
                {
                    _configEditor = CreateEditor(config);
                }

                if(_configEditor != null)
                {
                    _configEditor.OnInspectorGUI();
                }

                if(_actionEditor == null || _actionEditor.target != action)
                {
                    _actionEditor = CreateEditor(action);
                }

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Action Profile", EditorStyles.boldLabel);

                if(_actionEditor  != null)
                {
                    _actionEditor.OnInspectorGUI();
                }

            }
        }

    }

}
