namespace EmuVRShapeDrawer;
public class SaveSlot
{
    public double version { get; set; }
    public double time { get; set; }
    public bool realTime { get; set; }
    public double dimmer { get; set; }
    public bool ceilingLight { get; set; }
    public bool fan {get;set;}
    public int season {get;set;}
    public object[] objects {get;set;}
    public object[] compounds {get;set;}
    public object[] systems {get;set;}
    public List<Game> games { get; set; } 
}

public class Game
{
    public Position position { get; set; }
    public Rotation rotation { get; set; }
    public Scale scale { get; set; }
    public bool frozen { get; set; }
    public string path { get; set; }
}



public class Position
{
    public double x { get; set; }
    public double y { get; set; }
    public double z { get; set; }
}

public class Rotation
{
    public double x { get; set; }
    public double y { get; set; }
    public double z { get; set; }
    public double w { get; set; }
}

public class Scale
{
    public double x { get; set; }
    public double y { get; set; }
    public double z { get; set; }
}