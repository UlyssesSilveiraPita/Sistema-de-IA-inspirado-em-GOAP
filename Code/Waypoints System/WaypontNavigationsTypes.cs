using UnityEngine;
using UnityEngine.UI;
using up.AI.Actions;

namespace up.AI.Waypoints
{
    public enum RouteTraversalMode
    {
        Loop = 0,
        PingPong = 1,
        Once = 2
    }

    public enum RouteDirection
    {
        None = 0,
        Forward = 1,
        Backward = -1,
    }

    [System.Serializable]
    public struct WaypointNavigationConfig
    {
        [Header("Percurso basico")]
        public bool useRandom;
        public RouteTraversalMode traversalMode;
        public RouteDirection routeDirection;

        [Header("Inverter direcao aleatoriamente")]
        public bool allowRandomDirectionFlip;
        [Range(0f, 1f)]
        public float directionFlipChance;

        [Header("Troca de Rota")]
        public bool allowRouteChange;
        public float routeChangeProbability;

        [Header("Sub-acoes por waypoint (opcional)")]
        public ActionProfile onReachWaypointProfile;
        public bool allowNodeActionOverride;
        


    }




}