public abstract class Objective 
{
    protected bool isCompleted = false;
    public bool IsCompleted => isCompleted;    
    public abstract void CompleteObjective();
    // Abstrakt metode til at f� beskrivelse af objektivet
    public abstract string GetObjectiveDescription();
}
