using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Baviux;
using TMPro;

public interface ILeaderboardItemClickListener : IMessageHandler {
    void OnItemClick(string text);
}

public class LeaderboardItemClickHandler : MonoBehaviour, IPointerClickHandler {
    public TextMeshProUGUI textMesh;
    public List<GameObject> onClickListeners;

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        for (int i=0; i<onClickListeners.Count; i++) {
            onClickListeners[i].SendMessage<ILeaderboardItemClickListener>(f => f.OnItemClick( textMesh.text ));
        }
    }

}
