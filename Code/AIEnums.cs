namespace up.AI
{
    public enum AIRationality
    { 
        RATIONAL = 0,
        IRRATIONAL = 1, 
        
    }

    public enum AIMode
    { 
        None,
        World, // acoes mundanas
        Combat
    
    }

    public enum MoveMode
    {
        Stop = 0,
        Walk = 1,
        Run = 2,
        Sprint = 3,

    }

    public enum RotationMode
    {
        None,
        Auto,
        AgentDriven,
        ManualLookAt,
        LookAtPath,
        LookAtTarget

    }

    public enum RelativeMoveDirection
    {
        Away,
        Left,
        Right,
    }
}