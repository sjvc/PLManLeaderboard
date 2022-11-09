using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using Baviux;

public class PLManWebScrapper : MonoBehaviour {
    public GameObject loadingIndicator;

    private const string URL_LOGIN = "https://plman.i3a.ua.es/administrator/index.php";
    private const string URL       = "https://plman.i3a.ua.es/administrator/index.php?option=com_plman";

    public List<StudentGroup> groups {get; private set;} = new List<StudentGroup>();

    public event System.Action<System.Exception> OnRequestFinished;

    private static ObjectPool<StudentGroup> groupPool = new ObjectPool<StudentGroup>(null, group => {
        for (int i=0; i<group.students.Count; i++) {
            studentPool.Release(group.students[i]);
            group.Reset();
        }
    });
    private static ObjectPool<Student> studentPool = new ObjectPool<Student>(null, student => student.Reset());

    private void Awake() {
        loadingIndicator.SetActive(false);
    }

    public void GetDataFromServer(string user, string passsword) {
        if (loadingIndicator.activeSelf) {
            return; // Si ya se está haciendo un request, no hacemos nada
        }

        StartCoroutine(GetDataFromServerCoroutine(user, passsword));
        // StartCoroutine(GetFakeDataFromServerCoroutine()); // Descomentar (y comentar la línea anterior) para usar datos de prueba
    }

    private IEnumerator GetDataFromServerCoroutine(string user, string password) {
        loadingIndicator.SetActive(true);

        // Try to get contents
        HttpGetRequest getRequest = new HttpGetRequest();
        yield return getRequest.Send(URL);

        // Login needed?
        if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password) && getRequest.responseText.Contains("<input type=\"hidden\" name=\"task\" value=\"login\"/>\n")) { 
            // Find variable form fields
            string[] varFormFields = getRequest.responseText.Substring("<input type=\"hidden\" name=\"task\" value=\"login\"/>\n", "</fieldset>", false, false).Split("\n");
            if (varFormFields.Length != 2) {
                Debug.LogError("Couldn't find variable form fields");
                yield break;
            }

            // Post request to login
            HttpPostRequest postRequest = new HttpPostRequest();
            postRequest.AddFormField("username", user);
            postRequest.AddFormField("passwd", password);
            postRequest.AddFormField("option", "com_login");
            postRequest.AddFormField("task", "login");
            postRequest.AddFormField("return", varFormFields[0].Substring("value=\"", "\"", false, false));
            postRequest.AddFormField(varFormFields[1].Substring("name=\"", "\"", false, false), "1");

            yield return postRequest.Send(URL_LOGIN);

            // Once logged in, try to get contents now
            yield return getRequest.Send(URL);
        }

        loadingIndicator.SetActive(false);

        // PARSE RESULTS
        System.Exception exception = null;
        try {
            ParseResponseText(getRequest.responseText);
        } catch (System.Exception e) {
            exception = e;
        }

        // NO DATA = ERROR
        if (exception == null && groups.Count == 0) {
            exception = new System.Exception("No data retrieved.\nIs user/password correct?");
        }

        if (OnRequestFinished != null) {
            OnRequestFinished(exception);
        }
    }

    private IEnumerator GetFakeDataFromServerCoroutine() {
        loadingIndicator.SetActive(true);
        yield return new WaitForSeconds(1f); // Simular tiempo de esepra
        loadingIndicator.SetActive(false);

        // Create new data
        if (groups.Count == 0) {
            for (int g=1; g<=10; g++) {
                StudentGroup group = groupPool.Get();
                group.name = "Group " + g;
                group.averageScore = (decimal)Random.Range(0f, 10f);
                groups.Add(group);

                for (int s=1; s<=25; s++) {
                    Student student = studentPool.Get();
                    student.name = "Student " + s;
                    student.group = group.name;
                    student.score = (decimal)Random.Range(0f, 10f);
                    group.students.Add(student);
                }
            }
        }
        // Modifiy existing data
        else {
            for (int g=0; g<groups.Count; g++) {
                if (Random.Range(0, 5) == 0) groups[g].averageScore += (decimal)Random.Range(0f, 1f);
                for (int s=0; s<groups[g].students.Count; s++) {
                    if (Random.Range(0, 5) == 0) groups[g].students[s].score += (decimal)Random.Range(0f, 1f);
                }
            }
        }

        if (OnRequestFinished != null) {
            OnRequestFinished(null);
        }
    }

    private void ParseResponseText(string responseText) {
        for (int i=0; i<groups.Count; i++) {
            groupPool.Release(groups[i]);
        }
        groups.Clear();

        string[] lines = responseText.Substring("<tbody>", "</tbody>").Split('\n');

        Student student = null;
        for (int i=0; i<lines.Length; i++) {
            // NAME
            if (lines[i].Contains("<td align=\"left\">")) {
                student = studentPool.Get();
                student.name = Regex.Replace(lines[i], "<td.*><a.*>(.*)</a>.*</td>", "$1").Trim().ToUpper();

                string[] compundName = student.name.Split(',');
                if (compundName.Length == 2) {
                    student.name  = compundName[1].TrimStart() + " " + compundName[0];
                }
            }
            // GROUP
            else if (lines[i].Contains("<td align=\"center\">")) {
                student.group = Regex.Replace(lines[i], "<td.*><a.*>(.*)</a>.*</td>", "$1").Trim().Replace("Grupo_", "");

                // Add to students group
                StudentGroup group = groups.Find(g => g.name == student.group);
                if (group == null) {
                    group = groupPool.Get();
                    group.name = student.group;
                    groups.Add(group);
                }
                group.students.Add(student);
            }
            // SCORE
            else if (lines[i].Contains("<td align=\"right\">")) {
                try{
                    student.score = decimal.Parse(Regex.Replace(lines[i], "<td.*>(.*)</td>", "$1").Trim(), CultureInfo.InvariantCulture);
                } catch (System.Exception e) {
                    Debug.LogError(e);
                }
            }
        }

        // COMPUTE GROUPS AVERAGE SCORE
        for (int i=0; i<groups.Count; i++) {
            decimal groupScoreSum = 0;
            for (int j=0; j<groups[i].students.Count; j++) {
                if (groups[i].students[j].score > 0) {
                    groups[i].numStudentsWithScore++;
                    groupScoreSum += groups[i].students[j].score;
                }
            }
            groups[i].averageScore = groups[i].numStudentsWithScore > 0 ? groupScoreSum / groups[i].numStudentsWithScore : 0;
        }
    }

    private void DebugToDesktopFile(string text) {
        StreamWriter writer = new StreamWriter(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "debug.txt"), false);
        writer.Write(text);
        writer.Close();
    }

}
