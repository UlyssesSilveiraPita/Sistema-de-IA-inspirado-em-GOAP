using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.Utilities;


namespace up.AI.Waypoints
{
    public class WaypointManager : MonoBehaviour
    {
        [Header("Waypoints Group (Inicial)")]
        [SerializeField] private WaypointsGroup _startWaypointsGroup;

        public int PointCount => _currentGroup.Points.Count;

        [Header("RunTime")]
        [SerializeField] private WaypointsGroup _currentGroup;
        [SerializeField] private List<WaypointNode> _points = new List<WaypointNode>();
        [SerializeField] private int _currentIndex;
        [SerializeField] private bool _useRandom;
        [SerializeField] private RouteTraversalMode _traversalMode = RouteTraversalMode.Loop;
        [SerializeField] private RouteDirection _direction = RouteDirection.Forward;
        [SerializeField] private bool _allowRandomDirectionFlip;
        [SerializeField] private float _directionFlipChance;
        [SerializeField] private bool _allowRouteChange;
        [SerializeField] private float _routeChangeProbability;

        private WaypointsGroup _lastRouteChangeFromGroup;
        private int _lastRouteChangeFromIndex;
        private bool _hasRecentRouteChange;


        private void Start()
        {
            if(_startWaypointsGroup != null)
            {
                LoadFromGroup(_startWaypointsGroup);
            }
        }


        public void LoadFromGroup(WaypointsGroup group, int startIndex = 0)
        {
            if(group == null) {return; }

            if(group.Points != null && group.Points.Count > 0)
            {
                _currentGroup = group;
                _points.Clear();
                _points.AddRange(_currentGroup.Points);
                _currentIndex = startIndex;
            }

        }

        public void LoadWaypointNavigationConfig(WaypointNavigationConfig navigationConfig)
        {
            _useRandom = navigationConfig.useRandom;
            _traversalMode = navigationConfig.traversalMode;
            _direction  = navigationConfig.routeDirection;
            _allowRandomDirectionFlip  = navigationConfig.allowRandomDirectionFlip;
            _directionFlipChance = navigationConfig.directionFlipChance;
            _allowRouteChange = navigationConfig.allowRouteChange;
            _routeChangeProbability = navigationConfig.routeChangeProbability;
        }

        public WaypointNode GetCurrentPoint()
        {
            if(_points == null || _points.Count == 0) { return null; }
            
            _currentIndex = Mathf.Clamp(_currentIndex, 0 , _points.Count -1);
            return _points[_currentIndex];  
        }

        public WaypointNode Advance()
        {
            if (_points == null || _points.Count == 0) { return null; }

            if (_points.Count == 1)
            {
                _currentIndex = 0;
                return _points[_currentIndex];
            }

            var currentWP = GetCurrentPoint();
            if (currentWP.routeLinks != null && currentWP.routeLinks.Count > 0)
            {
                if (_allowRouteChange == true && Random.value <= _routeChangeProbability)
                {
                    var links = currentWP.routeLinks;
                    var candidates = new List<RouteLink>();

                    foreach (var link in links)
                    {
                        if(link == null || link.targetGroup == null) continue;

                        if(_hasRecentRouteChange == false)
                        {
                            candidates.Add(link);
                        }
                        else // if(_hasRecentRouteChange == true)
                        {
                            bool isBackToOrigin = link.targetGroup == _lastRouteChangeFromGroup && link.targetIndex == _lastRouteChangeFromIndex;
                            if (isBackToOrigin == false)
                            {
                                candidates.Add(link);
                            }
                        }
                    }

                    int idxLink = 0;
                    if(candidates.Count >= 1)
                    {
                        idxLink = Random.Range(0, candidates.Count);

                        _lastRouteChangeFromGroup = _currentGroup;
                        _lastRouteChangeFromIndex = _currentIndex;
                        _hasRecentRouteChange = true;

                        LoadFromGroup(candidates[idxLink].targetGroup);
                        _currentIndex = links[idxLink].targetIndex;

                        return _points[_currentIndex];
                    }

                }
            }

            _hasRecentRouteChange = false;
            _currentIndex = GetNextIndex(_currentIndex);
            return _points[_currentIndex];

        }

        private int GetNextIndex(int fromIndex)
        {
            if (_points == null || _points.Count == 0) { return 0; }

            int count = _points.Count;
            int lastIndex = count - 1;  

            //==Random==\\
            if(_useRandom == true)
            {
                return Random.Range(0, count);
            }

            if(_allowRandomDirectionFlip == true && Random.value <= _directionFlipChance)
            {
                _direction = (_direction == RouteDirection.Forward) ? RouteDirection.Backward : RouteDirection.Forward;
                Debug.Log($"Direcao Trocada para {_direction}");
            }


            //==Loop,Once,PingPong==\\
            switch(_traversalMode)
            {
                case RouteTraversalMode.Loop:
                case RouteTraversalMode.Once:

                    return (fromIndex + (int)_direction + count) % count;
 
                case RouteTraversalMode.PingPong:

                    int nextIndex = fromIndex + (int)_direction;                    
                    if(nextIndex > lastIndex)
                    {
                        nextIndex = lastIndex - 1;
                        _direction = RouteDirection.Backward;
                    }
                    else if(nextIndex < 0)
                    {
                        nextIndex = 1;
                        _direction = RouteDirection.Forward;
                    }
                        return nextIndex;
                        
            }

            return 0;
        }

    } 
}