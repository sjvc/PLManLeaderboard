public class Student {
    public string name;
    public string group;
    public decimal score;
 
    public Student() {
        
    }

    public Student(string name, string group, decimal score) {
        this.name = name;
        this.group = group;
        this.score = score;
    }

    public override string ToString() {
        return name;
    }
}