public class Student {
    public string name;
    public string group;
    public decimal score;
 
    public Student() {
        
    }

    public void Reset() {
        this.name = null;
        this.group = null;
        this.score = 0;
    }

    public override string ToString() {
        return name;
    }
}