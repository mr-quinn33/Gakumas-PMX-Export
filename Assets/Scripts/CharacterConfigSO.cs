using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

public interface ICharacterConfigSO
{
	Object Face { get; }
	
	int Size { get; }
	
	Object this[int x, int y] { get; }
	
	(Object mod, Object hair, Object body) this[int y] { get; }
}

[CreateAssetMenu(menuName = "Export/Character", fileName = "New Character Config", order = 0)]
public class CharacterConfigSO : SerializedScriptableObject, ICharacterConfigSO
{
	[SerializeField]
	private Object face;
	
	[TableMatrix(HorizontalTitle = "Parts", VerticalTitle = "Characters", DrawElementMethod = nameof(DrawObjectField))] [OdinSerialize] [SerializeField]
	private Object[,] configTable = new Object[3, 3];

	private static Object DrawObjectField(Rect position, Object value)
	{
		return EditorGUI.ObjectField(position, value, typeof(Object), false);
	}

	public Object Face => face;
	
	public int Size => configTable.GetLength(1);

	public Object this[int x, int y] => configTable[x, y];

	public (Object mod, Object hair, Object body) this[int y] => (this[0, y], this[1, y], this[2, y]);
}