using Codice.Client.Common.GameUI;
using System.Drawing.Drawing2D;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace up.AI.Perception
{
    [CustomEditor(typeof(EnemyPerception))]
    
    public class EnemyPerceptionEditor : Editor
    {
        private void OnSceneGUI()
        {
            EnemyPerception f = (EnemyPerception)target;

            if(f.EnemyAI == null || f.EnemyAI.config == null || f.Eye == null) return;

            var cfg = f.EnemyAI.config;

            Handles.color = Color.white;
            Handles.DrawWireArc(f.Eye.position, Vector3.up, Vector3.forward, 360, cfg.viewRadius);

            Vector3 angleA = f.DirForAngle(-cfg.viewAlgle * 0.5f);
            Vector3 angleB = f.DirForAngle(cfg.viewAlgle * 0.5f);

            Handles.color = Color.blue;
            Handles.DrawLine(f.Eye.position, f.Eye.position + angleA * cfg.viewRadius);
            Handles.DrawLine(f.Eye.position, f.Eye.position + angleB * cfg.viewRadius);

            // Angulo Periferico \\

            Vector3 angleAp = f.DirForAngle(-cfg.viewPeripheralAngle * 0.5f);
            Vector3 angleBp = f.DirForAngle(cfg.viewPeripheralAngle * 0.5f);

            Handles.color = Color.yellow;
            Handles.DrawLine(f.Eye.position, f.Eye.position + angleAp * cfg.viewRadius);
            Handles.DrawLine(f.Eye.position, f.Eye.position + angleBp * cfg.viewRadius);

        }
    }
}