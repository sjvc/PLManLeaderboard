using System.Collections;
using System.Collections.Generic;

public class StudentGroup {
    public string name;
    public int numStudentsWithScore;
    public decimal averageScore;
    public List<Student> students {get; private set;} = new List<Student>();

    public StudentGroup() {

    }

    public void Reset() {
        this.name = null;
        this.numStudentsWithScore = 0;
        this.averageScore = 0;
        this.students.Clear();
    }

    public override string ToString() {
        return name;
    }
}