using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "sriptableData/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName; // 이름

    [TextArea(3, 5)]
    public string characterDescription; // 설명

    public Sprite characterIcon; // 아이콘

    public int characterID; // 고유 ID
}