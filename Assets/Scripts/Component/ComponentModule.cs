using UnityEngine;


public abstract class AttackArchetype { }
public abstract class DefendArchetype { }
public abstract class TargetArchetype { } // I can't think of examples right now so this may not be needed
public class AttackAction { }
public class DefendAction { }
public class TargetAction { }

public abstract class Description { }

public abstract class ComponentModule
{
    public Description Description { get; private set; }
}
