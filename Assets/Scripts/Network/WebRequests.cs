using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


// Para ignorar el certificado no válido
public class TrustfulCertificateHandler : CertificateHandler {
    protected override bool ValidateCertificate(byte[] certificateData) {
        return true;
    }
}

public class HttpGetRequest {
    public string responseText{get; private set;}

    public IEnumerator Send(string url) {
        responseText = "";

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.certificateHandler = new TrustfulCertificateHandler();
        yield return www.SendWebRequest();
 
        if(www.result == UnityWebRequest.Result.ConnectionError) {
            Debug.LogError(www.error);
        }
        else {
            responseText = www.downloadHandler.text;
        }

        www.Dispose();
    }
}

public class HttpPostRequest {
    private List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

    public void AddFormField(string name, string value) {
        formData.Add(new MultipartFormDataSection(name, value));
    }

    public IEnumerator Send(string url) {
        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        www.certificateHandler = new TrustfulCertificateHandler();
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError(www.error);
        }

        www.Dispose();
    }
}