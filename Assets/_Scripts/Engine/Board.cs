using System.Collections.Generic;

public class Board
{
    private static int DEFAULT_NUMBER_OF_FIELDS = 24;
    private List<Field> fields;
    public Board()
    {
        this.fields = new List<Field>(DEFAULT_NUMBER_OF_FIELDS);
        for(int i = 0; i < DEFAULT_NUMBER_OF_FIELDS; i++)
        {
            this.fields.Add(new Field());
        }
    }

    public Board(Board other)
    {
        this.fields = new List<Field>(other.fields.Count);
        for(int i = 0; i < other.fields.Count; i++)
        {
            this.fields.Add(new Field(other.fields[i]));
        }
    }

    public Field GetField(int index)
    {
        return fields[index];
    }
}
