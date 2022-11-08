using System.Collections;
using System.Collections.Generic;

public class StudentGroup {
    public string name;
    public int numStudentsWithScore;
    public decimal averageScore;
    public List<Student> students {get; private set;} = new List<Student>();

    public StudentGroup(string name) {
        this.name = name;
    }

    public override string ToString() {
        return name;
    }
}