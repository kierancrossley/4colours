[System.Serializable] // allows for multiple classes with the same name 

public class Card
{
    public int id { get; private set; } // encapsulation 
    public string type { get; private set; }
    public string colour { get; private set; }

    public Card(int Id, string Type, string Colour) // constructor, same name as class as serialized 
    {
        id = Id;
        type = Type;
        colour = Colour;
    }
}